using System;
using System.Collections.Generic;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability;
using SuperNewRoles.Roles.Impostor;
using UnityEngine;

namespace SuperNewRoles.Roles.Madmates;

class MadKiller : RoleBase<MadKiller>
{
    public override RoleId Role => RoleId.MadKiller;
    public override Color32 RoleColor => Palette.ImpostorRed;
    public override List<Func<AbilityBase>> Abilities => new()
    {
        () => new MadKillerAbility(new MadKillerData(
            hasImpostorVision: SideKiller.MadKillerHasImpostorVision,
            couldUseVent: SideKiller.MadKillerCanUseVent,
            killCooldown: SideKiller.MadKillerKillCooldown,
            cannotBeSeenBeforePromotion: SideKiller.CannotSeeMadKillerBeforePromotion,
            cannotSeeImpostorBeforePromotion: SideKiller.MadKillerCannotSeeImpostorBeforePromotion
        ))
    };

    public override QuoteMod QuoteMod => QuoteMod.SuperNewRoles;
    public override RoleTypes IntroSoundType => RoleTypes.Impostor;
    public override short IntroNum => 1;

    public override AssignedTeamType AssignedTeam => AssignedTeamType.Impostor;
    public override WinnerTeamType WinnerTeam => WinnerTeamType.Impostor;
    public override TeamTag TeamTag => TeamTag.Madmate;
    public override RoleTag[] RoleTags => new[] { RoleTag.SpecialKiller };
    public override RoleOptionMenuType OptionTeam => RoleOptionMenuType.Hidden;
    public override RoleId[] RelatedRoleIds => new[] { RoleId.SideKiller };
}