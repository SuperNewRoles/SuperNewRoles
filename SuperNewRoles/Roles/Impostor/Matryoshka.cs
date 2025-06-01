using System;
using System.Collections.Generic;
using SuperNewRoles.Patches;
using SuperNewRoles.Roles.Ability;
using SuperNewRoles.Modules;
using UnityEngine;
using AmongUs.GameOptions;
using Hazel;
using SuperNewRoles.CustomOptions;

namespace SuperNewRoles.Roles.Impostor;

class Matryoshka : RoleBase<Matryoshka>
{
    public override RoleId Role { get; } = RoleId.Matryoshka;
    public override Color32 RoleColor { get; } = Palette.ImpostorRed;
    public override List<Func<AbilityBase>> Abilities { get; } = [
        () => new MatryoshkaAbility(new(
            WearReport: MatryoshkaWearReport,
            WearLimit: MatryoshkaWearLimit,
            WearTime: MatryoshkaWearTime,
            AdditionalKillCoolTime: MatryoshkaAddKillCoolTime,
            CoolTime: MatryoshkaCoolTime
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

    [CustomOptionFloat("MatryoshkaCoolTime", 0f, 180f, 2.5f, 30f, translationName: "CoolTime")]
    public static float MatryoshkaCoolTime;
    [CustomOptionInt("MatryoshkaWearLimit", 1, 15, 1, 3)]
    public static int MatryoshkaWearLimit;
    [CustomOptionBool("MatryoshkaWearReport", false)]
    public static bool MatryoshkaWearReport;
    [CustomOptionFloat("MatryoshkaWearTime", 0.5f, 60f, 0.5f, 7.5f)]
    public static float MatryoshkaWearTime;
    [CustomOptionFloat("MatryoshkaAddKillCoolTime", 0f, 30f, 0.5f, 2.5f)]
    public static float MatryoshkaAddKillCoolTime;
}