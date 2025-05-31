using System;
using System.Collections.Generic;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability;
using UnityEngine;

namespace SuperNewRoles.Roles.Neutral;

class PartTimer : RoleBase<PartTimer>
{
    public override RoleId Role { get; } = RoleId.PartTimer;
    public override Color32 RoleColor { get; } = new(0, 255, 0, byte.MaxValue); // 緑色
    public override List<Func<AbilityBase>> Abilities { get; } = [
        () => new PartTimerAbility(new PartTimerData(
            deathTurn: PartTimerDeathTurn,
            initialCooldown: PartTimerInitialCooldown,
            finalCooldown: PartTimerFinalCooldown,
            canSeeTargetRole: PartTimerCanSeeTargetRole,
            needAliveToWin: PartTimerNeedAliveToWin
        ))
    ];

    public override QuoteMod QuoteMod { get; } = QuoteMod.SuperNewRoles;
    public override RoleTypes IntroSoundType { get; } = RoleTypes.Shapeshifter;
    public override short IntroNum { get; } = 1;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Neutral;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Neutral;
    public override TeamTag TeamTag { get; } = TeamTag.Neutral;
    public override RoleTag[] RoleTags { get; } = [];
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Neutral;

    // PartTimerの設定
    [CustomOptionInt("PartTimerDeathTurn", 0, 15, 1, 3)]
    public static int PartTimerDeathTurn;

    [CustomOptionFloat("PartTimerInitialCooldown", 2.5f, 60f, 2.5f, 20f)]
    public static float PartTimerInitialCooldown;

    [CustomOptionFloat("PartTimerFinalCooldown", 2.5f, 120f, 2.5f, 90f)]
    public static float PartTimerFinalCooldown;

    [CustomOptionBool("PartTimerCanSeeTargetRole", true)]
    public static bool PartTimerCanSeeTargetRole;

    [CustomOptionBool("PartTimerNeedAliveToWin", false)]
    public static bool PartTimerNeedAliveToWin;
}

public record PartTimerData(
    int deathTurn,
    float initialCooldown,
    float finalCooldown,
    bool canSeeTargetRole,
    bool needAliveToWin
);