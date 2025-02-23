using System;
using UnityEngine;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability.CustomButton;

namespace SuperNewRoles.Roles.Ability;

public class MadmateAbility : AbilityBase
{
    public MadmateData MadData { get; private set; }
    public CustomVentAbility VentAbility { get; private set; }
    public KnowImpostorAbility KnowImpostorAbility { get; private set; }
    public ImpostorVisionAbility ImpostorVisionAbility { get; private set; }
    public MadmateAbility(MadmateData madData)
    {
        MadData = madData;
    }
    public override void Attach(PlayerControl player, ulong abilityId)
    {

        VentAbility = new CustomVentAbility(() => MadData.CouldUseVent);
        KnowImpostorAbility = new KnowImpostorAbility(MadData.CouldKnowImpostors);
        ImpostorVisionAbility = new ImpostorVisionAbility(() => MadData.HasImpostorVision);
        ExPlayerControl exPlayer = (ExPlayerControl)player;

        exPlayer.AttachAbility(VentAbility, IRoleBase.GenerateAbilityId(player.PlayerId, RoleId.Madmate, exPlayer.lastAbilityId++));
        exPlayer.AttachAbility(KnowImpostorAbility, IRoleBase.GenerateAbilityId(player.PlayerId, RoleId.Madmate, exPlayer.lastAbilityId++));
        exPlayer.AttachAbility(ImpostorVisionAbility, IRoleBase.GenerateAbilityId(player.PlayerId, RoleId.Madmate, exPlayer.lastAbilityId++));

        base.Attach(player, abilityId);
    }

    public override void AttachToLocalPlayer()
    {
    }
}
public class MadmateData
{
    public bool HasImpostorVision { get; }
    public bool CouldUseVent { get; }
    private bool _couldKnowImpostors;
    private int _taskNeeded;
    private bool _lastTaskChecked;
    public bool CouldKnowImpostors(ExPlayerControl exPlayer)
    {
        if (!_couldKnowImpostors) return false;
        if (_lastTaskChecked) return true;
        var (complete, all) = ModHelpers.TaskCompletedData(exPlayer.Data);
        if (complete == -1 || all == -1) return false;
        return _lastTaskChecked = complete >= _taskNeeded;
    }
    public MadmateData(bool hasImpostorVision, bool couldUseVent, bool couldKnowImpostors, int taskNeeded)
    {
        HasImpostorVision = hasImpostorVision;
        CouldUseVent = couldUseVent;
        _couldKnowImpostors = couldKnowImpostors;
        _taskNeeded = taskNeeded;
    }
}