using System;
using System.Collections.Generic;
using Hazel;

using SuperNewRoles.Helpers;
using SuperNewRoles.Mode;

namespace SuperNewRoles.Roles
{
    public static class Demon
    {
        public static void DemonCurse(this PlayerControl target, PlayerControl source = null)
        {
            try
            {
                if (source == null) source = PlayerControl.LocalPlayer;
                MessageWriter Writer = RPCHelper.StartRPC(CustomRPC.DemonCurse);
                Writer.Write(source.PlayerId);
                Writer.Write(target.PlayerId);
                Writer.EndRPC();
                RPCProcedure.DemonCurse(source.PlayerId, target.PlayerId);
            }
            catch (Exception e)
            {
                SuperNewRolesPlugin.Logger.LogError(e);
            }
        }

        public static List<PlayerControl> GetCurseData(this PlayerControl player)
        {
            return RoleClass.Demon.CurseDatas.ContainsKey(player.PlayerId) ? RoleClass.Demon.CurseDatas[player.PlayerId] : new();
        }

        public static List<PlayerControl> GetUntarget()
        {
            return RoleClass.Demon.CurseDatas.ContainsKey(CachedPlayer.LocalPlayer.PlayerId)
                ? RoleClass.Demon.CurseDatas[CachedPlayer.LocalPlayer.PlayerId]
                : (new());
        }

        public static bool IsCursed(this PlayerControl source, PlayerControl target)
        {
            if (source == null || source.Data.Disconnected || target == null || target.IsDead() || target.IsBot()) return true;
            if (source.PlayerId == target.PlayerId) return true;
            if (RoleClass.Demon.CurseDatas.ContainsKey(source.PlayerId))
            {
                if (RoleClass.Demon.CurseDatas[source.PlayerId].IsCheckListPlayerControl(target))
                {
                    return true;
                }
            }
            return false;
        }

        public static List<PlayerControl> GetIconPlayers(PlayerControl player = null)
        {
            if (player == null) player = PlayerControl.LocalPlayer;
            return RoleClass.Demon.CurseDatas.ContainsKey(player.PlayerId) ? RoleClass.Demon.CurseDatas[player.PlayerId] : (new());
        }
        public static bool IsViewIcon(PlayerControl player)
        {
            if (player == null) return false;
            foreach (var data in RoleClass.Demon.CurseDatas)
            {
                foreach (PlayerControl Player in data.Value)
                {
                    if (player.PlayerId == Player.PlayerId)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool IsButton()
        {
            return RoleHelpers.IsAlive(PlayerControl.LocalPlayer) && PlayerControl.LocalPlayer.IsRole(RoleId.Demon) && ModeHandler.IsMode(ModeId.Default);
        }

        public static bool IsWin(PlayerControl Demon)
        {
            foreach (PlayerControl player in CachedPlayer.AllPlayers)
            {
                if (player.PlayerId != Demon.PlayerId && !IsCursed(Demon, player))
                {
                    return false;
                }
            }
            return !RoleClass.Demon.IsAliveWin || !Demon.IsDead();
        }

        public static bool IsDemonWinFlag()
        {
            foreach (PlayerControl player in RoleClass.Demon.DemonPlayer)
            {
                if (IsWin(player))
                {
                    return true;
                }
            }
            return false;
        }
    }
}