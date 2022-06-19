using System.Linq;
using SuperNewRoles.Helpers;
using SuperNewRoles.Patch;

namespace SuperNewRoles.Roles
{
    public class RedRidingHood
    {
        public static void WrapUp(GameData.PlayerInfo player)
        {
            if (PlayerControl.LocalPlayer.isDead() && RoleHelpers.isRole(CustomRPC.RoleId.NiceRedRidingHood))
            {
                if (RoleClass.NiceRedRidingHood.Count >= 1)
                {
                    DeadPlayer deadPlayer = DeadPlayer.deadPlayers?.Where(x => x.player?.PlayerId == CachedPlayer.LocalPlayer.PlayerId)?.FirstOrDefault();
                    if (deadPlayer.killerIfExisting != null && deadPlayer.killerIfExisting.isDead())
                    {
                        if (EvilEraser.IsOKAndTryUse(EvilEraser.BlockTypes.RedRidingHoodRevive, deadPlayer.killerIfExisting))
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
                        }
                    }
                }
            }
        }
    }
}
