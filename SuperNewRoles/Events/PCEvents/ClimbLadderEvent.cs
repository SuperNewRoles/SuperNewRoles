using System;
using HarmonyLib;
using SuperNewRoles.Modules;
using SuperNewRoles.Modules.Events.Bases;
using UnityEngine;

namespace SuperNewRoles.Events.PCEvents;

public class ClimbLadderEventData : IEventData
{
    public ExPlayerControl player { get; }
    public Ladder source { get; }
    public byte climbLadderSid { get; }

    public ClimbLadderEventData(ExPlayerControl player, Ladder source, byte climbLadderSid)
    {
        this.player = player;
        this.source = source;
        this.climbLadderSid = climbLadderSid;
    }
}

public class ClimbLadderEvent : EventTargetBase<ClimbLadderEvent, ClimbLadderEventData>
{
    public static void Invoke(ExPlayerControl player, Ladder source, byte climbLadderSid)
    {
        var data = new ClimbLadderEventData(player, source, climbLadderSid);
        Instance.Awake(data);
    }
}

[HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.ClimbLadder))]
public static class ClimbLadderPatch
{
    public static void Postfix(PlayerPhysics __instance, Ladder source, byte climbLadderSid)
    {
        ClimbLadderEvent.Invoke(__instance.myPlayer, source, climbLadderSid);
    }
}