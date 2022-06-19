using Hazel;
using SuperNewRoles.Helpers;

namespace SuperNewRoles.Roles
{
    public static class EvilEraser
    {
        public enum BlockTypes
        {
            StuntmanGuard,
            MadStuntmanGuard,
            ClergymanLightOut,
            BaitReport,
            RedRidingHoodRevive,
            JackalSidekick,
            JackalSeerSidekick,
            NekomataExiled,
            FoxGuard
        }
        public static bool IsBlock(BlockTypes blocktype, PlayerControl player = null)
        {
            if (player == null) player = PlayerControl.LocalPlayer;
            if (!player.isRole(CustomRPC.RoleId.EvilEraser)) return false;
            if (RoleClass.EvilEraser.Counts.ContainsKey(player.PlayerId) && RoleClass.EvilEraser.Counts[player.PlayerId] <= 0)
            {
                return false;
            }
            return blocktype switch
            {
                BlockTypes.StuntmanGuard => true,
                BlockTypes.ClergymanLightOut => true,
                BlockTypes.BaitReport => true,
                BlockTypes.RedRidingHoodRevive => true,
                BlockTypes.JackalSidekick => true,
                BlockTypes.NekomataExiled => true,
                BlockTypes.FoxGuard => true,
                _ => false,
            };
        }
        public static bool IsBlockAndTryUse(BlockTypes blocktype, PlayerControl player = null)
        {
            bool BlockData = IsBlock(blocktype, player);
            if (BlockData)
            {
                UseCount(player);
            }
            return BlockData;
        }
        public static void UseCount(PlayerControl player)
        {
            MessageWriter writer = RPCHelper.StartRPC(CustomRPC.CustomRPC.UseEraserCount);
            writer.Write(player.PlayerId);
            writer.EndRPC();
            CustomRPC.RPCProcedure.UseEraserCount(player.PlayerId);
        }
        public static bool IsOKAndTryUse(BlockTypes blocktype, PlayerControl player = null)
        {
            return !IsBlockAndTryUse(blocktype, player);
        }
        public static bool IsWinGodGuard = false;
        public static bool IsGodWinGuard()
        {
            bool IsAlive = false;
            foreach (PlayerControl p in RoleClass.God.GodPlayer)
            {
                if (p.isAlive())
                {
                    IsAlive = true;
                }
            }
            if (!IsAlive)
            {
                return false;
            }
            if (IsWinGodGuard)
            {
                return true;
            }
            PlayerControl player = GetOnCount();
            if (player == null)
            {
                return false;
            }
            else
            {
                IsWinGodGuard = true;
                UseCount(player);
            }
            return false;
        }

        public static bool IsWinFoxGuard = false;
        public static bool IsFoxWinGuard()
        {
            bool IsAlive = false;
            foreach (PlayerControl p in RoleClass.Fox.FoxPlayer)
            {
                if (p.isAlive())
                {
                    IsAlive = true;
                }
            }
            if (!IsAlive)
            {
                return false;
            }
            if (IsWinFoxGuard)
            {
                return true;
            }
            PlayerControl player = GetOnCount();
            if (player == null)
            {
                return false;
            }
            else
            {
                IsWinFoxGuard = true;
                UseCount(player);
            }
            return false;
        }
        public static PlayerControl GetOnCount()
        {

            foreach (PlayerControl player in RoleClass.EvilEraser.EvilEraserPlayer)
            {
                if (!(RoleClass.EvilEraser.Counts.ContainsKey(player.PlayerId) && RoleClass.EvilEraser.Counts[player.PlayerId] <= 0))
                {
                    return player;
                }
            }
            return null;
        }
    }
}
