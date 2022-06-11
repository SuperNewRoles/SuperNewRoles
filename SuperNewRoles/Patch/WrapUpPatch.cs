using HarmonyLib;
using Hazel;
using SuperNewRoles.Buttons;
using SuperNewRoles.CustomRPC;
using SuperNewRoles.EndGame;
using SuperNewRoles.Helpers;
using SuperNewRoles.Mode;
using SuperNewRoles.Roles;
using SuperNewRoles.Sabotage;
using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNewRoles.Patch
{
    class WrapUpPatch
    {

        [HarmonyPatch(typeof(ExileController), nameof(ExileController.WrapUp))]
        public class ExileControllerWrapUpPatch
        {
            public static void Prefix(ExileController __instance)
            {
                WrapUpPatch.Prefix(__instance.exiled);
            }
            public static void Postfix(ExileController __instance)
            {
                WrapUpPatch.Postfix(__instance.exiled);
            }
        }

        [HarmonyPatch(typeof(AirshipExileController), nameof(AirshipExileController.WrapUpAndSpawn))]
        public class AirshipExileControllerWrapUpPatch
        {
            public static void Prefix(AirshipExileController __instance)
            {
                WrapUpPatch.Prefix(__instance.exiled);
            }
            public static void Postfix(AirshipExileController __instance)
            {
                WrapUpPatch.Postfix(__instance.exiled);
            }
        }

        public static void Prefix(GameData.PlayerInfo exiled)
        {
            RoleClass.IsCoolTimeSetted = false;
            if (exiled != null)
            {
                FalseCharges.WrapUp(exiled.Object);
            }
            if (ModeHandler.isMode(ModeId.Default))
            {
                if (SabotageManager.thisSabotage == SabotageManager.CustomSabotage.CognitiveDeficit)
                {
                    if (!Sabotage.CognitiveDeficit.main.IsLocalEnd)
                    {
                        Sabotage.CognitiveDeficit.main.UpdateTime = 0;
                    }
                }
                if (exiled == null) return;
                FinalStatusPatch.FinalStatusData.FinalStatuses[exiled.Object.PlayerId] = FinalStatus.Exiled;
                if (exiled.Object.PlayerId != CachedPlayer.LocalPlayer.PlayerId) return;
                if (exiled.Object.isRole(RoleId.SideKiller))
                {
                    var sideplayer = RoleClass.SideKiller.getSidePlayer(PlayerControl.LocalPlayer);
                    if (sideplayer != null)
                    {
                        if (!RoleClass.SideKiller.IsUpMadKiller)
                        {
                            sideplayer.RPCSetRoleUnchecked(RoleTypes.Impostor);
                            RoleClass.SideKiller.IsUpMadKiller = true;
                        }
                    }
                }
            }
        }

        public static void Postfix(GameData.PlayerInfo exiled)
        {
            SerialKiller.WrapUp();
            Assassin.WrapUp(exiled);
            CountChanger.CountChangerPatch.WrapUpPatch();
            CustomButton.MeetingEndedUpdate();

            PlayerControlHepler.refreshRoleDescription(PlayerControl.LocalPlayer);
            new LateTask(() => {
                RoleClass.IsMeeting = false;
            }, 0.1f, "SetIsMeeting");
            if (ModeHandler.isMode(ModeId.SuperHostRoles)) Mode.SuperHostRoles.WrapUpClass.WrapUp(exiled);
            ModeHandler.Wrapup(exiled);
            if (exiled == null) return;

            Seer.ExileControllerWrapUpPatch.WrapUpPostfix(exiled);
            RedRidingHood.WrapUp(exiled);
            Nekomata.NekomataEnd(exiled);

            exiled.Object.Exiled();
            exiled.IsDead = true;
            FinalStatusPatch.FinalStatusData.FinalStatuses[exiled.PlayerId] = FinalStatus.Exiled;
            var Player = ModHelpers.playerById(exiled.PlayerId);
            if (ModeHandler.isMode(ModeId.Default))
            {
                if (RoleClass.Lovers.SameDie && Player.IsLovers())
                {
                    if (AmongUsClient.Instance.AmHost)
                    {
                        PlayerControl SideLoverPlayer = Player.GetOneSideLovers();
                        if (SideLoverPlayer.isAlive())
                        {
                            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.RPCMurderPlayer, Hazel.SendOption.Reliable, -1);
                            writer.Write(SideLoverPlayer.PlayerId);
                            writer.Write(SideLoverPlayer.PlayerId);
                            writer.Write(byte.MaxValue);
                            AmongUsClient.Instance.FinishRpcImmediately(writer);
                            RPCProcedure.RPCMurderPlayer(SideLoverPlayer.PlayerId, SideLoverPlayer.PlayerId, byte.MaxValue);
                        }
                    }
                }
                EvilEraser.IsWinGodGuard = false;
                EvilEraser.IsWinFoxGuard = false;
                if (RoleHelpers.IsQuarreled(Player))
                {
                    var Side = RoleHelpers.GetOneSideQuarreled(Player);
                    if (Side.isDead())
                    {
                        MessageWriter Writer = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.ShareWinner, Hazel.SendOption.Reliable, -1);
                        Writer.Write(Player.PlayerId);
                        AmongUsClient.Instance.FinishRpcImmediately(Writer);
                        CustomRPC.RPCProcedure.ShareWinner(Player.PlayerId);
                        RoleClass.Quarreled.IsQuarreledWin = true;
                        CheckGameEndPatch.CustomEndGame((GameOverReason)CustomGameOverReason.QuarreledWin, false);
                    }
                }

                if (RoleClass.Jester.JesterPlayer.IsCheckListPlayerControl(Player))
                {

                    if (!RoleClass.Jester.IsJesterTaskClearWin || (Roles.RoleClass.Jester.IsJesterTaskClearWin && Patch.TaskCount.TaskDateNoClearCheck(Player.Data).Item2 - Patch.TaskCount.TaskDateNoClearCheck(Player.Data).Item1 == 0))
                    {
                        RPCProcedure.ShareWinner(Player.PlayerId);

                        MessageWriter Writer = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.ShareWinner, Hazel.SendOption.Reliable, -1);
                        Writer.Write(Player.PlayerId);
                        AmongUsClient.Instance.FinishRpcImmediately(Writer);
                        Roles.RoleClass.Jester.IsJesterWin = true;
                        CheckGameEndPatch.CustomEndGame((GameOverReason)CustomGameOverReason.JesterWin, false);
                    }
                }

                if (RoleClass.MadJester.MadJesterPlayer.IsCheckListPlayerControl(Player))
                {

                    if (!RoleClass.MadJester.IsMadJesterTaskClearWin || (RoleClass.MadJester.IsMadJesterTaskClearWin && TaskCount.TaskDateNoClearCheck(Player.Data).Item2 - TaskCount.TaskDateNoClearCheck(Player.Data).Item1 == 0))
                    {
                        RPCProcedure.ShareWinner(Player.PlayerId);

                        MessageWriter Writer = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.ShareWinner, Hazel.SendOption.Reliable, -1);
                        Writer.Write(Player.PlayerId);
                        AmongUsClient.Instance.FinishRpcImmediately(Writer);
                        Roles.RoleClass.MadJester.IsMadJesterWin = true;
                        CheckGameEndPatch.CustomEndGame((GameOverReason)CustomGameOverReason.MadJesterWin, false);
                    }
                }
            }
            Mode.SuperHostRoles.main.RealExiled = null;
        }
    }
}
