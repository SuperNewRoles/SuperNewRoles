using System;
using System.Collections.Generic;
using UnityEngine;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Roles.Ability;
using SuperNewRoles.Modules;
using SuperNewRoles.Events;
using SuperNewRoles.Modules.Events.Bases;
using SuperNewRoles.SuperTrophies;
using SuperNewRoles.Events.PCEvents;

namespace SuperNewRoles.Roles.CrewMate;

class BestFalseCharge : RoleBase<BestFalseCharge>
{
    public override RoleId Role { get; } = RoleId.BestFalseCharge;
    public override Color32 RoleColor { get; } = Color.white;
    public override List<Func<AbilityBase>> Abilities { get; } = [() => new AutoExileAfterMeeting()];

    public override QuoteMod QuoteMod { get; } = QuoteMod.SuperNewRoles;
    public override RoleTypes IntroSoundType { get; } = RoleTypes.Crewmate;
    public override short IntroNum { get; } = 1;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Crewmate;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Crewmate;
    public override TeamTag TeamTag { get; } = TeamTag.Crewmate;
    public override RoleTag[] RoleTags { get; } = [];
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Crewmate;
}

public class AutoExileAfterMeeting : AbilityBase
{
    public EventListener<WrapUpEventData> wrapUpEventListener;
    public override void AttachToLocalPlayer()
    {
        wrapUpEventListener = WrapUpEvent.Instance.AddListener(OnWrapUp);
    }
    private void OnWrapUp(WrapUpEventData data)
    {
        if (ExPlayerControl.LocalPlayer.IsDead())
            return;
        ExPlayerControl.LocalPlayer.RpcCustomDeath(CustomDeathType.FalseCharge);
    }
    public override void DetachToLocalPlayer()
    {
        base.DetachToLocalPlayer();
        if (wrapUpEventListener != null)
        {
            WrapUpEvent.Instance.RemoveListener(wrapUpEventListener);
            wrapUpEventListener = null;
        }
    }
}

public class BestFalseChargeNotExiledGameEndTrophy : SuperTrophyRole<BestFalseChargeNotExiledGameEndTrophy>
{
    public override TrophiesEnum TrophyId => TrophiesEnum.BestFalseChargeNotExiledGameEnd;

    public override TrophyRank TrophyRank => TrophyRank.Gold;

    public override RoleId[] TargetRoles => [RoleId.BestFalseCharge];

    private EventListener<DieEventData> dieEventListener;

    public override void OnRegister()
    {
        Complete();
        dieEventListener = DieEvent.Instance.AddListener(OnDie);
    }
    private void OnDie(DieEventData data)
    {
        if (data.player != ExPlayerControl.LocalPlayer)
            return;
        InComplete();
    }

    public override void OnDetached()
    {
        // 役職変わってもいいから問題なし
    }
}
