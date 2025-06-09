using System;
using System.Collections.Generic;
using SuperNewRoles.Roles.Ability;
using SuperNewRoles.Modules;
using UnityEngine;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions;

namespace SuperNewRoles.Roles.Impostor;

class Nun : RoleBase<Nun>
{
    public override RoleId Role { get; } = RoleId.Nun;
    public override Color32 RoleColor { get; } = Palette.ImpostorRed;
    public override List<Func<AbilityBase>> Abilities { get; } = [
        () => new NunAbility(NunCoolTime)
    ];

    public override QuoteMod QuoteMod { get; } = QuoteMod.SuperNewRoles;
    public override RoleTypes IntroSoundType { get; } = RoleTypes.Impostor;
    public override short IntroNum { get; } = 1;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Impostor;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Impostor;
    public override TeamTag TeamTag { get; } = TeamTag.Impostor;
    public override RoleTag[] RoleTags { get; } = [];
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Impostor;
    public override MapNames[] AvailableMaps { get; } = [MapNames.Airship];

    [CustomOptionFloat("NunCoolTime", 0f, 180f, 2.5f, 30f, translationName: "CoolTime")]
    public static float NunCoolTime;
}