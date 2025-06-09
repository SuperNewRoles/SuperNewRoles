using System;
using System.Collections.Generic;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability;
using UnityEngine;

namespace SuperNewRoles.Roles.Neutral;

class OrientalShaman : RoleBase<OrientalShaman>
{
    public override RoleId Role { get; } = RoleId.OrientalShaman;
    public override Color32 RoleColor { get; } = new(192, 177, 246, byte.MaxValue);
    public override List<Func<AbilityBase>> Abilities { get; } = [
        () => new OrientalShamanAbility(new OrientalShamanData(
            canUseVent: OrientalShamanCanUseVent,
            ventCooldown: OrientalShamanVentCooldown,
            canCreateServant: OrientalShamanCanCreateServant,
            servantCooldown: OrientalShamanServantCooldown,
            isImpostorVision: OrientalShamanImpostorVision,
            neededTaskComplete: OrientalShamanNeededTaskComplete,
            task: OrientalShamanTask,
            ventDuration: OrientalShamanVentDuration
        )),
        () => new SabotageCanUseAbility(() => sabotageCantUse()),
        () => new CanUseReportButtonAbility(() => !OrientalShamanCannotUseReportButton),
        () => new CanUseEmergencyButtonAbility(() => !OrientalShamanCannotUseEmergencyButton, () => ModTranslation.GetString("GodCannotUseEmergencyButtonText")),
    ];

    public override QuoteMod QuoteMod { get; } = QuoteMod.SuperNewRoles;
    public override RoleTypes IntroSoundType { get; } = RoleTypes.Shapeshifter;
    public override short IntroNum { get; } = 1;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Neutral;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Neutral;
    public override TeamTag TeamTag { get; } = TeamTag.Neutral;
    public override RoleTag[] RoleTags { get; } = [];
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Neutral;
    public override RoleId[] RelatedRoleIds { get; } = [RoleId.ShermansServant];

    [CustomOptionBool("OrientalShamanCanUseVent", true, translationName: "CanUseVent")]
    public static bool OrientalShamanCanUseVent;

    [CustomOptionFloat("OrientalShamanVentCooldown", 0f, 60f, 2.5f, 10f, parentFieldName: nameof(OrientalShamanCanUseVent), translationName: "VentCooldown")]
    public static float OrientalShamanVentCooldown;

    [CustomOptionFloat("OrientalShamanVentDuration", 0f, 60f, 2.5f, 10f, parentFieldName: nameof(OrientalShamanCanUseVent), translationName: "VentDuration")]
    public static float OrientalShamanVentDuration;

    [CustomOptionBool("OrientalShamanImpostorVision", true, translationName: "HasImpostorVision")]
    public static bool OrientalShamanImpostorVision;

    [CustomOptionBool("OrientalShamanCanCreateServant", true)]
    public static bool OrientalShamanCanCreateServant;

    [CustomOptionFloat("OrientalShamanServantCooldown", 2.5f, 60f, 2.5f, 30f, parentFieldName: nameof(OrientalShamanCanCreateServant))]
    public static float OrientalShamanServantCooldown;

    [CustomOptionBool("OrientalShamanNeededTaskComplete", true)]
    public static bool OrientalShamanNeededTaskComplete;

    [CustomOptionTask("OrientalShamanTask", 1, 1, 1, parentFieldName: nameof(OrientalShamanNeededTaskComplete))]
    public static TaskOptionData OrientalShamanTask;

    [CustomOptionBool("OrientalShamanCannotFixReactor", true)]
    public static bool OrientalShamanCannotFixReactor;

    [CustomOptionBool("OrientalShamanCannotFixComms", true)]
    public static bool OrientalShamanCannotFixComms;

    [CustomOptionBool("OrientalShamanCannotFixLights", true)]
    public static bool OrientalShamanCannotFixLights;

    [CustomOptionBool("OrientalShamanCannotUseReportButton", false)]
    public static bool OrientalShamanCannotUseReportButton;

    [CustomOptionBool("OrientalShamanCannotUseEmergencyButton", false)]
    public static bool OrientalShamanCannotUseEmergencyButton;

    [CustomOptionFloat("ShermansServantTransformCooldown", 0f, 60f, 2.5f, 0f)]
    public static float ShermansServantTransformCooldown;

    [CustomOptionFloat("ShermansServantSuicideCooldown", 2.5f, 120f, 2.5f, 30f)]
    public static float ShermansServantSuicideCooldown;

    private static SabotageType sabotageCantUse()
    {
        var sabotageCanUse = SabotageType.None;
        if (OrientalShamanCannotFixReactor)
        {
            sabotageCanUse |= SabotageType.Reactor | SabotageType.O2;
        }
        if (OrientalShamanCannotFixComms)
        {
            sabotageCanUse |= SabotageType.Comms;
        }
        if (OrientalShamanCannotFixLights)
        {
            sabotageCanUse |= SabotageType.Lights;
        }
        return sabotageCanUse;
    }
}

public record OrientalShamanData(
    bool canUseVent,
    float ventCooldown,
    float ventDuration,
    bool canCreateServant,
    float servantCooldown,
    bool isImpostorVision,
    bool neededTaskComplete,
    TaskOptionData task
);