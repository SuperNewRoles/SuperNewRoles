using System;
using System.Collections.Generic;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability;
using UnityEngine;
using UnityEngine.UI;

namespace SuperNewRoles.Roles.Neutral;

class Owl : RoleBase<Owl>
{
    public override RoleId Role { get; } = RoleId.Owl;
    public override Color32 RoleColor { get; } = new(169, 107, 46, byte.MaxValue);
    public override List<Func<AbilityBase>> Abilities { get; } = [
        () => new OwlAbility(new OwlData(
            OwlKillCooldown,
            OwlCanUseVent,
            OwlCanSpecialBlackoutDeadBodyCount,
            OwlSpecialBlackoutCool,
            OwlSpecialBlackoutTime,
            OwlCanKillOutsideOfBlackout,
            OwlCanTransportOutsideOfBlackout
        ))
    ];
    public override QuoteMod QuoteMod { get; } = QuoteMod.SuperNewRoles;
    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Neutral;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Neutral;
    public override TeamTag TeamTag { get; } = TeamTag.Neutral;
    public override RoleTag[] RoleTags { get; } = [RoleTag.SpecialKiller];
    public override short IntroNum { get; } = 1;
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Neutral;

    [CustomOptionFloat(nameof(OwlKillCooldown), 2.5f, 60f, 2.5f, 30f, translationName: "KillCoolTime")]
    public static float OwlKillCooldown;
    
    [CustomOptionBool(nameof(OwlCanUseVent), false, translationName: "CanUseVent")]
    public static bool OwlCanUseVent;

    [CustomOptionInt(nameof(OwlCanSpecialBlackoutDeadBodyCount), 0, 15, 1, 1)]
    public static int OwlCanSpecialBlackoutDeadBodyCount;

    [CustomOptionFloat(nameof(OwlSpecialBlackoutCool), 2.5f, 60f, 2.5f, 30f)]
    public static float OwlSpecialBlackoutCool;

    [CustomOptionFloat(nameof(OwlSpecialBlackoutTime), 2.5f, 60f, 2.5f, 10f)]
    public static float OwlSpecialBlackoutTime;

    [CustomOptionBool(nameof(OwlCanKillOutsideOfBlackout), true)]
    public static bool OwlCanKillOutsideOfBlackout;

    [CustomOptionBool(nameof(OwlCanTransportOutsideOfBlackout), true)]
    public static bool OwlCanTransportOutsideOfBlackout;
}
