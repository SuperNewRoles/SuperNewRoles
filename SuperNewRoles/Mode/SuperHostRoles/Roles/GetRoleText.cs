
using SuperNewRoles.Roles;

namespace SuperNewRoles.Mode.SuperHostRoles.Roles
{
    public static class GetRoleTextClass
    {
        public static string GetRoleTextPostfix(PlayerControl p)
        {
            string returndata = "";
            if (p.IsAlive())
            {
                if (p.IsRole(RoleId.SerialKiller))
                {
                    if (!(!RoleClass.SerialKiller.IsSuicideViews.ContainsKey(p.PlayerId) || !RoleClass.SerialKiller.IsSuicideViews[p.PlayerId]))
                    {
                        if (RoleClass.SerialKiller.SuicideTimers.TryGetValue(p.PlayerId, out float Time))
                        {
                            returndata = ModHelpers.Cs(RoleClass.SerialKiller.color, "(" + ((int)Time + 1).ToString() + ")");
                        }
                    }
                }
            }
            SuperNewRolesPlugin.Logger.LogInfo("[SHR:GetRoleText] Return Data:" + returndata);
            return returndata;
        }
        public static string GetNameTextPostfix()
        {
            string returndata = "";
            return returndata;
        }
    }
}