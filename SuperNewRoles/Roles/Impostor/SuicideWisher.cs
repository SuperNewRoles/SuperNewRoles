using System;
using System.Collections.Generic;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability;
using UnityEngine;

namespace SuperNewRoles.Roles.Impostor;

internal class SuicideWisher : RoleBase<SuicideWisher>
{
    public override RoleId Role => RoleId.SuicideWisher;
    public override Color32 RoleColor => Palette.ImpostorRed;
    public override List<Func<AbilityBase>> Abilities => new()
    {
        () => new SuicideWisherAbility(SuicideWisherCoolTime)
    };
    public override QuoteMod QuoteMod => QuoteMod.SuperNewRoles;
    public override AssignedTeamType AssignedTeam => AssignedTeamType.Impostor;
    public override WinnerTeamType WinnerTeam => WinnerTeamType.Impostor;
    public override TeamTag TeamTag => TeamTag.Impostor;
    public override RoleTag[] RoleTags => [];
    public override short IntroNum => 1;
    public override RoleTypes IntroSoundType => RoleTypes.Impostor;
    public override RoleOptionMenuType OptionTeam => RoleOptionMenuType.Impostor;

    [CustomOptionFloat("SuicideWisherCoolTime", 0f, 180f, 2.5f, 20f, translationName: "CoolTime")]
    public static float SuicideWisherCoolTime;
}