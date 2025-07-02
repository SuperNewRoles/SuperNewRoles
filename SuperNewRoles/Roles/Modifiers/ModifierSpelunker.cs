using System;
using System.Collections.Generic;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability;
using SuperNewRoles.Roles.Neutral;
using UnityEngine;

namespace SuperNewRoles.Roles.Modifiers;

class ModifierSpelunker : ModifierBase<ModifierSpelunker>
{
    public override ModifierRoleId ModifierRole => ModifierRoleId.ModifierSpelunker;

    public override Color32 RoleColor => new(255, 255, 0, byte.MaxValue); // 黄色

    public override List<Func<AbilityBase>> Abilities => [
        () => new SpelunkerAbility(new SpelunkerData(
            VentDeathChance: ModifierSpelunkerVentDeathChance,
            CommsDeathTime: ModifierSpelunkerIsDeathCommsOrPowerdown ? ModifierSpelunkerDeathCommsTime : -1f,
            PowerdownDeathTime: ModifierSpelunkerIsDeathCommsOrPowerdown ? ModifierSpelunkerDeathPowerdownTime : -1f,
            LiftDeathChance: ModifierSpelunkerLiftDeathChance,
            DoorOpenChance: ModifierSpelunkerDoorOpenChance,
            LadderDeathChance: ModifierSpelunkerLadderDeathChance
        )),
        () => new LadderDeathAbility(ModifierSpelunkerLadderDeathChance)
    ];

    public override QuoteMod QuoteMod => QuoteMod.SuperNewRoles;

    public override List<AssignedTeamType> AssignedTeams => [];

    public override WinnerTeamType WinnerTeam => WinnerTeamType.None;

    public override RoleTag[] RoleTags => [];

    public override short IntroNum => 1;

    public override bool AssignFilter => true;
    public override bool UseTeamSpecificAssignment => true;

    public override Func<ExPlayerControl, string> ModifierMark => (player) => "{0}" + ModHelpers.Cs(RoleColor, "⚠");

    public override RoleId[] DoNotAssignRoles => [RoleId.Spelunker];

    [CustomOptionBool("ModifierSpelunkerIsDeathCommsOrPowerdown", true, translationName: "SpelunkerIsDeathCommsOrPowerdown")]
    public static bool ModifierSpelunkerIsDeathCommsOrPowerdown = true;

    [CustomOptionFloat("ModifierSpelunkerDeathCommsTime", 5f, 120f, 2.5f, 20f, translationName: "SpelunkerDeathCommsTime", parentFieldName: nameof(ModifierSpelunkerIsDeathCommsOrPowerdown))]
    public static float ModifierSpelunkerDeathCommsTime = 20f;

    [CustomOptionFloat("ModifierSpelunkerDeathPowerdownTime", 5f, 120f, 2.5f, 20f, translationName: "SpelunkerDeathPowerdownTime", parentFieldName: nameof(ModifierSpelunkerIsDeathCommsOrPowerdown))]
    public static float ModifierSpelunkerDeathPowerdownTime = 20f;
    [CustomOptionInt("ModifierSpelunkerVentDeathChance", 0, 100, 5, 20, translationName: "SpelunkerVentDeathChance")]
    public static int ModifierSpelunkerVentDeathChance = 20;

    [CustomOptionInt("ModifierSpelunkerLiftDeathChance", 0, 100, 5, 20, translationName: "SpelunkerLiftDeathChance")]
    public static int ModifierSpelunkerLiftDeathChance = 20;

    [CustomOptionInt("ModifierSpelunkerDoorOpenChance", 0, 100, 5, 20, translationName: "SpelunkerDoorOpenChance")]
    public static int ModifierSpelunkerDoorOpenChance = 20;

    [CustomOptionInt("ModifierSpelunkerLadderDeathChance", 0, 100, 5, 20, translationName: "LadderDeadChance")]
    public static int ModifierSpelunkerLadderDeathChance = 20;
}