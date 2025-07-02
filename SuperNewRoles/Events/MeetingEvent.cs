using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using SuperNewRoles.Modules;
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

public class MeetingHudCalculateVotesOnPlayerOnlyHostEventData : IEventData
{
    public ExPlayerControl Source { get; }
    public byte SourceId { get; }
    public byte Target { get; set; }
    public int VoteCount { get; set; }

    public MeetingHudCalculateVotesOnPlayerOnlyHostEventData(ExPlayerControl source, byte sourceId, byte target, int voteCount)
    {
        Source = source;
        SourceId = sourceId;
        Target = target;
        VoteCount = voteCount;
    }
}

public class MeetingHudCalculateVotesOnPlayerOnlyHostEvent : EventTargetBase<MeetingHudCalculateVotesOnPlayerOnlyHostEvent, MeetingHudCalculateVotesOnPlayerOnlyHostEventData>
{
    public static MeetingHudCalculateVotesOnPlayerOnlyHostEventData Invoke(ExPlayerControl source, byte sourceId, byte target, int voteCount)
    {
        var data = new MeetingHudCalculateVotesOnPlayerOnlyHostEventData(source, sourceId, target, voteCount);
        Instance.Awake(data);
        return data;
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
[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.CheckForEndVoting))]
public static class MeetingHudCheckForEndVotingPatch
{
    public static bool Prefix(MeetingHud __instance)
    {
        if (__instance.playerStates.All((PlayerVoteArea ps) => ps.AmDead || ps.DidVote))
        {
            (Dictionary<byte, int> self, Dictionary<byte, int> additionalOnly) = CalculateVotesCustom(__instance);
            bool tie;
            KeyValuePair<byte, int> max = self.MaxPair(out tie);
            NetworkedPlayerInfo exiled = GameData.Instance.AllPlayers.ToSystemList().FirstOrDefault((NetworkedPlayerInfo v) => !tie && v.PlayerId == max.Key);
            List<MeetingHud.VoterState> array = new();
            for (int i = 0; i < __instance.playerStates.Length; i++)
            {
                PlayerVoteArea playerVoteArea = __instance.playerStates[i];
                MeetingHud.VoterState voterState = default;
                voterState.VoterId = playerVoteArea.TargetPlayerId;
                voterState.VotedForId = playerVoteArea.VotedFor;
                array.Add(voterState);
            }
            foreach (var addOnly in additionalOnly)
            {
                var exPlayer = Modules.ExPlayerControl.ById(addOnly.Key);
                if (exPlayer != null)
                {
                    int count = addOnly.Value;
                    // もう既に上で追加されてるから追加分だけ
                    while (count > 1)
                    {
                        MeetingHud.VoterState voterState = default;
                        voterState.VoterId = addOnly.Key;
                        voterState.VotedForId = __instance.playerStates[addOnly.Key].VotedFor;
                        array.Add(voterState);
                        count--;
                    }
                }
            }
            __instance.RpcVotingComplete(array.ToArray(), exiled, tie);
        }
        return false;
    }
    public static (Dictionary<byte, int> self, Dictionary<byte, int> additionalOnly) CalculateVotesCustom(MeetingHud __instance)
    {
        Dictionary<byte, int> dictionary = new();
        Dictionary<byte, int> additionalOnly = new();
        for (int i = 0; i < __instance.playerStates.Length; i++)
        {
            PlayerVoteArea playerVoteArea = __instance.playerStates[i];
            MeetingHudCalculateVotesOnPlayerOnlyHostEventData data = MeetingHudCalculateVotesOnPlayerOnlyHostEvent.Invoke(ExPlayerControl.ById(playerVoteArea.TargetPlayerId), playerVoteArea.TargetPlayerId, playerVoteArea.VotedFor, 1);
            playerVoteArea.VotedFor = data.Target;
            Logger.Info($"playerVoteArea.VotedFor: {playerVoteArea.VotedFor} data.VoteCount: {data.VoteCount} data.SourceId: {data.SourceId} data.Target: {data.Target}");
            if (data.VoteCount <= 0)
                playerVoteArea.VotedFor = byte.MaxValue;
            if (playerVoteArea.VotedFor != 252 && playerVoteArea.VotedFor != byte.MaxValue && playerVoteArea.VotedFor != 254)
            {
                if (dictionary.TryGetValue(playerVoteArea.VotedFor, out var value))
                    dictionary[playerVoteArea.VotedFor] = value + data.VoteCount;
                else
                    dictionary[playerVoteArea.VotedFor] = data.VoteCount;
                if (data.VoteCount > 1)
                    additionalOnly[playerVoteArea.TargetPlayerId] = data.VoteCount;
            }
        }
        return (dictionary, additionalOnly);
    }
}