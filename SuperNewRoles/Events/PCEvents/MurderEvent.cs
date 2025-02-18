using System;
using System.Collections.Generic;
using HarmonyLib;
using SuperNewRoles.Modules.Events.Bases;
using UnityEngine;

namespace SuperNewRoles.Events.PCEvents;

public class MurderEventData : IEventData
{
    public PlayerControl killer { get; }
    public PlayerControl target { get; }
    public MurderResultFlags resultFlags { get; }

    public MurderEventData(PlayerControl killer, PlayerControl target, MurderResultFlags resultFlags)
    {
        this.killer = killer;
        this.target = target;
        this.resultFlags = resultFlags;
    }
}

public class MurderEvent : EventTargetBase<MurderEvent, MurderEventData>
{
    public static void Invoke(PlayerControl killer, PlayerControl target, MurderResultFlags resultFlags)
    {
        var data = new MurderEventData(killer, target, resultFlags);
        Instance.Awake(data);
    }
}
[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
public static class MurderPatch
{
    public static void Postfix(PlayerControl __instance, PlayerControl target, MurderResultFlags resultFlags)
    {
        MurderEvent.Invoke(__instance, target, resultFlags);
    }
}