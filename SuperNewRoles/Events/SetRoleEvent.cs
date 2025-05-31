using SuperNewRoles.Modules;
using SuperNewRoles.Modules.Events.Bases;
using SuperNewRoles.Roles;

namespace SuperNewRoles.Events;

public class SetRoleEventData : IEventData
{
    public ExPlayerControl player { get; }
    public RoleId oldRole { get; }
    public RoleId newRole { get; }
    public SetRoleEventData(ExPlayerControl player, RoleId oldRole, RoleId newRole)
    {
        this.player = player;
        this.oldRole = oldRole;
        this.newRole = newRole;
    }
}

public class SetRoleEvent : EventTargetBase<SetRoleEvent, SetRoleEventData>
{
    public static void Invoke(ExPlayerControl player, RoleId oldRole, RoleId newRole)
    {
        var data = new SetRoleEventData(player, oldRole, newRole);
        Instance.Awake(data);
    }
}
