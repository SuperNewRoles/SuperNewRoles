using Hazel;
using SuperNewRoles.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

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
        public static bool IsBlock(BlockTypes blocktype,PlayerControl player = null)
        {
            if (player == null) player = PlayerControl.LocalPlayer;
            if (!player.isRole(CustomRPC.RoleId.EvilEraser)) return false;
            if (RoleClass.EvilEraser.Counts.ContainsKey(player.PlayerId) && RoleClass.EvilEraser.Counts[player.PlayerId] <= 0)
            {
                return false;
            }
            switch (blocktype)
            {
                case (BlockTypes.StuntmanGuard):
                    return true;
                case (BlockTypes.ClergymanLightOut):
                    return true;
                case (BlockTypes.BaitReport):
                    return true;
                case (BlockTypes.RedRidingHoodRevive):
                    return true;
                case (BlockTypes.JackalSidekick):
                    return true;
                case (BlockTypes.NekomataExiled):
                    return true;
                case (BlockTypes.FoxGuard):
                    return true;
            }
            return false;
        }
        public static bool IsBlockAndTryUse(BlockTypes blocktype,PlayerControl player = null)
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
        public static bool IsOKAndTryUse(BlockTypes blocktype,PlayerControl player = null) {
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
            if (player == null){
                return false;
            } else
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
            if (player == null){
                return false;
            } else
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
