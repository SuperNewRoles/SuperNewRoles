using System;
using System.Collections.Generic;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability;
using UnityEngine;

namespace SuperNewRoles.Roles.Neutral;

class PavlovsOwner : RoleBase<PavlovsOwner>
{
    public override RoleId Role => RoleId.PavlovsOwner;
    public override Color32 RoleColor => new(244, 169, 106, byte.MaxValue);
    public override List<Func<AbilityBase>> Abilities => new()
    {
        () => new PavlovsOwnerAbility(new(
            sidekickCooldown: PavlovsOwnerSidekickCooldown,
            maxSidekickCount: PavlovsOwnerMaxSidekickCount,
            suicideOnImpostorSidekick: PavlovsOwnerSuicideOnImpostorSidekick
        ))
    };
    public override QuoteMod QuoteMod => QuoteMod.SuperNewRoles;
    public override AssignedTeamType AssignedTeam => AssignedTeamType.Neutral;
    public override WinnerTeamType WinnerTeam => WinnerTeamType.Neutral;
    public override TeamTag TeamTag => TeamTag.Neutral;
    public override RoleTag[] RoleTags => new[] { RoleTag.SpecialKiller };
    public override short IntroNum => 1;
    public override RoleOptionMenuType OptionTeam => RoleOptionMenuType.Neutral;

    [CustomOptionFloat("PavlovsOwnerSidekickCooldown", 2.5f, 60f, 2.5f, 30f)]
    public static float PavlovsOwnerSidekickCooldown;

    [CustomOptionInt("PavlovsOwnerMaxSidekickCount", 0, 10, 1, 2)]
    public static int PavlovsOwnerMaxSidekickCount;

    [CustomOptionBool("PavlovsOwnerSuicideOnImpostorSidekick", true)]
    public static bool PavlovsOwnerSuicideOnImpostorSidekick;
    [CustomOptionBool("PavlovsDogCanUseVent", true)]
    public static bool PavlovsDogCanUseVent;

    [CustomOptionBool("PavlovsDogIsImpostorVision", true)]
    public static bool PavlovsDogIsImpostorVision;

    [CustomOptionFloat("PavlovsDogKillCooldown", 2.5f, 60f, 2.5f, 30f)]
    public static float PavlovsDogKillCooldown;

    [CustomOptionFloat("PavlovsDogRampageKillCooldown", 2.5f, 60f, 2.5f, 15f)]
    public static float PavlovsDogRampageKillCooldown;

    [CustomOptionFloat("PavlovsDogRampageSuicideTime", 10f, 120f, 5f, 60f)]
    public static float PavlovsDogRampageSuicideTime;

    [CustomOptionBool("PavlovsDogResetSuicideTimeOnMeeting", true)]
    public static bool PavlovsDogResetSuicideTimeOnMeeting;
}