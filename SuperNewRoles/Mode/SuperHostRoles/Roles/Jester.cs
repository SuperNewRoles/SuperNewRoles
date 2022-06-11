using SuperNewRoles.EndGame;
using SuperNewRoles.Helpers;
using SuperNewRoles.Patch;
using SuperNewRoles.Roles;
using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNewRoles.Mode.SuperHostRoles.Roles
{
    class Jester
    {
        public static void WrapUp(GameData.PlayerInfo exiled)
        {
            if (!AmongUsClient.Instance.AmHost) return ;
            if (exiled.Object.isRole(CustomRPC.RoleId.Jester))
            {
                var (complate, all) = TaskCount.TaskDateNoClearCheck(exiled);
                if (!RoleClass.Jester.IsJesterTaskClearWin || complate >= all)
                {
                    try
                    {
                        var Writer = RPCHelper.StartRPC(CustomRPC.CustomRPC.ShareWinner);
                        Writer.Write(exiled.Object.PlayerId);
                        Writer.EndRPC();
                        CustomRPC.RPCProcedure.ShareWinner(exiled.Object.PlayerId);
                        Writer = RPCHelper.StartRPC(CustomRPC.CustomRPC.SetWinCond);
                        Writer.Write((byte)CustomGameOverReason.JesterWin);
                        Writer.EndRPC();
                        CustomRPC.RPCProcedure.SetWinCond((byte)CustomGameOverReason.JesterWin);
                        var winplayers = new List<PlayerControl>();
                        winplayers.Add(exiled.Object);
                        //EndGameCheck.WinNeutral(winplayers);
                        Chat.WinCond = CustomGameOverReason.JesterWin;
                        Chat.Winner = new List<PlayerControl>();
                        Chat.Winner.Add(exiled.Object);
                    }
                    catch (Exception e)
                    {
                        SuperNewRolesPlugin.Logger.LogInfo("[SHR]てるてるWrapUpエラー:"+e);
                    }
                    EndGameCheck.CustomEndGame(MapUtilities.CachedShipStatus, GameOverReason.HumansByVote, false);
                }
            } else if (exiled.Object.isRole(CustomRPC.RoleId.MadJester))
            {
                var (complate, all) = TaskCount.TaskDateNoClearCheck(exiled);
                if (!RoleClass.MadJester.IsMadJesterTaskClearWin || complate >= all)
                {
                    try
                    {
                        var Writer = RPCHelper.StartRPC(CustomRPC.CustomRPC.ShareWinner);
                        Writer.Write(exiled.Object.PlayerId);
                        Writer.EndRPC();
                        CustomRPC.RPCProcedure.ShareWinner(exiled.Object.PlayerId);
                        Writer = RPCHelper.StartRPC(CustomRPC.CustomRPC.SetWinCond);
                        Writer.Write((byte)CustomGameOverReason.JesterWin);
                        Writer.EndRPC();
                        CustomRPC.RPCProcedure.SetWinCond((byte)CustomGameOverReason.ImpostorWin);
                        var winplayers = new List<PlayerControl>();
                        winplayers.Add(exiled.Object);
                        //EndGameCheck.WinNeutral(winplayers);
                        Chat.WinCond = CustomGameOverReason.ImpostorWin;
                        Chat.Winner = new List<PlayerControl>();
                        Chat.Winner.Add(exiled.Object);
                    }
                    catch (Exception e)
                    {
                        SuperNewRolesPlugin.Logger.LogInfo("[SHR]マッドてるてるWrapUpエラー:" + e);
                    }
                    EndGameCheck.CustomEndGame(MapUtilities.CachedShipStatus, GameOverReason.ImpostorByVote, false);
                }
            }
        }
    }
}
