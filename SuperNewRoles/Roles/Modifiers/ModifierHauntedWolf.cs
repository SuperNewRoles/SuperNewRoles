using System;
using System.Collections.Generic;
using System.Linq;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability;
using UnityEngine;

namespace SuperNewRoles.Roles.Modifiers;

class ModifierHauntedWolf : ModifierBase<ModifierHauntedWolf>
{
    public override ModifierRoleId ModifierRole => ModifierRoleId.ModifierHauntedWolf;
    public override Color32 RoleColor => CrewMate.HauntedWolf.Instance.RoleColor;
    public override List<Func<AbilityBase>> Abilities { get; } = [() => new HideMyRoleWhenAliveAbility(ModifierRoleId.ModifierHauntedWolf)];

    public override QuoteMod QuoteMod { get; } = QuoteMod.SuperNewRoles;

    public override List<AssignedTeamType> AssignedTeams => [AssignedTeamType.Crewmate];

    public override WinnerTeamType WinnerTeam => WinnerTeamType.Crewmate;

    public override RoleTag[] RoleTags => [];

    public override short IntroNum => 1;
    public override Func<ExPlayerControl, string> ModifierMark => (player) => "{0}" + ModHelpers.Cs(RoleColor, "狼");
    public override bool AssignFilter => ModifierHauntedWolfIsAssignMadAndFriendRoles;

    public override RoleId[] DoNotAssignRoles => CanNotAssignRoles();

    /// <summary>RoleId列挙型から、フィルター対象のマッド役職及びフレンズ役職のRoleIdを取得する</summary>
    /// <returns>フィルター対象の役職のRoleId配列</returns>
    static RoleId[] CanNotAssignRoles()
    {
        // 基本のフィルター
        List<RoleId> NotAssignRoles = [RoleId.HauntedWolf];

        // 設定が有効でないなら
        if (!ModifierHauntedWolfIsAssignMadAndFriendRoles) return [.. NotAssignRoles];

        // RoleId に "Mad" とつく役職を取得する
        List<RoleId> madRoles = [RoleId.BlackCat, RoleId.Worshiper, RoleId.BlackSanta, RoleId.SatsumaAndImo]; // 特殊な命名は 手動で追加
        madRoles.AddRange(Enum.GetValues<RoleId>().Where(id => id.ToString().StartsWith("Mad"))); // 列挙型から 自動取得

        List<RoleId> friendRoles = [RoleId.JackalFriends]; // まだJFしかいないので 手動での追加

        NotAssignRoles.AddRange(madRoles); // マッドロールをフィルター配列に追加
        NotAssignRoles.AddRange(friendRoles); // フレンズロールをフィルター配列に追加

        return [.. NotAssignRoles];
    }

    public readonly static List<RoleId> MadAndFriendRoleId = new();

    [CustomOptionBool("ModifierHauntedWolfIsAssignMadAndFriendRoles", true)]
    public static bool ModifierHauntedWolfIsAssignMadAndFriendRoles = true;

    [CustomOptionBool("ModifierHauntedWolfIsReverseSheriffDecision", true)]
    public static bool ModifierHauntedWolfIsReverseSheriffDecision = true;

}