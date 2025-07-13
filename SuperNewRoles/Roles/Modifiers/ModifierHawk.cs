using System;
using System.Collections.Generic;
using AmongUs.GameOptions;
using SuperNewRoles;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles;
using SuperNewRoles.Roles.Ability;
using UnityEngine;

class ModifierHawk : ModifierBase<ModifierHawk>
{
    public override ModifierRoleId ModifierRole => ModifierRoleId.ModifierHawk;
    public override Color32 RoleColor { get; } = new(200, 255, 200, byte.MaxValue); // 薄い緑色
    public override List<Func<AbilityBase>> Abilities { get; } = [
        () => new HawkAbility(
            ModifierHawkCoolTime,
            ModifierHawkDurationTime,
            ModifierHawkZoomMagnification,
            ModifierHawkCannotWalkInEffect
        )
    ];

    public override QuoteMod QuoteMod { get; } = QuoteMod.TheOtherRoles; // 元のModに合わせて
    public override short IntroNum { get; } = 1; // 標準的なクルーメイト

    public override List<AssignedTeamType> AssignedTeams { get; } = [];
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.None;
    public override RoleTag[] RoleTags { get; } = [];
    public override RoleId[] DoNotAssignRoles => [RoleId.NiceHawk, RoleId.MadHawk, RoleId.EvilHawk];

    public override bool AssignFilter => true;
    public override bool UseTeamSpecificAssignment => true;

    public override Func<ExPlayerControl, string> ModifierMark => (player) => "{0}" + ModHelpers.Cs(RoleColor, "<size=80%>Φ</size>");


    [CustomOptionFloat("ModifierHawkCoolTime", 0f, 120f, 2.5f, 30f, translationName: "CoolTime")]
    public static float ModifierHawkCoolTime;

    [CustomOptionFloat("ModifierHawkDurationTime", 0f, 60f, 0.5f, 10f, translationName: "DurationTime")]
    public static float ModifierHawkDurationTime;

    [CustomOptionFloat("ModifierHawkZoomMagnification", 1.25f, 10f, 0.25f, 2f, translationName: "HawkZoomMagnification")]
    public static float ModifierHawkZoomMagnification;

    [CustomOptionBool("ModifierHawkCannotWalkInEffect", true, translationName: "HawkCannotWalkInEffect")]
    public static bool ModifierHawkCannotWalkInEffect;
}