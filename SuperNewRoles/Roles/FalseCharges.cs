using Hazel;
using SuperNewRoles.EndGame;
using SuperNewRoles.Helpers;
using SuperNewRoles.Mode;
using SuperNewRoles.Mode.SuperHostRoles;
using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNewRoles.Roles
{
    public static class FalseCharges
    {
        public static void WrapUp(PlayerControl exiled)
        {
            if (ModeHandler.isMode(ModeId.Default))
            {
                if (exiled != null)
                {
                    if (PlayerControl.LocalPlayer.isDead() && RoleClass.FalseCharges.Turns != 255)
                    {
                        if (RoleClass.FalseCharges.Turns <= 0) return;
                        if (exiled.PlayerId == RoleClass.FalseCharges.FalseChargePlayer)
                        {
                            CustomRPC.RPCProcedure.ShareWinner(CachedPlayer.LocalPlayer.PlayerId);

                            MessageWriter Writer = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.ShareWinner, SendOption.Reliable, -1);
                            Writer.Write(CachedPlayer.LocalPlayer.PlayerId);
                            AmongUsClient.Instance.FinishRpcImmediately(Writer);
                            if (AmongUsClient.Instance.AmHost)
                            {
                                CheckGameEndPatch.CustomEndGame((GameOverReason)CustomGameOverReason.FalseChargesWin, false);
                            } else
                            {
                                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.CustomEndGame, SendOption.Reliable, -1);
                                writer.Write((byte)CustomGameOverReason.FalseChargesWin);
                                writer.Write(false);
                                AmongUsClient.Instance.FinishRpcImmediately(writer);
                            }
                        }
                    }
                }
                RoleClass.FalseCharges.Turns--;
            }
            else if (ModeHandler.isMode(ModeId.SuperHostRoles))
            {
                if (exiled != null)
                {
                    foreach (var data in RoleClass.FalseCharges.FalseChargePlayers)
                    {
                        if (exiled.PlayerId == data.Value)
                        {
                            if (RoleClass.FalseCharges.AllTurns.ContainsKey(data.Key) && RoleClass.FalseCharges.AllTurns[data.Key] > 0)
                            {
                                try
                                {
                                    foreach (PlayerControl p in CachedPlayer.AllPlayers)
                                    {
                                        if (!p.Data.Disconnected && p.PlayerId != data.Key)
                                        {
                                            p.RpcMurderPlayer(p);
                                        }
                                    }
                                    var player = ModHelpers.playerById(data.Key);
                                    var Writer = RPCHelper.StartRPC(CustomRPC.CustomRPC.ShareWinner);
                                    Writer.Write(player.PlayerId);
                                    Writer.EndRPC();
                                    CustomRPC.RPCProcedure.ShareWinner(player.PlayerId);
                                    Writer = RPCHelper.StartRPC(CustomRPC.CustomRPC.SetWinCond);
                                    Writer.Write((byte)CustomGameOverReason.FalseChargesWin);
                                    Writer.EndRPC();
                                    CustomRPC.RPCProcedure.SetWinCond((byte)CustomGameOverReason.FalseChargesWin);
                                    var winplayers = new List<PlayerControl>();
                                    winplayers.Add(player);
                                    //EndGameCheck.WinNeutral(winplayers);
                                    Chat.WinCond = CustomGameOverReason.FalseChargesWin;
                                    Chat.Winner = new List<PlayerControl>();
                                    Chat.Winner.Add(player);
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
}
