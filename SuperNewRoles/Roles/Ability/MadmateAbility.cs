using System;
using UnityEngine;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability.CustomButton;
using SuperNewRoles.CustomOptions.Categories;

namespace SuperNewRoles.Roles.Ability;

public class MadmateAbility : AbilityBase
{
    private readonly int _priority;
    public MadmateData MadData { get; private set; }
    public CustomVentAbility VentAbility { get; private set; }
    public KnowImpostorAbility KnowImpostorAbility { get; private set; }
    public ImpostorVisionAbility ImpostorVisionAbility { get; private set; }
    public CustomTaskAbility CustomTaskAbility { get; private set; }
    public SabotageCanUseAbility SabotageCanUseAbility { get; private set; }
    public MadmateAbility(MadmateData madData, int priority = AbilityPriority.Default)
    {
        MadData = madData;
        _priority = priority;
    }
    public override void AttachToAlls()
    {
        MadData.ResetTaskCheck();

        VentAbility = new CustomVentAbility(() => MadData.CouldUseVent, priority: _priority);
        KnowImpostorAbility = new KnowImpostorAbility(MadData.CouldKnowImpostors);
        ImpostorVisionAbility = new ImpostorVisionAbility(() => MadData.HasImpostorVision, priority: _priority);
        CustomTaskAbility = new CustomTaskAbility(
            isTaskTrigger: () => true,
            countsForCrewWin: () => false,
            requiredTaskCount: () => MadData.TaskNeeded,
            taskOptions: () => MadData.SpecialTasks,
            priority: _priority);
        SabotageCanUseAbility = new SabotageCanUseAbility(() => getCannotSabotageType());

        Player.AttachAbility(VentAbility, new AbilityParentAbility(this));
        Player.AttachAbility(KnowImpostorAbility, new AbilityParentAbility(this));
        Player.AttachAbility(ImpostorVisionAbility, new AbilityParentAbility(this));
        Player.AttachAbility(CustomTaskAbility, new AbilityParentAbility(this));
        Player.AttachAbility(SabotageCanUseAbility, new AbilityParentAbility(this));
    }

    private SabotageType getCannotSabotageType()
    {
        SabotageType type = SabotageType.None;
        if (MadmateOptions.MadmateCannotFixComms) type |= SabotageType.Comms;
        if (MadmateOptions.MadmateCannotFixElectrical) type |= SabotageType.Lights;
        if (MadmateOptions.MadmateCannotFixReactor) type |= SabotageType.Reactor | SabotageType.O2;
        return type;
    }
}
public class MadmateData
{
    public bool HasImpostorVision { get; }
    public bool CouldUseVent { get; }
    public TaskOptionData SpecialTasks { get; }
    public int TaskNeeded { get; }

    private bool _couldKnowImpostors;
    private bool _lastTaskChecked;
    public bool CouldKnowImpostors()
    {
        var (complete, all) = ModHelpers.TaskCompletedData(ExPlayerControl.LocalPlayer.Data);
        return CouldKnowImpostors(complete, all);
    }

    public void ResetTaskCheck()
    {
        _lastTaskChecked = false;
    }

    internal bool CouldKnowImpostors(int complete, int all)
    {
        if (!_couldKnowImpostors) return false;
        if (_lastTaskChecked) return true;
        if (complete == -1 || all == -1) return false;
        if (all <= 0) return _lastTaskChecked = TaskNeeded <= 0;
        return _lastTaskChecked = complete >= Math.Min(TaskNeeded, all);
    }
    public MadmateData(bool hasImpostorVision, bool couldUseVent, bool couldKnowImpostors, int taskNeeded, TaskOptionData specialTasks)
    {
        HasImpostorVision = hasImpostorVision;
        CouldUseVent = couldUseVent;
        _couldKnowImpostors = couldKnowImpostors;
        TaskNeeded = taskNeeded;
        SpecialTasks = specialTasks;
    }
}
