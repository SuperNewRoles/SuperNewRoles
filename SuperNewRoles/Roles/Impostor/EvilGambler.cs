using System;
using System.Collections.Generic;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability;
using UnityEngine;

namespace SuperNewRoles.Roles.Impostor;

class EvilGambler : RoleBase<EvilGambler>
{
    public override RoleId Role => RoleId.EvilGambler;
    public override Color32 RoleColor => Palette.ImpostorRed;
    public override List<Func<AbilityBase>> Abilities => new()
    {
        () => new EvilGamblerAbility(new EvilGamblerData(
            successKillCooldown: SuccessKillCooldown,
            failureKillCooldown: FailureKillCooldown,
            successChance: SuccessChance
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

    [CustomOptionFloat("EvilGamblerSuccessKillCooldown", 2.5f, 60f, 2.5f, 15f)]
    public static float SuccessKillCooldown;

    [CustomOptionFloat("EvilGamblerFailureKillCooldown", 2.5f, 60f, 2.5f, 45f)]
    public static float FailureKillCooldown;

    [CustomOptionInt("EvilGamblerSuccessChance", 10, 90, 10, 50)]
    public static int SuccessChance;
}