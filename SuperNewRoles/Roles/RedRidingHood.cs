using System.Linq;
using SuperNewRoles.Helpers;
using SuperNewRoles.Patch;

namespace SuperNewRoles.Roles
{
    public class RedRidingHood
    {
        public static void WrapUp(GameData.PlayerInfo player)
        {
            Logger.Info($"あ:{PlayerControl.LocalPlayer.isDead()} && {PlayerControl.LocalPlayer.isRole(CustomRPC.RoleId.NiceRedRidingHood)}");
            if (PlayerControl.LocalPlayer.isDead() && PlayerControl.LocalPlayer.isRole(CustomRPC.RoleId.NiceRedRidingHood))
            {
                Logger.Info("い:"+RoleClass.NiceRedRidingHood.Count);
                if (RoleClass.NiceRedRidingHood.Count >= 1)
                {
                    DeadPlayer deadPlayer = DeadPlayer.deadPlayers?.Where(x => x.player?.PlayerId == CachedPlayer.LocalPlayer.PlayerId)?.FirstOrDefault();
                    if (deadPlayer.killerIfExisting == null) return;
                    var killer = PlayerControl.AllPlayerControls.ToArray().ToList().FirstOrDefault((PlayerControl a)=> a.PlayerId == deadPlayer.killerIfExistingId);
                    
                        Logger.Info($"え:{killer.isDead()} || {killer.PlayerId == player.Object.PlayerId}");
                    
                    if (killer != null && (killer.isDead() || killer.PlayerId == player.Object.PlayerId))
                    {
                        Logger.Info($"お:{!EvilEraser.IsBlock(EvilEraser.BlockTypes.RedRidingHoodRevive, killer)}");
                        if (EvilEraser.IsOKAndTryUse(EvilEraser.BlockTypes.RedRidingHoodRevive, killer))
                        {
                            var Writer = RPCHelper.StartRPC(CustomRPC.CustomRPC.ReviveRPC);
                            Writer.Write(CachedPlayer.LocalPlayer.PlayerId);
                            Writer.EndRPC();
                            CustomRPC.RPCProcedure.ReviveRPC(CachedPlayer.LocalPlayer.PlayerId);
                            Writer = RPCHelper.StartRPC(CustomRPC.CustomRPC.CleanBody);
                            Writer.Write(CachedPlayer.LocalPlayer.PlayerId);
                            Writer.EndRPC();
                            RoleClass.NiceRedRidingHood.deadbodypos = null;
                            CustomRPC.RPCProcedure.CleanBody(CachedPlayer.LocalPlayer.PlayerId);
                            RoleClass.NiceRedRidingHood.Count--;
                            CachedPlayer.LocalPlayer.Data.IsDead = false;

                            RoleClass.NiceRedRidingHood.deadbodypos = null;
                            DeadPlayer.deadPlayers?.RemoveAll(x => x.player?.PlayerId == CachedPlayer.LocalPlayer.PlayerId);
                            //Logger.Info("やったぜ");
                        }
                    }
                }
            }
        }
    }
}
