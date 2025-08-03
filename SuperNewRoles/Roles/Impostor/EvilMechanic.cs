using System;
using System.Collections.Generic;
using UnityEngine;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Roles.Ability;
using SuperNewRoles.Modules;

namespace SuperNewRoles.Roles.Impostor;

class EvilMechanic : RoleBase<EvilMechanic>
{
    public override RoleId Role { get; } = RoleId.EvilMechanic;
    public override Color32 RoleColor { get; } = Palette.ImpostorRed;
    public override List<Func<AbilityBase>> Abilities { get; } = [() => new MechanicAbility(
        coolTime: EvilMechanicCoolTime,
        durationTime: EvilMechanicDurationTime,
        sprite: "MechanicButton_Evil.png")]; // インポスターなので常にベントが使える

    public override QuoteMod QuoteMod { get; } = QuoteMod.SuperNewRoles;
    public override RoleTypes IntroSoundType { get; } = RoleTypes.Shapeshifter;
    public override short IntroNum { get; } = 1;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Impostor;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Impostor;
    public override TeamTag TeamTag { get; } = TeamTag.Impostor;
    public override RoleTag[] RoleTags { get; } = [];
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Impostor;

    [CustomOptionFloat("EvilMechanicCoolTime", 2.5f, 60f, 2.5f, 30f)]
    public static float EvilMechanicCoolTime;

    [CustomOptionFloat("EvilMechanicDurationTime", 2.5f, 30f, 2.5f, 10f)]
    public static float EvilMechanicDurationTime;
}