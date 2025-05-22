using System;
using System.Collections.Generic;
using HarmonyLib;
using SuperNewRoles.Modules;
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
        CheckGameEndPatch.CouldCheckEndGame = false;
        new LateTask(() =>
        {
            CheckGameEndPatch.CouldCheckEndGame = true;
        }, 0.5f);
    }
}

[HarmonyPatch(typeof(AirshipExileController._WrapUpAndSpawn_d__11), nameof(AirshipExileController._WrapUpAndSpawn_d__11.MoveNext))]
public static class AirshipWrapUpPatch
{
    private static int _last;
    public static void Postfix(AirshipExileController._WrapUpAndSpawn_d__11 __instance)
    {
        if (_last == __instance.__4__this.GetInstanceID())
            return;
        _last = __instance.__4__this.GetInstanceID();
        Logger.Info("AirshipWrapUpPatch 開始");
        WrapUpEvent.Invoke(__instance.__4__this.initData.networkedPlayer);
        CheckGameEndPatch.CouldCheckEndGame = false;
        new LateTask(() =>
        {
            CheckGameEndPatch.CouldCheckEndGame = true;
        }, 0.5f);
    }
}