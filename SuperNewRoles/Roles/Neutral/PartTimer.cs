using System;
using System.Collections.Generic;
using System.Text;
using SuperNewRoles.Helpers;

namespace SuperNewRoles.Roles.Neutral
{
    public static class PartTimer
    {
        public static void FixedUpdate()
        {
            foreach (var data in RoleClass.PartTimer.PlayerDatas)
            {
                if (data.Value.IsDead())
                {
                    if (data.Key.PlayerId == CachedPlayer.LocalPlayer.PlayerId)
                    {
                        RoleClass.PartTimer.DeathTurn = RoleClass.PartTimer.DeathDefaultTurn;
                    }
                    RoleClass.PartTimer.Datas.Remove(data.Key.PlayerId);
                }
            }
        }
        public static void WrapUp()
        {
            if (!PlayerControl.LocalPlayer.IsRole(RoleId.PartTimer)) return;
            if (RoleClass.PartTimer.DeathTurn <= 0 && CachedPlayer.LocalPlayer.IsAlive() && !RoleClass.PartTimer.IsLocalOn)
            {
                PlayerControl.LocalPlayer.RpcExiledUnchecked();
            }
            RoleClass.PartTimer.DeathTurn--;
        }
    }
}