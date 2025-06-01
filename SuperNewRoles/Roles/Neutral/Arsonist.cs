using System;
using System.Collections.Generic;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability;
using UnityEngine;

namespace SuperNewRoles.Roles.Neutral;

class Arsonist : RoleBase<Arsonist>
{
    public override RoleId Role { get; } = RoleId.Arsonist;
    public override Color32 RoleColor { get; } = new Color32(255, 102, 0, 255); // オレンジ
    public override List<Func<AbilityBase>> Abilities { get; } = [
        () => new ArsonistAbility(new ArsonistData(
            douseCooldown: ArsonistDouseCooldown,
            douseDuration: ArsonistDouseDuration,
            canUseVent: ArsonistCanUseVent,
            isImpostorVision: ArsonistImpostorVision
        ))
    ];

    public override QuoteMod QuoteMod { get; } = QuoteMod.TheOtherRoles;
    public override RoleTypes IntroSoundType { get; } = RoleTypes.Shapeshifter;
    public override short IntroNum { get; } = 1;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Neutral;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Neutral;
    public override TeamTag TeamTag { get; } = TeamTag.Neutral;
    public override RoleTag[] RoleTags { get; } = [RoleTag.SpecialKiller];
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Neutral;

    [CustomOptionFloat("ArsonistDouseCooldown", 2.5f, 60f, 2.5f, 10f, translationName: "CoolTime")]
    public static float ArsonistDouseCooldown;

    [CustomOptionFloat("ArsonistDouseDuration", 0.5f, 10f, 0.5f, 1f)]
    public static float ArsonistDouseDuration;

    [CustomOptionBool("ArsonistCanUseVent", true, translationName: "CanUseVent")]
    public static bool ArsonistCanUseVent;

    [CustomOptionBool("ArsonistImpostorVision", true, translationName: "HasImpostorVision")]
    public static bool ArsonistImpostorVision;
}