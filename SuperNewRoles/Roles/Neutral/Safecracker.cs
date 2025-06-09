using System;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability;
using UnityEngine;

namespace SuperNewRoles.Roles.Neutral;

class Safecracker : RoleBase<Safecracker>
{
    public override RoleId Role { get; } = RoleId.Safecracker;
    public override Color32 RoleColor { get; } = new(248, 217, 88, byte.MaxValue);
    public override List<Func<AbilityBase>> Abilities { get; } = [
        () => new SafecrackerAbility(
            killGuardTaskRate: SafecrackerKillGuardTask,
            maxKillGuardCount: SafecrackerMaxKillGuardCount,
            exiledGuardTaskRate: SafecrackerExiledGuardTask,
            maxExiledGuardCount: SafecrackerMaxExiledGuardCount,
            useVentTaskRate: SafecrackerUseVentTask,
            useSaboTaskRate: SafecrackerUseSaboTask,
            impostorLightTaskRate: SafecrackerIsImpostorLightTask,
            checkImpostorTaskRate: SafecrackerCheckImpostorTask,
            changeTaskPrefab: SafecrackerChangeTaskPrefab,
            task: SafecrackerTask
        )
    ];

    public override QuoteMod QuoteMod { get; } = QuoteMod.SuperNewRoles;
    public override RoleTypes IntroSoundType { get; } = RoleTypes.Crewmate;
    public override short IntroNum { get; } = 1;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Neutral;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Neutral;
    public override TeamTag TeamTag { get; } = TeamTag.Neutral;
    public override RoleTag[] RoleTags { get; } = [RoleTag.SpecialWinner];
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Neutral;

    [CustomOptionFloat("SafecrackerKillGuardTask", 0f, 100f, 5f, 50f, translationName: "SafecrackerKillGuardTaskSetting", suffix: "%")]
    public static float SafecrackerKillGuardTask;

    [CustomOptionInt("SafecrackerMaxKillGuardCount", 1, 15, 1, 1, parentFieldName: nameof(SafecrackerKillGuardTask))]
    public static int SafecrackerMaxKillGuardCount;

    [CustomOptionFloat("SafecrackerExiledGuardTask", 0f, 100f, 5f, 50f, translationName: "SafecrackerExiledGuardTaskSetting", suffix: "%")]
    public static float SafecrackerExiledGuardTask;

    [CustomOptionInt("SafecrackerMaxExiledGuardCount", 1, 15, 1, 1, parentFieldName: nameof(SafecrackerExiledGuardTask))]
    public static int SafecrackerMaxExiledGuardCount;

    [CustomOptionFloat("SafecrackerUseVentTask", 0f, 100f, 5f, 50f, translationName: "SafecrackerUseVentTaskSetting", suffix: "%")]
    public static float SafecrackerUseVentTask;

    [CustomOptionFloat("SafecrackerUseSaboTask", 0f, 100f, 5f, 50f, translationName: "SafecrackerUseSaboTaskSetting", suffix: "%")]
    public static float SafecrackerUseSaboTask;

    [CustomOptionFloat("SafecrackerIsImpostorLightTask", 0f, 100f, 5f, 50f, translationName: "SafecrackerIsImpostorLightTaskSetting", suffix: "%")]
    public static float SafecrackerIsImpostorLightTask;

    [CustomOptionFloat("SafecrackerCheckImpostorTask", 0f, 100f, 5f, 50f, translationName: "SafecrackerCheckImpostorTaskSetting", suffix: "%")]
    public static float SafecrackerCheckImpostorTask;

    [CustomOptionBool("SafecrackerChangeTaskPrefab", false, translationName: "SafecrackerChangeTaskPrefabSetting")]
    public static bool SafecrackerChangeTaskPrefab;

    [CustomOptionTask("SafecrackerTask", 3, 3, 3)]
    public static TaskOptionData SafecrackerTask;
}