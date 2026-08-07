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

public class CustomTaskAbility : AbilityBase
{
    public Func<(bool isTaskTrigger, bool? countTask, int? all)> IsTaskTrigger { get; }
    public TaskOptionData? assignTaskData { get; }
    public TaskSelectionExclusionConfig TaskSelectionExclusionConfig { get; }
    public bool IsTaskSelectionExclusionActive =>
        Player?.GetAbility<CustomTaskTypeAbility>()?.IsPrefabReplacementMode == true &&
        TaskSelectionExclusionConfig?.IsActive == true;

    public CustomTaskAbility(
        Func<(bool isTaskTrigger, bool? countTask, int? all)> isTaskTrigger,
        TaskOptionData? assignTaskData = null,
        TaskSelectionExclusionConfig taskSelectionExclusionConfig = null)
    {
        IsTaskTrigger = isTaskTrigger;
        this.assignTaskData = assignTaskData;
        TaskSelectionExclusionConfig = taskSelectionExclusionConfig;
    }

    public (bool isTaskTrigger, bool? countTask, int? all)? CheckIsTaskTrigger()
    {
        return IsTaskTrigger?.Invoke();
    }
    public void AssignTasks()
    {
        // ローカルプレイヤーでない場合は処理しない（各プレイヤーが自分自身のタスクのみを設定するようにする）
        if (!Player.AmOwner) return;

        if (assignTaskData != null && assignTaskData.Total <= 0)
        {
            RpcUncheckedSetTasks(Player, new System.Collections.Generic.List<byte>());
            return;
        }

        CustomTaskTypeAbility customTaskTypeAbility = Player.GetAbility<CustomTaskTypeAbility>();
        if (customTaskTypeAbility != null && !customTaskTypeAbility.ShouldChangeTask())
        {
            int all = 0;
            if (assignTaskData != null)
            {
                all = assignTaskData.Total;
            }
            else
            {
                var task = Player.GetAllTaskForShowProgress();
                all = task.all;
            }
            customTaskTypeAbility.AssignTasks(all);
            return;
        }
        var taskData = assignTaskData;
        if (taskData == null && IsTaskSelectionExclusionActive)
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

        // プレイヤーが存在しない場合は処理しない
        if (Player == null) return;

        // タスクリストを作成
        Il2CppSystem.Collections.Generic.HashSet<TaskTypes> types = new();
        Il2CppSystem.Collections.Generic.List<byte> taskList = new();

        // CommonTasksを追加
        int startIndex = 0;
        var shuffledCommonTasks = TaskSelectionExclusion.GetAvailableTasks(Player, ShipStatus.Instance.CommonTasks).Shuffled();
        ShipStatus.Instance.AddTasksFromList(ref startIndex, GetTaskCountToAssign(taskData.Common, shuffledCommonTasks.Count), taskList, types, shuffledCommonTasks.ToIl2CppList());

        // ShortTasksを追加
        startIndex = 0;
        var shuffledShortTasks = TaskSelectionExclusion.GetAvailableTasks(Player, ShipStatus.Instance.ShortTasks).Shuffled();
        ShipStatus.Instance.AddTasksFromList(ref startIndex, GetTaskCountToAssign(taskData.Short, shuffledShortTasks.Count), taskList, types, shuffledShortTasks.ToIl2CppList());

        // LongTasksを追加
        startIndex = 0;
        var shuffledLongTasks = TaskSelectionExclusion.GetAvailableTasks(Player, ShipStatus.Instance.LongTasks).Shuffled();
        ShipStatus.Instance.AddTasksFromList(ref startIndex, GetTaskCountToAssign(taskData.Long, shuffledLongTasks.Count), taskList, types, shuffledLongTasks.ToIl2CppList());

        // タスクをプレイヤーに割り当てる
        RpcUncheckedSetTasks(Player, taskList.ToSystemList());
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
        return ShouldExcludeTask(taskType, IsTaskSelectionExclusionActive);
    }

    internal bool ShouldExcludeTask(TaskTypes taskType, bool isPrefabReplacementMode)
    {
        return isPrefabReplacementMode &&
               TaskSelectionExclusionConfig?.IsExcluded(taskType) == true;
    }

    [CustomRPC]
    public static void RpcUncheckedSetTasks(PlayerControl player, System.Collections.Generic.List<byte> taskList)
    {
        player.Data.SetTasks(taskList.ToArray());
        NameText.UpdateNameInfo(player);
    }
}
