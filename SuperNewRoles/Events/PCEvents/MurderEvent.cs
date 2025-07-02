using System;
using System.Collections.Generic;
using HarmonyLib;
using SuperNewRoles.Modules;
using SuperNewRoles.Modules.Events.Bases;
using UnityEngine;

namespace SuperNewRoles.Events.PCEvents;

public class MurderEventData : IEventData
{
    public ExPlayerControl killer { get; }
    public ExPlayerControl target { get; }
    public MurderResultFlags resultFlags { get; }

    public MurderEventData(ExPlayerControl killer, ExPlayerControl target, MurderResultFlags resultFlags)
    {
        this.killer = killer;
        this.target = target;
        this.resultFlags = resultFlags;
    }
}
public class TryKillEventData : IEventData
{
    public ExPlayerControl Killer { get; }
    public ExPlayerControl RefTarget { get; set; }
    public bool RefSuccess { get; set; }

    public TryKillEventData(ExPlayerControl killer, ExPlayerControl target, bool success)
    {
        this.Killer = killer;
        this.RefTarget = target;
        this.RefSuccess = success;
    }
}

public class MurderEvent : EventTargetBase<MurderEvent, MurderEventData>
{
    public static void Invoke(ExPlayerControl killer, ExPlayerControl target, MurderResultFlags resultFlags)
    {
        var data = new MurderEventData(killer, target, resultFlags);
        Instance.Awake(data);
    }
}

public class TryKillEvent : EventTargetBase<TryKillEvent, TryKillEventData>
{
    public static TryKillEventData Invoke(ExPlayerControl killer, ref ExPlayerControl target)
    {
        var data = new TryKillEventData(killer, target, true);
        Instance.Awake(data);
        target = data.RefTarget;
        return data;
    }
}
[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcMurderPlayer))]
public static class TryKillPatch
{
    public static bool Prefix(PlayerControl __instance, PlayerControl target, bool didSucceed)
    {
        if (!didSucceed)
            return true;
        CustomDeathExtensions.RpcCustomDeath(source: __instance, target: target, deathType: CustomDeathType.Kill);
        return false;
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