using System;
using System.Collections.Generic;
using UnityEngine;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Roles.Ability;
using SuperNewRoles.Modules;
using AmongUs.GameOptions;

namespace SuperNewRoles.Roles.Crewmate;

class NiceHawk : RoleBase<NiceHawk>
{
    public override RoleId Role => RoleId.NiceHawk;
    public override Color32 RoleColor { get; } = new(200, 255, 200, byte.MaxValue); // 薄い緑色
    public override List<Func<AbilityBase>> Abilities { get; } = [
        () => new HawkAbility(
            NiceHawkCoolTime,
            NiceHawkDurationTime,
            NiceHawkZoomMagnification,
            NiceHawkCannotWalkInEffect
        )
    ];

    public override QuoteMod QuoteMod { get; } = QuoteMod.TheOtherRoles; // 元のModに合わせて
    public override RoleTypes IntroSoundType { get; } = RoleTypes.Crewmate;
    public override short IntroNum { get; } = 1; // 標準的なクルーメイト

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Crewmate;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Crewmate;
    public override TeamTag TeamTag { get; } = TeamTag.Crewmate;
    public override RoleTag[] RoleTags { get; } = [];
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Crewmate;

    [CustomOptionFloat("NiceHawkCoolTime", 0f, 120f, 2.5f, 30f, translationName: "CoolTime")]
    public static float NiceHawkCoolTime;

    [CustomOptionFloat("NiceHawkDurationTime", 0f, 60f, 0.5f, 10f, translationName: "DurationTime")]
    public static float NiceHawkDurationTime;

    [CustomOptionFloat("NiceHawkZoomMagnification", 1.25f, 10f, 0.25f, 2f, translationName: "HawkZoomMagnification")]
    public static float NiceHawkZoomMagnification;

    [CustomOptionBool("NiceHawkCannotWalkInEffect", true, translationName: "HawkCannotWalkInEffect")]
    public static bool NiceHawkCannotWalkInEffect;
}