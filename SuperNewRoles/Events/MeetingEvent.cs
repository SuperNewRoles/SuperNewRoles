using System;
using System.Collections.Generic;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using SuperNewRoles.Modules.Events.Bases;
using UnityEngine;

namespace SuperNewRoles.Events;

public class CalledMeetingEventData : IEventData
{
    public PlayerControl reporter { get; }
    public NetworkedPlayerInfo target { get; }
    public bool isEmergencyMeeting { get; }

    public CalledMeetingEventData(PlayerControl reporter, NetworkedPlayerInfo target, bool isEmergencyMeeting)
    {
        this.reporter = reporter;
        this.target = target;
        this.isEmergencyMeeting = isEmergencyMeeting;
    }
}

public class CalledMeetingEvent : EventTargetBase<CalledMeetingEvent, CalledMeetingEventData>
{
    public static void Invoke(PlayerControl reporter, NetworkedPlayerInfo target, bool isEmergencyMeeting)
    {
        var data = new CalledMeetingEventData(reporter, target, isEmergencyMeeting);
        Instance.Awake(data);
    }
}

public class MeetingStartEventData : IEventData
{
    public int meetingCount { get; }

    public MeetingStartEventData(int meetingCount)
    {
        this.meetingCount = meetingCount;
    }
}

public class MeetingStartEvent : EventTargetBase<MeetingStartEvent, MeetingStartEventData>
{
    public static int MeetingCount = -1;

    public static void Invoke()
    {
        MeetingCount++;
        var data = new MeetingStartEventData(MeetingCount);
        Instance.Awake(data);
    }
}

public class MeetingCloseEventData : IEventData
{
    public int meetingCount { get; }

    public MeetingCloseEventData(int meetingCount)
    {
        this.meetingCount = meetingCount;
    }
}

public class MeetingCloseEvent : EventTargetBase<MeetingCloseEvent, MeetingCloseEventData>
{
    public static void Invoke()
    {
        var data = new MeetingCloseEventData(MeetingStartEvent.MeetingCount);
        Instance.Awake(data);
    }
}

public class MeetingUpdateEvent : EventTargetBase<MeetingUpdateEvent>
{
    public static void Invoke()
    {
        Instance.Awake();
    }
}

public class VotingCompleteEventData : IEventData
{
    public Il2CppStructArray<MeetingHud.VoterState> States { get; }
    public NetworkedPlayerInfo Exiled { get; }
    public bool IsTie { get; }

    public VotingCompleteEventData(Il2CppStructArray<MeetingHud.VoterState> states, NetworkedPlayerInfo exiled, bool isTie)
    {
        States = states;
        Exiled = exiled;
        IsTie = isTie;
    }
}

public class VotingCompleteEvent : EventTargetBase<VotingCompleteEvent, VotingCompleteEventData>
{
    public static void Invoke(Il2CppStructArray<MeetingHud.VoterState> states, NetworkedPlayerInfo exiled, bool tie)
    {
        var data = new VotingCompleteEventData(states, exiled, tie);
        Instance.Awake(data);
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.StartMeeting))]
public static class CalledMeetingPatch
{
    public static void Postfix(PlayerControl __instance, NetworkedPlayerInfo target)
    {
        CalledMeetingEvent.Invoke(__instance, target, false);
    }
}

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
public static class MeetingStartPatch
{
    public static void Postfix(MeetingHud __instance)
    {
        // 全てのExPlayerControlにPlayerVoteAreaを設定
        foreach (var playerState in __instance.playerStates)
        {
            var exPlayer = Modules.ExPlayerControl.ById(playerState.TargetPlayerId);
            if (exPlayer != null)
            {
                exPlayer.VoteArea = playerState;
            }
        }
        MeetingStartEvent.Invoke();
    }
}
[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Close))]
public static class MeetingClosePatch
{
    public static void Postfix()
    {
        MeetingCloseEvent.Invoke();
    }
}

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Update))]
public static class MeetingUpdatePatch
{
    public static void Postfix()
    {
        MeetingUpdateEvent.Invoke();
    }
}

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.VotingComplete))]
public static class VotingCompletePatch
{
    public static void Postfix(Il2CppStructArray<MeetingHud.VoterState> states, ref NetworkedPlayerInfo exiled, bool tie)
    {
        VotingCompleteEvent.Invoke(states, exiled, tie);
    }
}
[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.OnDestroy))]
public static class MeetingHudOnDestroyPatch
{
    public static void Postfix()
    {
        // 全てのExPlayerControlのVoteAreaをnullに設定
        foreach (var exPlayer in Modules.ExPlayerControl.ExPlayerControls)
        {
            if (exPlayer != null)
            {
                exPlayer.VoteArea = null;
            }
        }
    }
}
[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.CoStartGame))]
public static class CoStartGamePatch
{
    public static void Postfix()
    {
        MeetingStartEvent.MeetingCount = -1;
    }
}