using System;
using System.Collections.Generic;
using UnityEngine;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Roles.Ability;
using SuperNewRoles.Roles.Ability.CustomButton;
using SuperNewRoles.Modules;
using SuperNewRoles.Events.PCEvents;
using SuperNewRoles.Modules.Events.Bases;
using SuperNewRoles.Ability;
using SuperNewRoles.Events;

namespace SuperNewRoles.Roles.Impostor;

class DarkKiller : RoleBase<DarkKiller>
{
    public override RoleId Role { get; } = RoleId.DarkKiller;
    public override Color32 RoleColor { get; } = Palette.ImpostorRed;
    public override List<Func<AbilityBase>> Abilities { get; } = [
        () => new DarkKillerAbility()
    ];

    public override QuoteMod QuoteMod { get; } = QuoteMod.SuperNewRoles;
    public override RoleTypes IntroSoundType { get; } = RoleTypes.Impostor;
    public override short IntroNum { get; } = 1;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Impostor;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Impostor;
    public override TeamTag TeamTag { get; } = TeamTag.Impostor;
    public override RoleTag[] RoleTags { get; } = [];
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Impostor;

    [CustomOptionFloat("DarkKillerKillCoolTime", 2.5f, 60f, 2.5f, 30f, translationName: "KillCoolTime")]
    public static float DarkKillerKillCoolTime;

    [CustomOptionBool("DarkKillerCanUseVent", true, translationName: "CanUseVent")]
    public static bool DarkKillerCanUseVent;
}

public class DarkKillerAbility : AbilityBase
{
    public override void AttachToAlls()
    {
        base.AttachToAlls();

        // カスタムキルボタンを追加
        Player.AttachAbility(new CustomKillButtonAbility(
            () => ModHelpers.IsElectrical(), // キルボタンが無効化されていない時のみ使用可能
            () => DarkKiller.DarkKillerKillCoolTime, // カスタムキルクールタイム
            () => true // クルーメイトのみをターゲット
        ), new AbilityParentAbility(this));

        // ベント使用能力
        Player.AttachAbility(new CustomVentAbility(
            () => DarkKiller.DarkKillerCanUseVent
        ), new AbilityParentAbility(this));
    }
}