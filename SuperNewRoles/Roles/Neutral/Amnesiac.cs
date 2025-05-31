using System;
using System.Collections.Generic;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability;
using UnityEngine;

namespace SuperNewRoles.Roles.Neutral;

class Amnesiac : RoleBase<Amnesiac>
{
    public override RoleId Role { get; } = RoleId.Amnesiac;
    public override Color32 RoleColor { get; } = new Color32(128, 128, 128, byte.MaxValue); // グレー
    public override List<Func<AbilityBase>> Abilities { get; } =
    [
        () => new AmnesiacAbility(),
        () => new DeadBodyArrowsAbility(() => AmnesiacShowDeadBodyArrows, Amnesiac.Instance.RoleColor)
    ];

    public override QuoteMod QuoteMod { get; } = QuoteMod.SuperNewRoles;
    public override RoleTypes IntroSoundType { get; } = RoleTypes.Crewmate;
    public override short IntroNum { get; } = 1;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Neutral;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Neutral;
    public override TeamTag TeamTag { get; } = TeamTag.Neutral;
    public override RoleTag[] RoleTags { get; } = [];
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Neutral;
    public override RoleId[] RelatedRoleIds { get; } = [];

    // 役職オプションの設定
    [CustomOptionBool("AmnesiacShowDeadBodyArrows", false)]
    public static bool AmnesiacShowDeadBodyArrows;
}