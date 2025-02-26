using System;
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
    public Func<(bool isTaskTrigger, int all)> IsTaskTrigger { get; }
    public TaskOptionData? assignTaskData { get; }
    public CustomTaskAbility(Func<(bool isTaskTrigger, int all)> isTaskTrigger, TaskOptionData? assignTaskData = null)
    {
        IsTaskTrigger = isTaskTrigger;
        this.assignTaskData = assignTaskData;
    }

    public (bool isTaskTrigger, int all)? CheckIsTaskTrigger()
    {
        return IsTaskTrigger?.Invoke();
    }
    public void AssignTasks()
    {
        if (assignTaskData == null) return;

        // ShipStatusのインスタンスが存在しない場合は処理しない
        if (ShipStatus.Instance == null) return;

        // プレイヤーが存在しない場合は処理しない
        if (PlayerControl.LocalPlayer == null) return;

        // タスクリストを作成
        Il2CppSystem.Collections.Generic.HashSet<TaskTypes> types = new();
        Il2CppSystem.Collections.Generic.List<byte> taskList = new();

        // CommonTasksを追加
        int startIndex = 0;
        Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<NormalPlayerTask> commonTasks = ShipStatus.Instance.CommonTasks;
        var shuffledCommonTasks = commonTasks.ToSystemList().Shuffled();
        ShipStatus.Instance.AddTasksFromList(ref startIndex, assignTaskData.Common, taskList, types, shuffledCommonTasks.ToIl2CppList());

        // ShortTasksを追加
        startIndex = 0;
        var shortTasks = ShipStatus.Instance.ShortTasks;
        var shuffledShortTasks = shortTasks.ToSystemList().Shuffled();
        ShipStatus.Instance.AddTasksFromList(ref startIndex, assignTaskData.Short, taskList, types, shuffledShortTasks.ToIl2CppList());

        // LongTasksを追加
        startIndex = 0;
        var longTasks = ShipStatus.Instance.LongTasks;
        var shuffledLongTasks = longTasks.ToSystemList().Shuffled();
        ShipStatus.Instance.AddTasksFromList(ref startIndex, assignTaskData.Long, taskList, types, shuffledLongTasks.ToIl2CppList());

        // タスクをプレイヤーに割り当てる
        if (taskList.Count > 0)
        {
            RpcUncheckedSetTasks(PlayerControl.LocalPlayer, taskList.ToSystemList());
        }
    }
    [CustomRPC]
    public static void RpcUncheckedSetTasks(PlayerControl player, System.Collections.Generic.List<byte> taskList)
    {
        player.Data.SetTasks(taskList.ToArray());
    }

    public override void AttachToLocalPlayer()
    {

    }
}