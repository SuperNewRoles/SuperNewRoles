using System.Reflection;
using System.Runtime.Loader;
using Mono.Cecil;
using SuperNewRoles.RpcWeaver;
using Xunit;

public sealed class WeavingTests
{
    [Fact]
    public void WovenBodiesPreserveArgumentsInstancesNestedCallsAndExceptionHandlers()
    {
        using var fixture = new Fixture();
        var type = fixture.Type;
        var calls = new List<(int id, object? instance, object[] args)>();
        int receivedId = 0;
        int otherId = fixture.Id("InstanceRpc");
        fixture.SetEntry((id, instance, args) =>
        {
            if (receivedId == id) { receivedId = 0; return true; }
            calls.Add((id, instance, args));
            return id != otherId;
        });

        fixture.Invoke("StaticRpc", null, (byte)2, 2.5f);
        Assert.Equal(1, fixture.Count("Runs"));
        Assert.Equal(new object[] { (byte)2, 2.5f }, calls.Single().args);
        Assert.Null(calls.Single().instance);

        var instance = Activator.CreateInstance(type)!;
        fixture.Invoke("InstanceRpc", instance, (byte)2, 4.5f);
        Assert.Same(instance, calls.Last().instance);
        Assert.Equal(0f, type.GetField("Charge")!.GetValue(instance));
        receivedId = otherId;
        fixture.Invoke("InstanceRpc", instance, (byte)2, 4.5f);
        Assert.Equal(4.5f, type.GetField("Charge")!.GetValue(instance));
        Assert.Equal(2, calls.Count); // Receive did not send again.

        receivedId = fixture.Id("NestedRpc");
        fixture.Invoke("NestedRpc", null);
        Assert.Equal(fixture.Id("StaticRpc"), calls.Last().id); // A nested, different RPC still sends.
        Assert.Equal(3, calls.Count);
        var exception = Assert.Throws<TargetInvocationException>(() => fixture.Invoke("ExceptionalRpc", null, true));
        Assert.IsType<InvalidOperationException>(exception.InnerException);
        Assert.Equal(1, fixture.Count("FinallyRuns"));
        fixture.Invoke("ExceptionalRpc", null, false);
        Assert.Equal(2, fixture.Count("FinallyRuns"));
        fixture.Invoke("CollectionRpc", null, new Dictionary<byte, int[]> { [1] = new[] { 2, 3 } });
        Assert.Equal(6, fixture.Count("Runs"));
    }

    [Fact]
    public void ReweavingIsByteForByteIdempotentAndKeepsSymbols()
    {
        using var fixture = new Fixture();
        var bytes = File.ReadAllBytes(fixture.Path);
        var symbols = File.ReadAllBytes(System.IO.Path.ChangeExtension(fixture.Path, ".pdb"));
        Assert.Equal(0, RpcAssemblyWeaver.Weave(fixture.Path));
        Assert.Equal(bytes, File.ReadAllBytes(fixture.Path));
        Assert.Equal(symbols, File.ReadAllBytes(System.IO.Path.ChangeExtension(fixture.Path, ".pdb")));
    }

    [Fact]
    public void UnsupportedSignatureFailsWithoutChangingInput()
    {
        using var fixture = new Fixture(weave: false);
        using (var assembly = AssemblyDefinition.ReadAssembly(fixture.Path, new ReaderParameters { InMemory = true }))
        {
            var method = assembly.MainModule.Types.Single(t => t.Name == "RpcFixture").Methods.Single(m => m.Name == "StaticRpc");
            method.Parameters[0].ParameterType = new ByReferenceType(assembly.MainModule.TypeSystem.Byte);
            assembly.Write(fixture.Path);
        }
        var bytes = File.ReadAllBytes(fixture.Path);
        // Remove the old symbols because the deliberate test mutation rewrote the assembly without a PDB.
        File.Delete(System.IO.Path.ChangeExtension(fixture.Path, ".pdb"));
        Assert.Throws<NotSupportedException>(() => RpcAssemblyWeaver.Weave(fixture.Path));
        Assert.Equal(bytes, File.ReadAllBytes(fixture.Path));
    }

    [Fact]
    public void NonAbilityInstanceRpcFailsBeforeChangingAssembly()
    {
        using var fixture = new Fixture(weave: false);
        using (var assembly = AssemblyDefinition.ReadAssembly(fixture.Path, new ReaderParameters { InMemory = true }))
        {
            assembly.MainModule.Types.Single(t => t.Name == "RpcFixture").BaseType = assembly.MainModule.TypeSystem.Object;
            assembly.Write(fixture.Path);
        }
        File.Delete(System.IO.Path.ChangeExtension(fixture.Path, ".pdb"));
        var bytes = File.ReadAllBytes(fixture.Path);
        var error = Assert.Throws<NotSupportedException>(() => RpcAssemblyWeaver.Weave(fixture.Path));
        Assert.Contains("AbilityBase", error.Message);
        Assert.Equal(bytes, File.ReadAllBytes(fixture.Path));
    }

    [Fact]
    public void MissingHarmonyMetadataTypeReturnsFalse()
    {
        var method = typeof(HarmonyRegistryWeaver).GetMethod("HasHarmonyMetadata", BindingFlags.NonPublic | BindingFlags.Static)!;
        Assert.Equal(false, method.Invoke(null, new object?[] { null }));
    }

    [Fact]
    public void MissingHarmonyConstructorFallsBack()
    {
        using var module = ModuleDefinition.CreateModule("missing-constructor", ModuleKind.Dll);
        var type = new TypeDefinition("HarmonyLib", "HarmonyPatch", Mono.Cecil.TypeAttributes.Public, module.TypeSystem.Object);
        module.Types.Add(type);
        var constructor = new MethodReference(".ctor", module.TypeSystem.Void, type) { HasThis = true };
        constructor.Parameters.Add(new ParameterDefinition(module.TypeSystem.String));
        var attribute = new CustomAttribute(constructor);
        attribute.ConstructorArguments.Add(new CustomAttributeArgument(module.TypeSystem.String, "Target"));
        var merge = typeof(HarmonyRegistryWeaver).GetMethod("Merge", BindingFlags.NonPublic | BindingFlags.Static)!;
        Assert.Equal(false, merge.Invoke(null, new object[] { new[] { attribute }, new Dictionary<string, CustomAttributeArgument>() }));
    }

    [Fact]
    public void MissingExternalBaseReportsDependencyWithoutChangingDllOrSymbols()
    {
        using var fixture = new Fixture(weave: false);
        using (var assembly = AssemblyDefinition.ReadAssembly(fixture.Path,
                   new ReaderParameters { InMemory = true, ReadSymbols = true }))
        {
            var module = assembly.MainModule;
            var dependency = new AssemblyNameReference("MissingRpcBaseDependency", new Version(1, 0, 0, 0));
            module.AssemblyReferences.Add(dependency);
            module.Types.Single(t => t.Name == "RpcFixture").BaseType =
                new TypeReference("External", "RpcBase", module, dependency);
            assembly.Write(fixture.Path, new WriterParameters { WriteSymbols = true });
        }
        var dll = File.ReadAllBytes(fixture.Path);
        var pdbPath = System.IO.Path.ChangeExtension(fixture.Path, ".pdb");
        var pdb = File.ReadAllBytes(pdbPath);
        var error = Assert.Throws<InvalidOperationException>(() => RpcAssemblyWeaver.Weave(fixture.Path));
        Assert.Contains("MissingRpcBaseDependency", error.Message);
        Assert.Contains("RpcFixture", error.Message);
        Assert.IsType<AssemblyResolutionException>(error.InnerException);
        Assert.Equal(dll, File.ReadAllBytes(fixture.Path));
        Assert.Equal(pdb, File.ReadAllBytes(pdbPath));
    }

    private sealed class Fixture : IDisposable
    {
        public string Path { get; }
        private readonly string directory;
        private readonly AssemblyLoadContext context = new("weaver-fixture", isCollectible: true);
        private Assembly? assembly;
        public Type Type => assembly!.GetType("SuperNewRoles.Modules.RpcFixture")!;
        public Fixture(bool weave = true)
        {
            directory = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "snr-weaver-tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            string source = typeof(SuperNewRoles.Modules.RpcFixture).Assembly.Location;
            Path = System.IO.Path.Combine(directory, System.IO.Path.GetFileName(source));
            File.Copy(source, Path);
            File.Copy(System.IO.Path.ChangeExtension(source, ".pdb"), System.IO.Path.ChangeExtension(Path, ".pdb"));
            if (weave)
            {
                Assert.Equal(5, RpcAssemblyWeaver.Weave(Path));
                using var stream = File.OpenRead(Path);
                assembly = context.LoadFromStream(stream);
            }
        }
        public void SetEntry(Func<int, object?, object[], bool> entry) =>
            assembly!.GetType("SuperNewRoles.Modules.CustomRPCManager")!.GetField("Entry")!.SetValue(null, entry);
        public int Id(string method) => (int)Type.GetMethod(method)!.GetCustomAttributesData()
            .Single(a => a.AttributeType.Name == "WovenRpcAttribute").ConstructorArguments[0].Value!;
        public int Count(string field) => (int)Type.GetField(field)!.GetValue(null)!;
        public void Invoke(string method, object? instance, params object[] args) => Type.GetMethod(method)!.Invoke(instance, args);
        public void Dispose()
        {
            assembly = null;
            context.Unload();
            Directory.Delete(directory, recursive: true);
        }
    }
}
