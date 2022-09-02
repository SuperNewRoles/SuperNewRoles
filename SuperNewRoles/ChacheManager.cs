using System.Collections.Generic;
using SuperNewRoles.CustomRPC;

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
                QuarreledChache[p.PlayerId] = p.IsQuarreled(false) ? p.GetOneSideQuarreled(false) : null;
            }
        }
        public static void ResetLoversChache()
        {
            foreach (PlayerControl p in CachedPlayer.AllPlayers)
            {
                LoversChache[p.PlayerId] = p.IsLovers(false) ? p.GetOneSideLovers(false) : null;
            }
        }
        public static void ResetMyRoleChache()
        {
            foreach (PlayerControl p in CachedPlayer.AllPlayers)
            {
                MyRoleChache[p.PlayerId] = p.GetRole(false);
            }
        }
        public static void ResetMyGhostRoleChache()
        {
            foreach (PlayerControl p in CachedPlayer.AllPlayers)
            {
                MyGhostRoleChache[p.PlayerId] = p.GetGhostRole(false);
            }
        }
    }
}