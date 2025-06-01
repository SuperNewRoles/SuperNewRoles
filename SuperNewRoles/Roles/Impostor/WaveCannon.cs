using System;
using System.Collections.Generic;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability;
using UnityEngine;

namespace SuperNewRoles.Roles.Impostor;

class WaveCannon : RoleBase<WaveCannon>
{
    public override RoleId Role { get; } = RoleId.WaveCannon;
    public override Color32 RoleColor { get; } = Palette.ImpostorRed;
    public override List<Func<AbilityBase>> Abilities { get; } = [
        () => new WaveCannonAbility(WaveCannonCooldown, WaveCannonDuration, (WaveCannonType)AnimationTypeOption, IsSyncKillCoolTime)
    ];

    public override QuoteMod QuoteMod { get; } = QuoteMod.SuperNewRoles;
    public override RoleTypes IntroSoundType { get; } = RoleTypes.Shapeshifter;
    public override short IntroNum { get; } = 1;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Impostor;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Impostor;
    public override TeamTag TeamTag { get; } = TeamTag.Impostor;
    public override RoleTag[] RoleTags { get; } = [RoleTag.SpecialKiller];
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Impostor;

    [CustomOptionFloat("WaveCannonCooldown", 2.5f, 180f, 2.5f, 20f)]
    public static float WaveCannonCooldown;

    [CustomOptionFloat("WaveCannonDuration", 0.5f, 15f, 0.5f, 3f)]
    public static float WaveCannonDuration;

    [CustomOptionBool("WaveCannonSyncKillCoolTime", false)]
    public static bool IsSyncKillCoolTime;
    [CustomOptionSelect("WaveCannonAnimationType", typeof(WaveCannonTypeForOption), "WaveCannonAnimationType.")]
    public static WaveCannonTypeForOption AnimationTypeOption;
}

