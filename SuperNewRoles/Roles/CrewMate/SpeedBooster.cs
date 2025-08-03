using System;
using System.Collections.Generic;
using UnityEngine;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles;
using SuperNewRoles.Roles.Ability;
using SuperNewRoles.Roles.Ability.CustomButton;
using SuperNewRoles.Modules.Events;
using SuperNewRoles.Modules.Events.Bases;

namespace SuperNewRoles.Roles.Crewmate;
class SpeedBooster : RoleBase<SpeedBooster>
{
    public override RoleId Role => RoleId.SpeedBooster;
    public override Color32 RoleColor => Palette.CrewmateBlue;
    public override List<Func<AbilityBase>> Abilities => new() { () => new SpeedBoosterAbility(SpeedBoosterCooldown, SpeedBoosterDuration, SpeedBoosterMultiplier) };

    public override QuoteMod QuoteMod => QuoteMod.SuperNewRoles;
    public override RoleTypes IntroSoundType => RoleTypes.Crewmate;
    public override short IntroNum => 1;

    public override AssignedTeamType AssignedTeam => AssignedTeamType.Crewmate;
    public override WinnerTeamType WinnerTeam => WinnerTeamType.Crewmate;
    public override TeamTag TeamTag => TeamTag.Crewmate;
    public override RoleTag[] RoleTags => Array.Empty<RoleTag>();
    public override RoleOptionMenuType OptionTeam => RoleOptionMenuType.Crewmate;

    [CustomOptionFloat("SpeedBoosterCooldown", 0f, 180f, 2.5f, 30f, translationName: "CoolTime")]
    public static float SpeedBoosterCooldown;
    [CustomOptionFloat("SpeedBoosterDuration", 0.5f, 30f, 0.5f, 5f, translationName: "DurationTime")]
    public static float SpeedBoosterDuration;
    [CustomOptionFloat("SpeedBoosterMultiplier", 1f, 5f, 0.1f, 2f, translationName: "SpeedBoosterMultiplier")]
    public static float SpeedBoosterMultiplier;
}