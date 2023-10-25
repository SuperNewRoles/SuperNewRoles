using System;
using System.Collections.Generic;
using Hazel;
using SuperNewRoles.Helpers;
using SuperNewRoles.Mode;
using SuperNewRoles.Mode.SuperHostRoles;
using SuperNewRoles.Patches;

namespace SuperNewRoles.Roles;

public static class FalseCharges
{
    public static void WrapUp(PlayerControl exiled)
    {
        if (ModeHandler.IsMode(ModeId.Default))
        {
            if (exiled != null)
            {
                if (PlayerControl.LocalPlayer.IsRole(RoleId.FalseCharges) && PlayerControl.LocalPlayer.IsDead() && !CachedPlayer.LocalPlayer.Data.Disconnected && RoleClass.FalseCharges.Turns != 255)
                {
                    if (RoleClass.FalseCharges.Turns <= 0) return;
                    if (exiled.PlayerId == RoleClass.FalseCharges.FalseChargePlayer)
                    {
                        RPCProcedure.ShareWinner(CachedPlayer.LocalPlayer.PlayerId);
                        MessageWriter Writer = RPCHelper.StartRPC((byte)CustomRPC.ShareWinner);
                        Writer.Write(CachedPlayer.LocalPlayer.PlayerId);
                        Writer.EndRPC();
                        if (AmongUsClient.Instance.AmHost)
                        {
                            CheckGameEndPatch.CustomEndGame((GameOverReason)CustomGameOverReason.FalseChargesWin, false);
                        }
                        else
                        {
                            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CustomEndGame, SendOption.Reliable, -1);
                            writer.Write((byte)CustomGameOverReason.FalseChargesWin);
                            writer.Write(false);
                            AmongUsClient.Instance.FinishRpcImmediately(writer);
                        }
                    }
                }
            }
            RoleClass.FalseCharges.Turns--;
        }
        else if (ModeHandler.IsMode(ModeId.SuperHostRoles))
        {
            if (exiled != null)
            {
                foreach (var data in RoleClass.FalseCharges.FalseChargePlayers)
                {
                    if (exiled.PlayerId == data.Value && !exiled.Data.Disconnected)
                    {
                        if (RoleClass.FalseCharges.AllTurns.ContainsKey(data.Key) && RoleClass.FalseCharges.AllTurns[data.Key] > 0)
                        {
                            try
                            {
                                foreach (PlayerControl p in CachedPlayer.AllPlayers)
                                {
                                    if (!p.Data.Disconnected && p.PlayerId != data.Key)
                                    {
                                        p.RpcMurderPlayer(p, true);
                                    }
                                }
                                var player = ModHelpers.PlayerById(data.Key);
                                var Writer = RPCHelper.StartRPC(CustomRPC.ShareWinner);
                                Writer.Write(player.PlayerId);
                                Writer.EndRPC();
                                RPCProcedure.ShareWinner(player.PlayerId);
                                Writer = RPCHelper.StartRPC(CustomRPC.SetWinCond);
                                Writer.Write((byte)CustomGameOverReason.FalseChargesWin);
                                Writer.EndRPC();
                                RPCProcedure.SetWinCond((byte)CustomGameOverReason.FalseChargesWin);
                                var winplayers = new List<PlayerControl>
                                    {
                                        player
                                    };
                                //EndGameCheck.WinNeutral(winplayers);
                                Mode.SuperHostRoles.Chat.WinCond = CustomGameOverReason.FalseChargesWin;
                                Mode.SuperHostRoles.Chat.Winner = new List<PlayerControl>
                                    {
                                        player
                                    };
                            }
                            catch (Exception e)
                            {
                                SuperNewRolesPlugin.Logger.LogInfo("[SHR]冤罪師WrapUpエラー:" + e);
                            }
                            EndGameCheck.CustomEndGame(MapUtilities.CachedShipStatus, GameOverReason.HumansByVote, false);
                        }
                    }
                }
            }
            foreach (var data in RoleClass.FalseCharges.AllTurns)
            {
                RoleClass.FalseCharges.AllTurns[data.Key]--;
            }
        }
    }
}