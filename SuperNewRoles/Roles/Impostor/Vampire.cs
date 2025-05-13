using System;
using System.Collections.Generic;
using UnityEngine;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability;

namespace SuperNewRoles.Roles.Impostor;
class Vampire : RoleBase<Vampire>
{
    public override RoleId Role => RoleId.Vampire;
    public override Color32 RoleColor => Palette.ImpostorRed;
    public override List<Func<AbilityBase>> Abilities { get; } = [() => new VampireAbility()];

    public override QuoteMod QuoteMod => QuoteMod.SuperNewRoles;
    public override RoleTypes IntroSoundType => RoleTypes.Shapeshifter;
    public override short IntroNum => 1;

    public override AssignedTeamType AssignedTeam => AssignedTeamType.Impostor;
    public override WinnerTeamType WinnerTeam => WinnerTeamType.Impostor;
    public override TeamTag TeamTag => TeamTag.Impostor;
    public override RoleTag[] RoleTags => new RoleTag[] { RoleTag.SpecialKiller };
    public override RoleOptionMenuType OptionTeam => RoleOptionMenuType.Impostor;

    [CustomOptionFloat("VampireAbsorptionCooldown", 2.5f, 60f, 2.5f, 30f)]
    public static float VampireAbsorptionCooldown;

    [CustomOptionFloat("VampireKillDelay", 1f, 30f, 1f, 10f)]
    public static float VampireKillDelay;

    [CustomOptionInt("VampireBloodstainDuration", 1, 5, 1, 1)]
    public static int VampireBloodstainDuration;

    [CustomOptionBool("VampireBlackBloodstains", false)]
    public static bool VampireBlackBloodstains;

    [CustomOptionBool("VampireCreateDependents", false)]
    public static bool VampireCreateDependents;

    [CustomOptionFloat("VampireNightFallCooldown", 2.5f, 60f, 2.5f, 30f)]
    public static float VampireNightFallCooldown;

    [CustomOptionFloat("VampireDependentKillCooldown", 2.5f, 60f, 2.5f, 30f)]
    public static float VampireDependentKillCooldown;

    [CustomOptionBool("VampireDependentCanUseVent", true)]
    public static bool VampireDependentCanUseVent;

    [CustomOptionBool("VampireDependentImpostorVisionOnSabotage", true)]
    public static bool VampireDependentImpostorVisionOnSabotage;

    [CustomOptionBool("VampireCannotFixSabotage", true)]
    public static bool VampireCannotFixSabotage;

    [CustomOptionBool("VampireInvisibleOnAdmin", true)]
    public static bool VampireInvisibleOnAdmin;

    [CustomOptionBool("VampireNoDeathOnVitals", true)]
    public static bool VampireNoDeathOnVitals;

    [CustomOptionBool("VampireCannotUseDevice", true)]
    public static bool VampireCannotUseDevice;
}
public class VampireAbility : AbilityBase
{
    public override void AttachToAlls()
    {
        base.AttachToAlls();
    }
}