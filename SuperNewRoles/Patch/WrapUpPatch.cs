using System.Linq;
using HarmonyLib;
using Hazel;
using SuperNewRoles.Buttons;
using SuperNewRoles.CustomObject;
using SuperNewRoles.CustomRPC;
using SuperNewRoles.EndGame;
using SuperNewRoles.Helpers;
using SuperNewRoles.MapCustoms.Airship;
using SuperNewRoles.Mode;
using SuperNewRoles.Roles;
using SuperNewRoles.Sabotage;

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
            if (ModeHandler.IsMode(ModeId.Default))
            {
                if (SabotageManager.thisSabotage == SabotageManager.CustomSabotage.CognitiveDeficit)
                {
                    if (!Sabotage.CognitiveDeficit.Main.IsLocalEnd)
                    {
                        Sabotage.CognitiveDeficit.Main.UpdateTime = 0;
                    }
                }
                if (exiled == null) return;
                FinalStatusPatch.FinalStatusData.FinalStatuses[exiled.Object.PlayerId] = FinalStatus.Exiled;
                if (exiled.Object.PlayerId != CachedPlayer.LocalPlayer.PlayerId) return;
                if (exiled.Object.IsRole(RoleId.SideKiller))
                {
                    var sideplayer = RoleClass.SideKiller.GetSidePlayer(PlayerControl.LocalPlayer);
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
            Kunoichi.WrapUp();
            SerialKiller.WrapUp();
            Assassin.WrapUp();
            CountChanger.CountChangerPatch.WrapUpPatch();
            CustomButton.MeetingEndedUpdate();

            PlayerControlHepler.RefreshRoleDescription(PlayerControl.LocalPlayer);
            new LateTask(() =>
            {
                RoleClass.IsMeeting = false;
            }, 0.1f, "SetIsMeeting");
            if (ModeHandler.IsMode(ModeId.SuperHostRoles)) Mode.SuperHostRoles.WrapUpClass.WrapUp(exiled);
            ModeHandler.Wrapup(exiled);
            RedRidingHood.WrapUp(exiled);
            Roles.Neutral.Revolutionist.WrapUp();
            Roles.Neutral.Spelunker.WrapUp();
            Roles.Neutral.Hitman.WrapUp();
            Roles.Impostor.Matryoshka.WrapUp();
            Roles.Neutral.PartTimer.WrapUp();
            if (AmongUsClient.Instance.AmHost) {
                PlayerAnimation.PlayerAnimations.All(x =>
                {
                    x.RpcAnimation(RpcAnimationType.Stop);    
                    return false;
                });
            }
            SecretRoom.Reset();
            if (PlayerControl.LocalPlayer.IsRole(RoleId.Painter)) Roles.CrewMate.Painter.WrapUp();
            Roles.Neutral.Photographer.WrapUp();
            if (exiled == null) return;

            Seer.ExileControllerWrapUpPatch.WrapUpPostfix();
            Nekomata.NekomataEnd(exiled);

            exiled.Object.Exiled();
            exiled.IsDead = true;
            FinalStatusPatch.FinalStatusData.FinalStatuses[exiled.PlayerId] = FinalStatus.Exiled;
            var Player = ModHelpers.PlayerById(exiled.PlayerId);
            if (ModeHandler.IsMode(ModeId.Default))
            {
                if (RoleClass.Lovers.SameDie && Player.IsLovers())
                {
                    if (AmongUsClient.Instance.AmHost)
                    {
                        PlayerControl SideLoverPlayer = Player.GetOneSideLovers();
                        if (SideLoverPlayer.IsAlive())
                        {
                            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.RPCMurderPlayer, SendOption.Reliable, -1);
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
                    if (Side.IsDead())
                    {
                        MessageWriter Writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.ShareWinner, SendOption.Reliable, -1);
                        Writer.Write(Player.PlayerId);
                        AmongUsClient.Instance.FinishRpcImmediately(Writer);
                        RPCProcedure.ShareWinner(Player.PlayerId);
                        RoleClass.Quarreled.IsQuarreledWin = true;
                        CheckGameEndPatch.CustomEndGame((GameOverReason)CustomGameOverReason.QuarreledWin, false);
                    }
                }

                if (Player.IsRole(RoleId.Jester))
                {

                    if (!RoleClass.Jester.IsJesterTaskClearWin || (RoleClass.Jester.IsJesterTaskClearWin && Patch.TaskCount.TaskDateNoClearCheck(Player.Data).Item2 - Patch.TaskCount.TaskDateNoClearCheck(Player.Data).Item1 == 0))
                    {
                        RPCProcedure.ShareWinner(Player.PlayerId);
                        MessageWriter Writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.ShareWinner, SendOption.Reliable, -1);
                        Writer.Write(Player.PlayerId);
                        AmongUsClient.Instance.FinishRpcImmediately(Writer);
                        RoleClass.Jester.IsJesterWin = true;
                        CheckGameEndPatch.CustomEndGame((GameOverReason)CustomGameOverReason.JesterWin, false);
                    }
                }

                if (Player.IsRole(RoleId.MadJester))
                {
                    if (!RoleClass.MadJester.IsMadJesterTaskClearWin || (RoleClass.MadJester.IsMadJesterTaskClearWin && TaskCount.TaskDateNoClearCheck(Player.Data).Item2 - TaskCount.TaskDateNoClearCheck(Player.Data).Item1 == 0))
                    {
                        RPCProcedure.ShareWinner(Player.PlayerId);
                        MessageWriter Writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.ShareWinner, SendOption.Reliable, -1);
                        Writer.Write(Player.PlayerId);
                        AmongUsClient.Instance.FinishRpcImmediately(Writer);
                        RoleClass.MadJester.IsMadJesterWin = true;
                        CheckGameEndPatch.CustomEndGame((GameOverReason)CustomGameOverReason.MadJesterWin, false);
                    }
                }
            }
            Mode.SuperHostRoles.Main.RealExiled = null;
        }
    }
}