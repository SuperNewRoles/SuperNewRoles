using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using HarmonyLib;
using Hazel;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Modules;
using SuperNewRoles.Patches;
using SuperNewRoles.Roles.Ability;
using UnityEngine;

namespace SuperNewRoles.Roles.CrewMate;

class Taskmaster : RoleBase<Taskmaster>
{
    public override RoleId Role { get; } = RoleId.Taskmaster;

    public override Color32 RoleColor { get; } = new(64, 181, 255, 255);
    public override List<Func<AbilityBase>> Abilities { get; } = [
        () => new CustomTaskAbility(
            () => (true, TaskmasterEnableIndividualTasks ? true : null, TaskmasterEnableIndividualTasks ? TaskmasterTaskCount.Total : null),
            TaskmasterEnableIndividualTasks ? TaskmasterTaskCount : null
    )];

    public override QuoteMod QuoteMod { get; } = QuoteMod.SuperNewRoles;
    public override RoleTypes IntroSoundType { get; } = RoleTypes.Crewmate;
    public override short IntroNum { get; } = 1;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Crewmate;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Crewmate;

    public override TeamTag TeamTag { get; } = TeamTag.Crewmate;

    public override RoleTag[] RoleTags { get; } = [];
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Crewmate;

    [CustomOptionBool("TaskmasterEnableIndividualTasks", false)]
    public static bool TaskmasterEnableIndividualTasks;

    [CustomOptionTask("TaskmasterTaskCount", 5, 5, 5, parentFieldName: nameof(TaskmasterEnableIndividualTasks), parentActiveValue: true)]
    public static TaskOptionData TaskmasterTaskCount;

    [CustomOptionBool("TaskmasterCanFixSabotageInstantly", false)]
    public static bool TaskmasterCanFixSabotageInstantly;

    [CustomOptionBool("TaskmasterEnableReactorOxygenElevatorFix", true, parentFieldName: nameof(TaskmasterCanFixSabotageInstantly), parentActiveValue: true)]
    public static bool TaskmasterEnableReactorOxygenElevatorFix;

    [CustomOptionBool("TaskmasterEnableLightsFix", true, parentFieldName: nameof(TaskmasterCanFixSabotageInstantly), parentActiveValue: true)]
    public static bool TaskmasterEnableLightsFix;

    [CustomOptionBool("TaskmasterEnableCommsFix", true, parentFieldName: nameof(TaskmasterCanFixSabotageInstantly), parentActiveValue: true)]
    public static bool TaskmasterEnableCommsFix;
}

[HarmonyPatch(typeof(Console), nameof(Console.Use))]
public static class TaskmasterPatch
{
    private const float MinigameCloseDelaySeconds = 0.1f;

    public static void Postfix(Console __instance)
    {
        if (ExPlayerControl.LocalPlayer.Role != RoleId.Taskmaster) return;
        if (ModHelpers.IsSabotage(__instance.TaskTypes.FirstOrDefault()))
        {
            if (!Taskmaster.TaskmasterCanFixSabotageInstantly) return;
            ModHelpers.RpcFixingSabotage(__instance.TaskTypes.FirstOrDefault());
            CloseMinigameSafely();
        }
        else
        {
            NormalPlayerTask task = __instance.FindTask(ExPlayerControl.LocalPlayer)?.TryCast<NormalPlayerTask>();
            if (task != null)
            {
                task.NextStep();
                CloseMinigameSafely();
            }
        }
    }

    // Let the base Console.Use flow finish first; closing immediately can leave moveable false on some clients.
    private static void CloseMinigameSafely()
    {
        Minigame currentMinigame = Minigame.Instance;
        new LateTask(() =>
        {
            currentMinigame?.Close();

            PlayerControl localPlayer = PlayerControl.LocalPlayer;
            if (localPlayer?.Data == null || localPlayer.Data.IsDead) return;
            if (MeetingHud.Instance != null || ExileController.Instance != null) return;
            if (Minigame.Instance != null) return;

            localPlayer.moveable = true;
        }, MinigameCloseDelaySeconds, "TaskmasterCloseMinigame", log: false);
    }
}
