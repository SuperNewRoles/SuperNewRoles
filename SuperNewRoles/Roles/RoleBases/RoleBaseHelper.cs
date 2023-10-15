using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperNewRoles.Roles.Neutral;
using HarmonyLib;

namespace SuperNewRoles.Roles.RoleBases;
public static class RoleBaseHelper
{
    //public static Dictionary<RoleId, Type> allRoleIds = new()
    //{
    ////{ RoleId.SidekickWaveCannon, typeof(RoleBase<SidekickWaveCannon>) },
    //};

    public static void ClearAndReload()
    {
        RoleInfo.AllRoleInfo.Do(x => x.Value.ClearAndReloader());
    }
    public static void SetUpOptions()
    {
        try
        {
            foreach (var type in Assembly.GetAssembly(typeof(Role)).GetTypes()
                    .Where(t => t.IsSubclassOf(typeof(Role)) && !t.IsAbstract))
            {
                type.GetMethod("GetRoleInfo").Invoke(null, null);
            }
            RoleInfo.AllRoleInfo.Do(x => x.Value.OptionCreator());
        }
        catch (Exception ex) { Logger.Info($"SetUpOptions Error {ex.ToString()}", "★RoleBaseHelper");  }
        //new SidekickWaveCannon().SetUpOption();
    }
    public static void SetRole(PlayerControl player, RoleId roleId)
    {
        if (!RoleInfo.AllRoleInfo.TryGetValue(roleId, out var roleInfo)) return;

        var roleBase = (RoleBase)Activator.CreateInstance(roleInfo.ClassType);
        roleBase.SetRole(player);
    }
    public static void EraseRole(PlayerControl player, RoleId roleId)
    {
        if (!RoleInfo.AllRoleInfo.TryGetValue(roleId, out var roleInfo)) return;

        var roleBase = (RoleBase)Activator.CreateInstance(roleInfo.ClassType);
        roleBase.EraseRole(player);
    }
}