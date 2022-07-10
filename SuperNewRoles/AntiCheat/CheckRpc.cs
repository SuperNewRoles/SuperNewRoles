using System;
using System.Collections.Generic;
using System.Text;
using SuperNewRoles.CustomRPC;
using SuperNewRoles.Roles;

namespace SuperNewRoles.AntiCheat
{
    class CheckRpc
    {
        public static bool CheckRevive(PlayerControl player)
        {
            return true;
        }
        public static bool CheckSidekickPromotes(RoleId role)
        {
            switch (role)
            {
                case RoleId.Sidekick:
                    if (RoleClass.Jackal.SidekickPlayer.Count <= 0) return false;
                    foreach (PlayerControl p in RoleClass.Jackal.SidekickPlayer)
                    {
                        if (p.isAlive()) return false;
                    }
                    break;
                case RoleId.SidekickSeer:
                    if (RoleClass.JackalSeer.SidekickSeerPlayer.Count <= 0) return false;
                    foreach (PlayerControl p in RoleClass.JackalSeer.SidekickSeerPlayer)
                    {
                        if (p.isAlive()) return false;
                    }
                    break;
                default:
                    Logger.Error("CheckSidekickPromotesで不明なRoleIdを受け取りました:" + role, "AntiCheat");
                    return false;
            }
            return true;
        }
    }
}
