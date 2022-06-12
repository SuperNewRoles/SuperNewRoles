using SuperNewRoles.CustomRPC;
using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNewRoles
{
    class ChacheManager
    {
        public static Dictionary<int, RoleId> MyRoleChache;
        public static Dictionary<int, RoleId> MyGhostRoleChache;
        public static Dictionary<int, PlayerControl> LoversChache;
        public static Dictionary<int, PlayerControl> QuarreledChache;
        public static void Load()
        {
            MyRoleChache = new Dictionary<int, RoleId>();
            MyGhostRoleChache = new Dictionary<int, RoleId>();
            LoversChache = new Dictionary<int, PlayerControl>();
            QuarreledChache = new Dictionary<int, PlayerControl>();
        }
        public static void ResetChache()
        {
            ResetQuarreledChache();
            ResetLoversChache();
            ResetMyRoleChache();
            ResetMyGhostRoleChache();
        }
        public static void ResetQuarreledChache()
        {
            foreach (PlayerControl p in CachedPlayer.AllPlayers)
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
            foreach (PlayerControl p in CachedPlayer.AllPlayers)
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
            foreach (PlayerControl p in CachedPlayer.AllPlayers)
            {
                MyRoleChache[p.PlayerId] = p.getRole(false);
            }
        }
        public static void ResetMyGhostRoleChache()
        {
            foreach (PlayerControl p in CachedPlayer.AllPlayers)
            {
                MyGhostRoleChache[p.PlayerId] = p.getGhostRole(false);
            }
        }
    }
}
