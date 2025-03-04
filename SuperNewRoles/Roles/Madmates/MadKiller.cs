using System;
using System.Collections.Generic;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability;
using UnityEngine;

namespace SuperNewRoles.Roles.Madmates;

class MadKiller : RoleBase<MadKiller>
{
    public override RoleId Role => RoleId.MadKiller;
    public override Color32 RoleColor => Palette.ImpostorRed;
    public override List<Func<AbilityBase>> Abilities => new()
    {
        () => new MadKillerAbility(new MadKillerData(
            hasImpostorVision: true,
            couldUseVent: false,
            killCooldown: 0f
        ))
    };

    public override QuoteMod QuoteMod => QuoteMod.SuperNewRoles;
    public override RoleTypes IntroSoundType => RoleTypes.Impostor;
    public override short IntroNum => 1;

    public override AssignedTeamType AssignedTeam => AssignedTeamType.Crewmate;
    public override WinnerTeamType WinnerTeam => WinnerTeamType.Impostor;
    public override TeamTag TeamTag => TeamTag.Madmate;
    public override RoleTag[] RoleTags => new[] { RoleTag.SpecialKiller };
    public override RoleOptionMenuType OptionTeam => RoleOptionMenuType.Hidden;
    public override RoleId[] RelatedRoleIds => new[] { RoleId.SideKiller };
}