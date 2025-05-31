using System;
using System.Collections.Generic;
using SuperNewRoles.Roles.Ability;
using SuperNewRoles.CustomOptions;
using UnityEngine;
using AmongUs.GameOptions;
using SuperNewRoles.Modules;

namespace SuperNewRoles.Roles.Impostor;

class Doppelganger : RoleBase<Doppelganger>
{
    public override RoleId Role { get; } = RoleId.Doppelganger;
    public override Color32 RoleColor { get; } = Palette.ImpostorRed;
    public override List<Func<AbilityBase>> Abilities { get; } = [
        () => new DoppelgangerAbility(new(
            durationTime: DoppelgangerDurationTime,
            coolTime: DoppelgangerCoolTime,
            sucCool: DoppelgangerSucCool,
            notSucCool: DoppelgangerNotSucCool
        ))
    ];

    public override QuoteMod QuoteMod { get; } = QuoteMod.SuperNewRoles;
    public override RoleTypes IntroSoundType { get; } = RoleTypes.Impostor;
    public override short IntroNum { get; } = 1;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Impostor;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Impostor;
    public override TeamTag TeamTag { get; } = TeamTag.Impostor;
    public override RoleTag[] RoleTags { get; } = [];
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Impostor;

    [CustomOptionFloat("DoppelgangerCoolTime", 2.5f, 180f, 2.5f, 30f)]
    public static float DoppelgangerCoolTime;
    [CustomOptionFloat("DoppelgangerDurationTime", 2.5f, 60f, 2.5f, 15f)]
    public static float DoppelgangerDurationTime;
    [CustomOptionFloat("DoppelgangerSucCool", 2.5f, 60f, 2.5f, 15f)]
    public static float DoppelgangerSucCool;
    [CustomOptionFloat("DoppelgangerNotSucCool", 2.5f, 60f, 2.5f, 45f)]
    public static float DoppelgangerNotSucCool;
}