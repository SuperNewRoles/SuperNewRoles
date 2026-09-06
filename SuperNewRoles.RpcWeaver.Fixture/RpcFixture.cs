namespace SuperNewRoles.Modules;

[AttributeUsage(AttributeTargets.Method)]
public sealed class CustomRPCAttribute(bool onlyOtherPlayer = false) : Attribute
{
    public bool OnlyOtherPlayer { get; } = onlyOtherPlayer;
}

[AttributeUsage(AttributeTargets.Method)]
public sealed class WovenRpcAttribute(int id, int version) : Attribute
{
    public int Id { get; } = id;
    public int Version { get; } = version;
}

// No Unity or network dependency: tests observe what the inserted entry passes to the runtime.
public static class CustomRPCManager
{
    public static Func<int, object?, object[], bool> Entry = (_, _, _) => true;
    public static bool ShouldExecuteWovenRpc(int id, object? instance, object[] args) => Entry(id, instance, args);
}

public class RpcFixture : SuperNewRoles.Roles.Ability.AbilityBase
{
    public static int Runs;
    public static int FinallyRuns;
    public float Charge;

    [CustomRPC]
    public static void StaticRpc(byte player, float charge) => Runs++;

    [CustomRPC(true)]
    public void InstanceRpc(byte player, float charge) { Charge = charge; Runs++; }

    [CustomRPC]
    public static void NestedRpc() { StaticRpc(3, 1.5f); Runs++; }

    [CustomRPC]
    public static void ExceptionalRpc(bool fail)
    {
        try { if (fail) throw new InvalidOperationException("fixture"); Runs++; }
        finally { FinallyRuns++; }
    }

    [CustomRPC]
    public static void CollectionRpc(Dictionary<byte, int[]> values) => Runs += values.Count;
}
