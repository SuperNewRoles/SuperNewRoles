using System;
using System.Collections.Generic;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability;
using UnityEngine;

namespace SuperNewRoles.Roles.Neutral;

class BlackHatHacker : RoleBase<BlackHatHacker>
{
    public override RoleId Role { get; } = RoleId.BlackHatHacker;
    public override Color32 RoleColor { get; } = new(29, 255, 166, byte.MaxValue);
    public override List<Func<AbilityBase>> Abilities { get; } = [
        () => new BlackHatHackerAbility(new BlackHatHackerData(
            hackCooldown: BlackHatHackerHackCoolTime,
            hackCount: BlackHatHackerHackCountable,
            isNotInfectionIncrease: BlackHatHackerIsNotInfectionIncrease,
            hackInfectiousTime: BlackHatHackerHackInfectiousTime,
            notInfectiousTime: BlackHatHackerNotInfectiousTime,
            startSelfPropagation: BlackHatHackerStartSelfPropagation,
            infectionScope: BlackHatHackerInfectionScope,
            canInfectedAdmin: BlackHatHackerCanInfectedAdmin,
            canMoveWhenUsesAdmin: BlackHatHackerCanMoveWhenUsesAdmin,
            isAdminColor: BlackHatHackerIsAdminColor,
            canInfectedVitals: BlackHatHackerCanInfectedVitals,
            canDeadWin: BlackHatHackerCanDeadWin
        ))
    ];

    public override QuoteMod QuoteMod { get; } = QuoteMod.SuperNewRoles;
    public override RoleTypes IntroSoundType { get; } = RoleTypes.Crewmate;
    public override short IntroNum { get; } = 1;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Neutral;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Neutral;
    public override TeamTag TeamTag { get; } = TeamTag.Neutral;
    public override RoleTag[] RoleTags { get; } = [];
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Neutral;

    [CustomOptionFloat("BlackHatHackerHackCoolTime", 0f, 60f, 2.5f, 15f)]
    public static float BlackHatHackerHackCoolTime;

    [CustomOptionInt("BlackHatHackerHackCountable", 1, 15, 1, 1)]
    public static int BlackHatHackerHackCountable;

    [CustomOptionBool("BlackHatHackerIsNotInfectionIncrease", true)]
    public static bool BlackHatHackerIsNotInfectionIncrease;

    [CustomOptionFloat("BlackHatHackerHackInfectiousTime", 2.5f, 60f, 2.5f, 10f)]
    public static float BlackHatHackerHackInfectiousTime;

    [CustomOptionFloat("BlackHatHackerNotInfectiousTime", 0f, 30f, 2.5f, 12.5f)]
    public static float BlackHatHackerNotInfectiousTime;

    [CustomOptionSelect("BlackHatHackerStartSelfPropagation", typeof(BlackHatHackerSelfPropagationType), "BlackHatHackerStartSelfPropagationType.")]
    public static BlackHatHackerSelfPropagationType BlackHatHackerStartSelfPropagation;

    [CustomOptionSelect("BlackHatHackerInfectionScope", typeof(BlackHatHackerInfectionScopeType), "BlackHatHackerInfectionScopeType.")]
    public static BlackHatHackerInfectionScopeType BlackHatHackerInfectionScope;

    [CustomOptionBool("BlackHatHackerCanInfectedAdmin", false)]
    public static bool BlackHatHackerCanInfectedAdmin;

    [CustomOptionBool("BlackHatHackerCanMoveWhenUsesAdmin", false, parentFieldName: nameof(BlackHatHackerCanInfectedAdmin))]
    public static bool BlackHatHackerCanMoveWhenUsesAdmin;

    [CustomOptionBool("BlackHatHackerIsAdminColor", false, parentFieldName: nameof(BlackHatHackerCanInfectedAdmin))]
    public static bool BlackHatHackerIsAdminColor;

    [CustomOptionBool("BlackHatHackerCanInfectedVitals", false)]
    public static bool BlackHatHackerCanInfectedVitals;

    [CustomOptionBool("BlackHatHackerCanDeadWin", true)]
    public static bool BlackHatHackerCanDeadWin;
}

public enum BlackHatHackerSelfPropagationType
{
    Percent25 = 25,
    Percent50 = 50,
    Percent75 = 75,
    Percent90 = 90,
}

public enum BlackHatHackerInfectionScopeType
{
    Short = 0,
    Medium = 1,
    Long = 2,
}