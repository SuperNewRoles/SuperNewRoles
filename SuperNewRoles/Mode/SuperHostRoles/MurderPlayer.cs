
using Hazel;
using SuperNewRoles.CustomRPC;
using SuperNewRoles.EndGame;
using SuperNewRoles.Helpers;
using SuperNewRoles.Roles;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SuperNewRoles.Mode.SuperHostRoles
{
    class MurderPlayer
    {
        public static void Postfix(PlayerControl __instance, PlayerControl target)
        {
            if (!AmongUsClient.Instance.AmHost) return;
            if (target.isAlive()) return;
            FixedUpdate.SetRoleNames();
            if (target.isRole(RoleId.Sheriff) || target.isRole(RoleId.truelover) || target.isRole(RoleId.MadMaker))
            {
                target.RpcSetRoleDesync(RoleTypes.GuardianAngel);
            }
            if (target.IsQuarreled())
            {
                if (AmongUsClient.Instance.AmHost)
                {
                    var Side = RoleHelpers.GetOneSideQuarreled(target);
                    if (Side.isDead())
                    {
                        new LateTask(() =>
                        {
                            RPCProcedure.ShareWinner(target.PlayerId);

                            MessageWriter Writer = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.ShareWinner, Hazel.SendOption.Reliable, -1);
                            Writer.Write(target.PlayerId);
                            AmongUsClient.Instance.FinishRpcImmediately(Writer);
                            Writer = RPCHelper.StartRPC(CustomRPC.CustomRPC.SetWinCond);
                            Writer.Write((byte)CustomGameOverReason.QuarreledWin);
                            Writer.EndRPC();
                            CustomRPC.RPCProcedure.SetWinCond((byte)CustomGameOverReason.QuarreledWin);
                            var winplayers = new List<PlayerControl>();
                            winplayers.Add(target);
                            //EndGameCheck.WinNeutral(winplayers);
                            Chat.WinCond = CustomGameOverReason.QuarreledWin;
                            Chat.Winner = new List<PlayerControl>();
                            Chat.Winner.Add(target);
                            RoleClass.Quarreled.IsQuarreledWin = true;
                            SuperHostRoles.EndGameCheck.CustomEndGame(MapUtilities.CachedShipStatus, GameOverReason.HumansByTask, false);
                        }, 0.15f);
                    }
                }
            }
            if (RoleClass.Lovers.SameDie && target.IsLovers())
            {
                PlayerControl Side = target.GetOneSideLovers();
                if (Side.isAlive())
                {
                    Side.RpcMurderPlayer(Side);
                }
            }
            Roles.Bait.MurderPostfix(__instance,target);
            FixedUpdate.SetRoleName(target);
        }
    }
}
