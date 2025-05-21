using System;
using System.Collections;
using System.Linq;
using AmongUs.GameOptions;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using HarmonyLib;
using Hazel;
using SuperNewRoles.Buttons;
using SuperNewRoles.CustomObject;
using SuperNewRoles.Helpers;
using SuperNewRoles.MapCustoms;
using SuperNewRoles.MapCustoms.Airship;
using SuperNewRoles.Mode;
using SuperNewRoles.Mode.BattleRoyal;
using SuperNewRoles.Mode.SuperHostRoles;
using SuperNewRoles.Replay;
using SuperNewRoles.Roles;
using SuperNewRoles.Roles.Crewmate;
using SuperNewRoles.Roles.Impostor;
using SuperNewRoles.Roles.Neutral;
using SuperNewRoles.Roles.RoleBases;
using SuperNewRoles.Sabotage;
using UnityEngine;

namespace SuperNewRoles.Patches;

class WrapUpPatch
{
    [HarmonyPatch(typeof(ExileController), nameof(ExileController.WrapUp))]
    public class ExileControllerWrapUpPatch
    {
        public static IEnumerator WrapUpCoro(ExileController __instance)
        {
            if (__instance.initData?.networkedPlayer != null)
            {
                PlayerControl @object = __instance.initData.networkedPlayer?.Object;
                if (@object)
                {
                    @object.Exiled();
                }
                __instance.initData.networkedPlayer.IsDead = true;
            }
            if (DestroyableSingleton<TutorialManager>.InstanceExists || !GameManager.Instance.LogicFlow.IsGameOverDueToDeath())
            {
                yield return ShipStatus.Instance.PrespawnStep();
                __instance.ReEnableGameplay();
            }
            GameObject.Destroy(__instance.gameObject);
        }
        public static bool Prefix(ExileController __instance)
        {
            WrapUpPatch.Prefix(__instance.initData?.networkedPlayer);
            __instance.StartCoroutine(WrapUpCoro(__instance).WrapToIl2Cpp());
            return false;
        }
        public static void Postfix(ExileController __instance)
        {
            WrapUpPatch.Postfix(__instance, __instance.initData?.networkedPlayer);
        }
    }
    [HarmonyPatch(typeof(AirshipExileController._WrapUpAndSpawn_d__11), nameof(AirshipExileController._WrapUpAndSpawn_d__11.MoveNext))]
    public class AirshipExileControllerWrapUpPatch
    {
        private static int last;
        public static bool Prefix(AirshipExileController._WrapUpAndSpawn_d__11 __instance)
        {
            if (last == __instance.__4__this.GetInstanceID())
                return true;
            last = __instance.__4__this.GetInstanceID();
            WrapUpPatch.Prefix(__instance.__4__this.initData?.networkedPlayer);
            if (Balancer.currentAbilityUser != null && Balancer.IsDoubleExile && __instance.__4__this != ExileController.Instance)
            {
                if (__instance.__4__this.initData?.networkedPlayer != null)
                {
                    PlayerControl @object = __instance.__4__this.initData?.networkedPlayer?.Object;
                    if (@object)
                    {
                        @object.Exiled();
                    }
                    __instance.__4__this.initData.networkedPlayer.IsDead = true;
                }
                GameObject.Destroy(__instance.__4__this.gameObject);

                // 暗転をごり押しで解決
                if (MapCustomHandler.IsMapCustom(MapCustomHandler.MapCustomId.Airship, false) && MapCustom.AirshipRandomSpawn.GetBool())
                {
                    new LateTask(() =>
                    {
                        FastDestroyableSingleton<HudManager>.Instance.FullScreen.gameObject.SetActive(false);
                    }, 0.3f);
                }
                return false;
            }
            return true;
        }
        public static void Postfix(AirshipExileController __instance)
        {
            WrapUpPatch.Postfix(__instance, __instance.initData?.networkedPlayer);
        }
    }
    public static void Prefix(NetworkedPlayerInfo exiled)
    {
        if (exiled != null && exiled.Object == null)
        {
            exiled = null;
        }
        RoleClass.IsCoolTimeSetted = false;
        AntiBlackOut.OnWrapUp();
        if (AntiBlackOut.RealExiled != null)
            exiled = AntiBlackOut.RealExiled;
        FalseCharges.WrapUp(exiled != null ? exiled.Object : null);
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
                if (sideplayer != null && sideplayer.IsAlive())
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
    public static void Postfix(ExileController __instance, NetworkedPlayerInfo exiled)
    {
        if (exiled != null && exiled.Object == null)
        {
            exiled = null;
        }
        if (ReplayManager.IsReplayMode)
        {
            ReplayLoader.OnWrapUp();
        }

        if (AntiBlackOut.RealExiled != null)
            exiled = AntiBlackOut.RealExiled;

        // |:========== 追放の有無問わず 会議終了時に行う処理 開始 ==========:|
        SelectRoleSystem.OnWrapUp();

        VentInfo.OnWrapUp();

        Shielder.WrapUp();
        Kunoichi.WrapUp();
        SerialKiller.WrapUp();
        Assassin.WrapUp();
        CountChanger.CountChangerPatch.WrapUpPatch();
        RoleClass.IsFirstMeetingEnd = true;
        RoleClass.IsfirstResetCool = false;
        EmergencyMinigamePatch.FirstEmergencyCooldown.OnWrapUp(exiled != null);
        CustomButton.MeetingEndedUpdate();

        PlayerControlHelper.RefreshRoleDescription(PlayerControl.LocalPlayer);
        if (ModeHandler.IsMode(ModeId.SuperHostRoles)) Mode.SuperHostRoles.WrapUpClass.WrapUp(exiled);
        ModeHandler.Wrapup(exiled);
        Pteranodon.WrapUp();
        Revolutionist.WrapUp();
        Spelunker.WrapUp();
        Hitman.WrapUp();
        Matryoshka.WrapUp();
        PartTimer.WrapUp();
        KnightProtected_Patch.WrapUp();
        Clergyman.WrapUp();
        Balancer.WrapUp(exiled == null ? null : exiled.Object);
        Speeder.WrapUp();
        CustomRoles.OnWrapUp(exiled?.Object);
        Rocket.WrapUp(exiled == null ? null : exiled.Object);

        PlayerAnimation.PlayerAnimations.Values.All(x => { x.HandleAnim(RpcAnimationType.Stop); return false; });
        new LateTask(() => PlayerAnimation.PlayerAnimations.Values.All(x => { x.HandleAnim(RpcAnimationType.Stop); return false; }), 0.5f);

        SecretRoom.Reset();
        if (PlayerControl.LocalPlayer.IsRole(RoleId.Painter)) Painter.WrapUp();
        Photographer.WrapUp();
        Cracker.WrapUp();
        RoleClass.IsMeeting = false;
        SeerHandler.WrapUpPatch.WrapUpPostfix();
        Vampire.SetActiveBloodStaiWrapUpPatch();
        Roles.Crewmate.Celebrity.AbilityOverflowingBrilliance.WrapUp();
        Roles.Neutral.TheThreeLittlePigs.TheFirstLittlePig.WrapUp();
        BlackHatHacker.WrapUp();
        WellBehaver.WrapUp();
        foreach (PlayerControl p in PlayerControl.AllPlayerControls)
        {
            p.resetChange();
        }
        RoleClass.Doppelganger.Targets = new();

        Crook.WrapUp.GeneralProcess(exiled == null ? null : exiled.Object);

        FixAfterMeetingVent();

        Logger.Info("[追放の有無問わず 会議終了時に行う処理] 通過", "WrapUp");

        // |:========== 追放が発生していた場合のみ 会議終了時に行う処理 開始 ==========:|

        if (exiled == null) return;

        exiled.Object.Exiled();
        exiled.IsDead = true;
        FinalStatusPatch.FinalStatusData.FinalStatuses[exiled.PlayerId] = FinalStatus.Exiled;

        var Player = ModHelpers.PlayerById(exiled.PlayerId);

        if (exiled.Object.IsRole(RoleId.Jumbo) && exiled.Object.IsCrew())
        {
            GameManager.Instance.RpcEndGame((GameOverReason)CustomGameOverReason.NoWinner, false);
            return;
        }
        Vampire.DependentsExileWrapUpPatch(exiled.Object);
        SoothSayer_Patch.WrapUp(exiled.Object);
        Nekomata.NekomataEnd(exiled);
        NekoKabocha.OnWrapUp(exiled.Object);

        if (ModeHandler.IsMode(ModeId.Default))
        {
            if (RoleClass.Lovers.SameDie && Player.IsLovers())
            {
                if (AmongUsClient.Instance.AmHost)
                {
                    PlayerControl SideLoverPlayer = Player.GetOneSideLovers();
                    if (SideLoverPlayer.IsAlive())
                    {
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.RPCMurderPlayer, SendOption.Reliable, -1);
                        writer.Write(SideLoverPlayer.PlayerId);
                        writer.Write(SideLoverPlayer.PlayerId);
                        writer.Write(byte.MaxValue);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RPCProcedure.RPCMurderPlayer(SideLoverPlayer.PlayerId, SideLoverPlayer.PlayerId, byte.MaxValue);
                        SideLoverPlayer.RpcSetFinalStatus(FinalStatus.LoversBomb);
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
                    MessageWriter Writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ShareWinner, SendOption.Reliable, -1);
                    Writer.Write(Player.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(Writer);
                    RPCProcedure.ShareWinner(Player.PlayerId);
                    RoleClass.Quarreled.IsQuarreledWin = true;
                    CheckGameEndPatch.CustomEndGame((GameOverReason)CustomGameOverReason.QuarreledWin, false);
                }
            }

            if (Player.IsRole(RoleId.Jester))
            {

                if (!RoleClass.Jester.IsJesterTaskClearWin || (RoleClass.Jester.IsJesterTaskClearWin && Patches.TaskCount.TaskDateNoClearCheck(Player.Data).Item2 - Patches.TaskCount.TaskDateNoClearCheck(Player.Data).Item1 == 0))
                {
                    RPCProcedure.ShareWinner(Player.PlayerId);
                    MessageWriter Writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ShareWinner, SendOption.Reliable, -1);
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
                    Player.RpcSetFinalStatus(FinalStatus.MadJesterExiled);
                    RPCProcedure.ShareWinner(Player.PlayerId);
                    MessageWriter Writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ShareWinner, SendOption.Reliable, -1);
                    Writer.Write(Player.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(Writer);
                    RoleClass.MadJester.IsMadJesterWin = true;
                    CheckGameEndPatch.CustomEndGame((GameOverReason)CustomGameOverReason.MadJesterWin, false);
                }
            }
        }
        else if (ModeHandler.IsMode(ModeId.SuperHostRoles))
        {
            CheckForEndVotingPatch.ResetExiledPlayerName();
        }
        Mode.SuperHostRoles.Main.RealExiled = null;

        Logger.Info("[追放が発生していた場合のみ 会議終了時に行う処理] 通過", "WrapUp");
    }
    static void FixAfterMeetingVent()
    {
        //ベントがなければ帰れ！！！
        if (!ShipStatus.Instance.Systems.TryGetValue(SystemTypes.Ventilation, out ISystemType vent))
            return;
        VentilationSystem ventilationSystem = vent.TryCast<VentilationSystem>();
        // VentiSystemでなければ帰れ！！！
        if (ventilationSystem == null)
            return;
        ventilationSystem.PlayersInsideVents = new();
        ventilationSystem.IsDirty = true;
    }
}