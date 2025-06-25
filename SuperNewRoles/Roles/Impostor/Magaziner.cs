using System;
using System.Collections.Generic;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability;
using UnityEngine;

namespace SuperNewRoles.Roles.Impostor;

internal class Magaziner : RoleBase<Magaziner>
{
    public override RoleId Role => RoleId.Magaziner;
    public override Color32 RoleColor => Palette.ImpostorRed;
    public override List<Func<AbilityBase>> Abilities => [
        () => new MagazinerAbility(new MagazinerAbilityData(MagazinerSetKillTime))
    ];

    public override QuoteMod QuoteMod => QuoteMod.SuperNewRoles;
    public override AssignedTeamType AssignedTeam => AssignedTeamType.Impostor;
    public override WinnerTeamType WinnerTeam => WinnerTeamType.Impostor;
    public override TeamTag TeamTag => TeamTag.Impostor;
    public override RoleTag[] RoleTags => [RoleTag.Support, RoleTag.ImpostorTeam];
    public override short IntroNum => 1;
    public override RoleOptionMenuType OptionTeam => RoleOptionMenuType.Impostor;

    [CustomOptionFloat("MagazinerSetKillTime", 0f, 60f, 2.5f, 10f)]
    public static float MagazinerSetKillTime;
}