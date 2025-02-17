using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace SuperNewRoles.Events;

public class TaskCompleteEventData
{
    public PlayerControl player { get; }
    public int taskIndex { get; }

    public TaskCompleteEventData(PlayerControl player, int taskIndex)
    {
        this.player = player;
        this.taskIndex = taskIndex;
    }
}

public delegate void TaskCompleteEventListener(TaskCompleteEventData data);

public static class TaskCompleteEvent
{
    private static readonly List<TaskCompleteEventListener> taskCompleteListeners = new();

    public static TaskCompleteEventListener AddTaskCompleteListener(TaskCompleteEventListener listener)
    {
        taskCompleteListeners.Add(listener);
        return listener;
    }

    public static void RemoveTaskCompleteListener(TaskCompleteEventListener listener)
    {
        taskCompleteListeners.Remove(listener);
    }

    public static void InvokeTaskComplete(PlayerControl player, int taskIndex)
    {
        var data = new TaskCompleteEventData(player, taskIndex);
        foreach (var listener in taskCompleteListeners.ToArray())
        {
            try
            {
                listener.Invoke(data);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error in task complete event listener: {e}");
            }
        }
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CompleteTask))]
public static class TaskCompletePatch
{
    public static void Postfix(PlayerControl __instance, int idx)
    {
        TaskCompleteEvent.InvokeTaskComplete(__instance, idx);
    }
}