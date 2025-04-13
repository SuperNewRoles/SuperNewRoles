using System;
using System.Collections.Generic;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability;
using UnityEngine;

namespace SuperNewRoles.Roles.Modifiers;

class ModifierGuesser : ModifierBase<ModifierGuesser>
{
    public override ModifierRoleId ModifierRole => ModifierRoleId.Guesser;

    public override Color32 RoleColor => Crewmate.NiceGuesser.Instance.RoleColor;

    public override List<Func<AbilityBase>> Abilities => [() => new GuesserAbility(2, 2, true, true)];

    public override QuoteMod QuoteMod => QuoteMod.SuperNewRoles;

    public override List<AssignedTeamType> AssignedTeams => [AssignedTeamType.Crewmate, AssignedTeamType.Impostor, AssignedTeamType.Neutral];

    public override WinnerTeamType WinnerTeam => WinnerTeamType.Crewmate;

    public override TeamTag TeamTag => TeamTag.Crewmate;

    public override RoleTag[] RoleTags => [];

    public override short IntroNum => 1;
    [CustomOptionBool("Guesser.ShowGuesser11", false)]
    public static bool ShowGuesser = true;
}
