using System;
using System.Collections.Generic;
using UnityEngine;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Roles.Ability;
using SuperNewRoles.Modules;
using AmongUs.GameOptions;

namespace SuperNewRoles.Roles.Crewmate;

class WiseMan : RoleBase<WiseMan>
{
    public override RoleId Role => RoleId.WiseMan;
    public override Color32 RoleColor { get; } = new(85, 180, 236, byte.MaxValue); // 青色

    public override List<Func<AbilityBase>> Abilities { get; } = [
        () => new WiseManAbility(
            WiseManCoolTime,
            WiseManDurationTime,
            WiseManEnableWiseWalk
        )
    ];

    public override QuoteMod QuoteMod { get; } = QuoteMod.SuperNewRoles;
    public override RoleTypes IntroSoundType { get; } = RoleTypes.Crewmate;
    public override short IntroNum { get; } = 1;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Crewmate;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Crewmate;
    public override TeamTag TeamTag { get; } = TeamTag.Crewmate;
    public override RoleTag[] RoleTags { get; } = [RoleTag.Information];
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Crewmate;

    [CustomOptionFloat("WiseManCoolTime", 2.5f, 60f, 2.5f, 30f, translationName: "CoolTime")]
    public static float WiseManCoolTime;

    [CustomOptionFloat("WiseManDurationTime", 2.5f, 30f, 2.5f, 10f, translationName: "DurationTime")]
    public static float WiseManDurationTime;

    [CustomOptionBool("WiseManEnableWiseManWalk", false)]
    public static bool WiseManEnableWiseWalk;
}