using SuperNewRoles.CustomRPC;
using SuperNewRoles.Helpers;
using SuperNewRoles.Patch;
using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNewRoles.Roles
{
    public static class Levelinger
    {
        public static void MurderPlayer(PlayerControl __instance,PlayerControl target)
        {
            if (__instance.PlayerId != CachedPlayer.LocalPlayer.PlayerId) return;
            if (__instance.isRole(RoleId.Levelinger))
            {
                RoleClass.Levelinger.ThisXP = RoleClass.Levelinger.ThisXP + RoleClass.Levelinger.OneKillXP;
            } else if (target.isRole(RoleId.Levelinger))
            {
                LevelingerRevive();
            }
        }
        public static void LevelingerRevive()
        {
            if (RoleClass.Levelinger.IsUseOKRevive)
            {
                if (RoleClass.Levelinger.ReviveUseXP <= RoleClass.Levelinger.ThisXP)
                {
                    var Writer = RPCHelper.StartRPC(CustomRPC.CustomRPC.ReviveRPC);
                    Writer.Write(CachedPlayer.LocalPlayer.PlayerId);
                    Writer.EndRPC();
                    CustomRPC.RPCProcedure.ReviveRPC(CachedPlayer.LocalPlayer.PlayerId);
                    Writer = RPCHelper.StartRPC(CustomRPC.CustomRPC.CleanBody);
                    Writer.Write(CachedPlayer.LocalPlayer.PlayerId);
                    Writer.EndRPC();
                    CustomRPC.RPCProcedure.CleanBody(CachedPlayer.LocalPlayer.PlayerId);
                    CachedPlayer.LocalPlayer.Data.IsDead = false;
                    DeadPlayer.deadPlayers?.RemoveAll(x => x.player?.PlayerId == CachedPlayer.LocalPlayer.PlayerId);
                    RoleClass.Levelinger.ThisXP -= RoleClass.Levelinger.ReviveUseXP;
                }
            }
        }
    }
}
