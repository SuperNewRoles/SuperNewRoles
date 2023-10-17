
using System;
using SuperNewRoles.Roles.RoleBases;
using SuperNewRoles.Roles.RoleBases.Interfaces;

namespace SuperNewRoles.Roles.Role;
public class RoleInfo
{
    public Type RoleObjectType { get; }
    public RoleId Role { get; }
    public string NameKey { get; }
    public TeamRoleType Team { get; }
    public bool IsGhostRole { get; }
    private Func<PlayerControl, RoleBase> _createInstance { get; }
    public RoleInfo(
        Type roleObjectType,
        Func<PlayerControl, RoleBase> createInstance,
        RoleId role,
        string namekey,
        TeamRoleType team
        )
    {
        this.RoleObjectType = roleObjectType;
        this.Role = role;
        this._createInstance = createInstance;
        this.NameKey = namekey;
        this.Team = team;
        this.IsGhostRole = RoleObjectType.IsSubclassOf(typeof(IGhostRole));
        RoleInfoManager.RoleInfos.Add(role, this);
    }
    public RoleBase CreateInstance(PlayerControl player)
    {
        if (_createInstance != null)
            return _createInstance(player);
        //Instance作成Functionが設定ていない場合はActivatorで作成
        return Activator.CreateInstance(RoleObjectType, player as object) as RoleBase;
    }
}