using System;
using HarmonyLib;
using SuperNewRoles.Modules.Events.Bases;

namespace SuperNewRoles.Events.PCEvents;

public class DieEventData : IEventData
{
    public PlayerControl player { get; }
    public DieEventData(PlayerControl player)
    {
        this.player = player;
    }
}

public class DieEvent : EventTargetBase<DieEvent, DieEventData>
{
    public static void Invoke(PlayerControl player)
    {
        var data = new DieEventData(player);
        Instance.Awake(data);
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Die))]
public static class DiePatch
{
    public static void Postfix(PlayerControl __instance)
    {
        DieEvent.Invoke(__instance);
    }
}

