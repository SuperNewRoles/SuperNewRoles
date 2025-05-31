using System;
using System.Collections.Generic;
using HarmonyLib;
using SuperNewRoles.Modules.Events.Bases;
using UnityEngine;

namespace SuperNewRoles.Events.PCEvents;

public class TaskCompleteEventData : IEventData
{
    public PlayerControl player { get; }
    public uint taskIndex { get; }

    public TaskCompleteEventData(PlayerControl player, uint taskIndex)
    {
        this.player = player;
        this.taskIndex = taskIndex;
    }
}

public class TaskCompleteEvent : EventTargetBase<TaskCompleteEvent, TaskCompleteEventData>
{
    public static void Invoke(PlayerControl player, uint taskIndex)
    {
        var data = new TaskCompleteEventData(player, taskIndex);
        Instance.Awake(data);
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CompleteTask))]
public static class TaskCompletePatch
{
    public static void Postfix(PlayerControl __instance, uint idx)
    {
        TaskCompleteEvent.Invoke(__instance, idx);
    }
}