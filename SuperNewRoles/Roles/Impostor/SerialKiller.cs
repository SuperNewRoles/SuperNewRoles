using System;
using System.Collections.Generic;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability;
using UnityEngine;

namespace SuperNewRoles.Roles.Impostor;

class SerialKiller : RoleBase<SerialKiller>
{
    public override RoleId Role { get; } = RoleId.SerialKiller;
    public override Color32 RoleColor { get; } = Palette.ImpostorRed;
    public override List<Func<AbilityBase>> Abilities { get; } = [
        () => new ChangeKillTimerAbility(
            killTimerGetter: () => SerialKillerKillCooldown
        ),
        () => new SuicideTimerAbility(
            suicideTimeGetter: () => SerialKillerSuicideTime,
            resetOnMeetingGetter: () => SerialKillerResetOnMeeting
        ),
        () => new CustomSaboAbility(
            canSabotage: () => SerialKillerCanSabotage
        ),
        () => new CustomVentAbility(
            canUseVent: () => SerialKillerCanUseVent
        )
    ];

    public override QuoteMod QuoteMod { get; } = QuoteMod.TheOtherRoles;
    public override RoleTypes IntroSoundType { get; } = RoleTypes.Impostor;
    public override short IntroNum { get; } = 1;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Impostor;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Impostor;
    public override TeamTag TeamTag { get; } = TeamTag.Impostor;
    public override RoleTag[] RoleTags { get; } = [RoleTag.SpecialKiller];
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Impostor;

    [CustomOptionBool("SerialKillerResetOnMeeting", false)]
    public static bool SerialKillerResetOnMeeting;

    [CustomOptionFloat("SerialKillerKillCooldown", 2.5f, 120f, 2.5f, 20f)]
    public static float SerialKillerKillCooldown;

    [CustomOptionFloat("SerialKillerSuicideTime", 2.5f, 120f, 2.5f, 40f)]
    public static float SerialKillerSuicideTime;

    [CustomOptionBool("SerialKillerCanUseVent", true)]
    public static bool SerialKillerCanUseVent;

    [CustomOptionBool("SerialKillerCanSabotage", true)]
    public static bool SerialKillerCanSabotage;
}