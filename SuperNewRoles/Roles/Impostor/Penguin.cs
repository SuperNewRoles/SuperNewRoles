using System;
using System.Collections.Generic;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability;
using UnityEngine;

namespace SuperNewRoles.Roles.Impostor;

class Penguin : RoleBase<Penguin>
{
    public override RoleId Role { get; } = RoleId.Penguin;
    public override Color32 RoleColor { get; } = Palette.ImpostorRed;
    public override List<Func<AbilityBase>> Abilities { get; } = [
        () => new PenguinAbility(PenguinCooldown, PenguinDuration, PenguinMeetingKill, PenguinCanDefaulKill)
    ];

    public override QuoteMod QuoteMod { get; } = QuoteMod.SuperNewRoles;
    public override RoleTypes IntroSoundType { get; } = RoleTypes.Shapeshifter;
    public override short IntroNum { get; } = 1;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Impostor;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Impostor;
    public override TeamTag TeamTag { get; } = TeamTag.Impostor;
    public override RoleTag[] RoleTags { get; } = [RoleTag.SpecialKiller];
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Impostor;

    [CustomOptionFloat("PenguinCooldown", 5f, 60f, 2.5f, 30f)]
    public static float PenguinCooldown;

    [CustomOptionFloat("PenguinDuration", 5f, 60f, 2.5f, 15f)]
    public static float PenguinDuration;

    [CustomOptionBool("PenguinMeetingKill", true)]
    public static bool PenguinMeetingKill;

    [CustomOptionBool("PenguinCanDefaulKill", true)]
    public static bool PenguinCanDefaulKill;
}