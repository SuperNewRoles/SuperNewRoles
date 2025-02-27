using System;
using System.Collections.Generic;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability;
using SuperNewRoles.WaveCannonObj;
using UnityEngine;

namespace SuperNewRoles.Roles.Neutral;

class WaveCannonJackal : RoleBase<WaveCannonJackal>
{
    public override RoleId Role { get; } = RoleId.WaveCannonJackal;
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
            sidekickType: JackalSidekickType
        )),
        () => new WaveCannonAbility(
            coolDown: WaveCannonCooldown,
            effectDuration: WaveCannonEffectDuration,
            type: WaveCannonType
        )
    ];

    public override QuoteMod QuoteMod { get; } = QuoteMod.SuperNewRoles;
    public override RoleTypes IntroSoundType { get; } = RoleTypes.Shapeshifter;
    public override short IntroNum { get; } = 1;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Neutral;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Neutral;
    public override TeamTag TeamTag { get; } = TeamTag.Jackal;
    public override RoleTag[] RoleTags { get; } = [RoleTag.SpecialKiller];
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Neutral;
    public override RoleId[] RelatedRoleIds { get; } = [RoleId.Sidekick, RoleId.JackalFriends];

    [CustomOptionFloat("WaveCannonJackalKillCooldown", 2.5f, 60f, 2.5f, 30f)]
    public static float JackalKillCooldown;

    [CustomOptionBool("WaveCannonJackalCanUseVent", true)]
    public static bool JackalCanUseVent;

    [CustomOptionBool("WaveCannonJackalImpostorVision", true)]
    public static bool JackalImpostorVision;

    [CustomOptionBool("WaveCannonJackalCanCreateSidekick", true)]
    public static bool JackalCanCreateSidekick;

    [CustomOptionFloat("WaveCannonJackalSidekickCooldown", 2.5f, 60f, 2.5f, 30f, parentFieldName: nameof(JackalCanCreateSidekick))]
    public static float JackalSidekickCooldown;

    [CustomOptionBool("WaveCannonJackalInfiniteJackal", true)]
    public static bool JackalInfiniteJackal;

    [CustomOptionSelect("WaveCannonJackalSidekickType", typeof(JackalSidekickType), "JackalSidekickType.", parentFieldName: nameof(JackalCanCreateSidekick))]
    public static JackalSidekickType JackalSidekickType;

    [CustomOptionFloat("WaveCannonJackalWaveCannonCooldown", 10f, 60f, 5f, 30f)]
    public static float WaveCannonCooldown;

    [CustomOptionFloat("WaveCannonJackalWaveCannonEffectDuration", 1f, 10f, 1f, 6f)]
    public static float WaveCannonEffectDuration;

    [CustomOptionSelect("WaveCannonJackalWaveCannonType", typeof(WaveCannonTypeForOption), "WaveCannonAnimationType.")]
    public static WaveCannonType WaveCannonType = WaveCannonType.Tank;
}