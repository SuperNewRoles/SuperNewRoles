using System;
using System.Collections.Generic;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability;
using UnityEngine;

namespace SuperNewRoles.Roles.Crewmate;

internal class JumpDancer : RoleBase<JumpDancer>
{
    public override RoleId Role => RoleId.JumpDancer;
    public override Color32 RoleColor => new(175, 225, 214, byte.MaxValue);
    public override List<Func<AbilityBase>> Abilities => [() => new JumpDancerAbility(JumpDancerCoolTime)];
    public override QuoteMod QuoteMod => QuoteMod.SuperNewRoles;
    public override AssignedTeamType AssignedTeam => AssignedTeamType.Crewmate;
    public override WinnerTeamType WinnerTeam => WinnerTeamType.Crewmate;
    public override TeamTag TeamTag => TeamTag.Crewmate;
    public override RoleTag[] RoleTags => [];
    public override short IntroNum => 1;
    public override RoleTypes IntroSoundType => RoleTypes.Crewmate;
    public override RoleOptionMenuType OptionTeam => RoleOptionMenuType.Crewmate;

    // 保存されるオプション
    [CustomOptionFloat("JumpDancerCoolTime", 0f, 120f, 2.5f, 25f, translationName: "CoolTime")]
    public static float JumpDancerCoolTime = 25f;
}