using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using SuperNewRoles.Modules;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SuperNewRoles.Roles.Ability;

/// <summary>
/// プレイヤーのタスクタイプを指定されたタスクタイプに変更するAbility
/// </summary>
public class CustomTaskTypeAbility : AbilityBase
{
    public TaskTypes TargetTaskType { get; }
    public MapNames? TargetMap { get; }
    public bool ChangeAllTasks { get; }

    public CustomTaskTypeAbility(TaskTypes targetTaskType, bool changeAllTasks = false, MapNames? targetMap = null)
    {
        TargetTaskType = targetTaskType;
        ChangeAllTasks = changeAllTasks;
        TargetMap = targetMap;
    }

    public bool ShouldChangeTask()
    {
        return true;
    }
}

// タスクタイプ変更のパッチ
[HarmonyPatch]
public static class CustomTaskTypePatches
{
    [HarmonyPatch(typeof(Console), nameof(Console.Use))]
    public static class ConsolePatch
    {
        private static Minigame preMinigame;

        static void Prefix(Console __instance)
        {
            var customTaskTypeAbility = ExPlayerControl.LocalPlayer.GetAbility<CustomTaskTypeAbility>();
            if (customTaskTypeAbility == null) return;

            if (!customTaskTypeAbility.ShouldChangeTask()) return;

            __instance.CanUse(PlayerControl.LocalPlayer.Data, out bool canUse, out bool _);
            if (!canUse) return;

            PlayerTask task = __instance.FindTask(PlayerControl.LocalPlayer);
            if (task.TaskType is TaskTypes.FixLights or TaskTypes.RestoreOxy or TaskTypes.ResetReactor or
                TaskTypes.ResetSeismic or TaskTypes.FixComms or TaskTypes.StopCharles or TaskTypes.MushroomMixupSabotage)
                return;

            preMinigame = task.MinigamePrefab;
            ShipStatus ship = GetTargetShip(customTaskTypeAbility.TargetTaskType, customTaskTypeAbility.TargetMap);
            var targetTask = GetTargetTaskFromShip(ship, customTaskTypeAbility.TargetTaskType);
            if (targetTask != null)
            {
                task.MinigamePrefab = targetTask.MinigamePrefab;
            }
        }

        static void Postfix(Console __instance)
        {
            var customTaskTypeAbility = ExPlayerControl.LocalPlayer.GetAbility<CustomTaskTypeAbility>();
            if (customTaskTypeAbility == null) return;

            if (!customTaskTypeAbility.ShouldChangeTask()) return;

            __instance.CanUse(PlayerControl.LocalPlayer.Data, out bool canUse, out bool _);
            if (!canUse) return;

            PlayerTask task = __instance.FindTask(PlayerControl.LocalPlayer);
            if (preMinigame != null)
            {
                task.MinigamePrefab = preMinigame;
                preMinigame = null;
            }
        }

        private static ShipStatus GetTargetShip(TaskTypes targetTaskType, MapNames? targetMap)
        {
            // 特定のマップが指定されている場合はそのマップから取得
            if (targetMap.HasValue)
            {
                return targetMap.Value switch
                {
                    MapNames.Fungle => MapLoader.Fungle,
                    _ => ShipStatus.Instance
                };
            }

            // 現在のマップから取得
            return ShipStatus.Instance;
        }

        private static NormalPlayerTask GetTargetTaskFromShip(ShipStatus ship, TaskTypes targetTaskType)
        {
            // ショートタスクから探す
            var shortTask = ship.ShortTasks.FirstOrDefault(x => x.TaskType == targetTaskType);
            if (shortTask != null) return shortTask;

            // ロングタスクから探す
            var longTask = ship.LongTasks.FirstOrDefault(x => x.TaskType == targetTaskType);
            if (longTask != null) return longTask;

            // コモンタスクから探す
            var commonTask = ship.CommonTasks.FirstOrDefault(x => x.TaskType == targetTaskType);
            if (commonTask != null) return commonTask;

            return null;
        }
    }
}