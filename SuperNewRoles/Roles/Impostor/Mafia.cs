using System;
using System.Collections.Generic;
using UnityEngine;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Roles.Ability;
using SuperNewRoles.Modules;
using System.Linq;

namespace SuperNewRoles.Roles.Impostor;

class Mafia : RoleBase<Mafia>
{
    public override RoleId Role { get; } = RoleId.Mafia;
    public override Color32 RoleColor { get; } = Palette.ImpostorRed;
    public override List<Func<AbilityBase>> Abilities { get; } = [
        () => new MafiaAbility()
    ];

    public override QuoteMod QuoteMod { get; } = QuoteMod.SuperNewRoles;
    public override RoleTypes IntroSoundType { get; } = RoleTypes.Impostor;
    public override short IntroNum { get; } = 1;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Impostor;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Impostor;
    public override TeamTag TeamTag { get; } = TeamTag.Impostor;
    public override RoleTag[] RoleTags { get; } = [];
    public override RoleOptionMenuType OptionTeam => RoleOptionMenuType.Impostor;

    /// <summary>
    /// キルが可能かどうかを判定する
    /// 生き残っている自分以外のインポスターが全員マフィアにならないとキルできない
    /// </summary>
    public static bool IsKillFlag()
    {
        return !ExPlayerControl.ExPlayerControls.Any(x => x.IsAlive() && x.IsImpostor() && x.Role != RoleId.Mafia);
    }
}

public class MafiaAbility : AbilityBase
{
    public override void AttachToAlls()
    {
        base.AttachToAlls();

        // バニラキルボタンを制御するためのCustomKillButtonAbilityを追加
        Player.AttachAbility(new CustomKillButtonAbility(
            canKill: () => Mafia.IsKillFlag(),
            killCooldown: () => PlayerControl.LocalPlayer.killTimer,
            onlyCrewmates: () => true
        ), new AbilityParentAbility(this));
    }
}
