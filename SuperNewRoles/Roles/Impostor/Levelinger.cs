
using SuperNewRoles.Helpers;
using SuperNewRoles.Patches;

namespace SuperNewRoles.Roles
{
    public static class Levelinger
    {
        public static void MurderPlayer(PlayerControl __instance, PlayerControl target)
        {
            if (__instance.PlayerId != CachedPlayer.LocalPlayer.PlayerId) return;
            if (__instance.IsRole(RoleId.Levelinger))
            {
                RoleClass.Levelinger.ThisXP += RoleClass.Levelinger.OneKillXP;
            }
            else if (target.IsRole(RoleId.Levelinger))
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
                    var Writer = RPCHelper.StartRPC(CustomRPC.ReviveRPC);
                    Writer.Write(CachedPlayer.LocalPlayer.PlayerId);
                    Writer.EndRPC();
                    RPCProcedure.ReviveRPC(CachedPlayer.LocalPlayer.PlayerId);
                    Writer = RPCHelper.StartRPC(CustomRPC.CleanBody);
                    Writer.Write(CachedPlayer.LocalPlayer.PlayerId);
                    Writer.EndRPC();
                    RPCProcedure.CleanBody(CachedPlayer.LocalPlayer.PlayerId);
                    CachedPlayer.LocalPlayer.Data.IsDead = false;
                    DeadPlayer.deadPlayers?.RemoveAll(x => x.player?.PlayerId == CachedPlayer.LocalPlayer.PlayerId);
                    RoleClass.Levelinger.ThisXP -= RoleClass.Levelinger.ReviveUseXP;
                }
            }
        }
    }
}