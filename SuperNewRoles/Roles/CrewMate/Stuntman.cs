using System;
using System.Collections.Generic;
using UnityEngine;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Roles.Ability;
using SuperNewRoles.Modules;

namespace SuperNewRoles.Roles.Crewmate;

class Stuntman : RoleBase<Stuntman>
{
    public override RoleId Role { get; } = RoleId.Stuntman;
    public override Color32 RoleColor { get; } = new(173, 255, 47, byte.MaxValue); // 黄緑色
    public override List<Func<AbilityBase>> Abilities { get; } = [() => new StuntmanAbility(StuntmanAbilityCount)];

    public override QuoteMod QuoteMod { get; } = QuoteMod.SuperNewRoles;
    public override RoleTypes IntroSoundType { get; } = RoleTypes.Crewmate;
    public override short IntroNum { get; } = 1;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Crewmate;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Crewmate;
    public override TeamTag TeamTag { get; } = TeamTag.Crewmate;
    public override RoleTag[] RoleTags { get; } = [];
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Crewmate;

    [CustomOptionInt("StuntmanAbilityCount", 1, 10, 1, 1, translationName: "UseLimit")]
    public static int StuntmanAbilityCount;
}
