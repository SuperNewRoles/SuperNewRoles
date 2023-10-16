using System;
using System.Collections.Generic;
using HarmonyLib;
using InnerNet;
using SuperNewRoles.Mode;
using SuperNewRoles.Mode.SuperHostRoles;
using SuperNewRoles.Roles;
using SuperNewRoles.Roles.Crewmate;
using SuperNewRoles.Roles.Impostor;
using SuperNewRoles.Roles.Impostor.MadRole;
using SuperNewRoles.Roles.Neutral;
using UnityEngine;
using static SuperNewRoles.ModHelpers;

namespace SuperNewRoles.Patches;

#region PlayerControlCheckProtectPatch


[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CheckProtect))]
static class CheckProtectPatch
{
    public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
    {
        if (!ModeHandler.IsMode(ModeId.SuperHostRoles)) return true;

        bool useAbility = false;
        RoleId angelRole = __instance.GetGhostRole();

        switch (angelRole)
        {
            case RoleId.GhostMechanic:
                TaskTypes sabotageType = TaskTypes.None;
                foreach (PlayerTask task in PlayerControl.LocalPlayer.myTasks) // 亡霊整備士自身のタスクを検索した場合サボタージュを取得できなかった為, ホストのタスクを参照している。
                {
                    if (task.TaskType is TaskTypes.FixLights or TaskTypes.RestoreOxy or TaskTypes.ResetReactor or TaskTypes.ResetSeismic or TaskTypes.FixComms or TaskTypes.StopCharles)
                    {
                        sabotageType = task.TaskType;
                        break; // サボタージュが1つ取得された時点で, タスクを取得するforeachから抜ける
                    }
                }
                // タスクが取得されていて, 使用回数を使い切っていない場合リペアを発動する。
                if (sabotageType != TaskTypes.None && RoleClass.GhostMechanic.AbilityUsedCountSHR[__instance.PlayerId] < CustomOptionHolder.GhostMechanicRepairLimit.GetInt())
                {
                    useAbility = true;
                    Sabotage.FixSabotage.RepairProcsee.ReceiptOfSabotageFixing(sabotageType);
                    RoleClass.GhostMechanic.AbilityUsedCountSHR[__instance.PlayerId]++;
                }
                break;
        }

        if (useAbility) // アビリティが発動された場合, 守護の表示を行う
        {
            __instance.RpcShowGuardEffect(__instance);
        }

        return false;
    }
}
#endregion
