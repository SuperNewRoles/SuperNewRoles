using System;
using System.Collections.Generic;
using UnityEngine;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Roles.Ability;
using SuperNewRoles.Modules;

namespace SuperNewRoles.Roles.Crewmate;

class Busker : RoleBase<Busker>
{
    public override RoleId Role { get; } = RoleId.Busker;
    public override Color32 RoleColor { get; } = new(255, 172, 117, byte.MaxValue); // オレンジ色
    public override List<Func<AbilityBase>> Abilities { get; } = [() => new BuskerPseudocideAbility(BuskerCoolTime, BuskerPseudocideDuration)];

    public override QuoteMod QuoteMod { get; } = QuoteMod.SuperNewRoles;
    public override RoleTypes IntroSoundType { get; } = RoleTypes.Crewmate;
    public override short IntroNum { get; } = 1;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Crewmate;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Crewmate;
    public override TeamTag TeamTag { get; } = TeamTag.Crewmate;
    public override RoleTag[] RoleTags { get; } = [];
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Crewmate;

    [CustomOptionFloat("BuskerCoolTime", 0f, 60f, 2.5f, 20f, translationName: "CoolTime")]
    public static float BuskerCoolTime;

    [CustomOptionFloat("BuskerPseudocideDurationOption", 5f, 60f, 5f, 30f, translationName: "BuskerPseudocideDurationOption")]
    public static float BuskerPseudocideDuration;
}