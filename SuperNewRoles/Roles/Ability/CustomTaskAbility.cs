using System;
using AmongUs.GameOptions;
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
    public TaskSelectionExclusionConfig TaskSelectionExclusionConfig { get; }
    public bool IsTaskSelectionExclusionActive =>
        TaskSelectionExclusionConfig?.IsActive == true;

    public CustomTaskAbility(
        Func<bool?> isTaskTrigger = null,
        Func<bool?> countsForCrewWin = null,
        Func<int?> requiredTaskCount = null,
        Func<TaskOptionData> taskOptions = null,
        int priority = AbilityPriority.Default,
        TaskSelectionExclusionConfig taskSelectionExclusionConfig = null)
    {
        IsTaskTrigger = isTaskTrigger;
        CountsForCrewWin = countsForCrewWin;
        RequiredTaskCount = requiredTaskCount;
        TaskOptions = taskOptions;
        Priority = priority;
        TaskSelectionExclusionConfig = taskSelectionExclusionConfig;
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
        var taskData = taskOptions;
        if (taskData == null && player.GetAbilities<CustomTaskAbility>()
            .Any(ability => ability?.IsTaskSelectionExclusionActive == true))
        {
            var options = GameOptionsManager.Instance?.CurrentGameOptions;
            if (options == null) return;

            taskData = CreateVanillaTaskData(
                options.GetInt(Int32OptionNames.NumShortTasks),
                options.GetInt(Int32OptionNames.NumLongTasks),
                options.GetInt(Int32OptionNames.NumCommonTasks));
        }
        if (taskData == null) return;

        // ShipStatusのインスタンスが存在しない場合は処理しない
        if (ShipStatus.Instance == null) return;

        // タスクリストを作成
        Il2CppSystem.Collections.Generic.HashSet<TaskTypes> types = new();
        Il2CppSystem.Collections.Generic.List<byte> taskList = new();

        // CommonTasksを追加
        int startIndex = 0;
        var shuffledCommonTasks = TaskSelectionExclusion.GetAvailableTasks(player, ShipStatus.Instance.CommonTasks).Shuffled();
        ShipStatus.Instance.AddTasksFromList(ref startIndex, GetTaskCountToAssign(taskData.Common, shuffledCommonTasks.Count), taskList, types, shuffledCommonTasks.ToIl2CppList());

        // ShortTasksを追加
        startIndex = 0;
        var shuffledShortTasks = TaskSelectionExclusion.GetAvailableTasks(player, ShipStatus.Instance.ShortTasks).Shuffled();
        ShipStatus.Instance.AddTasksFromList(ref startIndex, GetTaskCountToAssign(taskData.Short, shuffledShortTasks.Count), taskList, types, shuffledShortTasks.ToIl2CppList());

        // LongTasksを追加
        startIndex = 0;
        var shuffledLongTasks = TaskSelectionExclusion.GetAvailableTasks(player, ShipStatus.Instance.LongTasks).Shuffled();
        ShipStatus.Instance.AddTasksFromList(ref startIndex, GetTaskCountToAssign(taskData.Long, shuffledLongTasks.Count), taskList, types, shuffledLongTasks.ToIl2CppList());

        // タスクをプレイヤーに割り当てる
        RpcUncheckedSetTasks(player, taskList.ToSystemList());
    }

    internal static TaskOptionData CreateVanillaTaskData(int shortTasks, int longTasks, int commonTasks)
    {
        if (shortTasks + longTasks + commonTasks == 0)
            shortTasks = 1;

        return new TaskOptionData(shortTasks, longTasks, commonTasks);
    }

    internal static int GetTaskCountToAssign(int requestedCount, int candidateCount)
    {
        return candidateCount == 0 ? 0 : requestedCount;
    }

    public bool ShouldExcludeTask(TaskTypes taskType)
    {
        return TaskSelectionExclusionConfig?.IsExcluded(taskType) == true;
    }

    [CustomRPC]
    public static void RpcUncheckedSetTasks(PlayerControl player, System.Collections.Generic.List<byte> taskList)
    {
        player.Data.SetTasks(taskList.ToArray());
        NameText.UpdateNameInfo(player);
    }
}
