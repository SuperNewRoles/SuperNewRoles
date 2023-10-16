using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SuperNewRoles.Roles.Role;
using SuperNewRoles.Roles.RoleBases.Interfaces;

namespace SuperNewRoles.Roles.RoleBases;
public static class RoleBaseManager
{
    public static PlayerData<RoleBase> PlayerRoles { get; private set; } = new();
    private static Dictionary<Type, List<RoleBase>> AllInterfaces = new();
    public static readonly List<RoleInfo> RoleInfos = new();
    public static void ClearAndReloads()
    {
        PlayerRoles = new();
        AllInterfaces = new();
    }
    public static IReadOnlyList<T> GetInterfaces<T>()
    {
        if (!AllInterfaces.TryGetValue(typeof(T), out List<RoleBase> RoleBases) ||
            RoleBases == null)
            return new List<T>();
        return RoleBases.Cast<T>().ToList();
    }
    public static RoleBase SetRole(PlayerControl player, RoleId role)
    {
        //処理を後で書く
        RoleInfo roleInfo = RoleInfoManager.GetRoleInfo(role);
        RoleBase roleBase = roleInfo.CreateInstance(player);
        PlayerRoles[player] = roleBase;
        //全てのインターフェイスを取得
        Type roleType = roleInfo.RoleObjectType;
        Type[] Interfaces = roleType.GetInterfaces();
        foreach (Type Interface in Interfaces)
        {
            if (!AllInterfaces.TryGetValue(Interface, out List<RoleBase> RoleBases))
                AllInterfaces[Interface] = RoleBases = new(1);
            RoleBases.Add(roleBase);
        }
        return roleBase;
    }
    public static RoleBase GetRoleBaseById(this byte PlayerId)
    {
        return PlayerRoles[PlayerId];
    }
    public static RoleBase GetRoleBase(this PlayerControl player)
    {
        return PlayerRoles[player];
    }
}
