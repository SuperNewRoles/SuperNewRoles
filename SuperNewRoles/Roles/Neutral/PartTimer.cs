using System;
using System.Collections.Generic;
using System.Text;
namespace SuperNewRoles.Roles.Neutral
{
    public static class PartTimer
    {
        //ここにコードを書きこんでください
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
            if (!PlayerControl.LocalPlayer.IsRole(CustomRPC.RoleId.PartTimer)) return;
            if (RoleClass.PartTimer.DeathTurn <= 0)
            {
                PlayerControl.LocalPlayer.RpcMurderPlayer(PlayerControl.LocalPlayer);
            }
            RoleClass.PartTimer.DeathTurn--;
        }
    }
}