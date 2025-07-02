using System;
using UnityEngine;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability.CustomButton;
using SuperNewRoles.CustomOptions.Categories;

namespace SuperNewRoles.Roles.Ability;

public class MadmateAbility : AbilityBase
{
    public MadmateData MadData { get; private set; }
    public CustomVentAbility VentAbility { get; private set; }
    public KnowImpostorAbility KnowImpostorAbility { get; private set; }
    public ImpostorVisionAbility ImpostorVisionAbility { get; private set; }
    public CustomTaskAbility CustomTaskAbility { get; private set; }
    public SabotageCanUseAbility SabotageCanUseAbility { get; private set; }
    public MadmateAbility(MadmateData madData)
    {
        MadData = madData;
    }
    public override void AttachToAlls()
    {
        VentAbility = new CustomVentAbility(() => MadData.CouldUseVent);
        KnowImpostorAbility = new KnowImpostorAbility(MadData.CouldKnowImpostors);
        ImpostorVisionAbility = new ImpostorVisionAbility(() => MadData.HasImpostorVision);
        CustomTaskAbility = new CustomTaskAbility(() => (true, false, MadData.TaskNeeded), MadData.SpecialTasks);
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
        if (!_couldKnowImpostors) return false;
        if (_lastTaskChecked) return true;
        var (complete, all) = ModHelpers.TaskCompletedData(ExPlayerControl.LocalPlayer.Data);
        if (complete == -1 || all == -1) return false;
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