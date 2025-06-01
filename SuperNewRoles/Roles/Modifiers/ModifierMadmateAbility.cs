/*using System;
using System.Collections.Generic;
using SuperNewRoles.Roles.Ability;
using UnityEngine;

namespace SuperNewRoles.Roles.Modifiers;

class ModifierMadmate : ModifierBase<ModifierMadmate>
{
    public override ModifierRoleId ModifierRole => ModifierRoleId.ModifierMadmate;

    public override Color32 RoleColor => Crewmate.NiceGuesser.Instance.RoleColor;

    public override List<Func<AbilityBase>> Abilities => [];//[() => new ModifierMadmateAbility()];

    public override QuoteMod QuoteMod => QuoteMod.SuperNewRoles;

    public override List<AssignedTeamType> AssignedTeams => [];

    public override WinnerTeamType WinnerTeam => WinnerTeamType.Crewmate;

    public override TeamTag TeamTag => TeamTag.Crewmate;

    public override RoleTag[] RoleTags => [];

    public override short IntroNum => 1;
    public override string ModifierMark => ModHelpers.Cs(RoleColor, "マッド{0}");

}
*/