using System;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability;
using SuperNewRoles.Roles.Ability.CustomButton;
using SuperNewRoles.Roles.Impostor;
using UnityEngine;

namespace SuperNewRoles.Roles.Crewmate;

internal class NiceTeleporter : RoleBase<NiceTeleporter>
{
    public override RoleId Role { get; } = RoleId.NiceTeleporter;
    public override Color32 RoleColor { get; } = Color.blue;
    public override List<Func<AbilityBase>> Abilities { get; } = [
        () => new TeleporterAbility(
            new TeleporterAbilityData(NiceTeleporterCooldown, NiceTeleporterWaitingTime)
        )
    ];

    public override QuoteMod QuoteMod { get; } = QuoteMod.SuperNewRoles;
    public override RoleTypes IntroSoundType { get; } = RoleTypes.Tracker;
    public override short IntroNum { get; } = 1;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Crewmate;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Crewmate;
    public override TeamTag TeamTag { get; } = TeamTag.Crewmate;
    public override RoleTag[] RoleTags { get; } = [RoleTag.Support];
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Crewmate;

    [CustomOptionFloat("NiceTeleporterCooldown", 5f, 120f, 5f, 45f, translationName: "CoolTime")]
    public static float NiceTeleporterCooldown;
    [CustomOptionFloat("NiceTeleporterWaitingTime", 0f, 10f, 1f, 3f, translationName: "TeleporterWaitingTime")]
    public static float NiceTeleporterWaitingTime;

}