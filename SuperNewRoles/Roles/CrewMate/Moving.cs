using System;
using System.Collections.Generic;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability;
using UnityEngine;

namespace SuperNewRoles.Roles.Crewmate;

internal class Moving : RoleBase<Moving>
{
    public override RoleId Role { get; } = RoleId.Moving;
    public override Color32 RoleColor { get; } = new(0, 255, 0, byte.MaxValue);
    public override List<Func<AbilityBase>> Abilities { get; } = [
        () => new MovingAbility(
            new MovingAbilityData(MovingCooldown)
        )
    ];

    public override QuoteMod QuoteMod { get; } = QuoteMod.SuperNewRoles;
    public override RoleTypes IntroSoundType { get; } = RoleTypes.Tracker;
    public override short IntroNum { get; } = 1;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Crewmate;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Crewmate;
    public override TeamTag TeamTag { get; } = TeamTag.Crewmate;
    public override RoleTag[] RoleTags { get; } = [RoleTag.Support];
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Crewmate;

    [CustomOptionFloat("MovingCooldown", 5f, 120f, 5f, 45f, translationName: "CoolTime")]
    public static float MovingCooldown;
}