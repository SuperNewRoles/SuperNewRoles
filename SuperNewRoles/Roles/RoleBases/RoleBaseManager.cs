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
        RoleInfo roleInfo = RoleInfoManager.GetRoleInfo(role);
        if (roleInfo == null)
            return null;
        RoleBase roleBase = roleInfo.CreateInstance(player);
        PlayerRoles[player] = roleBase;
        //全てのインターフェイスを取得
        Type roleType = roleInfo.RoleObjectType;
        Type[] Interfaces = roleType.GetInterfaces();
        foreach (Type Interface in Interfaces)
        {
            if (!AllInterfaces.TryGetValue(Interface, out List<RoleBase> IRoleBases))
                AllInterfaces[Interface] = IRoleBases = new(1);
            IRoleBases.Add(roleBase);
        }
        return roleBase;
    }
    public static void ClearRole(PlayerControl player, RoleBase roleBase)
    {
        roleBase.Dispose();
        PlayerRoles.Remove(player);
        //Interfacesから削除
        Type roleType = roleBase.Roleinfo.RoleObjectType;
        Type[] Interfaces = roleType.GetInterfaces();
        foreach (Type Interface in Interfaces)
        {
            if (AllInterfaces.TryGetValue(Interface, out List<RoleBase> IRoleBases))
                IRoleBases.Remove(roleBase);
        }
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
