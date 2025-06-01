using System;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability;
using UnityEngine;

namespace SuperNewRoles.Roles.Madmates;

class BlackSanta : RoleBase<BlackSanta>
{
    public override RoleId Role => RoleId.BlackSanta;
    public override Color32 RoleColor => Palette.ImpostorRed;
    public override List<Func<AbilityBase>> Abilities => [
        () => new MadmateAbility(new(BlackSantaHasImpostorVision, BlackSantaCouldUseVent, BlackSantaCanKnowImpostors, BlackSantaNeededTaskCount, BlackSantaIsSpecialTasks ? BlackSantaSpecialTasks : null)),
        () => new SantaAbility(
            new(
                InitialCount: BlackSantaCanUseAbilityCount,
                Cooldown: BlackSantaAbilityCooldown,
                TryLoversToDeath: BlackSantaTryLoverToDeath,
                TryMadRolesToDeath: true,
                TryJackalFriendsToDeath: true,
                ForImpostor: true,
                Roles: new List<(RoleId role, int percentage)>
                {
                    (RoleId.EvilGuesser, BlackSantaEvilGuesserPercentage),
                    // (RoleId.EvilMechanic, BlackSantaEvilMechanicPercentage),
                    (RoleId.SelfBomber, BlackSantaSelfBomberPercentage),
                    // (RoleId.Slugger, BlackSantaSluggerPercentage),
                    (RoleId.Penguin, BlackSantaPenguinPercentage),
                    (RoleId.WaveCannon, BlackSantaWaveCannonPercentage),
                }
            ), "BlackSantaButton.png"
        )
    ];
    public override QuoteMod QuoteMod => QuoteMod.SuperNewRoles;
    public override AssignedTeamType AssignedTeam => AssignedTeamType.Crewmate;
    public override WinnerTeamType WinnerTeam => WinnerTeamType.Impostor;
    public override TeamTag TeamTag => TeamTag.Madmate;
    public override RoleTag[] RoleTags => [];
    public override short IntroNum => 1;
    public override RoleTypes IntroSoundType => RoleTypes.Shapeshifter;
    public override RoleOptionMenuType OptionTeam => RoleOptionMenuType.Crewmate;
    public override RoleId[] RelatedRoleIds => [RoleId.EvilGuesser, RoleId.SelfBomber, RoleId.Penguin, RoleId.WaveCannon];
    // サンタ機能
    [CustomOptionFloat("BlackSantaAbilityCooldown", 0f, 180f, 2.5f, 25f, translationName: "SantaAbilityCooldown")]
    public static float BlackSantaAbilityCooldown;
    [CustomOptionInt("BlackSantaCanUseAbilityCount", 1, 15, 1, 1, translationName: "SantaCanUseAbilityCount")]
    public static int BlackSantaCanUseAbilityCount;
    [CustomOptionBool("BlackSantaTryLoverToDeath", false, translationName: "SantaTryLoverToDeath")]
    public static bool BlackSantaTryLoverToDeath;

    // 役職配布設定
    [CustomOptionInt("BlackSantaEvilGuesserPercentage", 0, 100, 5, 0)]
    public static int BlackSantaEvilGuesserPercentage;
    /*[CustomOptionInt("BlackSantaEvilMechanicPercentage", 0, 100, 5, 0)]
    public static int BlackSantaEvilMechanicPercentage;*/
    [CustomOptionInt("BlackSantaSelfBomberPercentage", 0, 100, 5, 0)]
    public static int BlackSantaSelfBomberPercentage;
    /*[CustomOptionInt("BlackSantaSluggerPercentage", 0, 100, 5, 0)]
    public static int BlackSantaSluggerPercentage;*/
    [CustomOptionInt("BlackSantaPenguinPercentage", 0, 100, 5, 0)]
    public static int BlackSantaPenguinPercentage;
    [CustomOptionInt("BlackSantaWaveCannonPercentage", 0, 100, 5, 0)]
    public static int BlackSantaWaveCannonPercentage;

    // マッドメイト機能
    [CustomOptionBool("BlackSantaCouldUseVent", true, translationName: "CanUseVent")]
    public static bool BlackSantaCouldUseVent;

    [CustomOptionBool("BlackSantaHasImpostorVision", true, translationName: "HasImpostorVision")]
    public static bool BlackSantaHasImpostorVision;

    [CustomOptionBool("BlackSantaCanKnowImpostors", true, translationName: "MadmateCanKnowImpostors")]
    public static bool BlackSantaCanKnowImpostors;

    // 狂信
    [CustomOptionInt("BlackSantaNeededTaskCount", 0, 10, 1, 1, translationName: "MadmateNeededTaskCount", parentFieldName: nameof(BlackSantaCanKnowImpostors))]
    public static int BlackSantaNeededTaskCount;

    [CustomOptionBool("BlackSantaIsSpecialTasks", false, translationName: "MadmateIsSpecialTasks")]
    public static bool BlackSantaIsSpecialTasks;
    [CustomOptionTask("BlackSantaSpecialTasks", 1, 1, 1, translationName: "MadmateSpecialTasks", parentFieldName: nameof(BlackSantaIsSpecialTasks))]
    public static TaskOptionData BlackSantaSpecialTasks;
}