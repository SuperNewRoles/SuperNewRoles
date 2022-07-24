using System;
using System.Collections.Generic;
using System.Text;
using SuperNewRoles.CustomRPC;

namespace SuperNewRoles.Roles.Neutral
{
    public static class Spelunker
    {
        public static bool CheckSetRole(PlayerControl player, RoleId role)
        {
            if (player.IsRole(RoleId.Spelunker)) {
                if (role != RoleId.Spelunker)
                {
                    player.RpcMurderPlayer(player);
                    return false;
                }
            }
            return true;
        }
        //ここにコードを書きこんでください
    }
}