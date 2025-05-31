using System;
using System.Collections.Generic;
using UnityEngine;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Roles.Ability;
using SuperNewRoles.Modules;

namespace SuperNewRoles.Roles.Crewmate;

class Celebrity : RoleBase<Celebrity>
{
    public override RoleId Role { get; } = RoleId.Celebrity;
    public override Color32 RoleColor { get; } = new Color32(255, 215, 0, byte.MaxValue); // 黄金色
    public override List<Func<AbilityBase>> Abilities { get; } = [() => new CelebrityAbility(
        new CelebrityData() {
            EnableGlowEffect = CelebrityEnableGlowEffect,
            GlowOnlyWhileAlive = CelebrityGlowOnlyWhileAlive,
            YellowChangedRole = CelebrityYellowChangedRole
        })];

    public override QuoteMod QuoteMod { get; } = QuoteMod.SuperNewRoles;
    public override RoleTypes IntroSoundType { get; } = RoleTypes.Crewmate;
    public override short IntroNum { get; } = 1;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Crewmate;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Crewmate;
    public override TeamTag TeamTag { get; } = TeamTag.Crewmate;
    public override RoleTag[] RoleTags { get; } = [];
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Crewmate;

    // タスクフェイズ中、設定したキルクール秒数毎に画面を光らせるか
    [CustomOptionBool("CelebrityEnableGlowEffect", true)]
    public static bool CelebrityEnableGlowEffect;

    // 画面が光るのは生きている間のみか(会議更新)
    [CustomOptionBool("CelebrityGlowOnlyWhileAlive", true, parentFieldName: nameof(CelebrityEnableGlowEffect))]
    public static bool CelebrityGlowOnlyWhileAlive;

    // 役職が変わっても名前の色がスターのままになるか
    [CustomOptionBool("CelebrityYellowChangedRole", true)]
    public static bool CelebrityYellowChangedRole;
}