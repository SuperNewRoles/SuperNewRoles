using System;
using System.Collections.Generic;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability;
using UnityEngine;

namespace SuperNewRoles.Roles.Impostor;

public enum TriggerHappyBulletSpreadAngleOption
{
    Angle15 = 15,
    Angle30 = 30,
    Angle45 = 45,
    Angle60 = 60,
}

class TriggerHappy : RoleBase<TriggerHappy>
{
    public override RoleId Role { get; } = RoleId.TriggerHappy;
    public override Color32 RoleColor { get; } = Palette.ImpostorRed;
    public override List<Func<AbilityBase>> Abilities { get; } = [
        () => new TriggerHappyAbility(
            new TriggerHappyData(
                useLimit: TriggerHappyUseLimit,
                cooldown: TriggerHappyCooldown,
                syncKillCoolTime: TriggerHappySyncKillCoolTime,
                duration: TriggerHappyDuration,
                requiredHits: TriggerHappyRequiredHits,
                range: TriggerHappyRange,
                bulletSize: TriggerHappyBulletSize,
                pierceWalls: TriggerHappyPierceWalls,
                bulletSpreadAngleOption: TriggerHappyBulletSpreadAngleOption
            )
        )
    ];

    public override QuoteMod QuoteMod { get; } = QuoteMod.SuperNewRoles;
    public override RoleTypes IntroSoundType { get; } = RoleTypes.Shapeshifter;
    public override short IntroNum { get; } = 1;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Impostor;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Impostor;
    public override TeamTag TeamTag { get; } = TeamTag.Impostor;
    public override RoleTag[] RoleTags { get; } = [RoleTag.SpecialKiller];
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Impostor;

    // オプション設定
    [CustomOptionInt("TriggerHappyUseLimit", 1, 10, 1, 1)]
    public static int TriggerHappyUseLimit;

    [CustomOptionFloat("TriggerHappyCooldown", 2.5f, 60f, 2.5f, 30f)]
    public static float TriggerHappyCooldown;

    [CustomOptionBool("TriggerHappySyncKillCoolTime", false)]
    public static bool TriggerHappySyncKillCoolTime;

    [CustomOptionFloat("TriggerHappyDuration", 1f, 15f, 1f, 5f)]
    public static float TriggerHappyDuration;

    [CustomOptionInt("TriggerHappyRequiredHits", 1, 20, 1, 2)]
    public static int TriggerHappyRequiredHits;

    [CustomOptionFloat("TriggerHappyRange", 1f, 30f, 1f, 5f)]
    public static float TriggerHappyRange;

    [CustomOptionFloat("TriggerHappyBulletSize", 0.25f, 3f, 0.25f, 1f)]
    public static float TriggerHappyBulletSize;

    [CustomOptionBool("TriggerHappyPierceWalls", false)]
    public static bool TriggerHappyPierceWalls;

    [CustomOptionSelect("TriggerHappyBulletSpreadAngle", typeof(TriggerHappyBulletSpreadAngleOption), "TriggerHappyBulletSpread.", defaultValue: TriggerHappyBulletSpreadAngleOption.Angle45)]
    public static TriggerHappyBulletSpreadAngleOption TriggerHappyBulletSpreadAngleOption;
}
