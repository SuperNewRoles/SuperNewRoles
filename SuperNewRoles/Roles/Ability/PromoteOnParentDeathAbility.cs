using System;
using AmongUs.GameOptions;
using SuperNewRoles.Events;
using SuperNewRoles.Events.PCEvents;
using SuperNewRoles.Modules;
using SuperNewRoles.Modules.Events.Bases;
using System.Linq;

namespace SuperNewRoles.Roles.Ability;

public class PromoteOnParentDeathAbility : AbilityBase
{
    public AbilityParentRole Owner { get; }
    public RoleId PromoteRole { get; }
    public RoleTypes PromoteRoleVanilla { get; }
    public Action<ExPlayerControl> OnPromoted { get; set; } = (player) => { };

    private EventListener _fixedUpdateEventListener;
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
        _fixedUpdateEventListener = FixedUpdateEvent.Instance.AddListener(OnFixedUpdate);
    }
    public override void DetachToLocalPlayer()
    {
        base.DetachToLocalPlayer();
        _fixedUpdateEventListener?.RemoveListener();
    }
    private void OnFixedUpdate()
    {
        if (_hasPromoted) return;
        if (Owner != null && Owner.Player != null && Owner.Player.IsAlive()) return;
        Promote();
        _hasPromoted = true;
    }
    private void Promote()
    {
        ExPlayerControl exPlayer = Player;
        if (exPlayer.Role == PromoteRole) return;
        if (exPlayer.IsDead()) return;

        RpcPromote(exPlayer, PromoteRole, PromoteRoleVanilla);
        // Playerはこの時点でnullになってるのでexPlayerを渡す
        OnPromoted?.Invoke(exPlayer);
    }
    [CustomRPC]
    public static void RpcPromote(ExPlayerControl player, RoleId roleId, RoleTypes roleType)
    {
        player.SetRole(roleId);
        RoleManager.Instance.SetRole(player, roleType);
        NameText.UpdateAllNameInfo();
    }
}

