using System;
using System.Collections.Generic;
using HarmonyLib;
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
public class MeetingStartEvent : EventTargetBase<MeetingStartEvent>
{
    public static void Invoke()
    {
        Instance.Awake();
    }
}
public class MeetingCloseEvent : EventTargetBase<MeetingCloseEvent>
{
    public static void Invoke()
    {
        Instance.Awake();
    }
}

public class MeetingUpdateEvent : EventTargetBase<MeetingUpdateEvent>
{
    public static void Invoke()
    {
        Instance.Awake();
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.StartMeeting))]
public static class MeetingStartPatch
{
    public static void Postfix(PlayerControl __instance, NetworkedPlayerInfo target)
    {
        CalledMeetingEvent.Invoke(__instance, target, false);
    }
}

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
public static class MeetingStartPatch2
{
    public static void Postfix()
    {
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