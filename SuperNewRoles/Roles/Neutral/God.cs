using System;
using System.Collections.Generic;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability;
using UnityEngine;

namespace SuperNewRoles.Roles.Neutral;


class God : RoleBase<God>
{
    public override RoleId Role { get; } = RoleId.God;
    public override Color32 RoleColor { get; } = Color.yellow;
    public override List<Func<AbilityBase>> Abilities { get; } = [
        () => new KnowOtherAbility(x => MeetingHud.Instance != null ? true : x.IsAlive(), () => true),
        () => new KnowVoteAbility(() => !GodSeeVote),
        () => new CustomTaskAbility(() => (GodNeededTask, false, GodTaskOption.Total), GodNeededTask ? GodTaskOption : null),
        () => new SabotageCanUseAbility(() => sabotageCantUse()),
        () => new CanUseReportButtonAbility(() => !GodCannotUseReportButton),
        () => new CanUseEmergencyButtonAbility(() => !GodCannotUseEmergencyButton, () => ModTranslation.GetString("GodCannotUseEmergencyButtonText"))
    ];

    public override QuoteMod QuoteMod { get; } = QuoteMod.SuperNewRoles;
    public override RoleTypes IntroSoundType { get; } = RoleTypes.Crewmate;
    public override short IntroNum { get; } = 1;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Neutral;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Neutral;
    public override TeamTag TeamTag { get; } = TeamTag.Neutral;
    public override RoleTag[] RoleTags { get; } = [];
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Neutral;
    public override RoleId[] RelatedRoleIds { get; } = [];

    [CustomOptionBool("GodSeeVote", false)]
    public static bool GodSeeVote;
    [CustomOptionBool("GodNeededTask", false)]
    public static bool GodNeededTask;
    [CustomOptionTask("GodTaskOption", 1, 1, 1, parentFieldName: nameof(GodNeededTask))]
    public static TaskOptionData GodTaskOption;
    [CustomOptionBool("GodCannotFixReactor", true)]
    public static bool GodCannotFixReactor;
    [CustomOptionBool("GodCannotFixComms", true)]
    public static bool GodCannotFixComms;
    [CustomOptionBool("GodCannotFixLights", true)]
    public static bool GodCannotFixLights;
    [CustomOptionBool("GodCannotUseReportButton", true)]
    public static bool GodCannotUseReportButton;
    [CustomOptionBool("GodCannotUseEmergencyButton", true)]
    public static bool GodCannotUseEmergencyButton;

    private static SabotageType sabotageCantUse()
    {
        var sabotageCanUse = SabotageType.None;
        if (GodCannotFixReactor)
        {
            sabotageCanUse |= SabotageType.Reactor | SabotageType.O2;
        }
        if (GodCannotFixComms)
        {
            sabotageCanUse |= SabotageType.Comms;
        }
        if (GodCannotFixLights)
        {
            sabotageCanUse |= SabotageType.Lights;
        }
        return sabotageCanUse;
    }
}