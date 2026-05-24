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
        () => new CustomTaskTypeAbility(TaskTypes.LiftWeights, BodyBuilderChangeAllTaskLiftWeights, MapNames.Fungle),
        () => new CustomTaskAbility(
            () => (true, BodyBuilderTaskOptionAvailable, BodyBuilderTaskOptionAvailable ? BodyBuilderTaskOption.Total : null),
            BodyBuilderTaskOptionAvailable ? BodyBuilderTaskOption : null
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
    public static bool BodyBuilderChangeAllTaskLiftWeights;

    [CustomOptionBool("BodyBuilderTaskOptionAvailable", false)]
    public static bool BodyBuilderTaskOptionAvailable;

    [CustomOptionTask("BodyBuilderTaskOption", 2, 3, 1, parentFieldName: nameof(BodyBuilderTaskOptionAvailable), parentActiveValue: true)]
    public static TaskOptionData BodyBuilderTaskOption;
}