using System.Linq;
using System.Reflection;
using SuperNewRoles.Modules;
using Xunit;
using Xunit.Abstractions;

namespace SuperNewRoles.Tests;

public class StartupRpcDiscoveryTests(ITestOutputHelper output)
{
    [Fact]
    public void DiscoveryPreservesEveryExistingRpcAndItsFlagsWithoutRepeatedPatches()
    {
        var assembly = typeof(CustomRPCManager).Assembly;
        var oldScan = assembly.GetTypes().SelectMany(type => type.GetMethods(
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)).ToArray();
        var oldMethods = oldScan.Where(method => method.GetCustomAttribute<CustomRPCAttribute>() != null).ToArray();
        var newMethods = CustomRPCManager.DiscoverRpcMethods(assembly).ToArray();
        static string Identity(MethodInfo method) =>
            $"{method.DeclaringType}:{method.MetadataToken}:{CustomRPCManager.GetDeterministicRpcId(method)}:{method.GetCustomAttribute<CustomRPCAttribute>()!.OnlyOtherPlayer}";

        Assert.Equal(oldMethods.Select(Identity).Distinct().OrderBy(x => x), newMethods.Select(Identity).OrderBy(x => x));
        output.WriteLine($"Old method inspections: {oldScan.Length}; declared methods: {assembly.GetTypes().Sum(type => type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly).Length)}; old RPC registrations: {oldMethods.Length}; new: {newMethods.Length}");
    }
}
