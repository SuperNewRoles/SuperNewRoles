using System;
using System.Collections.Generic;
using HarmonyLib;
using SuperNewRoles.Modules.Events.Bases;
using UnityEngine;

namespace SuperNewRoles.Events;

public class MeetingStartEventData : IEventData
{
    public PlayerControl reporter { get; }
    public NetworkedPlayerInfo target { get; }
    public bool isEmergencyMeeting { get; }

    public MeetingStartEventData(PlayerControl reporter, NetworkedPlayerInfo target, bool isEmergencyMeeting)
    {
        this.reporter = reporter;
        this.target = target;
        this.isEmergencyMeeting = isEmergencyMeeting;
    }
}


public class MeetingStartEvent : EventTargetBase<MeetingStartEvent, MeetingStartEventData>
{
    public static void Invoke(PlayerControl reporter, NetworkedPlayerInfo target, bool isEmergencyMeeting)
    {
        var data = new MeetingStartEventData(reporter, target, isEmergencyMeeting);
        Instance.Awake(data);
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
        MeetingStartEvent.Invoke(__instance, target, false);
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