using System;
using System.Collections.Generic;
using Hazel;
using SuperNewRoles.CustomRPC;
using SuperNewRoles.Intro;
using SuperNewRoles.Mode;
using SuperNewRoles.Roles;
using System.Linq;

namespace SuperNewRoles.Roles
{
    public class Tactician
    {
        public static bool CanSeeRoles(PlayerControl p1)
        {
            if (RoleClass.Tactician.AlliancePlayer.ContainsKey(p1.PlayerId))
            {
                byte p2 = RoleClass.Tactician.AlliancePlayer[p1.PlayerId];
                PlayerControl p4 = null;
                foreach (PlayerControl p3 in CachedPlayer.AllPlayers)
                {
                    if (p3.PlayerId == p2) p4 = p3;
                }
                return p1.IsDead() || p4.IsDead();
            }
            return false;
        }
    }
}