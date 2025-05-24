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
public class MurderPrefixEventData : IEventData
{
    public ExPlayerControl Killer { get; }
    public ExPlayerControl RefTarget { get; set; }
    public MurderResultFlags RefResultFlags { get; set; }
    public bool RefSuccess { get; set; }

    public MurderPrefixEventData(ExPlayerControl killer, ExPlayerControl target, MurderResultFlags resultFlags, bool success)
    {
        this.Killer = killer;
        this.RefTarget = target;
        this.RefResultFlags = resultFlags;
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

public class MurderPrefixEvent : EventTargetBase<MurderPrefixEvent, MurderPrefixEventData>
{
    public static MurderPrefixEventData Invoke(ExPlayerControl killer, ExPlayerControl target, MurderResultFlags resultFlags)
    {
        var data = new MurderPrefixEventData(killer, target, resultFlags, true);
        Instance.Awake(data);
        return data;
    }
}
[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
public static class MurderPatch
{
    public static bool Prefix(PlayerControl __instance, ref PlayerControl target, ref MurderResultFlags resultFlags)
    {
        var data = MurderPrefixEvent.Invoke(__instance, target, resultFlags);
        target = data.RefTarget;
        resultFlags = data.RefResultFlags;
        return data.RefSuccess;
    }
    public static void Postfix(PlayerControl __instance, PlayerControl target, MurderResultFlags resultFlags)
    {
        MurderEvent.Invoke(__instance, target, resultFlags);
    }
}