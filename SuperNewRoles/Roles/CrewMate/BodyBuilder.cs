using System;
using System.Collections.Generic;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability;
using UnityEngine;

namespace SuperNewRoles.Roles.Crewmate;

class BodyBuilder : RoleBase<BodyBuilder>
{
    public override RoleId Role { get; } = RoleId.BodyBuilder;
    public override Color32 RoleColor { get; } = new(214, 143, 94, byte.MaxValue);
    public override List<Func<AbilityBase>> Abilities { get; } = [
        () => new BodyBuilderAbility(),
        () => new LiftWeightsMinigameAbility(),
        () => new CustomTaskTypeAbility(TaskTypes.LiftWeights, ChangeAllTaskLiftWeights, MapNames.Fungle),
        () => new CustomTaskAbility(
            () => (true, TaskOptionAvailable, TaskOptionAvailable ? TaskOption.Total : null),
            TaskOptionAvailable ? TaskOption : null
        )
    ];

    public override QuoteMod QuoteMod { get; } = QuoteMod.SuperNewRoles;
    public override RoleTypes IntroSoundType { get; } = RoleTypes.Crewmate;
    public override short IntroNum { get; } = 1;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Crewmate;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Crewmate;
    public override TeamTag TeamTag { get; } = TeamTag.Crewmate;
    public override RoleTag[] RoleTags { get; } = [];
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Crewmate;

    [CustomOptionBool("BodyBuilderChangeAllTaskLiftWeights", false)]
    public static bool ChangeAllTaskLiftWeights;

    [CustomOptionBool("BodyBuilderTaskOptionAvailable", false)]
    public static bool TaskOptionAvailable;

    [CustomOptionTask("BodyBuilderTaskOption", 2, 3, 1, parentFieldName: nameof(TaskOptionAvailable), parentActiveValue: true)]
    public static TaskOptionData TaskOption;
}