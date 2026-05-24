using System;
using System.Collections.Generic;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability;
using UnityEngine;

namespace SuperNewRoles.Roles.Impostor;

internal class RemoteController : RoleBase<RemoteController>
{
    public override RoleId Role { get; } = RoleId.RemoteController;
    public override Color32 RoleColor { get; } = Palette.ImpostorRed;
    public override List<Func<AbilityBase>> Abilities { get; } =
    [
        () => new RemoteControllerAbility(
            RemoteControllerMarkCooldown,
            RemoteControllerOperationInfinite,
            RemoteControllerOperationDuration),
    ];

    public override QuoteMod QuoteMod { get; } = QuoteMod.SuperNewRoles;
    public override RoleTypes IntroSoundType { get; } = RoleTypes.Impostor;
    public override short IntroNum { get; } = 1;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Impostor;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Impostor;
    public override TeamTag TeamTag { get; } = TeamTag.Impostor;
    public override RoleTag[] RoleTags { get; } = [RoleTag.Killer, RoleTag.SpecialKiller, RoleTag.ImpostorTeam];
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Impostor;

    [CustomOptionFloat("RemoteControllerMarkCooldown", 0f, 60f, 2.5f, 20f, translationName: "CoolTime")]
    public static float RemoteControllerMarkCooldown;

    [CustomOptionBool("RemoteControllerOperationInfinite", false)]
    public static bool RemoteControllerOperationInfinite;

    [CustomOptionFloat("RemoteControllerOperationDuration", 2.5f, 120f, 2.5f, 10f, parentFieldName: nameof(RemoteControllerOperationInfinite), parentActiveValue: false)]
    public static float RemoteControllerOperationDuration;
}

