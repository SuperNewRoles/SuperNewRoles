using System;
using System.Collections.Generic;
using UnityEngine;

namespace SuperNewRoles.Modules;

public class MurderEventData
{
    public PlayerControl murderer { get; }
    public PlayerControl target { get; }

    public MurderEventData(PlayerControl murderer, PlayerControl target)
    {
        this.murderer = murderer;
        this.target = target;
    }
}

public delegate void MurderEventListener(MurderEventData data);

public static class MurderEvent
{
    private static readonly List<MurderEventListener> killedMeListeners = new();

    public static MurderEventListener AddKilledMeListener(MurderEventListener listener)
    {
        killedMeListeners.Add(listener);
        return listener;
    }

    public static void RemoveListener(MurderEventListener listener)
    {
        killedMeListeners.Remove(listener);
    }

    public static void InvokeKilledMe(PlayerControl murderer, PlayerControl target)
    {
        var data = new MurderEventData(murderer, target);
        foreach (var listener in killedMeListeners.ToArray())
        {
            try
            {
                listener.Invoke(data);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error in murder event listener: {e}");
            }
        }
    }
}