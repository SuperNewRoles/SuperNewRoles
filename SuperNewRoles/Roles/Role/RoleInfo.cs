
using System;
using SuperNewRoles.Roles.RoleBases;

namespace SuperNewRoles.Roles.Role;
public class RoleInfo
{
    public Type RoleObjectType { get; }
    public RoleId Role { get; }
    private Func<PlayerControl, RoleBase> _createInstance { get; }
    public RoleInfo(
        Type roleObjectType,
        Func<PlayerControl, RoleBase> createInstance,
        RoleId role
        )
    {
        this.RoleObjectType = roleObjectType;
        this.Role = role;
        this._createInstance = createInstance;
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