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
            killCooldown: WaveCannonJackalKillCooldown,
            canUseVent: WaveCannonJackalCanUseVent,
            canCreateSidekick: WaveCannonJackalCanCreateSidekick,
            sidekickCooldown: WaveCannonJackalSidekickCooldown,
            isImpostorVision: WaveCannonJackalImpostorVision,
            isInfiniteJackal: WaveCannonJackalInfiniteJackal,
            sidekickType: (RoleId)WaveCannonJackalSidekickType
        )),
        () => new WaveCannonAbility(
            coolDown: WaveCannonJackalCooldown,
            effectDuration: WaveCannonJackalEffectDuration,
            type: WaveCannonJackalType,
            isResetKillCooldown: WaveCannonJackalIsSyncKillCoolTime
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
    public override RoleId[] RelatedRoleIds { get; } = [RoleId.Sidekick, RoleId.JackalFriends, RoleId.SidekickWaveCannon];

    [CustomOptionFloat("WaveCannonJackalKillCooldown", 2.5f, 60f, 2.5f, 30f)]
    public static float WaveCannonJackalKillCooldown;

    [CustomOptionBool("WaveCannonJackalCanUseVent", true, translationName: "CanUseVent")]
    public static bool WaveCannonJackalCanUseVent;

    [CustomOptionBool("WaveCannonJackalImpostorVision", true, translationName: "HasImpostorVision")]
    public static bool WaveCannonJackalImpostorVision;

    [CustomOptionBool("WaveCannonJackalCanCreateSidekick", true)]
    public static bool WaveCannonJackalCanCreateSidekick;

    [CustomOptionFloat("WaveCannonJackalSidekickCooldown", 2.5f, 60f, 2.5f, 30f, parentFieldName: nameof(WaveCannonJackalCanCreateSidekick))]
    public static float WaveCannonJackalSidekickCooldown;

    [CustomOptionBool("WaveCannonJackalInfiniteJackal", true)]
    public static bool WaveCannonJackalInfiniteJackal;

    [CustomOptionSelect("WaveCannonJackalSidekickType", typeof(WaveCannonJackalSidekickType), "JackalSidekickType.", parentFieldName: nameof(WaveCannonJackalCanCreateSidekick))]
    public static WaveCannonJackalSidekickType WaveCannonJackalSidekickType;

    [CustomOptionBool("WaveCannonJackalCreateBulletToJackal", true, parentFieldName: nameof(WaveCannonJackalSidekickType), parentActiveValue: WaveCannonJackalSidekickType.Bullet)]
    public static bool WaveCannonJackalCreateBulletToJackal;
    [CustomOptionBool("WaveCannonJackalNewJackalHaveWaveCannon", true, parentFieldName: nameof(WaveCannonJackalCreateBulletToJackal))]
    public static bool WaveCannonJackalNewJackalHaveWaveCannon;

    [CustomOptionFloat("WaveCannonJackalBulletLoadBulletCooltime", 0f, 180f, 2.5f, 20f, parentFieldName: nameof(WaveCannonJackalSidekickType), parentActiveValue: WaveCannonJackalSidekickType.Bullet)]
    public static float WaveCannonJackalBulletLoadBulletCooltime;
    [CustomOptionFloat("WaveCannonJackalBulletLoadedChargeTime", 0f, 15f, 0.5f, 5f, parentFieldName: nameof(WaveCannonJackalSidekickType), parentActiveValue: WaveCannonJackalSidekickType.Bullet)]
    public static float WaveCannonJackalBulletLoadedChargeTime;

    [CustomOptionBool("WaveCannonJackalBulletCanSeeParent", true, parentFieldName: nameof(WaveCannonJackalSidekickType), parentActiveValue: WaveCannonJackalSidekickType.Bullet)]
    public static bool WaveCannonJackalBulletCanSeeParent;

    [CustomOptionBool("WaveCannonJackalCanSeeBullet", true, parentFieldName: nameof(WaveCannonJackalSidekickType), parentActiveValue: WaveCannonJackalSidekickType.Bullet)]
    public static bool WaveCannonJackalCanSeeBullet;

    [CustomOptionFloat("WaveCannonJackalWaveCannonCooldown", 2.5f, 60f, 2.5f, 30f)]
    public static float WaveCannonJackalCooldown;

    [CustomOptionFloat("WaveCannonJackalWaveCannonEffectDuration", 1f, 10f, 1f, 6f)]
    public static float WaveCannonJackalEffectDuration;

    [CustomOptionBool("WaveCannonJackalSyncKillCoolTime", true, translationName: "WaveCannonSyncKillCoolTime")]
    public static bool WaveCannonJackalIsSyncKillCoolTime;

    [CustomOptionSelect("WaveCannonJackalWaveCannonType", typeof(WaveCannonTypeForOption), "WaveCannonAnimationType.")]
    public static WaveCannonType WaveCannonJackalType = WaveCannonType.Tank;
}
public enum WaveCannonJackalSidekickType
{
    Sidekick = RoleId.Sidekick,
    Friends = RoleId.JackalFriends,
    SidekickWaveCannon = RoleId.SidekickWaveCannon,
    Bullet = RoleId.Bullet,
}