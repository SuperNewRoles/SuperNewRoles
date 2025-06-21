using System;
using System.Collections.Generic;
using System.Linq;
using Rewired.Utils.Classes.Data;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability;
using UnityEngine;

namespace SuperNewRoles.Roles.Modifiers;

class ModifierHauntedWolf : ModifierBase<ModifierHauntedWolf>
{
    public override ModifierRoleId ModifierRole => ModifierRoleId.ModifierHauntedWolf;
    public override Color32 RoleColor => CrewMate.HauntedWolf.Instance.RoleColor;
    public override List<Func<AbilityBase>> Abilities { get; } = [() => new HideMyRoleWhenAliveAbility(ModifierRoleId.ModifierHauntedWolf)];

    public override QuoteMod QuoteMod { get; } = QuoteMod.SuperNewRoles;

    public override List<AssignedTeamType> AssignedTeams => [AssignedTeamType.Crewmate];

    public override WinnerTeamType WinnerTeam => WinnerTeamType.Crewmate;

    public override RoleTag[] RoleTags => [];

    public override short IntroNum => 1;
    public override Func<ExPlayerControl, string> ModifierMark => (player) => "{0}" + ModHelpers.Cs(RoleColor, "狼");
    public override bool AssignFilter => ModifierHauntedWolfIsAssignMadAndFriendRoles;

    public override RoleId[] DoNotAssignRoles => [];

    public readonly static List<RoleId> MadAndFriendRoleId = new();

    [CustomOptionBool("ModifierHauntedWolfIsAssignMadAndFriendRoles", true)]
    public static bool ModifierHauntedWolfIsAssignMadAndFriendRoles = true;

    [CustomOptionBool("ModifierHauntedWolfIsReverseSheriffDecision", true)]
    public static bool ModifierHauntedWolfIsReverseSheriffDecision = true;

}