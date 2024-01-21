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
    public static Dictionary<string, HashSet<RoleBase>> RoleBaseTypes { get; private set; } = new();
    private static Dictionary<string, HashSet<RoleBase>> AllInterfaces = new();
    private static HashSet<IFixedUpdaterAll> fixedUpdaterAlls;
    public static void ClearAndReloads()
    {
        PlayerRoles = new();
        RoleBaseTypes = new();
        AllInterfaces = new();
        fixedUpdaterAlls = new();
    }
    public static IReadOnlySet<IFixedUpdaterAll> GetFixedUpdaterAlls()
    {
        return fixedUpdaterAlls;
    }
    public static IReadOnlyList<T> GetInterfaces<T>()
    {
        if (!AllInterfaces.TryGetValue(typeof(T).Name, out HashSet<RoleBase> RoleBases) ||
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
        if (!RoleBaseTypes.ContainsKey(roleInfo.RoleObjectTypeName))
            RoleBaseTypes.Add(roleInfo.RoleObjectTypeName, new());
        RoleBaseTypes[roleInfo.RoleObjectTypeName].Add(roleBase);
        //全てのインターフェイスを取得
        Type roleType = roleInfo.RoleObjectType;
        Type[] Interfaces = roleType.GetInterfaces();
        foreach (Type Interface in Interfaces)
        {
            if (!AllInterfaces.TryGetValue(Interface.Name, out HashSet<RoleBase> IRoleBases))
                AllInterfaces[Interface.Name] = IRoleBases = new(1);
            IRoleBases.Add(roleBase);
        }
        if (roleBase is IFixedUpdaterAll fixedUpdaterAll)
            fixedUpdaterAlls.Add(fixedUpdaterAll);
        return roleBase;
    }
    public static void ClearRole(PlayerControl player, RoleBase roleBase)
    {
        roleBase.Dispose();
        PlayerRoles.Remove(player);
        RoleBaseTypes[roleBase.Roleinfo.RoleObjectTypeName].Remove(roleBase);
        //Interfacesから削除
        Type roleType = roleBase.Roleinfo.RoleObjectType;
        Type[] Interfaces = roleType.GetInterfaces();
        foreach (Type Interface in Interfaces)
        {
            if (AllInterfaces.TryGetValue(Interface.Name, out HashSet<RoleBase> IRoleBases))
                IRoleBases.Remove(roleBase);
        }
        if (roleBase is IFixedUpdaterAll fixedUpdaterAll)
            fixedUpdaterAlls.Remove(fixedUpdaterAll);
    }
    public static RoleBase GetLocalRoleBase()
    {
        return PlayerRoles.Local;
    }
    public static bool TryGetLocalRoleBase(out RoleBase result)
    {
        result = PlayerRoles.Local;
        return result != null;
    }
    public static T GetLocalRoleBase<T>() where T : RoleBase
    {
        return PlayerRoles.Local as T;
    }
    public static bool TryGetLocalRoleBase<T>(out T result) where T : RoleBase
    {
        result = PlayerRoles.Local as T;
        return result != null;
    }
    public static RoleBase GetRoleBaseById(this byte PlayerId)
    {
        return PlayerRoles[PlayerId];
    }
    public static bool TryGetRoleBaseById(this byte PlayerId, out RoleBase result)
    {
        result = PlayerRoles[PlayerId];
        return result != null;
    }
    public static RoleBase GetRoleBase(this PlayerControl player)
    {
        return PlayerRoles[player];
    }
    public static bool TryGetRoleBase(this PlayerControl player, out RoleBase result)
    {
        result = PlayerRoles[player];
        return result != null;
    }
    public static IReadOnlyList<T> GetRoleBases<T>() where T : RoleBase
    {
        return RoleBaseTypes.TryGetValue(typeof(T).Name, out HashSet<RoleBase> value) ? value.Cast<T>().ToList() : new();
    }
    public static T GetRoleBase<T>(this PlayerControl player) where T : RoleBase
    {
        return PlayerRoles[player] as T;
    }
    public static bool TryGetRoleBase<T>(this PlayerControl player, out T result) where T : RoleBase
    {
        result = PlayerRoles[player] as T;
        return result != null;
    }
}
