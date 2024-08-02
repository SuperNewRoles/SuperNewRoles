using System.Collections.Generic;
using AmongUs.GameOptions;
using Hazel;
using SuperNewRoles.Helpers;
using SuperNewRoles.Patches;
using SuperNewRoles.Roles;
using SuperNewRoles.Roles.Crewmate;
using SuperNewRoles.Roles.Impostor;
using SuperNewRoles.Roles.Neutral;
namespace SuperNewRoles.Mode.SuperHostRoles;

class MurderPlayer
{
    public static void Postfix(PlayerControl __instance, PlayerControl target)
    {
        if (target.IsAlive()) return;
        if (__instance.PlayerId == CachedPlayer.LocalPlayer.PlayerId && PlayerControl.LocalPlayer.IsRole(RoleId.Finder))
        {
            RoleClass.Finder.KillCount++;
        }
        if (!AmongUsClient.Instance.AmHost) return;

        ChangeName.UpdateRoleName(__instance, ChangeNameType.SelfOnly);

        if (__instance.IsRole(RoleId.Finder))
        {
            RoleClass.Finder.KillCounts[__instance.PlayerId]++;
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
                        EndGameCheck.CustomEndGame(MapUtilities.CachedShipStatus, CustomGameOverReason.QuarreledWin, false);
                    }, 0.15f, "Quarreled Murder EndGame");
                }
            }
        }
        if (RoleClass.Lovers.SameDie && target.IsLovers())
        {
            PlayerControl Side = target.GetOneSideLovers();
            if (Side.IsAlive())
            {
                Side.RpcMurderPlayer(Side, true);
            }
        }
        Roles.Bait.MurderPostfix(__instance, target);
        ChangeName.UpdateRoleName(target, ChangeNameType.SelfOnly);
        SeerHandler.WrapUpPatch.MurderPlayerPatch.ShowFlash_SHR(target);
    }
}