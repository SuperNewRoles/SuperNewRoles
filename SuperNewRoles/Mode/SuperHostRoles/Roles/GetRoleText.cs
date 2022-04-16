using SuperNewRoles.CustomRPC;
using SuperNewRoles.Roles;
using System;
using System.Collections.Generic;
using System.Text;

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
                if (p.isRole(RoleId.DoubralKiller))
                {
                    if (!(!RoleClass.DoubralKiller.IsSuicideViewsL.ContainsKey(p.PlayerId) || !RoleClass.DoubralKiller.IsSuicideViewsL[p.PlayerId]))
                    {
                        if (RoleClass.DoubralKiller.SuicideTimersL.TryGetValue(p.PlayerId, out float Time))
                        {
                            returndata = ModHelpers.cs(RoleClass.DoubralKiller.color, "(" + ((int)Time + 1).ToString() + ")");
                        }
                    }
                    if (!(!RoleClass.DoubralKiller.IsSuicideViewsR.ContainsKey(p.PlayerId) || !RoleClass.DoubralKiller.IsSuicideViewsR[p.PlayerId]))
                    {
                        if (RoleClass.DoubralKiller.SuicideTimersR.TryGetValue(p.PlayerId, out float Time))
                        {
                            returndata = ModHelpers.cs(RoleClass.DoubralKiller.color, "(" + ((int)Time + 1).ToString() + ")");
                        }
                    }
                }
            }
            SuperNewRolesPlugin.Logger.LogInfo("returnデータ:"+returndata);
            return returndata;
        }
        public static string GetNameTextPostfix(PlayerControl p)
        {
            string returndata = "";
            return returndata;
        }
    }
}
