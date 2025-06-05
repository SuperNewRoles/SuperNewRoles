using System;
using System.Collections.Generic;
using UnityEngine;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Roles.Ability;
using SuperNewRoles.Modules;
using SuperNewRoles.Events;
using SuperNewRoles.Events.PCEvents;
using SuperNewRoles.Modules.Events.Bases;

namespace SuperNewRoles.Roles.Impostor;

class EvilSeer : RoleBase<EvilSeer>
{
    public override RoleId Role { get; } = RoleId.EvilSeer;
    public override Color32 RoleColor { get; } = Palette.ImpostorRed;
    public override List<Func<AbilityBase>> Abilities { get; } = [
        () => new SeerAbility(new SeerData() {
            Mode = EvilSeerSeerMode,
            LimitSoulDuration = EvilSeerLimitSoulDuration,
            SoulDuration = EvilSeerSoulDuration,
            ColorMode = EvilSeerAbilityColorModeSelect is EvilSeerAbility.All or EvilSeerAbility.SoulOnly ? EvilSeerColorMode : CrewMate.DeadBodyColorMode.None
        }),
        () => new DeadBodyArrowsAbility(() => EvilSeerShowArrows, colorMode : EvilSeerAbilityColorModeSelect is EvilSeerAbility.All or EvilSeerAbility.ArrowOnly ? EvilSeerColorMode : CrewMate.DeadBodyColorMode.None),
    ];
    public override QuoteMod QuoteMod { get; } = QuoteMod.SuperNewRoles;
    public override RoleTypes IntroSoundType { get; } = RoleTypes.Phantom;
    public override short IntroNum { get; } = 1;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Impostor;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Impostor;
    public override TeamTag TeamTag { get; } = TeamTag.Impostor;
    public override RoleTag[] RoleTags { get; } = [RoleTag.Information];
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Impostor;


    [CustomOptionSelect("EvilSeer.SeerColorMode", typeof(CrewMate.DeadBodyColorMode), "Seer.SeerColorMode.", translationName: "Seer.SeerColorMode")]
    public static CrewMate.DeadBodyColorMode EvilSeerColorMode = CrewMate.DeadBodyColorMode.None;
    [CustomOptionSelect("EvilSeer.AbilityColorModeSelect", typeof(EvilSeerAbility), "EvilSeer.AbilityColorModeSelect.", translationName: "EvilSeer.AbilityColorModeSelect", parentFieldName: nameof(EvilSeerColorMode))]
    public static EvilSeerAbility EvilSeerAbilityColorModeSelect = EvilSeerAbility.SoulOnly;


    [CustomOptionSelect("EvilSeer.Mode", typeof(CrewMate.SeerMode), "Seer.Mode.", translationName: "Seer.Mode")]
    public static CrewMate.SeerMode EvilSeerSeerMode = CrewMate.SeerMode.Both;


    [CustomOptionBool("EvilSeer.LimitSoulDuration", false, translationName: "Seer.LimitSoulDuration")]
    public static bool EvilSeerLimitSoulDuration = false;

    [CustomOptionFloat("EvilSeer.SoulDuration", 2.5f, 60f, 2.5f, 30f, translationName: "Seer.SoulDuration", parentFieldName: nameof(EvilSeerLimitSoulDuration))]
    public static float EvilSeerSoulDuration = 30f;


    [CustomOptionBool("EvilSeer.ShowArrows", true, translationName: "VultureShowArrows")]
    public static bool EvilSeerShowArrows;
}

public enum EvilSeerAbility
{
    All,
    SoulOnly,
    ArrowOnly
}