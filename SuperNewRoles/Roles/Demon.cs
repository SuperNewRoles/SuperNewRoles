using Hazel;
using SuperNewRoles.CustomRPC;
using SuperNewRoles.Helpers;
using SuperNewRoles.Mode;
using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNewRoles.Roles
{
    public static class Demon
    {
        public static void DemonCurse(this PlayerControl target, PlayerControl source = null)
        {
            try
            {
                if (source == null) source = PlayerControl.LocalPlayer;
                MessageWriter Writer = RPCHelper.StartRPC(CustomRPC.CustomRPC.DemonCurse);
                Writer.Write(source.PlayerId);
                Writer.Write(target.PlayerId);
                Writer.EndRPC();
                RPCProcedure.DemonCurse(source.PlayerId, target.PlayerId);
            } catch (Exception e)
            {
                SuperNewRolesPlugin.Logger.LogError(e);
            }
        }

        public static List<PlayerControl> GetCurseData(this PlayerControl player)
        {
            return RoleClass.Demon.CurseDatas.ContainsKey(player.PlayerId) ? RoleClass.Demon.CurseDatas[player.PlayerId] : new List<PlayerControl>();
        }

        public static List<PlayerControl> GetUntarget()
        {
            if (RoleClass.Demon.CurseDatas.ContainsKey(CachedPlayer.LocalPlayer.PlayerId))
            {
                return RoleClass.Demon.CurseDatas[CachedPlayer.LocalPlayer.PlayerId];
            }
            return new List<PlayerControl>();
        }

        public static bool IsCursed(this PlayerControl source, PlayerControl target)
        {
            if (source == null || source.Data.Disconnected || target == null || target.isDead() || target.IsBot()) return true;
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
            if (RoleClass.Demon.CurseDatas.ContainsKey(player.PlayerId))
            {
                return RoleClass.Demon.CurseDatas[player.PlayerId];
            }
            return new List<PlayerControl>();
        }
        public static bool IsViewIcon(PlayerControl player) {
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

        public static bool IsButton() {
            return RoleHelpers.isAlive(PlayerControl.LocalPlayer) && PlayerControl.LocalPlayer.isRole(RoleId.Demon) && ModeHandler.isMode(ModeId.Default);
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
            if (RoleClass.Demon.IsAliveWin && Demon.isDead()) return false;
            return true;
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
