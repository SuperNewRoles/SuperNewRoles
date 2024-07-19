using System;
using System.Collections.Generic;
using SuperNewRoles.Helpers;
using SuperNewRoles.Patches;
using SuperNewRoles.Roles;

namespace SuperNewRoles.Mode.SuperHostRoles.Roles;

class Jester
{
    public static void WrapUp(NetworkedPlayerInfo exiled)
    {
        if (!AmongUsClient.Instance.AmHost) return;
        if (exiled.Object.IsRole(RoleId.Jester))
        {
            var (Complete, all) = TaskCount.TaskDateNoClearCheck(exiled);
            if (!RoleClass.Jester.IsJesterTaskClearWin || Complete >= all)
            {
                try
                {
                    var Writer = RPCHelper.StartRPC(CustomRPC.ShareWinner);
                    Writer.Write(exiled.Object.PlayerId);
                    Writer.EndRPC();
                    RPCProcedure.ShareWinner(exiled.Object.PlayerId);
                    Writer = RPCHelper.StartRPC(CustomRPC.SetWinCond);
                    Writer.Write((byte)CustomGameOverReason.JesterWin);
                    Writer.EndRPC();
                    RPCProcedure.SetWinCond((byte)CustomGameOverReason.JesterWin);
                    var winplayers = new List<PlayerControl>
                        {
                            exiled.Object
                        };
                    //EndGameCheck.WinNeutral(winplayers);
                    Chat.WinCond = CustomGameOverReason.JesterWin;
                    Chat.Winner = new List<PlayerControl>
                        {
                            exiled.Object
                        };
                }
                catch (Exception e)
                {
                    SuperNewRolesPlugin.Logger.LogInfo("[SHR:Error] Jester WrapUp Error:" + e);
                }
                EndGameCheck.CustomEndGame(MapUtilities.CachedShipStatus, CustomGameOverReason.JesterWin, false);
            }
        }
        else if (exiled.Object.IsRole(RoleId.MadJester))
        {
            var (Complete, all) = TaskCount.TaskDateNoClearCheck(exiled);
            if (!RoleClass.MadJester.IsMadJesterTaskClearWin || Complete >= all)
            {
                try
                {
                    exiled.Object.RpcSetFinalStatus(FinalStatus.MadJesterExiled);
                    var Writer = RPCHelper.StartRPC(CustomRPC.ShareWinner);
                    Writer.Write(exiled.Object.PlayerId);
                    Writer.EndRPC();
                    RPCProcedure.ShareWinner(exiled.Object.PlayerId);
                    Writer = RPCHelper.StartRPC(CustomRPC.SetWinCond);
                    Writer.Write((byte)CustomGameOverReason.ImpostorWin);
                    Writer.EndRPC();
                    RPCProcedure.SetWinCond((byte)CustomGameOverReason.ImpostorWin);
                    var winplayers = new List<PlayerControl>
                        {
                            exiled.Object
                        };
                    //EndGameCheck.WinNeutral(winplayers);
                    Chat.WinCond = CustomGameOverReason.ImpostorWin;
                    Chat.Winner = new List<PlayerControl>
                        {
                            exiled.Object
                        };
                }
                catch (Exception e)
                {
                    SuperNewRolesPlugin.Logger.LogInfo("[SHR:Error] Mad Jester WrapUp Error:" + e);
                }
                EndGameCheck.CustomEndGame(MapUtilities.CachedShipStatus, (CustomGameOverReason)GameOverReason.ImpostorByVote, false);
            }
        }
    }
}