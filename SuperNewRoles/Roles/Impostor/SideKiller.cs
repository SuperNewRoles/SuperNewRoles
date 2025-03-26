using System;
using System.Collections.Generic;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability;
using SuperNewRoles.Roles.Ability.CustomButton;
using UnityEngine;

namespace SuperNewRoles.Roles.Impostor;

class SideKiller : RoleBase<SideKiller>
{
    public override RoleId Role => RoleId.SideKiller;
    public override Color32 RoleColor => Palette.ImpostorRed;
    public override List<Func<AbilityBase>> Abilities => new()
    {
        () => new SideKillerAbility(new SideKillerData(
            killCooldown: SideKillerKillCooldown,
            madKillerKillCooldown: MadKillerKillCooldown,
            madKillerCanUseVent: MadKillerCanUseVent,
            madKillerHasImpostorVision: MadKillerHasImpostorVision,
            cannotSeeMadKillerBeforePromotion: CannotSeeMadKillerBeforePromotion
        ))
    };

    public override QuoteMod QuoteMod => QuoteMod.SuperNewRoles;
    public override RoleTypes IntroSoundType => RoleTypes.Impostor;
    public override short IntroNum => 1;

    public override AssignedTeamType AssignedTeam => AssignedTeamType.Impostor;
    public override WinnerTeamType WinnerTeam => WinnerTeamType.Impostor;
    public override TeamTag TeamTag => TeamTag.Impostor;
    public override RoleTag[] RoleTags => new[] { RoleTag.SpecialKiller };
    public override RoleOptionMenuType OptionTeam => RoleOptionMenuType.Impostor;
    public override RoleId[] RelatedRoleIds => new[] { RoleId.MadKiller };

    [CustomOptionFloat("SideKillerKillCooldown", 2.5f, 60f, 2.5f, 30f)]
    public static float SideKillerKillCooldown;

    [CustomOptionFloat("MadKillerKillCooldown", 2.5f, 60f, 2.5f, 30f)]
    public static float MadKillerKillCooldown;

    [CustomOptionBool("MadKillerCanUseVent", false)]
    public static bool MadKillerCanUseVent;

    [CustomOptionBool("MadKillerHasImpostorVision", false)]
    public static bool MadKillerHasImpostorVision;

    [CustomOptionBool("CannotSeeMadKillerBeforePromotion", false)]
    public static bool CannotSeeMadKillerBeforePromotion;

    [CustomOptionBool("MadKillerCannotSeeImpostorBeforePromotion", false, parentFieldName: nameof(CannotSeeMadKillerBeforePromotion))]
    public static bool MadKillerCannotSeeImpostorBeforePromotion;
}