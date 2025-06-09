using System;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using HarmonyLib;
using SuperNewRoles.Events;
using SuperNewRoles.Events.PCEvents;
using SuperNewRoles.Modules;
using SuperNewRoles.Modules.Events.Bases;
using SuperNewRoles.Roles.Ability.CustomButton;
using UnityEngine;

namespace SuperNewRoles.Roles.Ability;

public class SafecrackerAbility : AbilityBase
{
    public float KillGuardTaskRate { get; }
    public int MaxKillGuardCount { get; }
    public float ExiledGuardTaskRate { get; }
    public int MaxExiledGuardCount { get; }
    public float UseVentTaskRate { get; }
    public float UseSaboTaskRate { get; }
    public float ImpostorLightTaskRate { get; }
    public float CheckImpostorTaskRate { get; }
    public bool ChangeTaskPrefab { get; }

    private int _allTaskCount;
    private int _killGuardCount;
    private int _exiledGuardCount;
    private Dictionary<CheckTasks, bool> _unlockedAbilities;

    private EventListener<TryKillEventData> _onTryKillListener;
    private EventListener<ExileEventData> _exileListener;
    private EventListener<TaskCompleteEventData> _taskCompleteListener;
    private TaskOptionData _task;


    public enum CheckTasks
    {
        KillGuard,
        ExiledGuard,
        UseVent,
        UseSabo,
        ImpostorLight,
        CheckImpostor
    }

    public SafecrackerAbility(
        float killGuardTaskRate,
        int maxKillGuardCount,
        float exiledGuardTaskRate,
        int maxExiledGuardCount,
        float useVentTaskRate,
        float useSaboTaskRate,
        float impostorLightTaskRate,
        float checkImpostorTaskRate,
        bool changeTaskPrefab,
        TaskOptionData task)
    {
        KillGuardTaskRate = killGuardTaskRate;
        MaxKillGuardCount = maxKillGuardCount;
        ExiledGuardTaskRate = exiledGuardTaskRate;
        MaxExiledGuardCount = maxExiledGuardCount;
        UseVentTaskRate = useVentTaskRate;
        UseSaboTaskRate = useSaboTaskRate;
        ImpostorLightTaskRate = impostorLightTaskRate;
        CheckImpostorTaskRate = checkImpostorTaskRate;
        ChangeTaskPrefab = changeTaskPrefab;

        _unlockedAbilities = new();
        _killGuardCount = 0;
        _exiledGuardCount = 0;

        _task = task;
    }

    public override void AttachToAlls()
    {
        base.AttachToAlls();
        _allTaskCount = _task.Total;
        _unlockedAbilities.Clear();
        _killGuardCount = 0;
        _exiledGuardCount = 0;

        _onTryKillListener = TryKillEvent.Instance.AddListener(OnTryKill);
        _exileListener = ExileEvent.Instance.AddListener(OnExile);
        _taskCompleteListener = TaskCompleteEvent.Instance.AddListener(OnTaskComplete);
        Player.AttachAbility(new CustomTaskAbility(() => (true, false, null), _task), new AbilityParentAbility(this));
        Player.AttachAbility(new CustomTaskTypeAbility(TaskTypes.UnlockSafe, ChangeTaskPrefab, MapNames.Airship), new AbilityParentAbility(this));
    }

    public override void DetachToAlls()
    {
        base.DetachToAlls();

        TryKillEvent.Instance.RemoveListener(_onTryKillListener);
        ExileEvent.Instance.RemoveListener(_exileListener);
        TaskCompleteEvent.Instance.RemoveListener(_taskCompleteListener);
    }


    private int GetTotalTaskCount()
    {
        return Player?.Data?.Tasks?.Count ?? 0;
    }

    private int GetCompletedTaskCount()
    {
        return ModHelpers.TaskCompletedData(Player.Player.Data).completed;
    }

    private bool CheckTaskProgress(CheckTasks taskType, float requiredRate)
    {
        if (requiredRate <= 0f) return false;
        if (_unlockedAbilities.TryGetValue(taskType, out var unlocked) && unlocked) return true;

        var requiredTasks = Mathf.CeilToInt(_allTaskCount * (requiredRate / 100f));
        if (GetCompletedTaskCount() < requiredTasks) return false;

        _unlockedAbilities[taskType] = true;
        return true;
    }

    public bool CanKillGuard() => CheckTaskProgress(CheckTasks.KillGuard, KillGuardTaskRate) && _killGuardCount < MaxKillGuardCount;

    public bool CanExiledGuard() => CheckTaskProgress(CheckTasks.ExiledGuard, ExiledGuardTaskRate) && _exiledGuardCount < MaxExiledGuardCount;

    public bool CanUseVent() => CheckTaskProgress(CheckTasks.UseVent, UseVentTaskRate);

    public bool CanUseSabo() => CheckTaskProgress(CheckTasks.UseSabo, UseSaboTaskRate);

    public bool HasImpostorLight() => CheckTaskProgress(CheckTasks.ImpostorLight, ImpostorLightTaskRate);

    public bool CanCheckImpostor() => CheckTaskProgress(CheckTasks.CheckImpostor, CheckImpostorTaskRate);

    private void OnTaskComplete(TaskCompleteEventData data)
    {
        if (data.player != Player) return;

        CheckAllAbilities();
    }

    // todo
    private void OnTryKill(TryKillEventData data)
    {
        if (data.RefTarget != Player) return;

        if (CanKillGuard())
        {
            data.RefSuccess = false;
            _killGuardCount++;
            if (data.Killer.AmOwner)
                ExPlayerControl.LocalPlayer.ResetKillCooldown();

            if (Player.AmOwner)
            {
                // TODO: Add translation key
                HudManager.Instance.ShowPopUp(ModTranslation.GetString("SafecrackerKillGuardActivated"));
            }
        }
    }

    private void OnExile(ExileEventData data)
    {
        if (data.exiled != Player) return;

        if (CanExiledGuard())
        {
            Player.Player.Revive();
            _exiledGuardCount++;

            if (Player.AmOwner)
            {
                HudManager.Instance.ShowPopUp(ModTranslation.GetString("SafecrackerExiledGuardActivated"));
            }
        }
    }

    private void CheckAllAbilities()
    {
        CanKillGuard();
        CanExiledGuard();
        if (CanUseVent() && !Player.HasAbility<CustomVentAbility>())
        {
            Player.AttachAbility(new CustomVentAbility(() => true), new AbilityParentAbility(this));
        }
        if (CanUseSabo() && !Player.HasAbility<CustomSaboAbility>())
        {
            Player.AttachAbility(new CustomSaboAbility(() => true), new AbilityParentAbility(this));
        }

        HasImpostorLight();
        CanCheckImpostor();
    }
}