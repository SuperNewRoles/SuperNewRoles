using System;
using System.Collections.Generic;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability;
using UnityEngine;

namespace SuperNewRoles.Roles.Neutral;

class Jackal : RoleBase<Jackal>
{
    public override RoleId Role { get; } = RoleId.Jackal;
    public override Color32 RoleColor { get; } = new(0, 180, 235, byte.MaxValue);
    public override List<Func<AbilityBase>> Abilities { get; } = [
        () => new JackalAbility(new JackalData(
            canKill: true,
            killCooldown: JackalKillCooldown,
            canUseVent: JackalCanUseVent,
            canCreateSidekick: JackalCanCreateSidekick,
            sidekickCooldown: JackalSidekickCooldown,
            isImpostorVision: JackalImpostorVision,
            isInfiniteJackal: JackalInfiniteJackal,
            sidekickType: (RoleId)JackalSidekickType
        ))
    ];

    public override QuoteMod QuoteMod { get; } = QuoteMod.TheOtherRoles;
    public override RoleTypes IntroSoundType { get; } = RoleTypes.Shapeshifter;
    public override short IntroNum { get; } = 1;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Neutral;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Neutral;
    public override TeamTag TeamTag { get; } = TeamTag.Jackal;
    public override RoleTag[] RoleTags { get; } = [RoleTag.SpecialKiller];
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Neutral;
    public override RoleId[] RelatedRoleIds { get; } = [RoleId.Sidekick, RoleId.JackalFriends, RoleId.SidekickWaveCannon];

    [CustomOptionFloat("JackalKillCooldown", 2.5f, 60f, 2.5f, 30f)]
    public static float JackalKillCooldown;

    [CustomOptionBool("JackalCanUseVent", true, translationName: "CanUseVent")]
    public static bool JackalCanUseVent;

    [CustomOptionBool("JackalImpostorVision", true, translationName: "HasImpostorVision")]
    public static bool JackalImpostorVision;

    [CustomOptionBool("JackalCanCreateSidekick", true)]
    public static bool JackalCanCreateSidekick;

    [CustomOptionFloat("JackalSidekickCooldown", 2.5f, 60f, 2.5f, 30f, parentFieldName: nameof(JackalCanCreateSidekick))]
    public static float JackalSidekickCooldown;

    [CustomOptionBool("JackalInfiniteJackal", true)]
    public static bool JackalInfiniteJackal;

    [CustomOptionSelect("JackalSidekickType", typeof(JackalSidekickType), "JackalSidekickType.", parentFieldName: nameof(JackalCanCreateSidekick))]
    public static JackalSidekickType JackalSidekickType;
}
public enum JackalSidekickType
{
    Sidekick = RoleId.Sidekick,
    Friends = RoleId.JackalFriends,
    SidekickWaveCannon = RoleId.SidekickWaveCannon,
}