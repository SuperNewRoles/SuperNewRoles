using System;
using System.Collections.Generic;
using HarmonyLib;
using SuperNewRoles.Modules.Events.Bases;
using UnityEngine;

namespace SuperNewRoles.Events;

public class WrapUpEventData : IEventData
{
    public NetworkedPlayerInfo exiled { get; }
    public WrapUpEventData(NetworkedPlayerInfo exiled)
    {
        this.exiled = exiled;
    }
}

public class WrapUpEvent : EventTargetBase<WrapUpEvent, WrapUpEventData>
{
    public static void Invoke(NetworkedPlayerInfo exiled)
    {
        var data = new WrapUpEventData(exiled);
        Instance.Awake(data);
    }
}

[HarmonyPatch(typeof(ExileController), nameof(ExileController.WrapUp))]
public static class WrapUpPatch
{
    public static void Postfix(ExileController __instance)
    {
        WrapUpEvent.Invoke(__instance.initData.networkedPlayer);
    }
}

[HarmonyPatch(typeof(AirshipExileController), nameof(AirshipExileController.WrapUpAndSpawn))]
public static class AirshipWrapUpPatch
{
    public static void Postfix(AirshipExileController __instance)
    {
        WrapUpEvent.Invoke(__instance.initData.networkedPlayer);
    }
}