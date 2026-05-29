using System;
using System.Collections.Generic;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability;
using UnityEngine;

namespace SuperNewRoles.Roles.Impostor;

class RocketLauncher : RoleBase<RocketLauncher>
{
    public override RoleId Role { get; } = RoleId.RocketLauncher;
    public override Color32 RoleColor { get; } = Palette.ImpostorRed;
    public override List<Func<AbilityBase>> Abilities { get; } = [
        () => new RocketLauncherAbility(new RocketLauncherData(
            RocketLauncherLaunchCooldown,
            RocketLauncherCollideWithPlayers,
            RocketLauncherKillImpostors,
            RocketLauncherExplosionRange,
            RocketLauncherLimitLoadedTime,
            RocketLauncherLoadedTimeLimit,
            RocketLauncherCanUseNormalKill))
    ];

    public override QuoteMod QuoteMod { get; } = QuoteMod.SuperNewRoles;
    public override RoleTypes IntroSoundType { get; } = RoleTypes.Impostor;
    public override short IntroNum { get; } = 1;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Impostor;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Impostor;
    public override TeamTag TeamTag { get; } = TeamTag.Impostor;
    public override RoleTag[] RoleTags { get; } = [RoleTag.SpecialKiller];
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Impostor;

    [CustomOptionFloat("RocketLauncherLaunchCooldown", 2.5f, 60f, 2.5f, 30f)]
    public static float RocketLauncherLaunchCooldown;

    [CustomOptionBool("RocketLauncherCollideWithPlayers", true)]
    public static bool RocketLauncherCollideWithPlayers;

    [CustomOptionBool("RocketLauncherKillImpostors", false)]
    public static bool RocketLauncherKillImpostors;

    [CustomOptionFloat("RocketLauncherExplosionRange", 0.25f, 2f, 0.25f, 0.5f)]
    public static float RocketLauncherExplosionRange;

    [CustomOptionBool("RocketLauncherLimitLoadedTime", false)]
    public static bool RocketLauncherLimitLoadedTime;

    [CustomOptionFloat("RocketLauncherLoadedTimeLimit", 2.5f, 60f, 2.5f, 10f, parentFieldName: nameof(RocketLauncherLimitLoadedTime), suffix: "Seconds")]
    public static float RocketLauncherLoadedTimeLimit;

    [CustomOptionBool("RocketLauncherCanUseNormalKill", true)]
    public static bool RocketLauncherCanUseNormalKill;
}
