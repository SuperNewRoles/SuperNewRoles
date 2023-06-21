using System.Collections;
using System.Collections.Generic;
using AmongUs.GameOptions;
using Hazel;
using SuperNewRoles.Helpers;
using SuperNewRoles.Patches;
using SuperNewRoles.Roles;
using UnityEngine;
using static SuperNewRoles.Helpers.RPCHelper;

namespace SuperNewRoles.Mode.SuperHostRoles;

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

        foreach (PlayerControl p in RoleClass.RemoteSheriff.RemoteSheriffPlayer)
        {
            if (p.IsAlive() && !p.IsMod()) p.RpcResetAbilityCooldown();
        }
        foreach (PlayerControl p in RoleClass.Arsonist.ArsonistPlayer)
        {
            if (p.IsAlive() && !p.IsMod()) p.RpcResetAbilityCooldown();
        }
        foreach (PlayerControl p in SuperNewRoles.Roles.Impostor.MadRole.Worshiper.RoleData.Player)
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
                    SideLoverPlayer.RpcSetFinalStatus(FinalStatus.LoversBomb);
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
                    MessageWriter Writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ShareWinner, SendOption.Reliable, -1);
                    Writer.Write(exiled.Object.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(Writer);
                    Writer = RPCHelper.StartRPC(CustomRPC.SetWinCond);
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