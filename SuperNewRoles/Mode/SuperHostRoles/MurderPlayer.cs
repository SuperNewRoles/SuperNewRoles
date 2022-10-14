using System.Collections.Generic;
using Hazel;
using SuperNewRoles.Helpers;
using SuperNewRoles.Patches;
using SuperNewRoles.Roles;

namespace SuperNewRoles.Mode.SuperHostRoles
{
    class MurderPlayer
    {
        public static void Postfix(PlayerControl __instance, PlayerControl target)
        {
            if (!AmongUsClient.Instance.AmHost) return;
            if (target.IsAlive()) return;
            FixedUpdate.SetRoleNames();
            if (target.IsRole(RoleId.Sheriff) || target.IsRole(RoleId.truelover) || target.IsRole(RoleId.MadMaker))
            {
                target.RpcSetRoleDesync(RoleTypes.GuardianAngel);
            }
            if (target.IsQuarreled())
            {
                if (AmongUsClient.Instance.AmHost)
                {
                    var Side = RoleHelpers.GetOneSideQuarreled(target);
                    if (Side.IsDead())
                    {
                        new LateTask(() =>
                        {
                            RPCProcedure.ShareWinner(target.PlayerId);
                            MessageWriter Writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ShareWinner, SendOption.Reliable, -1);
                            Writer.Write(target.PlayerId);
                            AmongUsClient.Instance.FinishRpcImmediately(Writer);
                            Writer = RPCHelper.StartRPC(CustomRPC.SetWinCond);
                            Writer.Write((byte)CustomGameOverReason.QuarreledWin);
                            Writer.EndRPC();
                            RPCProcedure.SetWinCond((byte)CustomGameOverReason.QuarreledWin);
                            var winplayers = new List<PlayerControl>
                            {
                                target
                            };
                            //EndGameCheck.WinNeutral(winplayers);
                            Chat.WinCond = CustomGameOverReason.QuarreledWin;
                            Chat.Winner = new List<PlayerControl>
                            {
                                target
                            };
                            RoleClass.Quarreled.IsQuarreledWin = true;
                            EndGameCheck.CustomEndGame(MapUtilities.CachedShipStatus, GameOverReason.HumansByTask, false);
                        }, 0.15f, "Quarreled Murder EndGame");
                    }
                }
            }
            if (RoleClass.Lovers.SameDie && target.IsLovers())
            {
                PlayerControl Side = target.GetOneSideLovers();
                if (Side.IsAlive())
                {
                    Side.RpcMurderPlayer(Side);
                }
            }
            Roles.Bait.MurderPostfix(__instance, target);
            FixedUpdate.SetRoleName(target);
        }
    }
}