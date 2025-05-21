using HarmonyLib;
using SuperNewRoles.Modules.Events.Bases;

namespace SuperNewRoles.Events;

public class VentEventData : IEventData
{
    public Vent vent { get; }
    public VentEventData(Vent vent)
    {
        this.vent = vent;
    }
}

public class PlayerPhysicsRpcEnterVentPrefixEventData : IEventData
{
    public int ventId { get; }
    public bool result { get; set; }
    public PlayerPhysicsRpcEnterVentPrefixEventData(int ventId)
    {
        this.ventId = ventId;
        result = true;
    }
}

public class PlayerPhysicsRpcEnterVentPrefixEvent : EventTargetBase<PlayerPhysicsRpcEnterVentPrefixEvent, PlayerPhysicsRpcEnterVentPrefixEventData>
{
    public static void Invoke(int ventId, out bool result)
    {
        var data = new PlayerPhysicsRpcEnterVentPrefixEventData(ventId);
        Instance.Awake(data);
        result = data.result;
    }
}

[HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.RpcEnterVent))]
public static class VentUsePatch
{
    public static bool Prefix(int ventId)
    {
        PlayerPhysicsRpcEnterVentPrefixEvent.Invoke(ventId, out bool result);
        return result;
    }
}