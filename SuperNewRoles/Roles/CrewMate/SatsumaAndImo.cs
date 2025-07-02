using System;
using System.Collections.Generic;
using UnityEngine;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability;

namespace SuperNewRoles.Roles.CrewMate;

class SatsumaAndImo : RoleBase<SatsumaAndImo>
{
    private enum SatsumaTeam { Crewmate, Madmate }
    private SatsumaTeam _teamState;

    public override RoleId Role => RoleId.SatsumaAndImo;
    public override Color32 RoleColor => new(153, 0, 68, byte.MaxValue);
    public override List<Func<AbilityBase>> Abilities => new() {
        () => new SatsumaAndImoAbility(),
        () => new CustomTaskAbility(() => (false, null, 0))
    };

    public override QuoteMod QuoteMod => QuoteMod.SuperNewRoles;
    public override RoleTypes IntroSoundType => RoleTypes.Crewmate;
    public override short IntroNum => 1;

    public override AssignedTeamType AssignedTeam => AssignedTeamType.Crewmate;
    public override WinnerTeamType WinnerTeam => WinnerTeamType.Crewmate;
    public override TeamTag TeamTag => TeamTag.Crewmate;
    public override RoleTag[] RoleTags => Array.Empty<RoleTag>();
    public override RoleOptionMenuType OptionTeam => RoleOptionMenuType.Crewmate;
}