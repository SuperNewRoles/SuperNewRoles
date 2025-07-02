using System;
using System.Collections.Generic;
using SuperNewRoles.Events;
using SuperNewRoles.Modules;
using SuperNewRoles.Modules.Events.Bases;

namespace SuperNewRoles.Roles.Ability;

public class HideMyRoleWhenAliveAbility : AbilityBase
{
    /// <summary>Abilityを有しているのは役職か(モデファイラではないか)</summary>
    private readonly bool _hasHideRole;
    /// <summary>Abilityを有しているのはモデファイラか(役職ではないか)</summary>
    private readonly bool _hasHideModifier;

    /// <summary>Abilityを有しているモデファイラのModifierRoleId</summary>
    private readonly ModifierRoleId? HideModifierRoleId;

    public HideMyRoleWhenAliveAbility(ModifierRoleId? modifierRoleId = null)
    {
        _hasHideRole = modifierRoleId == null;
        _hasHideModifier = modifierRoleId != null;

        HideModifierRoleId = modifierRoleId;
    }

    public RoleId FalseRoleId(ExPlayerControl player) => player == null || player.Player == null
        ? RoleId.Crewmate
        : _hasHideRole
            ? (player.Data.Role.IsImpostor ? RoleId.Impostor : RoleId.Crewmate)
            : player.Role;

    static string CrewmateName => ModHelpers.CsWithTranslation(CrewMate.Crewmate.Instance.RoleColor, CrewMate.Crewmate.Instance.Role.ToString());
    static string ImpostorName => ModHelpers.CsWithTranslation(Impostor.Impostor.Instance.RoleColor, Impostor.Impostor.Instance.Role.ToString());

    /// <summary>非表示にする条件が満たされているかの確認</summary>
    /// <param name="player">確認対象</param>
    /// <returns>true => 満たしている(非表示) / false => 満たしていない(表示)</returns>
    public (bool role, bool modifier) IsHide(ExPlayerControl player) => player == null || player.Player == null || !player.AmOwner || player.IsDead()
        ? (false, false)
        : (_hasHideRole, _hasHideModifier);

    /// <summary>
    /// モディファイアを非表示にする条件が満たされているかの確認
    /// 及び 隠す対象のモディファイアであるかの確認
    /// </summary>
    /// <param name="player">確認対象のプレイヤー</param>
    /// <param name="id">確認対象のModifierRoleId</param>
    /// <returns>true => 満たしている(非表示) / false => 満たしていない or 対象のModifierRoleIdでない(表示)</returns>
    public bool IsCheckTargetModifierRoleHidden(ExPlayerControl player, ModifierRoleId id) => IsHide(player).modifier && HideModifierRoleId == id;

    /// <summary>非表示の条件が満たされている場合、役職名を上書きする</summary>
    /// <param name="player">確認対象</param>
    /// <param name="roleName">役職名</param>
    public void DisplayRoleName(ExPlayerControl player, ref string roleName)
    {
        if (!IsHide(player).role) return;
        roleName = $"{(player.Data.Role.IsImpostor ? ImpostorName : CrewmateName)}";
    }

    /// <summary>役職表示用のRoleBaseを取得する</summary>
    /// <param name="player">取得対象</param>
    /// <param name="roleBase">表示する役職のRoleBase</param>
    public void DisplayFalseRoleBase(ExPlayerControl player, ref IRoleBase roleBase)
    {
        if (!IsHide(player).role) return;

        var canGetRoleBase = CustomRoleManager.TryGetRoleById(
                player.Data.Role.IsImpostor
                ? RoleId.Impostor
                : RoleId.Crewmate, out var falseRole
                );

        if (!canGetRoleBase) return;

        roleBase = falseRole;
    }
}