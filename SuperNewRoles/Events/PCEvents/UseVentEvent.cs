using HarmonyLib;
using SuperNewRoles.Modules.Events.Bases;

namespace SuperNewRoles.Events.PCEvents;

public class UseVentEventData : IEventData
{
    public PlayerControl user { get; }
    public int ventId { get; }
    public UseVentEventData(PlayerControl user, int ventId)
    {
        this.user = user;
        this.ventId = ventId;
    }
}

public class UseVentEvent : EventTargetBase<UseVentEvent, UseVentEventData>
{
    public static void Invoke(PlayerControl user, int ventId)
    {
        var data = new UseVentEventData(user, ventId);
        Instance.Awake(data);
    }
}
public class EnterVentEvent : EventTargetBase<EnterVentEvent, UseVentEventData>
{
    public static void Invoke(PlayerControl user, int ventId)
    {
        var data = new UseVentEventData(user, ventId);
        Instance.Awake(data);
    }
}
public class ExitVentEvent : EventTargetBase<ExitVentEvent, UseVentEventData>
{
    public static void Invoke(PlayerControl user, int ventId)
    {
        var data = new UseVentEventData(user, ventId);
        Instance.Awake(data);
    }
}

[HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.CoEnterVent))]
public static class UseVentPatch
{
    public static void Postfix(PlayerPhysics __instance, int id)
    {
        UseVentEvent.Invoke(__instance.myPlayer, id);
        EnterVentEvent.Invoke(__instance.myPlayer, id);
    }
}
[HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.CoExitVent))]
public static class ExitVentPatch
{
    public static void Postfix(PlayerPhysics __instance, int id)
    {
        UseVentEvent.Invoke(__instance.myPlayer, id);
        ExitVentEvent.Invoke(__instance.myPlayer, id);
    }
}