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

class EvilNekomata : RoleBase<EvilNekomata>
{
    public override RoleId Role { get; } = RoleId.EvilNekomata;
    public override Color32 RoleColor { get; } = Palette.ImpostorRed;

    public override List<Func<AbilityBase>> Abilities { get; } = new()
    {
        () => new RevengeExileAbility(EvilNekomataExcludeImpostor)
    };

    public override QuoteMod QuoteMod { get; } = QuoteMod.AuLibHalt;
    public override RoleTypes IntroSoundType { get; } = RoleTypes.Crewmate;
    public override short IntroNum { get; } = 1;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Impostor;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Impostor;
    public override TeamTag TeamTag { get; } = TeamTag.Impostor;
    public override RoleTag[] RoleTags { get; } = Array.Empty<RoleTag>();
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Impostor;

    [CustomOptionBool("EvilNekomataExcludeImpostor", false)]
    public static bool EvilNekomataExcludeImpostor;
}