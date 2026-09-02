using System;
using AmongUs.GameOptions;
using SuperNewRoles.Events;
using SuperNewRoles.Modules;

namespace SuperNewRoles.Roles.Ability;

public class PromoteOnParentDeathAbility : AbilityBase
{
    public AbilityParentRole Owner { get; }
    public RoleId PromoteRole { get; }
    public RoleTypes PromoteRoleVanilla { get; }
    public Action<ExPlayerControl> OnPromoted { get; set; } = (player) => { };

    private bool _hasPromoted = false;

    public PromoteOnParentDeathAbility(AbilityParentRole owner, RoleId promoteRole, RoleTypes promoteRoleVanilla)
    {
        Owner = owner;
        PromoteRole = promoteRole;
        PromoteRoleVanilla = promoteRoleVanilla;
    }

    public override void AttachToLocalPlayer()
    {
        base.AttachToLocalPlayer();
        SubscribeWithAbility(FixedUpdateEvent.Instance, OnFixedUpdate);
    }
    private void OnFixedUpdate()
    {
        if (_hasPromoted) return;
        if (IsOwnerStillOriginalRole()) return;
        Promote();
        _hasPromoted = true;
    }
    private bool IsOwnerStillOriginalRole()
    {
        return Owner?.Player != null &&
            Owner.Player.IsAlive() &&
            Owner.ParentRole != null &&
            // 親Abilityの役職と実際に割り当てられている役職が違う場合は、親の役職が変わったとみなす
            Owner.Player.Role == Owner.ParentRole.Role;
    }
    private void Promote()
    {
        ExPlayerControl exPlayer = Player;
        if (exPlayer.Role == PromoteRole) return;
        if (exPlayer.IsDead()) return;

        RpcPromote(exPlayer, PromoteRole, PromoteRoleVanilla);
    }
    [CustomRPC]
    public static void RpcPromote(ExPlayerControl player, RoleId roleId, RoleTypes roleType)
    {
        if (player.IsDead()) return;

        // SetRole でこの Ability が外れる前に取る。受信側でも OnPromoted を実行する。
        Action<ExPlayerControl> onPromoted = null;
        foreach (var ability in player.GetAbilities<PromoteOnParentDeathAbility>())
        {
            if (ability.PromoteRole != roleId) continue;
            onPromoted += ability.OnPromoted;
        }

        player.SetRole(roleId);
        RoleManager.Instance.SetRole(player, roleType);
        NameText.UpdateAllNameInfo();
        // Playerはこの時点でnullになってるのでexPlayerを渡す
        onPromoted?.Invoke(player);
    }
}
