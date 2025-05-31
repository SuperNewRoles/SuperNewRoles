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
using SuperNewRoles.Roles.Crewmate;

namespace SuperNewRoles.Roles.Impostor;
internal class EvilSpeedBooster : RoleBase<EvilSpeedBooster>
{
    public override RoleId Role => RoleId.EvilSpeedBooster;
    public override Color32 RoleColor => Palette.ImpostorRed;
    public override List<Func<AbilityBase>> Abilities => new() { () => new SpeedBoosterAbility(EvilSpeedBoosterCooldown, EvilSpeedBoosterDuration, EvilSpeedBoosterMultiplier) };

    public override QuoteMod QuoteMod => QuoteMod.SuperNewRoles;
    public override RoleTypes IntroSoundType => RoleTypes.Impostor;
    public override short IntroNum => 1;

    public override AssignedTeamType AssignedTeam => AssignedTeamType.Impostor;
    public override WinnerTeamType WinnerTeam => WinnerTeamType.Impostor;
    public override TeamTag TeamTag => TeamTag.Impostor;
    public override RoleTag[] RoleTags => Array.Empty<RoleTag>();
    public override RoleOptionMenuType OptionTeam => RoleOptionMenuType.Impostor;

    [CustomOptionFloat("EvilSpeedBoosterCooldown", 0f, 180f, 2.5f, 30f, translationName: "CoolTime")]
    public static float EvilSpeedBoosterCooldown;
    [CustomOptionFloat("EvilSpeedBoosterDuration", 0.5f, 30f, 0.5f, 5f, translationName: "DurationTime")]
    public static float EvilSpeedBoosterDuration;
    [CustomOptionFloat("EvilSpeedBoosterMultiplier", 1f, 5f, 0.1f, 2f, translationName: "SpeedBoosterMultiplier")]
    public static float EvilSpeedBoosterMultiplier;
}