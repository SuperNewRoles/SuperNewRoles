using System;
using System.Collections.Generic;
using UnityEngine;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Roles.Ability;
using SuperNewRoles.Modules;

namespace SuperNewRoles.Roles.Crewmate;

class Doorr : RoleBase<Doorr>
{
    public override RoleId Role { get; } = RoleId.Doorr;
    public override Color32 RoleColor { get; } = new(205, 133, 63, byte.MaxValue); // 茶色
    public override List<Func<AbilityBase>> Abilities { get; } = [() => new DoorrAbility(DoorrCoolTime)];

    public override QuoteMod QuoteMod { get; } = QuoteMod.SuperNewRoles;
    public override RoleTypes IntroSoundType { get; } = RoleTypes.Crewmate;
    public override short IntroNum { get; } = 1;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Crewmate;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Crewmate;
    public override TeamTag TeamTag { get; } = TeamTag.Crewmate;
    public override RoleTag[] RoleTags { get; } = [];
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Crewmate;

    [CustomOptionFloat("DoorrCoolTime", 0f, 60f, 2.5f, 10f, translationName: "CoolTime")]
    public static float DoorrCoolTime;
}