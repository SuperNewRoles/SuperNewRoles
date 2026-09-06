using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace SuperNewRoles.RpcWeaver;

public static class RpcAssemblyWeaver
{
    private const string RpcAttribute = "SuperNewRoles.Modules.CustomRPCAttribute";
    private const string WovenAttribute = "SuperNewRoles.Modules.WovenRpcAttribute";
    private const int Version = 1;

    private static bool IsAbilityType(TypeDefinition type)
    {
        for (TypeDefinition? current = type; current != null;)
        {
            if (current.FullName == "SuperNewRoles.Roles.Ability.AbilityBase")
                return true;
            var baseType = current.BaseType;
            if (baseType == null) break;
            try
            {
                current = baseType.Resolve() ?? throw new InvalidOperationException(
                    $"Cannot validate RPC owner {type.FullName}: base type {baseType.FullName} in {baseType.Scope} was not found.");
            }
            catch (AssemblyResolutionException ex)
            {
                throw new InvalidOperationException(
                    $"Cannot validate RPC owner {type.FullName}: dependency {ex.AssemblyReference.FullName} for base type {baseType.FullName} could not be resolved. Supply its directory through referencesFile.", ex);
            }
        }
        return false;
    }

    public static int Weave(string assemblyPath, string? referencesFile = null)
    {
        assemblyPath = Path.GetFullPath(assemblyPath);
        bool symbols = File.Exists(Path.ChangeExtension(assemblyPath, ".pdb"));
        using var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(Path.GetDirectoryName(assemblyPath));
        resolver.AddSearchDirectory(Path.GetDirectoryName(typeof(object).Assembly.Location));
        if (referencesFile != null)
            foreach (var directory in File.ReadAllLines(referencesFile).Select(Path.GetDirectoryName).Distinct())
                resolver.AddSearchDirectory(directory);
        using var assembly = AssemblyDefinition.ReadAssembly(assemblyPath,
            new ReaderParameters { ReadSymbols = symbols, InMemory = true, AssemblyResolver = resolver });
        var module = assembly.MainModule;
        var types = AllTypes(module.Types).ToArray();
        var methods = types.SelectMany(t => t.Methods)
            .Where(m => m.CustomAttributes.Any(a => a.AttributeType.FullName == RpcAttribute)).ToArray();
        var manager = types.Single(t => t.FullName == "SuperNewRoles.Modules.CustomRPCManager");
        var prefix = manager.Methods.Single(m => m.Name == "ShouldExecuteWovenRpc");
        var markerConstructor = types.Single(t => t.FullName == WovenAttribute).Methods.Single(m => m.IsConstructor);
        var ids = new Dictionary<int, string>();
        // Validate the entire assembly before changing any method.
        foreach (var method in methods)
        {
            // Match CustomRPCManager.Load: instance RPCs require AbilityBase ownership.
            if (!method.IsStatic && !IsAbilityType(method.DeclaringType))
                throw new NotSupportedException($"RPC instance must derive from AbilityBase: {method.FullName}");
            if (!method.HasBody || method.DeclaringType.IsValueType || method.ReturnType.MetadataType != MetadataType.Void ||
                method.HasGenericParameters || HasGenericDeclaringType(method.DeclaringType) ||
                method.Parameters.Any(p => p.ParameterType.IsByReference || p.ParameterType.IsPointer || p.ParameterType.IsFunctionPointer))
                throw new NotSupportedException($"Unsupported RPC signature: {method.FullName}");
            string signature = Signature(method);
            int id = Id(signature);
            if (ids.TryGetValue(id, out var existing))
                throw new InvalidOperationException($"RPC ID collision: {existing} / {signature}");
            ids.Add(id, signature);
            var marker = method.CustomAttributes.SingleOrDefault(a => a.AttributeType.FullName == WovenAttribute);
            if (marker != null && ((int)marker.ConstructorArguments[0].Value != id ||
                                   (int)marker.ConstructorArguments[1].Value != Version))
                throw new InvalidOperationException($"Invalid existing weave marker: {signature}");
        }

        int changed = 0;
        foreach (var method in methods)
        {
            if (method.CustomAttributes.Any(a => a.AttributeType.FullName == WovenAttribute))
                continue;
            int id = Id(Signature(method));
            var body = method.Body;
            var first = body.Instructions[0];
            var processor = body.GetILProcessor();
            void Emit(Instruction instruction) => processor.InsertBefore(first, instruction);

            Emit(Instruction.Create(OpCodes.Ldc_I4, id));
            Emit(method.IsStatic ? Instruction.Create(OpCodes.Ldnull) : Instruction.Create(OpCodes.Ldarg_0));
            Emit(Instruction.Create(OpCodes.Ldc_I4, method.Parameters.Count));
            Emit(Instruction.Create(OpCodes.Newarr, module.TypeSystem.Object));
            for (int i = 0; i < method.Parameters.Count; i++)
            {
                var parameter = method.Parameters[i];
                Emit(Instruction.Create(OpCodes.Dup));
                Emit(Instruction.Create(OpCodes.Ldc_I4, i));
                Emit(Instruction.Create(OpCodes.Ldarg, parameter));
                if (parameter.ParameterType.IsValueType)
                    Emit(Instruction.Create(OpCodes.Box, parameter.ParameterType));
                Emit(Instruction.Create(OpCodes.Stelem_Ref));
            }
            Emit(Instruction.Create(OpCodes.Call, prefix));
            Emit(Instruction.Create(OpCodes.Brtrue, first));
            Emit(Instruction.Create(OpCodes.Ret));
            // The original body, branch targets, exception handlers and sequence points stay intact.
            body.MaxStackSize = Math.Max(body.MaxStackSize, 6);
            var marker = new CustomAttribute(markerConstructor);
            marker.ConstructorArguments.Add(new CustomAttributeArgument(module.TypeSystem.Int32, id));
            marker.ConstructorArguments.Add(new CustomAttributeArgument(module.TypeSystem.Int32, Version));
            method.CustomAttributes.Add(marker);
            changed++;
        }
        if (changed != 0)
        {
            string temporary = assemblyPath + ".rpcweaver.tmp.dll";
            try
            {
                assembly.Write(temporary, new WriterParameters { WriteSymbols = symbols });
                File.Move(temporary, assemblyPath, overwrite: true);
                if (symbols)
                    File.Move(Path.ChangeExtension(temporary, ".pdb"), Path.ChangeExtension(assemblyPath, ".pdb"), overwrite: true);
            }
            finally
            {
                File.Delete(temporary);
                File.Delete(Path.ChangeExtension(temporary, ".pdb"));
            }
        }
        return changed;
    }

    private static IEnumerable<TypeDefinition> AllTypes(IEnumerable<TypeDefinition> types)
    {
        foreach (var type in types)
        {
            yield return type;
            foreach (var nested in AllTypes(type.NestedTypes))
                yield return nested;
        }
    }

    private static bool HasGenericDeclaringType(TypeDefinition? type) =>
        type != null && (type.HasGenericParameters || HasGenericDeclaringType(type.DeclaringType));

    // Must match CustomRPCManager.GetStableMethodSignature, including nested and generic parameter types.
    internal static string Signature(MethodDefinition method) =>
        $"{StableTypeName(method.DeclaringType)}.{method.Name}({string.Join(",", method.Parameters.Select(p => StableTypeName(p.ParameterType)))})";

    private static string StableTypeName(TypeReference type)
    {
        if (type is ArrayType array)
            return StableTypeName(array.ElementType) + "[" + new string(',', array.Rank - 1) + "]";
        if (type is GenericInstanceType generic)
            return NonGenericTypeName(generic.ElementType) + "<" + string.Join(",", generic.GenericArguments.Select(StableTypeName)) + ">";
        if (type is GenericParameter)
            return type.Name;
        return NonGenericTypeName(type);
    }

    private static string NonGenericTypeName(TypeReference type) => type.IsNested
        ? NonGenericTypeName(type.DeclaringType) + "." + StripArity(type.Name)
        : StripArity(type.FullName.Replace('/', '.'));

    private static string StripArity(string name) => name.Split('`')[0];

    private static int Id(string signature)
    {
        uint hash = 2166136261;
        foreach (byte value in Encoding.UTF8.GetBytes(signature))
            hash = unchecked((hash ^ value) * 16777619);
        return unchecked((int)hash);
    }
}
