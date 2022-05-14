using SuperNewRoles.CustomRPC;
using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNewRoles
{
    class ChacheManager
    {
        public static Dictionary<int, RoleId> MyRoleChache;
        public static Dictionary<int, PlayerControl> LoversChache;
        public static Dictionary<int, PlayerControl> QuarreledChache;
        public static void Load()
        {
            MyRoleChache = new Dictionary<int, RoleId>();
            LoversChache = new Dictionary<int, PlayerControl>();
            QuarreledChache = new Dictionary<int, PlayerControl>();
        }
        public static void ResetChache()
        {
            ResetQuarreledChache();
            ResetLoversChache();
            ResetMyRoleChache();
        }
        public static void ResetQuarreledChache()
        {
            foreach (PlayerControl p in PlayerControl.AllPlayerControls)
            {
                if (p.IsQuarreled(false))
                {
                    QuarreledChache[p.PlayerId] = p.GetOneSideQuarreled(false);
                }
                else
                {
                    QuarreledChache[p.PlayerId] = null;
                }
            }
        }
        public static void ResetLoversChache()
        {
            foreach (PlayerControl p in PlayerControl.AllPlayerControls)
            {
                if (p.IsLovers(false))
                {
                    LoversChache[p.PlayerId] = p.GetOneSideLovers(false);
                } else
                {
                    LoversChache[p.PlayerId] = null;
                }
            }
        }
        public static void ResetMyRoleChache()
        {
            foreach (PlayerControl p in PlayerControl.AllPlayerControls)
            {
                SuperNewRolesPlugin.Logger.LogInfo("SetLog");
                MyRoleChache[p.PlayerId] = p.getRole(false);
            }
        }
    }
}
