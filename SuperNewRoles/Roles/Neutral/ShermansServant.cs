using System;
using System.Collections.Generic;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability;
using UnityEngine;

namespace SuperNewRoles.Roles.Neutral;

class ShermansServant : RoleBase<ShermansServant>
{
    public override RoleId Role { get; } = RoleId.ShermansServant;
    public override Color32 RoleColor { get; } = OrientalShaman.Instance.RoleColor;
    public override List<Func<AbilityBase>> Abilities { get; } = [
        () => new ShermansServantAbility(new ShermansServantData(
            transformCooldown: OrientalShaman.ShermansServantTransformCooldown,
            suicideCooldown: OrientalShaman.ShermansServantSuicideCooldown
        ))
    ];

    public override QuoteMod QuoteMod { get; } = QuoteMod.SuperNewRoles;
    public override RoleTypes IntroSoundType { get; } = RoleTypes.Crewmate;
    public override short IntroNum { get; } = 1;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Crewmate;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Neutral;
    public override TeamTag TeamTag { get; } = TeamTag.Neutral;
    public override RoleTag[] RoleTags { get; } = [];
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Hidden;
}

public record ShermansServantData(
    float transformCooldown,
    float suicideCooldown
);