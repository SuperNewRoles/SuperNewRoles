using System;
using System.Collections.Generic;
using UnityEngine;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Roles.Ability;
using SuperNewRoles.Modules;

namespace SuperNewRoles.Roles.Crewmate;

class NiceMechanic : RoleBase<NiceMechanic>
{
    public override RoleId Role { get; } = RoleId.NiceMechanic;
    public override Color32 RoleColor { get; } = new(82, 108, 173, byte.MaxValue);
    public override List<Func<AbilityBase>> Abilities { get; } = [() => new MechanicAbility(
        coolTime: NiceMechanicCoolTime,
        durationTime: NiceMechanicDurationTime,
        sprite: "MechanicButton_Nice.png"),
        () => new CustomVentAbility(() => NiceMechanicUseVent)];

    public override QuoteMod QuoteMod { get; } = QuoteMod.SuperNewRoles;
    public override RoleTypes IntroSoundType { get; } = RoleTypes.Crewmate;
    public override short IntroNum { get; } = 1;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Crewmate;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Crewmate;
    public override TeamTag TeamTag { get; } = TeamTag.Crewmate;
    public override RoleTag[] RoleTags { get; } = [];
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Crewmate;

    [CustomOptionFloat("NiceMechanicCoolTime", 2.5f, 60f, 2.5f, 30f)]
    public static float NiceMechanicCoolTime;

    [CustomOptionFloat("NiceMechanicDurationTime", 2.5f, 30f, 2.5f, 10f)]
    public static float NiceMechanicDurationTime;

    [CustomOptionBool("NiceMechanicUseVent", true)]
    public static bool NiceMechanicUseVent;
}