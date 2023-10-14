using System.Linq;
using SuperNewRoles.Helpers;

namespace SuperNewRoles.Roles;

public class RedRidingHood
{
    public static void WrapUp(GameData.PlayerInfo player)
    {
        if (PlayerControl.LocalPlayer.IsDead() && PlayerControl.LocalPlayer.IsRole(RoleId.NiceRedRidingHood))
        {
            Logger.Info("い:" + RoleClass.NiceRedRidingHood.Count);
            if (RoleClass.NiceRedRidingHood.Count >= 1)
            {
                DeadPlayer deadPlayer = DeadPlayer.deadPlayers?.Where(x => x.player?.PlayerId == CachedPlayer.LocalPlayer.PlayerId)?.FirstOrDefault();
                if (deadPlayer.killerIfExisting == null) return;
                var killer = PlayerControl.AllPlayerControls.FirstOrDefault((PlayerControl a) => a.PlayerId == deadPlayer.killerIfExistingId);

                if (killer != null && ((CustomOptionHolder.NiceRedRidinIsKillerDeathRevive.GetBool() && killer.IsDead()) || (player != null && killer.PlayerId == player.Object.PlayerId)))
                {
                    Logger.Info($"お:{!EvilEraser.IsBlock(EvilEraser.BlockTypes.RedRidingHoodRevive, killer)}");
                    if (EvilEraser.IsOKAndTryUse(EvilEraser.BlockTypes.RedRidingHoodRevive, killer))
                    {
                        var Writer = RPCHelper.StartRPC(CustomRPC.ReviveRPC);
                        Writer.Write(CachedPlayer.LocalPlayer.PlayerId);
                        Writer.EndRPC();
                        RPCProcedure.ReviveRPC(CachedPlayer.LocalPlayer.PlayerId);
                        Writer = RPCHelper.StartRPC(CustomRPC.CleanBody);
                        Writer.Write(CachedPlayer.LocalPlayer.PlayerId);
                        Writer.EndRPC();
                        RoleClass.NiceRedRidingHood.deadbodypos = null;
                        RPCProcedure.CleanBody(CachedPlayer.LocalPlayer.PlayerId);
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