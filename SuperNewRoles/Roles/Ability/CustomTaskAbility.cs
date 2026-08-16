using System;
using UnityEngine;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability.CustomButton;
using System.Collections.Generic;
using Hazel;
using Il2CppSystem.Collections.Generic;
using System.Linq;

namespace SuperNewRoles.Roles.Ability;

public class CustomTaskAbility : AbilityBase, IPrioritizedAbility
{
    public int Priority { get; }
    public Func<bool?> IsTaskTrigger { get; }
    public Func<bool?> CountsForCrewWin { get; }
    public Func<int?> RequiredTaskCount { get; }
    public Func<TaskOptionData> TaskOptions { get; }

    public CustomTaskAbility(
        Func<bool?> isTaskTrigger = null,
        Func<bool?> countsForCrewWin = null,
        Func<int?> requiredTaskCount = null,
        Func<TaskOptionData> taskOptions = null,
        int priority = AbilityPriority.Default)
    {
        IsTaskTrigger = isTaskTrigger;
        CountsForCrewWin = countsForCrewWin;
        RequiredTaskCount = requiredTaskCount;
        TaskOptions = taskOptions;
        Priority = priority;
    }

    public void AssignTasks()
        => AssignTasks(Player, TaskOptions?.Invoke(), Player?.GetAbility<CustomTaskTypeAbility>());

    internal static void AssignTasks(
        ExPlayerControl player,
        TaskOptionData taskOptions,
        CustomTaskTypeAbility customTaskTypeAbility = null)
    {
        // ローカルプレイヤーでない場合は処理しない（各プレイヤーが自分自身のタスクのみを設定するようにする）
        if (player == null || !player.AmOwner) return;

        if (taskOptions != null && taskOptions.Total <= 0)
        {
            RpcUncheckedSetTasks(player, new System.Collections.Generic.List<byte>());
            return;
        }

        if (customTaskTypeAbility != null && !customTaskTypeAbility.ShouldChangeTask())
        {
            int all = taskOptions?.Total ?? player.GetAllTaskForShowProgress().all;
            customTaskTypeAbility.AssignTasks(all);
            return;
        }
        if (taskOptions == null) return;

        // ShipStatusのインスタンスが存在しない場合は処理しない
        if (ShipStatus.Instance == null) return;

        // タスクリストを作成
        Il2CppSystem.Collections.Generic.HashSet<TaskTypes> types = new();
        Il2CppSystem.Collections.Generic.List<byte> taskList = new();

        // CommonTasksを追加
        int startIndex = 0;
        Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<NormalPlayerTask> commonTasks = ShipStatus.Instance.CommonTasks;
        var shuffledCommonTasks = commonTasks.ToSystemList().Shuffled();
        ShipStatus.Instance.AddTasksFromList(ref startIndex, taskOptions.Common, taskList, types, shuffledCommonTasks.ToIl2CppList());

        // ShortTasksを追加
        startIndex = 0;
        var shortTasks = ShipStatus.Instance.ShortTasks;
        var shuffledShortTasks = shortTasks.ToSystemList().Shuffled();
        ShipStatus.Instance.AddTasksFromList(ref startIndex, taskOptions.Short, taskList, types, shuffledShortTasks.ToIl2CppList());

        // LongTasksを追加
        startIndex = 0;
        var longTasks = ShipStatus.Instance.LongTasks;
        var shuffledLongTasks = longTasks.ToSystemList().Shuffled();
        ShipStatus.Instance.AddTasksFromList(ref startIndex, taskOptions.Long, taskList, types, shuffledLongTasks.ToIl2CppList());

        // タスクをプレイヤーに割り当てる
        RpcUncheckedSetTasks(player, taskList.ToSystemList());
    }
    [CustomRPC]
    public static void RpcUncheckedSetTasks(PlayerControl player, System.Collections.Generic.List<byte> taskList)
    {
        player.Data.SetTasks(taskList.ToArray());
        NameText.UpdateNameInfo(player);
    }
}
