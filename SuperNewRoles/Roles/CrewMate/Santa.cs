using System;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability;
using UnityEngine;

namespace SuperNewRoles.Roles.Crewmate;

internal class Santa : RoleBase<Santa>
{
    public override RoleId Role => RoleId.Santa;
    public override Color32 RoleColor => new(255, 178, 178, byte.MaxValue);
    public override List<Func<AbilityBase>> Abilities => [() => new SantaAbility(
        new(
            InitialCount: SantaCanUseAbilityCount,
            Cooldown: SantaAbilityCooldown,
            TryLoversToDeath: SantaTryLoverToDeath,
            TryMadRolesToDeath: SantaTryMadmateToDeath,
            TryJackalFriendsToDeath: SantaTryJackalFriendsToDeath,
            ForImpostor: false,
            Roles: new List<(RoleId role, int percentage)>
            {
                // (RoleId.SpeedBooster, SantaSpeedBoosterPercentage),
                // (RoleId.Clergyman, SantaClergymanPercentage),
                (RoleId.NiceGuesser, SantaNiceGuesserPercentage),
                //(RoleId.Lighter, SantaLighterPercentage),
                (RoleId.Sheriff, SantaSheriffPercentage),
                (RoleId.Balancer, SantaBalancerPercentage),
                (RoleId.Celebrity, SantaCelebrityPercentage),
                (RoleId.HomeSecurityGuard, SantaHomeSecurityGuardPercentage),
            }
        ), "SantaButton.png"
    )];
    public override QuoteMod QuoteMod => QuoteMod.SuperNewRoles;
    public override AssignedTeamType AssignedTeam => AssignedTeamType.Crewmate;
    public override WinnerTeamType WinnerTeam => WinnerTeamType.Crewmate;
    public override TeamTag TeamTag => TeamTag.Crewmate;
    public override RoleTag[] RoleTags => [];
    public override short IntroNum => 1;
    public override RoleTypes IntroSoundType => RoleTypes.Crewmate;
    public override RoleOptionMenuType OptionTeam => RoleOptionMenuType.Crewmate;
    public override RoleId[] RelatedRoleIds => [RoleId.NiceGuesser, RoleId.Sheriff, RoleId.Balancer, RoleId.Celebrity, RoleId.HomeSecurityGuard];

    [CustomOptionFloat("SantaAbilityCooldown", 0f, 180f, 2.5f, 25f)]
    public static float SantaAbilityCooldown;
    [CustomOptionInt("SantaCanUseAbilityCount", 1, 15, 1, 1)]
    public static int SantaCanUseAbilityCount;
    [CustomOptionBool("SantaTryLoverToDeath", false)]
    public static bool SantaTryLoverToDeath;
    [CustomOptionBool("SantaTryMadmateToDeath", false)]
    public static bool SantaTryMadmateToDeath;
    [CustomOptionBool("SantaTryJackalFriendsToDeath", false)]
    public static bool SantaTryJackalFriendsToDeath;

    // ==============================
    // 役職たちの設定
    // ==============================

    /*[CustomOptionInt("SantaSpeedBoosterPercentage", 0, 100, 5, 0)]
    public static int SantaSpeedBoosterPercentage;
    [CustomOptionInt("SantaClergymanPercentage", 0, 100, 5, 0)]
    public static int SantaClergymanPercentage;*/
    [CustomOptionInt("SantaNiceGuesserPercentage", 0, 100, 5, 0)]
    public static int SantaNiceGuesserPercentage;
    /*[CustomOptionInt("SantaLighterPercentage", 0, 100, 5, 0)]
    public static int SantaLighterPercentage;*/
    [CustomOptionInt("SantaSheriffPercentage", 0, 100, 5, 0)]
    public static int SantaSheriffPercentage;
    [CustomOptionInt("SantaBalancerPercentage", 0, 100, 5, 0)]
    public static int SantaBalancerPercentage;
    [CustomOptionInt("SantaCelebrityPercentage", 0, 100, 5, 0)]
    public static int SantaCelebrityPercentage;
    [CustomOptionInt("SantaHomeSecurityGuardPercentage", 0, 100, 5, 0)]
    public static int SantaHomeSecurityGuardPercentage;
}