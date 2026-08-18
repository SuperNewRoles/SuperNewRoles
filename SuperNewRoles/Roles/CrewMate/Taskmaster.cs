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
            isTaskTrigger: () => true,
            countsForCrewWin: () => TaskmasterEnableIndividualTasks ? true : null,
            requiredTaskCount: () => TaskmasterEnableIndividualTasks ? TaskmasterTaskCount.Total : null,
            taskOptions: () => TaskmasterEnableIndividualTasks ? TaskmasterTaskCount : null
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

    internal static bool CanFixSabotageInstantly(TaskTypes taskType) =>
        CanFixSabotageInstantly(
            taskType,
            TaskmasterCanFixSabotageInstantly,
            TaskmasterEnableReactorOxygenElevatorFix,
            TaskmasterEnableLightsFix,
            TaskmasterEnableCommsFix
        );

    internal static bool CanFixSabotageInstantly(
        TaskTypes taskType,
        bool enabled,
        bool reactorOxygenElevator,
        bool lights,
        bool comms)
    {
        if (!enabled || !ModHelpers.IsSabotage(taskType))
            return false;

        return taskType switch
        {
            TaskTypes.RestoreOxy or TaskTypes.ResetReactor or TaskTypes.ResetSeismic or TaskTypes.StopCharles => reactorOxygenElevator,
            TaskTypes.FixLights => lights,
            TaskTypes.FixComms => comms,
            // キノコカオスには個別設定がないため、親設定に従う。
            TaskTypes.MushroomMixupSabotage => true,
            _ => false,
        };
    }
}

[HarmonyPatch(typeof(Console), nameof(Console.Use))]
public static class TaskmasterPatch
{
    private const float MinigameCloseDelaySeconds = 0.1f;
    private const float MovementRestoreDelaySeconds = 0.35f;

    public static void Postfix(Console __instance)
    {
        if (ExPlayerControl.LocalPlayer.Role != RoleId.Taskmaster) return;
        TaskTypes taskType = __instance.TaskTypes.FirstOrDefault();
        if (ModHelpers.IsSabotage(taskType))
        {
            if (!Taskmaster.CanFixSabotageInstantly(taskType)) return;
            UseConsoleInstantly(() => ModHelpers.RpcFixingSabotage(taskType));
            return;
        }

        NormalPlayerTask task = __instance.FindTask(ExPlayerControl.LocalPlayer)?.TryCast<NormalPlayerTask>();
        if (task != null)
            UseConsoleInstantly(() => AdvanceTask(task));
    }

    private static void AdvanceTask(NormalPlayerTask task)
    {
        bool wasComplete = task.IsComplete;
        task.NextStep();

        // NextStep updates the task but, unlike a minigame's normal completion path,
        // does not show the task-complete feedback itself.
        if (!wasComplete && task.IsComplete && HudManager.Instance)
            HudManager.Instance.ShowTaskComplete();
    }

    private static void UseConsoleInstantly(Action action)
    {
        try
        {
            action();
        }
        catch (Exception ex)
        {
            Logger.Warning($"Taskmaster instant console use failed; closing minigame anyway: {ex}", "Taskmaster");
        }
        finally
        {
            CloseMinigameSafely();
        }
    }

    private static void CloseMinigameSafely()
    {
        Minigame targetMinigame = Minigame.Instance;

        new LateTask(() =>
        {
            Minigame visibleMinigame = targetMinigame;
            string overlayMenuName = null;
            try
            {
                if (MeetingHud.Instance != null || ExileController.Instance != null) return;
                if (!targetMinigame || Minigame.Instance != targetMinigame) return;

                visibleMinigame = GetVisibleMinigame(targetMinigame);
                overlayMenuName = GetActiveOverlayMenuName(visibleMinigame);

                try
                {
                    // Close the displayed stage, not only the MultistageMinigame container.
                    // RefuelStage.Close also releases its held touch/refuel state.
                    visibleMinigame.Close();
                }
                catch (Exception ex)
                {
                    Logger.Warning($"Taskmaster visible minigame close failed; scheduling fallback cleanup: {ex}", "Taskmaster");
                }

                // Some touch-only minigames do not remove their controller menu when closed
                // programmatically. Use the exact menu name captured after Begin completed.
                CloseControllerOverlay(overlayMenuName);
            }
            catch (Exception ex)
            {
                Logger.Warning($"Taskmaster minigame cleanup failed; scheduling fallback cleanup: {ex}", "Taskmaster");
            }
            finally
            {
                ScheduleMovementRestore(targetMinigame, visibleMinigame, overlayMenuName);
            }
        }, MinigameCloseDelaySeconds, "TaskmasterCloseMinigame", log: false);
    }

    private static Minigame GetVisibleMinigame(Minigame minigame)
    {
        if (!minigame) return minigame;

        MultistageMinigame multistageMinigame = minigame.TryCast<MultistageMinigame>();
        return multistageMinigame && multistageMinigame.stage
            ? multistageMinigame.stage
            : minigame;
    }

    private static string GetActiveOverlayMenuName(Minigame visibleMinigame)
    {
        ControllerManager controllerManager = ControllerManager.Instance;
        if (visibleMinigame && visibleMinigame.gameObject)
        {
            string visibleMenuName = visibleMinigame.gameObject.name;
            if (controllerManager && controllerManager.IsMenuActiveAtAll(visibleMenuName))
                return visibleMenuName;
        }

        ControllerUiElementsState currentUiState = controllerManager?.CurrentUiState;
        if (currentUiState != null && !currentUiState.IsScene && !string.IsNullOrEmpty(currentUiState.MenuName))
            return currentUiState.MenuName;

        return visibleMinigame && visibleMinigame.gameObject
            ? visibleMinigame.gameObject.name
            : null;
    }

    private static void CloseControllerOverlay(string menuName)
    {
        if (string.IsNullOrEmpty(menuName)) return;

        ControllerManager controllerManager = ControllerManager.Instance;
        if (!controllerManager || !controllerManager.IsMenuActiveAtAll(menuName)) return;

        try
        {
            controllerManager.CloseOverlayMenu(menuName);
        }
        catch (Exception ex)
        {
            Logger.Warning($"Taskmaster controller overlay close failed: {ex}", "Taskmaster");
        }

        try
        {
            if (controllerManager && controllerManager.IsMenuActiveAtAll(menuName))
            {
                Logger.Warning($"Taskmaster overlay {menuName} remained active; resetting controller menus.", "Taskmaster");
                controllerManager.CloseAndResetAll();
            }
        }
        catch (Exception ex)
        {
            Logger.Warning($"Taskmaster controller menu reset failed: {ex}", "Taskmaster");
        }
    }

    private static void ScheduleMovementRestore(Minigame targetMinigame, Minigame visibleMinigame, string overlayMenuName)
    {
        new LateTask(() =>
        {
            PlayerControl localPlayer = null;
            bool shouldRestoreMovement = false;
            try
            {
                localPlayer = PlayerControl.LocalPlayer;
                if (localPlayer?.Data == null || localPlayer.Data.IsDead) return;
                if (MeetingHud.Instance != null || ExileController.Instance != null) return;

                Minigame activeMinigame = Minigame.Instance;
                if (activeMinigame && activeMinigame != targetMinigame) return;
                shouldRestoreMovement = true;

                CloseControllerOverlay(overlayMenuName);

                if (visibleMinigame && visibleMinigame != activeMinigame)
                {
                    Logger.Warning("Taskmaster visible minigame stage remained active after Close; forcing cleanup.", "Taskmaster");
                    try
                    {
                        UnityEngine.Object.Destroy(visibleMinigame.gameObject);
                    }
                    catch (Exception ex)
                    {
                        Logger.Warning($"Taskmaster fallback visible minigame destruction failed: {ex}", "Taskmaster");
                    }
                }

                // Minigame.Close normally destroys its object after a 0.25 second coroutine.
                if (activeMinigame)
                {
                    Logger.Warning("Taskmaster minigame remained active after Close; forcing cleanup.", "Taskmaster");
                    try
                    {
                        UnityEngine.Object.Destroy(activeMinigame.gameObject);
                    }
                    catch (Exception ex)
                    {
                        Logger.Warning($"Taskmaster fallback minigame destruction failed: {ex}", "Taskmaster");
                    }

                    if (Minigame.Instance == activeMinigame)
                        Minigame.Instance = null;
                }
            }
            catch (Exception ex)
            {
                Logger.Warning($"Taskmaster movement restoration failed: {ex}", "Taskmaster");
            }
            finally
            {
                if (shouldRestoreMovement && localPlayer)
                    localPlayer.moveable = true;
            }
        }, MovementRestoreDelaySeconds, "TaskmasterRestoreMovement", log: false);
    }
}
