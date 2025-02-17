using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace SuperNewRoles.Events;

public class MeetingEventData
{
    public PlayerControl reporter { get; }
    public NetworkedPlayerInfo target { get; }
    public bool isEmergencyMeeting { get; }

    public MeetingEventData(PlayerControl reporter, NetworkedPlayerInfo target, bool isEmergencyMeeting)
    {
        this.reporter = reporter;
        this.target = target;
        this.isEmergencyMeeting = isEmergencyMeeting;
    }
}

public delegate void MeetingStartEventListener(MeetingEventData data);
public delegate void MeetingCloseEventListener();

public static class MeetingEvent
{
    private static readonly List<MeetingStartEventListener> meetingStartListeners = new();
    private static readonly List<MeetingCloseEventListener> meetingCloseListeners = new();

    public static MeetingStartEventListener AddMeetingStartListener(MeetingStartEventListener listener)
    {
        meetingStartListeners.Add(listener);
        return listener;
    }

    public static MeetingCloseEventListener AddMeetingCloseListener(MeetingCloseEventListener listener)
    {
        meetingCloseListeners.Add(listener);
        return listener;
    }

    public static void RemoveStartListener(MeetingStartEventListener listener)
    {
        meetingStartListeners.Remove(listener);
    }

    public static void RemoveCloseListener(MeetingCloseEventListener listener)
    {
        meetingCloseListeners.Remove(listener);
    }

    public static void InvokeMeetingStart(PlayerControl reporter, NetworkedPlayerInfo target, bool isEmergencyMeeting)
    {
        var data = new MeetingEventData(reporter, target, isEmergencyMeeting);
        foreach (var listener in meetingStartListeners.ToArray())
        {
            try
            {
                listener.Invoke(data);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error in meeting start event listener: {e}");
            }
        }
    }

    public static void InvokeMeetingClose()
    {
        foreach (var listener in meetingCloseListeners.ToArray())
        {
            try
            {
                listener.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError($"Error in meeting close event listener: {e}");
            }
        }
    }
}

[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.StartMeeting))]
public static class MeetingStartPatch
{
    public static void Postfix(ShipStatus __instance, PlayerControl reporter, NetworkedPlayerInfo target)
    {
        MeetingEvent.InvokeMeetingStart(reporter, target, false);
    }
}

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Close))]
public static class MeetingClosePatch
{
    public static void Postfix()
    {
        MeetingEvent.InvokeMeetingClose();
    }
}