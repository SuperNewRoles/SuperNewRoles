using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
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
        ResetVentOccupancy();
        var data = new WrapUpEventData(exiled);
        Instance.Awake(data);
    }

    private static void ResetVentOccupancy()
    {
        if (ShipStatus.Instance == null) return;
        if (!ShipStatus.Instance.Systems.TryGetValue(SystemTypes.Ventilation, out var system)) return;
        if (!system.Il2CppIs(out VentilationSystem ventilation)) return;

        // 会議でベントから出されてもバニラの滞在記録が残るため、掃除時に前のベントへ戻されてしまう。
        // 旧版の FixAfterMeetingVent と同様、追放の有無やマップに関係なく記録を消して同期する。
        ventilation.PlayersInsideVents.Clear();
        ventilation.IsDirty = true;
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
        }, 0.5f, "WrapUpEnableCouldCheckEndGame");
    }
}

[HarmonyCoroutinePatch(typeof(AirshipExileController), nameof(AirshipExileController.WrapUpAndSpawn))]
public static class AirshipWrapUpPatch
{
    // スレッドセーフなHashSetを使用して処理済みインスタンスを管理
    private static readonly HashSet<int> _processedInstances = new();

    // 古いエントリを定期的にクリアするためのタイマー
    private static DateTime _lastCleanup = DateTime.UtcNow;
    private static readonly TimeSpan CleanupInterval = TimeSpan.FromMinutes(5);

    public static void Postfix(object __instance)
    {
        AirshipExileController airshipExileController = HarmonyCoroutinePatchProcessor.GetParentFromCoroutine<AirshipExileController>(__instance);
        if (airshipExileController == null) return;

        var instanceId = airshipExileController.GetInstanceID();

        // より効率的な重複チェック
        if (!_processedInstances.Add(instanceId))
        {
            return; // 既に処理済み
        }

        Logger.Info("AirshipWrapUpPatch 開始");
        WrapUpEvent.Invoke(airshipExileController.initData.networkedPlayer);
        CheckGameEndPatch.CouldCheckEndGame = false;
        new LateTask(() =>
        {
            CheckGameEndPatch.CouldCheckEndGame = true;
        }, 0.5f, "AirshipWrapUpEnableCouldCheckEndGame");
    }
}
