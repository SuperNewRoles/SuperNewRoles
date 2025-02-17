using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace SuperNewRoles.Events;

public class WrapUpEventData
{
    public NetworkedPlayerInfo exiled { get; }
    public WrapUpEventData(NetworkedPlayerInfo exiled)
    {
        this.exiled = exiled;
    }
}

public delegate void WrapUpEventListener(WrapUpEventData data);

public static class WrapUpEvent
{
    private static readonly List<WrapUpEventListener> wrapUpListeners = new();

    public static WrapUpEventListener AddWrapUpListener(WrapUpEventListener listener)
    {
        wrapUpListeners.Add(listener);
        return listener;
    }

    public static void RemoveWrapUpListener(WrapUpEventListener listener)
    {
        wrapUpListeners.Remove(listener);
    }

    public static void InvokeWrapUp(NetworkedPlayerInfo exiled)
    {
        foreach (var listener in wrapUpListeners.ToArray())
        {
            try
            {
                listener.Invoke(new WrapUpEventData(exiled));
            }
            catch (Exception e)
            {
                Debug.LogError($"Error in wrap up event listener: {e}");
            }
        }
    }
}

[HarmonyPatch(typeof(ExileController), nameof(ExileController.WrapUp))]
public static class WrapUpPatch
{
    public static void Postfix(ExileController __instance)
    {
        WrapUpEvent.InvokeWrapUp(__instance.initData.networkedPlayer);
    }
}
[HarmonyPatch(typeof(AirshipExileController), nameof(AirshipExileController.WrapUpAndSpawn))]
public static class AirshipWrapUpPatch
{
    public static void Postfix(AirshipExileController __instance)
    {
        WrapUpEvent.InvokeWrapUp(__instance.initData.networkedPlayer);
    }
}