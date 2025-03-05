using AmongUs.GameOptions;
using SuperNewRoles.Events;
using SuperNewRoles.Events.PCEvents;
using SuperNewRoles.Modules;
using SuperNewRoles.Modules.Events.Bases;

namespace SuperNewRoles.Roles.Ability;

public class PromoteOnParentDeathAbility : AbilityBase
{
    public AbilityParentBase Owner { get; }
    public RoleId PromoteRole { get; }
    public RoleTypes PromoteRoleVanilla { get; }

    private EventListener<DieEventData> DieEventListener;
    private EventListener<DisconnectEventData> DisconnectEventListener;

    public PromoteOnParentDeathAbility(AbilityParentBase owner, RoleId promoteRole, RoleTypes promoteRoleVanilla)
    {
        Owner = owner;
        PromoteRole = promoteRole;
        PromoteRoleVanilla = promoteRoleVanilla;
    }

    public override void AttachToLocalPlayer()
    {
        DieEventListener = DieEvent.Instance.AddListener(OnDie);
        DisconnectEventListener = DisconnectEvent.Instance.AddListener(OnDisconnect);
    }
    public override void DetachToLocalPlayer()
    {
        base.DetachToLocalPlayer();
        DieEvent.Instance.RemoveListener(DieEventListener);
        DisconnectEvent.Instance.RemoveListener(DisconnectEventListener);
    }
    private void OnDisconnect(DisconnectEventData data)
    {
        if (Owner.Player == null) return;
        if (data.disconnectedPlayer == null) return;
        if (Owner.Player.PlayerId == data.disconnectedPlayer.PlayerId)
        {
            Promote();
        }
    }
    private void OnDie(DieEventData data)
    {
        if (Owner.Player == null) return;
        if (data.player == null) return;
        if (Owner.Player.PlayerId == data.player.PlayerId)
        {
            Promote();
        }
    }
    private void Promote()
    {
        if (Owner.Player == null) return;
        ExPlayerControl exPlayer = Player;
        if (exPlayer.Role == PromoteRole) return;
        RpcPromote(exPlayer, PromoteRole, PromoteRoleVanilla);
    }
    [CustomRPC]
    public static void RpcPromote(ExPlayerControl player, RoleId roleId, RoleTypes roleType)
    {
        player.SetRole(roleId);
        RoleManager.Instance.SetRole(player, roleType);
        NameText.UpdateAllNameInfo();
    }
}

