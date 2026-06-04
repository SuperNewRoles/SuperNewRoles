using System;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using HarmonyLib;
using SuperNewRoles.Ability;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Events.PCEvents;
using SuperNewRoles.Modules;
using SuperNewRoles.Modules.Events.Bases;
using SuperNewRoles.Patches;
using SuperNewRoles.Roles.Ability;
using UnityEngine;

namespace SuperNewRoles.Roles.Impostor;

class Tasker : RoleBase<Tasker>
{
    public override RoleId Role { get; } = RoleId.Tasker;
    public override Color32 RoleColor { get; } = Palette.ImpostorRed;
    public override List<Func<AbilityBase>> Abilities { get; } = [
        () => new CustomTaskAbility(() => (true, false, null), GetTaskData()),
        () => new KillableAbility(() => TaskerCanKill),
        () => new TaskerAbility()
    ];

    public override QuoteMod QuoteMod { get; } = QuoteMod.SuperNewRoles;
    public override RoleTypes IntroSoundType { get; } = RoleTypes.Impostor;
    public override short IntroNum { get; } = 1;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Impostor;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Impostor;
    public override TeamTag TeamTag { get; } = TeamTag.Impostor;
    public override RoleTag[] RoleTags { get; } = [RoleTag.ImpostorTeam];
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Impostor;

    [CustomOptionBool("TaskerEnableIndividualTasks", true, translationName: "TaskOption")]
    public static bool TaskerEnableIndividualTasks;

    [CustomOptionTask("TaskerTaskCount", 1, 1, 1, parentFieldName: nameof(TaskerEnableIndividualTasks))]
    public static TaskOptionData TaskerTaskCount;

    [CustomOptionBool("TaskerCanKill", true)]
    public static bool TaskerCanKill;
    [CustomOptionBool("TaskerIsKillCoolTaskNow", true, parentFieldName: nameof(TaskerCanKill))]
    public static bool TaskerIsKillCoolTaskNow;

    private static TaskOptionData GetTaskData()
    {
        if (TaskerEnableIndividualTasks && TaskerTaskCount is { Total: > 0 })
            return TaskerTaskCount;

        var options = GameOptionsManager.Instance.CurrentGameOptions;
        return new TaskOptionData(
            shortOption: options.GetInt(Int32OptionNames.NumShortTasks),
            longOption: options.GetInt(Int32OptionNames.NumLongTasks),
            commonOption: options.GetInt(Int32OptionNames.NumCommonTasks));
    }
}

public class TaskerAbility : AbilityBase
{
    private EventListener<TaskCompleteEventData> _taskCompleteListener;

    public override void AttachToAlls()
    {
        _taskCompleteListener = TaskCompleteEvent.Instance.AddListener(OnTaskComplete);
    }

    public override void DetachToAlls()
    {
        _taskCompleteListener?.RemoveListener();
        _taskCompleteListener = null;
    }

    private void OnTaskComplete(TaskCompleteEventData data)
    {
        if (!AmongUsClient.Instance.AmHost) return;
        if (data.player == null || data.player.PlayerId != Player.PlayerId) return;
        if (Player.IsDead() || !Player.IsTaskComplete()) return;

        EndGamer.EndGameImpostorWin();
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
public static class TaskerPlayerControlFixedUpdatePatch
{
    private static float? _killTimerBeforeFixedUpdate;

    [HarmonyPrefix]
    [HarmonyPriority(Priority.First)]
    public static void Prefix(PlayerControl __instance)
    {
        _killTimerBeforeFixedUpdate = null;
        if (!ShouldHoldKillTimer(__instance)) return;

        _killTimerBeforeFixedUpdate = __instance.killTimer;
    }

    [HarmonyPostfix]
    [HarmonyPriority(Priority.Last)]
    public static void Postfix(PlayerControl __instance)
    {
        if (!_killTimerBeforeFixedUpdate.HasValue) return;
        if (__instance != PlayerControl.LocalPlayer)
        {
            _killTimerBeforeFixedUpdate = null;
            return;
        }

        float maxTimer = GameOptionsManager.Instance.CurrentGameOptions.GetFloat(FloatOptionNames.KillCooldown);
        ExPlayerControl.LocalPlayer.SetKillTimerUnchecked(_killTimerBeforeFixedUpdate.Value, maxTimer);
        _killTimerBeforeFixedUpdate = null;
    }

    private static bool ShouldHoldKillTimer(PlayerControl player)
    {
        if (player == null || player != PlayerControl.LocalPlayer) return false;
        if (Tasker.TaskerIsKillCoolTaskNow) return false;

        var localPlayer = ExPlayerControl.LocalPlayer;
        if (localPlayer == null || localPlayer.Role != RoleId.Tasker || localPlayer.IsDead()) return false;
        if (player.CanMove || Minigame.Instance == null) return false;
        if (Minigame.Instance.MyNormTask == null || !Minigame.Instance.MyNormTask.Owner.AmOwner) return false;

        return true;
    }
}

[HarmonyPatch(typeof(NormalPlayerTask), nameof(NormalPlayerTask.FixedUpdate))]
public static class TaskerNormalPlayerTaskFixedUpdatePatch
{
    public static void Postfix(NormalPlayerTask __instance)
    {
        if (__instance == null) return;
        if (__instance.IsComplete)
        {
            if (__instance.Arrow?.isActiveAndEnabled == true)
                __instance.Arrow.gameObject.SetActive(false);
            return;
        }

        if (ExPlayerControl.LocalPlayer?.Role != RoleId.Tasker) return;
        if (__instance.TaskStep <= 0 || __instance.Arrow == null) return;

        __instance.Arrow.gameObject.SetActive(true);
    }
}
