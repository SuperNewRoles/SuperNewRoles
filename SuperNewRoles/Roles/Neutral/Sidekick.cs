using System;
using System.Collections.Generic;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability;
using UnityEngine;

namespace SuperNewRoles.Roles.Neutral;

class Sidekick : RoleBase<Sidekick>
{
    public override RoleId Role { get; } = RoleId.Sidekick;
    public override Color32 RoleColor { get; } = Jackal.Instance.RoleColor;
    public override List<Func<AbilityBase>> Abilities { get; } = [
        () => new SidekickAbility(
            canUseVent: Jackal.JackalCanUseVent
        )
    ];

    public override QuoteMod QuoteMod { get; } = QuoteMod.SuperNewRoles;
    public override RoleTypes IntroSoundType { get; } = RoleTypes.Crewmate;
    public override short IntroNum { get; } = 1;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Neutral;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Neutral;
    public override TeamTag TeamTag { get; } = TeamTag.Neutral;
    public override RoleTag[] RoleTags { get; } = [];
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Hidden;
}

public class SidekickAbility : AbilityBase
{
    public CustomVentAbility VentAbility { get; private set; }
    public bool CanUseVent { get; }
    public KnowOtherAbility KnowJackalAbility { get; private set; }
    public SidekickAbility(bool canUseVent)
    {
        CanUseVent = canUseVent;
    }

    public override void Attach(PlayerControl player, ulong abilityId, AbilityParentBase parent)
    {
        VentAbility = new CustomVentAbility(
            () => CanUseVent
        );
        KnowJackalAbility = new KnowOtherAbility(
            (player) => player.IsJackalTeam(),
            () => true
        );

        ExPlayerControl exPlayer = (ExPlayerControl)player;

        AbilityParentAbility parentAbility = new(this);
        exPlayer.AttachAbility(VentAbility, parentAbility);
        exPlayer.AttachAbility(KnowJackalAbility, parentAbility);
        base.Attach(player, abilityId, parent);
    }

    public override void AttachToLocalPlayer()
    {
    }
}