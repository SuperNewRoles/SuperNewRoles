using Mono.Cecil;
using Mono.Cecil.Cil;

namespace SuperNewRoles.RpcWeaver;

/// <summary>Generates metadata factories, keeping Harmony's own wrapper and native backend.</summary>
public static class HarmonyRegistryWeaver
{
    private const string Registry = "SuperNewRoles.Modules.GeneratedHarmonyRegistry";
    private static readonly string[] Kinds = { "Prefix", "Postfix", "Transpiler", "Finalizer", "ReversePatch", "ILManipulator" };
    private static readonly string[] Auxiliary = { "Prepare", "Cleanup", "TargetMethod", "TargetMethods" };
    private sealed record Patch(MethodDefinition Method, int Kind, Dictionary<string, CustomAttributeArgument> Fields);
    private sealed record Entry(TypeDefinition Type, List<Patch>? Patches);

    public static (int Classes, int Prepared) Weave(string path, string? referencesFile = null)
    {
        path = Path.GetFullPath(path);
        using var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(Path.GetDirectoryName(path));
        resolver.AddSearchDirectory(Path.GetDirectoryName(typeof(object).Assembly.Location));
        if (referencesFile != null)
            foreach (var dir in File.ReadAllLines(referencesFile).Select(Path.GetDirectoryName).Distinct())
                resolver.AddSearchDirectory(dir);
        bool symbols = File.Exists(Path.ChangeExtension(path, ".pdb"));
        using var assembly = AssemblyDefinition.ReadAssembly(path, new ReaderParameters
        {
            InMemory = true, ReadSymbols = symbols, AssemblyResolver = resolver
        });
        var module = assembly.MainModule;
        var types = AllTypes(module.Types).OrderBy(t => t.MetadataToken.ToInt32()).ToArray();
        var registry = types.Single(t => t.FullName == Registry);
        var create = registry.Methods.Single(m => m.Name == "Create");
        // Factories are immutable for a compiled assembly. An incremental build can invoke us again.
        if (registry.Methods.Any(m => m.Name.StartsWith("BuildEntry_", StringComparison.Ordinal)))
            return (registry.Methods.Count(m => m.Name.StartsWith("BuildEntry_", StringComparison.Ordinal)), 0);
        var entries = types.Where(t => t.FullName != "SuperNewRoles.Modules.StartupHarmonyPatcher/BatchContainer" &&
            HasHarmonyMetadata(t)).Select(Analyze).ToArray();
        var entryType = types.Single(t => t.FullName == "SuperNewRoles.Modules.StartupPatchEntry");
        var entryCtor = entryType.Methods.Single(m => m.IsConstructor);
        var harmonyMethod = entryType.Fields.Single(f => f.Name == "Patches").FieldType.GetElementType().Resolve();
        var harmonyCtor = module.ImportReference(harmonyMethod.Methods.Single(m => m.IsConstructor && m.Parameters.Count == 0));
        // Refer to the target's framework, never the build tool's System.Private.CoreLib version.
        TypeReference Core(string ns, string name, bool valueType = false) => new(ns, name, module, module.TypeSystem.Object.Scope, valueType);
        var getType = new MethodReference("GetTypeFromHandle", Core("System", "Type"), Core("System", "Type"));
        getType.Parameters.Add(new ParameterDefinition(Core("System", "RuntimeTypeHandle", true)));
        var getMethod = new MethodReference("GetMethodFromHandle", Core("System.Reflection", "MethodBase"), Core("System.Reflection", "MethodBase"));
        getMethod.Parameters.Add(new ParameterDefinition(Core("System", "RuntimeMethodHandle", true)));
        getMethod.Parameters.Add(new ParameterDefinition(Core("System", "RuntimeTypeHandle", true)));
        var methodInfo = Core("System.Reflection", "MethodInfo");
        var kindType = entryType.Fields.Single(f => f.Name == "Kinds").FieldType.GetElementType();

        create.Body = new MethodBody(create) { MaxStackSize = 8 };
        create.DebugInformation.SequencePoints.Clear();
        var main = create.Body.GetILProcessor();
        main.Emit(OpCodes.Ldc_I4, entries.Length);
        main.Emit(OpCodes.Newarr, entryType);
        for (int i = 0; i < entries.Length; i++)
        {
            var entry = entries[i];
            // Resolve handles without attribute enumeration or a search by method name.
            var factory = new MethodDefinition("BuildEntry_" + i, MethodAttributes.Assembly | MethodAttributes.Static, entryType);
            registry.Methods.Add(factory);
            var il = factory.Body.GetILProcessor();
            factory.Body.MaxStackSize = 12;
            EmitType(il, entry.Type, module, getType);
            if (entry.Patches == null)
            {
                il.Emit(OpCodes.Ldnull);
                il.Emit(OpCodes.Ldnull);
            }
            else
            {
                il.Emit(OpCodes.Ldc_I4, entry.Patches.Count);
                il.Emit(OpCodes.Newarr, module.ImportReference(harmonyMethod));
                for (int j = 0; j < entry.Patches.Count; j++)
                {
                    var patch = entry.Patches[j];
                    il.Emit(OpCodes.Dup);
                    il.Emit(OpCodes.Ldc_I4, j);
                    il.Emit(OpCodes.Newobj, harmonyCtor);
                    foreach (var field in patch.Fields)
                    {
                        var definition = harmonyMethod.Fields.Single(f => f.Name == field.Key);
                        il.Emit(OpCodes.Dup);
                        EmitValue(il, field.Value, module, getType);
                        if (definition.FieldType is GenericInstanceType nullable && nullable.ElementType.FullName == "System.Nullable`1")
                        {
                            var ctor = new MethodReference(".ctor", module.TypeSystem.Void, module.ImportReference(nullable)) { HasThis = true };
                            // Nullable<T>'s member signature uses !0, not the concrete enum.
                            ctor.Parameters.Add(new ParameterDefinition(nullable.ElementType.Resolve().GenericParameters[0]));
                            il.Emit(OpCodes.Newobj, ctor);
                        }
                        il.Emit(OpCodes.Stfld, module.ImportReference(definition));
                    }
                    il.Emit(OpCodes.Dup);
                    il.Emit(OpCodes.Ldtoken, patch.Method);
                    il.Emit(OpCodes.Ldtoken, entry.Type);
                    il.Emit(OpCodes.Call, getMethod);
                    il.Emit(OpCodes.Castclass, methodInfo);
                    il.Emit(OpCodes.Stfld, module.ImportReference(harmonyMethod.Fields.Single(f => f.Name == "method")));
                    il.Emit(OpCodes.Stelem_Ref);
                }
                il.Emit(OpCodes.Ldc_I4, entry.Patches.Count);
                il.Emit(OpCodes.Newarr, kindType);
                for (int j = 0; j < entry.Patches.Count; j++)
                {
                    il.Emit(OpCodes.Dup);
                    il.Emit(OpCodes.Ldc_I4, j);
                    il.Emit(OpCodes.Ldc_I4, entry.Patches[j].Kind);
                    il.Emit(OpCodes.Stelem_I4);
                }
            }
            il.Emit(OpCodes.Newobj, entryCtor);
            il.Emit(OpCodes.Ret);
            main.Emit(OpCodes.Dup);
            main.Emit(OpCodes.Ldc_I4, i);
            main.Emit(OpCodes.Call, factory);
            main.Emit(OpCodes.Stelem_Ref);
        }
        main.Emit(OpCodes.Ret);
        string temporary = path + ".harmony.tmp.dll";
        try
        {
            assembly.Write(temporary, new WriterParameters { WriteSymbols = symbols });
            File.Move(temporary, path, overwrite: true);
            if (symbols) File.Move(Path.ChangeExtension(temporary, ".pdb"), Path.ChangeExtension(path, ".pdb"), overwrite: true);
        }
        finally
        {
            File.Delete(temporary);
            File.Delete(Path.ChangeExtension(temporary, ".pdb"));
        }
        return (entries.Length, entries.Count(e => e.Patches != null));
    }

    private static Entry Analyze(TypeDefinition type)
    {
        Entry Fallback() => new(type, null);
        if (type.BaseType?.FullName != "System.Object" || HasGenericOwner(type)) return Fallback();
        // 2.10.2 drops the class priority in some merges; Android 2.16 combines it differently.
        // Let the installed Harmony preserve its own rule rather than baking either interpretation.
        if (type.CustomAttributes.Any(a => a.AttributeType.FullName == "HarmonyLib.HarmonyPriority")) return Fallback();
        if (type.Methods.Any(m => Auxiliary.Any(n => m.Name == n || m.CustomAttributes.Any(a => a.AttributeType.FullName == "HarmonyLib.Harmony" + n))))
            return Fallback();
        var fields = new Dictionary<string, CustomAttributeArgument>();
        if (!Merge(type.CustomAttributes, fields)) return Fallback();
        var methodType = type.Module.TypeSystem.Int32;
        if (!fields.ContainsKey("methodType")) fields.Add("methodType", new(methodType, 0));
        if (fields.TryGetValue("debug", out var debug) && Equals(debug.Value, true)) return Fallback();
        var patches = new List<Patch>();
        foreach (var method in type.Methods)
        {
            int kind = Array.FindIndex(Kinds, k => method.Name == k || method.CustomAttributes.Any(a => a.AttributeType.FullName == "HarmonyLib.Harmony" + k));
            if (kind < 0) continue;
            if (kind > 1 || !method.IsStatic || method.HasGenericParameters) return Fallback();
            // Multi-target method annotations have HarmonyX-specific expansion rules: preserve them via fallback.
            if (method.CustomAttributes.Count(a => a.AttributeType.FullName == "HarmonyLib.HarmonyPatch" &&
                a.ConstructorArguments.Any(p => p.Type.FullName == "System.String")) > 1) return Fallback();
            var merged = new Dictionary<string, CustomAttributeArgument>(fields);
            if (!Merge(method.CustomAttributes, merged)) return Fallback();
            if (merged.TryGetValue("debug", out debug) && Equals(debug.Value, true)) return Fallback();
            patches.Add(new(method, kind + 1, merged)); // HarmonyPatchType: All=0, Prefix=1, Postfix=2.
        }
        return patches.Count == 0 ? Fallback() : new(type, patches);
    }

    private static bool Merge(IEnumerable<CustomAttribute> attributes, Dictionary<string, CustomAttributeArgument> fields)
    {
        foreach (var attribute in attributes)
        {
            string name = attribute.AttributeType.FullName;
            if (!name.StartsWith("HarmonyLib.", StringComparison.Ordinal))
            {
                if (IsHarmonyMetadata(attribute)) return false;
                continue;
            }
            if (name is "HarmonyLib.HarmonyPrefix" or "HarmonyLib.HarmonyPostfix" or "HarmonyLib.HarmonyArgument") continue;
            if (attribute.HasFields || attribute.HasProperties) return false;
            switch (name)
            {
                case "HarmonyLib.HarmonyPatch":
                    foreach (var arg in attribute.ConstructorArguments)
                    {
                        string? field = arg.Type.FullName switch
                        {
                            "System.Type" => "declaringType",
                            "System.String" => "methodName",
                            "System.Type[]" => "argumentTypes",
                            "HarmonyLib.MethodType" => "methodType",
                            _ => null
                        };
                        if (field == null || arg.Value == null) return false;
                        // The string-only declaring-type overload is deliberately not precomputed.
                        if (arg.Type.FullName == "System.String")
                        {
                            MethodDefinition? constructor;
                            try { constructor = attribute.Constructor.Resolve(); }
                            catch (AssemblyResolutionException) { return false; }
                            if (constructor == null || constructor.Parameters.Any(p => p.Name == "typeName")) return false;
                        }
                        fields[field] = arg;
                    }
                    break;
                case "HarmonyLib.HarmonyPriority": fields["priority"] = attribute.ConstructorArguments[0]; break;
                case "HarmonyLib.HarmonyBefore": fields["before"] = attribute.ConstructorArguments[0]; break;
                case "HarmonyLib.HarmonyAfter": fields["after"] = attribute.ConstructorArguments[0]; break;
                case "HarmonyLib.HarmonyDebug": fields["debug"] = new(attribute.AttributeType.Module.TypeSystem.Boolean, true); break;
                default: return false;
            }
        }
        return true;
    }

    private static bool IsHarmonyMetadata(CustomAttribute attribute)
    {
        for (var type = attribute.AttributeType.Resolve(); type != null; type = type.BaseType?.Resolve())
            if (type.Fields.Any(f => f.IsPublic && f.Name == "info" && f.FieldType.FullName == "HarmonyLib.HarmonyMethod"))
                return true;
        return false;
    }

    private static bool HasHarmonyMetadata(TypeDefinition? type) => type != null &&
        (type.CustomAttributes.Any(IsHarmonyMetadata) ||
        (type.BaseType != null && type.BaseType.FullName != "System.Object" && HasHarmonyMetadata(type.BaseType.Resolve())));

    private static bool HasGenericOwner(TypeDefinition? type) => type != null && (type.HasGenericParameters || HasGenericOwner(type.DeclaringType));

    private static void EmitType(ILProcessor il, TypeReference type, ModuleDefinition module, MethodReference getType)
    {
        il.Emit(OpCodes.Ldtoken, module.ImportReference(type));
        il.Emit(OpCodes.Call, getType);
    }

    private static void EmitValue(ILProcessor il, CustomAttributeArgument arg, ModuleDefinition module, MethodReference getType)
    {
        if (arg.Value is TypeReference type) EmitType(il, type, module, getType);
        else if (arg.Value is string text) il.Emit(OpCodes.Ldstr, text);
        else if (arg.Value is CustomAttributeArgument[] array)
        {
            il.Emit(OpCodes.Ldc_I4, array.Length);
            il.Emit(OpCodes.Newarr, module.ImportReference(((ArrayType)arg.Type).ElementType));
            for (int i = 0; i < array.Length; i++)
            {
                il.Emit(OpCodes.Dup);
                il.Emit(OpCodes.Ldc_I4, i);
                EmitValue(il, array[i], module, getType);
                il.Emit(OpCodes.Stelem_Ref);
            }
        }
        else il.Emit(OpCodes.Ldc_I4, Convert.ToInt32(arg.Value));
    }

    private static IEnumerable<TypeDefinition> AllTypes(IEnumerable<TypeDefinition> types)
    {
        foreach (var type in types)
        {
            yield return type;
            foreach (var nested in AllTypes(type.NestedTypes)) yield return nested;
        }
    }
}
