using System;
using System.Collections.Generic;
using System.Text;
using SuperNewRoles.CustomRPC;
using SuperNewRoles.Roles;

namespace SuperNewRoles.Mode.SuperHostRoles.Roles
{
    public static class GetRoleTextClass
    {
        public static string GetRoleTextPostfix(PlayerControl p)
        {
            string returndata = "";
            if (p.isAlive())
            {
                if (p.isRole(RoleId.SerialKiller))
                {
                    if (!(!RoleClass.SerialKiller.IsSuicideViews.ContainsKey(p.PlayerId) || !RoleClass.SerialKiller.IsSuicideViews[p.PlayerId]))
                    {
                        if (RoleClass.SerialKiller.SuicideTimers.TryGetValue(p.PlayerId, out float Time))
                        {
                            returndata = ModHelpers.cs(RoleClass.SerialKiller.color, "(" + ((int)Time + 1).ToString() + ")");
                        }
                    }
                }
            }
            SuperNewRolesPlugin.Logger.LogInfo("[SHR:GetRoleText] Return Data:" + returndata);
            return returndata;
        }
        public static string GetNameTextPostfix(PlayerControl p)
        {
            string returndata = "";
            return returndata;
        }
    }
}
