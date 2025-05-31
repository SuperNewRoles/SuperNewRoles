using System;
using System.Collections.Generic;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles;
using SuperNewRoles.Roles.Ability;
using UnityEngine;

class EvilHawk : RoleBase<EvilHawk>
{
    public override RoleId Role => RoleId.EvilHawk;
    public override Color32 RoleColor { get; } = Palette.ImpostorRed;
    public override List<Func<AbilityBase>> Abilities { get; } = [
        () => new HawkAbility(
            EvilHawkCoolTime,
            EvilHawkDurationTime,
            EvilHawkZoomMagnification,
            EvilHawkCannotWalkInEffect
        )
    ];

    public override QuoteMod QuoteMod { get; } = QuoteMod.TheOtherRoles; // 元のModに合わせて
    public override RoleTypes IntroSoundType { get; } = RoleTypes.Impostor;
    public override short IntroNum { get; } = 1; // 標準的なクルーメイト

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Impostor;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Impostor;
    public override TeamTag TeamTag { get; } = TeamTag.Impostor;
    public override RoleTag[] RoleTags { get; } = [];
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Impostor;

    [CustomOptionFloat("EvilHawkCoolTime", 0f, 120f, 2.5f, 30f, translationName: "CoolTime")]
    public static float EvilHawkCoolTime;

    [CustomOptionFloat("EvilHawkDurationTime", 0f, 60f, 0.5f, 10f, translationName: "DurationTime")]
    public static float EvilHawkDurationTime;

    [CustomOptionFloat("EvilHawkZoomMagnification", 1.25f, 10f, 0.25f, 2f, translationName: "HawkZoomMagnification")]
    public static float EvilHawkZoomMagnification;

    [CustomOptionBool("EvilHawkCannotWalkInEffect", true, translationName: "HawkCannotWalkInEffect")]
    public static bool EvilHawkCannotWalkInEffect;
}