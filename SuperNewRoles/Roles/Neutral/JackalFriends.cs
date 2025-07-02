using System;
using System.Collections.Generic;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability;
using UnityEngine;

namespace SuperNewRoles.Roles.Neutral;

class JackalFriends : RoleBase<JackalFriends>
{
    public override RoleId Role { get; } = RoleId.JackalFriends;
    public override Color32 RoleColor { get; } = Jackal.Instance.RoleColor;
    public override List<Func<AbilityBase>> Abilities { get; } = [
        () => new JFriendAbility(new(
            CanUseVent: JackalFriendsCanUseVent,
            IsImpostorVision: JackalFriendsImpostorVision,
            CouldKnowJackals: JackalFriendsCouldKnowJackals,
            TaskNeeded: JackalFriendsTaskNeed,
            SpecialTasks: JackalFriendsCustomTaskCount ? JackalFriendsTaskOption : null
        ))
    ];

    public override QuoteMod QuoteMod { get; } = QuoteMod.SuperNewRoles;
    public override RoleTypes IntroSoundType { get; } = RoleTypes.Crewmate;
    public override short IntroNum { get; } = 1;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Crewmate;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Neutral;
    public override TeamTag TeamTag { get; } = TeamTag.Jackal;
    public override RoleTag[] RoleTags { get; } = [];
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Crewmate;

    [CustomOptionBool("JackalFriendsCanUseVent", true, translationName: "CanUseVent")]
    public static bool JackalFriendsCanUseVent;

    [CustomOptionBool("JackalFriendsImpostorVision", true, translationName: "HasImpostorVision")]
    public static bool JackalFriendsImpostorVision;

    [CustomOptionBool("JackalFriendsDontAssignIfJackalNotAssigned", true)]
    public static bool JackalFriendsDontAssignIfJackalNotAssigned;

    [CustomOptionBool("JackalFriendsCouldKnowJackals", true)]
    public static bool JackalFriendsCouldKnowJackals;

    [CustomOptionInt("JackalFriendsTaskNeed", 0, 10, 1, 0, parentFieldName: nameof(JackalFriendsCouldKnowJackals))]
    public static int JackalFriendsTaskNeed;

    [CustomOptionBool("JackalFriendsCustomTaskCount", false, parentFieldName: nameof(JackalFriendsCouldKnowJackals))]
    public static bool JackalFriendsCustomTaskCount;

    [CustomOptionTask("JackalFriendsTaskOption", 1, 1, 1, parentFieldName: nameof(JackalFriendsCustomTaskCount), translationName: "TaskOption")]
    public static TaskOptionData JackalFriendsTaskOption;
}