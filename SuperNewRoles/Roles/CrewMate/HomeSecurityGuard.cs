using System;
using System.Collections.Generic;
using UnityEngine;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Roles.Ability;
using SuperNewRoles.Modules;

namespace SuperNewRoles.Roles.CrewMate;

class HomeSecurityGuard : RoleBase<HomeSecurityGuard>
{
    public override RoleId Role { get; } = RoleId.HomeSecurityGuard;
    public override Color32 RoleColor { get; } = new Color32(200, 200, 200, byte.MaxValue); // グレー色
    public override List<Func<AbilityBase>> Abilities { get; } = [() => new CustomTaskAbility(
        () => {
            var exPlayer = ExPlayerControl.LocalPlayer;
            if (exPlayer.PlayerId != PlayerControl.LocalPlayer.PlayerId) return (false, 0);
            return (true, 0); // タスク数を0に設定
        },
        new TaskOptionData(0, 0, 0) // タスク数を0に設定
    )];

    public override QuoteMod QuoteMod { get; } = QuoteMod.SuperNewRoles;
    public override RoleTypes IntroSoundType { get; } = RoleTypes.Crewmate;
    public override short IntroNum { get; } = 1;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Crewmate;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Crewmate;
    public override TeamTag TeamTag { get; } = TeamTag.Crewmate;
    public override RoleTag[] RoleTags { get; } = [];
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Crewmate;
}