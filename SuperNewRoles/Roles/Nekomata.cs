using HarmonyLib;
using Hazel;
using SuperNewRoles.EndGame;
using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNewRoles.Roles
{
    class Nekomata
    {
        public static void NekomataEnd(GameData.PlayerInfo __instance) {
            if (__instance == null) return; 
            if (AmongUsClient.Instance.AmHost) {
                if (__instance != null && RoleClass.NiceNekomata.NiceNekomataPlayer.IsCheckListPlayerControl(__instance.Object) || RoleClass.EvilNekomata.EvilNekomataPlayer.IsCheckListPlayerControl(__instance.Object))
                {
                    List<PlayerControl> p = new List<PlayerControl>();
                    foreach (PlayerControl p1 in PlayerControl.AllPlayerControls) {
                        if (p1.PlayerId != __instance.PlayerId && p1.isAlive()) {
                            p.Add(p1);
                        }
                    }

                    MessageWriter RPCWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.ExiledRPC, Hazel.SendOption.Reliable, -1);
                    RPCWriter.Write(__instance.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(RPCWriter);
                    CustomRPC.RPCProcedure.ExiledRPC(__instance.PlayerId);
                    NekomataProc(p);
                }
            }
        }
        public static void NekomataProc(List<PlayerControl> p){
            SuperNewRolesPlugin.Logger.LogInfo("Proc");
            var rdm = ModHelpers.GetRandomIndex(p);
            var random = p[rdm];
            SuperNewRolesPlugin.Logger.LogInfo(random.nameText.text);
            MessageWriter RPCWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.NekomataExiledRPC, Hazel.SendOption.Reliable, -1);
            RPCWriter.Write(random.PlayerId);
            AmongUsClient.Instance.FinishRpcImmediately(RPCWriter);
            CustomRPC.RPCProcedure.ExiledRPC(random.PlayerId);
            if ((RoleClass.NiceNekomata.NiceNekomataPlayer.IsCheckListPlayerControl(random) || RoleClass.EvilNekomata.EvilNekomataPlayer.IsCheckListPlayerControl(random))&& RoleClass.NiceNekomata.IsChain) {
                p.RemoveAt(rdm);
                NekomataProc(p);
            }
            if (RoleClass.Jester.JesterPlayer.IsCheckListPlayerControl(random)) {
                if (!Roles.RoleClass.Jester.IsJesterTaskClearWin || (Roles.RoleClass.Jester.IsJesterTaskClearWin && Patch.TaskCount.TaskDateNoClearCheck(random.Data).Item2 - Patch.TaskCount.TaskDateNoClearCheck(random.Data).Item1 == 0))
                {
                    CustomRPC.RPCProcedure.ShareWinner(random.PlayerId);
                    MessageWriter Writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.ShareWinner, Hazel.SendOption.Reliable, -1);
                    Writer.Write(random.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(Writer);
                    Roles.RoleClass.Jester.IsJesterWin = true;
                    ShipStatus.RpcEndGame((GameOverReason)CustomGameOverReason.JesterWin, false);
                }
            }
        }
        [HarmonyPatch(typeof(ExileController), nameof(ExileController.WrapUp))]
        public class CheckEndGamePatch
        {
            public static void Postfix(ExileController __instance)
            {
                try
                {
                    NekomataEnd(__instance.exiled);
                }
                catch (Exception e)
                {
                    SuperNewRolesPlugin.Logger.LogInfo("CHECKERROR:" + e);
                }
            }
        }
        [HarmonyPatch(typeof(AirshipExileController), nameof(AirshipExileController.WrapUpAndSpawn))]
        public class CheckAirShipEndGamePatch
        {
            public static void Postfix(AirshipExileController __instance)
            {
                try
                {
                    NekomataEnd(__instance.exiled);
                }
                catch (Exception e)
                {
                    SuperNewRolesPlugin.Logger.LogInfo("CHECKERROR:" + e);
                }
            }
        }
    }
}
