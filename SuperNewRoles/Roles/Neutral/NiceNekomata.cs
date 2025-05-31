using System;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Modules;
using SuperNewRoles.Events;
using SuperNewRoles.Modules.Events.Bases;
using SuperNewRoles.Roles.Ability;
using UnityEngine;
using SuperNewRoles.Roles.Madmates;

namespace SuperNewRoles.Roles.Neutral;

class NiceNekomata : RoleBase<NiceNekomata>
{
    public override RoleId Role { get; } = RoleId.NiceNekomata;
    public override Color32 RoleColor { get; } = new(255, 255, 255, byte.MaxValue);

    public override List<Func<AbilityBase>> Abilities { get; } = new()
    {
        () => new RevengeExileAbility(false)
    };

    public override QuoteMod QuoteMod { get; } = QuoteMod.AuLibHalt;
    public override RoleTypes IntroSoundType { get; } = RoleTypes.Crewmate;
    public override short IntroNum { get; } = 1;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Crewmate;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Crewmate;
    public override TeamTag TeamTag { get; } = TeamTag.Crewmate;
    public override RoleTag[] RoleTags { get; } = Array.Empty<RoleTag>();
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Crewmate;
}