using System.Collections;
using System.Collections.Generic;
using Hazel;
using SuperNewRoles.CustomRPC;
using SuperNewRoles.EndGame;
using SuperNewRoles.Helpers;
using SuperNewRoles.Patches;
using SuperNewRoles.Roles;
using UnityEngine;

namespace SuperNewRoles.Mode.SuperHostRoles
{
    class WrapUpClass
    {
        public static void WrapUp(GameData.PlayerInfo exiled)
        {
            if (!AmongUsClient.Instance.AmHost) return;
            FixedUpdate.SetRoleNames();
            foreach (PlayerControl p in BotManager.AllBots)
            {
                p.RpcSetName(p.GetDefaultName());
            }
            /*
            new LateTask(() =>
            {
                foreach (PlayerControl p in CachedPlayer.AllPlayers)
                {
                    byte reactorId = 3;
                    if (PlayerControl.GameOptions.MapId == 2) reactorId = 21;
                    MessageWriter MurderWriter = AmongUsClient.Instance.StartRpcImmediately(p.NetId, (byte)RpcCalls.MurderPlayer, SendOption.Reliable, p.GetClientId());
                    MessageExtensions.WriteNetObject(MurderWriter, BotHandler.Bot);
                    AmongUsClient.Instance.FinishRpcImmediately(MurderWriter);
                    MessageWriter SabotageWriter = AmongUsClient.Instance.StartRpcImmediately(MapUtilities.CachedShipStatus.NetId, (byte)RpcCalls.RepairSystem, SendOption.Reliable, p.GetClientId());
                    SabotageWriter.Write(reactorId);
                    MessageExtensions.WriteNetObject(SabotageWriter, p);
                    SabotageWriter.Write((byte)128);
                    AmongUsClient.Instance.FinishRpcImmediately(SabotageWriter);
                    MessageWriter SabotageFixWriter = AmongUsClient.Instance.StartRpcImmediately(MapUtilities.CachedShipStatus.NetId, (byte)RpcCalls.RepairSystem, SendOption.Reliable, p.GetClientId());
                    SabotageFixWriter.Write(reactorId);
                    MessageExtensions.WriteNetObject(SabotageFixWriter, p);
                    SabotageFixWriter.Write((byte)16);
                    AmongUsClient.Instance.FinishRpcImmediately(SabotageFixWriter);
                    if (PlayerControl.GameOptions.MapId == 4)
                    {
                        MessageWriter SabotageFixWriter2 = AmongUsClient.Instance.StartRpcImmediately(MapUtilities.CachedShipStatus.NetId, (byte)RpcCalls.RepairSystem, SendOption.Reliable, p.GetClientId());
                        SabotageFixWriter2.Write(reactorId);
                        MessageExtensions.WriteNetObject(SabotageFixWriter2, p);
                        SabotageFixWriter2.Write((byte)17);
                        AmongUsClient.Instance.FinishRpcImmediately(SabotageFixWriter2);
                    }
                }
            }, 5f, "AntiBlack");*/
            foreach (PlayerControl p in RoleClass.RemoteSheriff.RemoteSheriffPlayer)
            {
                if (p.IsAlive() && !p.IsMod()) p.RpcResetAbilityCooldown();
            }
            foreach (PlayerControl p in RoleClass.Arsonist.ArsonistPlayer)
            {
                if (p.IsAlive() && !p.IsMod()) p.RpcResetAbilityCooldown();
            }
            AmongUsClient.Instance.StartCoroutine(nameof(ResetName));

            static IEnumerator ResetName()
            {
                yield return new WaitForSeconds(1);
                FixedUpdate.SetRoleNames();
            }
            Roles.BestFalseCharge.WrapUp();
            if (exiled == null) return;
            if (exiled.Object.IsRole(RoleId.Sheriff) || exiled.Object.IsRole(RoleId.truelover) || exiled.Object.IsRole(RoleId.MadMaker))
            {
                exiled.Object.RpcSetRoleDesync(RoleTypes.GuardianAngel);
            }
            if (RoleClass.Lovers.SameDie && exiled.Object.IsLovers())
            {
                if (AmongUsClient.Instance.AmHost)
                {
                    PlayerControl SideLoverPlayer = exiled.Object.GetOneSideLovers();
                    if (SideLoverPlayer.IsAlive())
                    {
                        SideLoverPlayer.RpcCheckExile();
                    }
                }
            }
            if (exiled.Object.IsQuarreled())
            {
                if (AmongUsClient.Instance.AmHost)
                {
                    var Side = RoleHelpers.GetOneSideQuarreled(exiled.Object);
                    if (Side.IsDead())
                    {
                        RPCProcedure.ShareWinner(exiled.Object.PlayerId);
                        MessageWriter Writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.ShareWinner, SendOption.Reliable, -1);
                        Writer.Write(exiled.Object.PlayerId);
                        AmongUsClient.Instance.FinishRpcImmediately(Writer);
                        Writer = RPCHelper.StartRPC(CustomRPC.CustomRPC.SetWinCond);
                        Writer.Write((byte)CustomGameOverReason.QuarreledWin);
                        Writer.EndRPC();
                        RPCProcedure.SetWinCond((byte)CustomGameOverReason.QuarreledWin);
                        var winplayers = new List<PlayerControl>
                        {
                            exiled.Object
                        };
                        //EndGameCheck.WinNeutral(winplayers);
                        Chat.WinCond = CustomGameOverReason.QuarreledWin;
                        Chat.Winner = new List<PlayerControl>
                        {
                            exiled.Object
                        };
                        RoleClass.Quarreled.IsQuarreledWin = true;
                        EndGameCheck.CustomEndGame(MapUtilities.CachedShipStatus, GameOverReason.HumansByTask, false);
                    }
                }
            }
            Roles.Jester.WrapUp(exiled);
            Roles.Nekomata.WrapUp(exiled);
        }
    }
}