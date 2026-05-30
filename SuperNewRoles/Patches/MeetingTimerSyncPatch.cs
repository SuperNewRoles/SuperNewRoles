using HarmonyLib;
using SuperNewRoles.Events;
using SuperNewRoles.Modules;
using UnityEngine;

namespace SuperNewRoles.Patches;

[HarmonyPatch]
public static class MeetingTimerSyncPatch
{
    // タイマーの同期を行う間隔。
    private const float SyncInterval = 1f;
    // これくらいのズレは許容する。これ以上のズレがある場合にのみ、ホストのタイマーに合わせる。
    private const float SnapThreshold = 0.35f;

    private static float syncTimer;

    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
    [HarmonyPostfix]
    public static void MeetingHudStartPostfix()
    {
        syncTimer = 0f;
    }

    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Close))]
    [HarmonyPostfix]
    public static void MeetingHudClosePostfix()
    {
        syncTimer = 0f;
    }

    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Update))]
    [HarmonyPostfix]
    public static void MeetingHudUpdatePostfix(MeetingHud __instance)
    {
        if (!AmongUsClient.Instance.AmHost || PlayerControl.LocalPlayer == null || !CanSendState(__instance.state))
            return;

        syncTimer -= Time.deltaTime;
        if (syncTimer > 0f)
            return;

        syncTimer = SyncInterval;
        RpcSyncMeetingTimer(MeetingStartEvent.MeetingCount, __instance.state, __instance.discussionTimer);
    }

    [CustomRPC]
    public static void RpcSyncMeetingTimer(int meetingCount, MeetingHud.VoteStates hostState, float hostDiscussionTimer)
    {
        if (AmongUsClient.Instance.AmHost)
            return;

        MeetingHud meetingHud = MeetingHud.Instance;
        if (meetingHud == null || meetingCount != MeetingStartEvent.MeetingCount || !CanApplyState(meetingHud.state, hostState))
        {
            Logger.Error("Received invalid meeting timer sync RPC");
            Logger.Error($"MeetingCount: {meetingCount}, HostState: {hostState}, LocalState: {meetingHud?.state}");
            return;
        }

        if (Mathf.Abs(meetingHud.discussionTimer - hostDiscussionTimer) < SnapThreshold)
            return;

        meetingHud.discussionTimer = hostDiscussionTimer;
    }

    private static bool CanSendState(MeetingHud.VoteStates state)
    {
        return IsVotingPhase(state) || state == MeetingHud.VoteStates.Results;
    }

    private static bool CanApplyState(MeetingHud.VoteStates localState, MeetingHud.VoteStates hostState)
    {
        if (!CanSendState(hostState))
            return false;

        if (localState == MeetingHud.VoteStates.Animating)
            return IsVotingPhase(hostState);

        if (hostState == MeetingHud.VoteStates.Results)
            return localState == MeetingHud.VoteStates.Results;

        return IsVotingPhase(localState);
    }

    private static bool IsVotingPhase(MeetingHud.VoteStates state)
    {
        return state is MeetingHud.VoteStates.Discussion
            or MeetingHud.VoteStates.NotVoted
            or MeetingHud.VoteStates.Voted;
    }
}
