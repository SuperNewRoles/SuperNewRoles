using System;
using System.Collections.Generic;
using HarmonyLib;
using SuperNewRoles.Mode;
using SuperNewRoles.Modules;
using SuperNewRoles.Modules.Events.Bases;
using UnityEngine;

namespace SuperNewRoles.Events.PCEvents;

public class ReportDeadBodyHostEventData : IEventData
{
    public PlayerControl reporter { get; }
    public NetworkedPlayerInfo target { get; }
    public ReportDeadBodyHostEventData(PlayerControl reporter, NetworkedPlayerInfo target)
    {
        this.reporter = reporter;
        this.target = target;
    }
}

public class ReportDeadBodyHostEvent : EventTargetBase<ReportDeadBodyHostEvent, ReportDeadBodyHostEventData>
{
    public static bool Invoke(PlayerControl reporter, NetworkedPlayerInfo target)
    {
        var data = new ReportDeadBodyHostEventData(reporter, target);
        Instance.Awake(data);
        return true;
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.ReportDeadBody))]
public static class ReportDeadBodyHostPatch
{
    public static bool Prefix(PlayerControl __instance, NetworkedPlayerInfo target)
    {
        if (ModeManager.IsMode(ModeId.BattleRoyal))
            return false;
        if (AmongUsClient.Instance.AmHost && target != null && Roles.Crewmate.PsychometristReportBlock.IsBlocked(target.PlayerId))
            return false;
        if (AmongUsClient.Instance.AmHost)
            return ReportDeadBodyHostEvent.Invoke(__instance, target);
        return true;
    }
}
