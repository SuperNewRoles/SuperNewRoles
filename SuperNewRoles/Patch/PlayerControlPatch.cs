using HarmonyLib;
using Hazel;
using InnerNet;
using SuperNewRoles.CustomOption;
using SuperNewRoles.CustomRPC;
using SuperNewRoles.EndGame;
using SuperNewRoles.Helpers;
using SuperNewRoles.Intro;
using SuperNewRoles.Mode;
using SuperNewRoles.Mode.SuperHostRoles;
using SuperNewRoles.Patch;
using SuperNewRoles.Roles;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static SuperNewRoles.Helpers.DesyncHelpers;
using static SuperNewRoles.ModHelpers;

namespace SuperNewRoles.Patches
{
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Shapeshift))]
    class RpcShapesihftPatch
    {
        public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target, [HarmonyArgument(1)] bool shouldAnimate)
        {
            SyncSetting.CustomSyncSettings();
            if (RoleClass.Assassin.TriggerPlayer != null) return false;
            if (target.IsBot()) return true;
            if (__instance.PlayerId == target.PlayerId)
            {
                if (ModeHandler.isMode(ModeId.SuperHostRoles) && AmongUsClient.Instance.AmHost)
                {
                    if (__instance.isRole(RoleId.RemoteSheriff))
                    {
                        __instance.RpcProtectPlayer(__instance, 0);
                        new LateTask(() =>
                        {
                            __instance.RpcMurderPlayer(__instance);
                        }, 0.5f);
                    }
                }
                return true;
            }
            if (ModeHandler.isMode(ModeId.SuperHostRoles))
            {
                switch (__instance.getRole())
                {
                    case RoleId.RemoteSheriff:
                        if (AmongUsClient.Instance.AmHost)
                        {
                            if (target.isDead()) return true;
                            if (!RoleClass.RemoteSheriff.KillCount.ContainsKey(__instance.PlayerId) || RoleClass.RemoteSheriff.KillCount[__instance.PlayerId] >= 1)
                            {
                                if (!Sheriff.IsRemoteSheriffKill(target) || target.isRole(RoleId.RemoteSheriff))
                                {
                                    FinalStatusPatch.FinalStatusData.FinalStatuses[__instance.PlayerId] = FinalStatus.SheriffMisFire;
                                    __instance.RpcMurderPlayer(__instance);
                                    return true;
                                }
                                else
                                {
                                    FinalStatusPatch.FinalStatusData.FinalStatuses[target.PlayerId] = FinalStatus.SheriffKill;
                                    if (RoleClass.RemoteSheriff.KillCount.ContainsKey(__instance.PlayerId))
                                    {
                                        RoleClass.RemoteSheriff.KillCount[__instance.PlayerId]--;
                                    }
                                    else
                                    {
                                        RoleClass.RemoteSheriff.KillCount[__instance.PlayerId] = (int)CustomOptions.RemoteSheriffKillMaxCount.getFloat() - 1;
                                    }
                                    if (RoleClass.RemoteSheriff.IsKillTeleport)
                                    {
                                        __instance.RpcMurderPlayerCheck(target);
                                    }
                                    else
                                    {
                                        target.RpcMurderPlayer(target);
                                        __instance.RpcProtectPlayer(__instance, 0);
                                    }
                                    Mode.SuperHostRoles.FixedUpdate.SetRoleName(__instance);
                                    return true;
                                }
                            }
                            else
                            {
                                return true;
                            }
                        }
                        return true;
                    case RoleId.SelfBomber:
                        if (AmongUsClient.Instance.AmHost)
                        {
                            foreach (PlayerControl p in CachedPlayer.AllPlayers)
                            {
                                if (p.isAlive() && p.PlayerId != __instance.PlayerId)
                                {
                                    if (SelfBomber.GetIsBomb(__instance, p))
                                    {
                                        __instance.RpcMurderPlayerCheck(p);
                                    }
                                }
                            }
                            __instance.RpcMurderPlayer(__instance);
                        }
                        return false;
                    case RoleId.Samurai:
                        if (!RoleClass.Samurai.SwordedPlayer.Contains(__instance.PlayerId))
                        {
                            if (AmongUsClient.Instance.AmHost || !RoleClass.Samurai.Sword)
                            {
                                foreach (PlayerControl p in CachedPlayer.AllPlayers)
                                {
                                    if (p.isAlive() && p.PlayerId != __instance.PlayerId)
                                    {
                                        if (Samurai.Getsword(__instance, p))
                                        {
                                            __instance.RpcMurderPlayerCheck(p);
                                            Samurai.IsSword();
                                        }
                                    }
                                }
                            }
                            RoleClass.Samurai.SwordedPlayer.Add(__instance.PlayerId);
                        }
                        return false;
                    case RoleId.Arsonist:
                        if (AmongUsClient.Instance.AmHost)
                        {
                            foreach (PlayerControl p in RoleClass.Arsonist.ArsonistPlayer)
                            {
                                if (Arsonist.IsWin(p))
                                {
                                    RPCProcedure.ShareWinner(CachedPlayer.LocalPlayer.PlayerId);

                                    MessageWriter Writer = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.ShareWinner, SendOption.Reliable, -1);
                                    Writer.Write(p.PlayerId);
                                    AmongUsClient.Instance.FinishRpcImmediately(Writer);

                                    Writer = RPCHelper.StartRPC(CustomRPC.CustomRPC.SetWinCond);
                                    Writer.Write((byte)CustomGameOverReason.ArsonistWin);
                                    Writer.EndRPC();
                                    RPCProcedure.SetWinCond((byte)CustomGameOverReason.ArsonistWin);

                                    RoleClass.Arsonist.TriggerArsonistWin = true;
                                    EndGame.AdditionalTempData.winCondition = EndGame.WinCondition.ArsonistWin;
                                    // SuperNewRolesPlugin.Logger.LogInfo("CheckAndEndGame");
                                    __instance.enabled = false;
                                    ShipStatus.RpcEndGame((GameOverReason)EndGame.CustomGameOverReason.ArsonistWin, false);
                                    return true;
                                }
                            }
                        }
                        return false;
                }
            }
            return true;
        }
    }
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CheckProtect))]
    class CheckProtectPatch
    {
        public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
        {
            if (ModeHandler.isMode(ModeId.SuperHostRoles)) return false;
            return true;
        }
    }

    [HarmonyPatch(typeof(ShapeshifterMinigame), nameof(ShapeshifterMinigame.Shapeshift))]
    class ShapeshifterMinigameShapeshiftPatch
    {
        public static bool Prefix(ShapeshifterMinigame __instance, [HarmonyArgument(0)] PlayerControl player)
        {
            if (player.IsBot()) return false;
            if (PlayerControl.LocalPlayer.inVent)
            {
                __instance.Close();
                return false;
            }
            if (PlayerControl.LocalPlayer.isRole(RoleId.RemoteSheriff))
            {
                if (RoleClass.RemoteSheriff.KillMaxCount > 0)
                {
                    if (ModeHandler.isMode(ModeId.SuperHostRoles))
                    {
                        new LateTask(() =>
                        {
                            if (AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Started)
                            {
                                PlayerControl.LocalPlayer.RpcRevertShapeshift(true);
                            }
                        }, 1.5f);
                        PlayerControl.LocalPlayer.RpcShapeshift(player, true);
                    }
                    else if (ModeHandler.isMode(ModeId.Default))
                    {
                        if (player.isAlive())
                        {
                            var Target = player;
                            var misfire = !Roles.Sheriff.IsRemoteSheriffKill(Target);
                            var TargetID = Target.PlayerId;
                            var LocalID = CachedPlayer.LocalPlayer.PlayerId;

                            PlayerControl.LocalPlayer.RpcShapeshift(PlayerControl.LocalPlayer, true);
                            new LateTask(() =>
                            {
                                CachedPlayer.LocalPlayer.transform.localScale *= 1.4f;
                            }, 1.1f);

                            CustomRPC.RPCProcedure.SheriffKill(LocalID, TargetID, misfire);

                            MessageWriter killWriter = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.SheriffKill, Hazel.SendOption.Reliable, -1);
                            killWriter.Write(LocalID);
                            killWriter.Write(TargetID);
                            killWriter.Write(misfire);
                            AmongUsClient.Instance.FinishRpcImmediately(killWriter);
                            RoleClass.RemoteSheriff.KillMaxCount--;
                        }
                        Sheriff.ResetKillCoolDown();
                    };
                }
                __instance.Close();
                return false;
            }
            PlayerControl.LocalPlayer.RpcShapeshift(player, true);
            __instance.Close();
            return false;

        }
    }
    [HarmonyPatch(typeof(KillButton), nameof(KillButton.DoClick))]
    class KillButtonDoClickPatch
    {
        public static bool Prefix(KillButton __instance)
        {
            if (!ModeHandler.isMode(ModeId.Default))
            {
                if (ModeHandler.isMode(ModeId.SuperHostRoles))
                {
                    if (PlayerControl.LocalPlayer.isRole(RoleId.RemoteSheriff))
                    {
                        if (__instance.isActiveAndEnabled && PlayerControl.LocalPlayer.isAlive() && PlayerControl.LocalPlayer.CanMove && !__instance.isCoolingDown && RoleClass.RemoteSheriff.KillMaxCount > 0)
                        {
                            DestroyableSingleton<RoleManager>.Instance.SetRole(PlayerControl.LocalPlayer, RoleTypes.Shapeshifter);
                            foreach (PlayerControl p in CachedPlayer.AllPlayers)
                            {
                                p.Data.Role.NameColor = Color.white;
                            }
                            CachedPlayer.LocalPlayer.Data.Role.TryCast<ShapeshifterRole>().UseAbility();
                            foreach (PlayerControl p in CachedPlayer.AllPlayers)
                            {
                                if (p.isImpostor())
                                {
                                    p.Data.Role.NameColor = RoleClass.ImpostorRed;
                                }
                            }
                            DestroyableSingleton<RoleManager>.Instance.SetRole(PlayerControl.LocalPlayer, RoleTypes.Crewmate);
                            PlayerControl.LocalPlayer.killTimer = 0.001f;
                        }
                        return false;
                    }
                }
                return true;
            }
            if (__instance.isActiveAndEnabled && __instance.currentTarget && !__instance.isCoolingDown && PlayerControl.LocalPlayer.isAlive() && PlayerControl.LocalPlayer.CanMove)
            {
                if (!(__instance.currentTarget.isRole(CustomRPC.RoleId.Bait) || __instance.currentTarget.isRole(CustomRPC.RoleId.NiceRedRidingHood)) && PlayerControl.LocalPlayer.isRole(CustomRPC.RoleId.Vampire))
                {
                    PlayerControl.LocalPlayer.killTimer = RoleHelpers.getCoolTime(PlayerControl.LocalPlayer);
                    RoleClass.Vampire.target = __instance.currentTarget;
                    RoleClass.Vampire.KillTimer = DateTime.Now;
                    RoleClass.Vampire.Timer = RoleClass.Vampire.KillDelay;
                    return false;
                }
                bool showAnimation = true;
                /*
                if (PlayerControl.LocalPlayer.isRole(RoleType.Ninja) && Ninja.isStealthed(PlayerControl.LocalPlayer))
                {
                    showAnimation = false;
                }
                */

                // Use an unchecked kill command, to allow shorter kill cooldowns etc. without getting kicked
                MurderAttemptResult res = ModHelpers.checkMuderAttemptAndKill(PlayerControl.LocalPlayer, __instance.currentTarget, showAnimation: showAnimation);
                // Handle blank kill
                if (res == MurderAttemptResult.BlankKill)
                {
                    PlayerControl.LocalPlayer.killTimer = RoleHelpers.getCoolTime(PlayerControl.LocalPlayer);
                }
                __instance.SetTarget(null);
            }
            return false;
        }
    }
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CheckMurder))]
    static class CheckMurderPatch
    {
        public static bool isKill = false;
        public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
        {
            if (__instance.IsBot() || target.IsBot()) return false;

            if (__instance.isDead()) return false;
            if (target.isDead()) return false;
            if (__instance.PlayerId == target.PlayerId) { __instance.RpcMurderPlayer(target); return false; }
            if (!RoleClass.IsStart && AmongUsClient.Instance.GameMode != GameModes.FreePlay)
                return false;
            if (!AmongUsClient.Instance.AmHost)
            {
                return true;
            }
            switch (ModeHandler.GetMode())
            {
                case ModeId.Zombie:
                    return false;
                case ModeId.BattleRoyal:
                    if (isKill)
                    {
                        return false;
                    }
                    if (Mode.BattleRoyal.main.StartSeconds <= 0)
                    {
                        if (Mode.BattleRoyal.main.IsTeamBattle)
                        {
                            foreach (List<PlayerControl> teams in Mode.BattleRoyal.main.Teams)
                            {
                                if (teams.Count > 0)
                                {
                                    if (teams.IsCheckListPlayerControl(__instance) && teams.IsCheckListPlayerControl(target))
                                    {
                                        return false;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        SuperNewRolesPlugin.Logger.LogInfo("レートタスク:" + (AmongUsClient.Instance.Ping / 1000f) * 2f);
                        isKill = true;

                        if (__instance.PlayerId != 0)
                        {
                            __instance.RpcMurderPlayer(target);
                            target.Data.IsDead = true;
                            isKill = false;
                        }
                        else
                        {
                            new LateTask(() =>
                            {
                                if (__instance.isAlive() && target.isAlive())
                                {
                                    __instance.RpcMurderPlayer(target);
                                }
                                isKill = false;
                            }, (AmongUsClient.Instance.Ping / 1000f) * 1.1f);
                        }
                    }
                    return false;
                case ModeId.SuperHostRoles:
                    if (RoleClass.Assassin.TriggerPlayer != null) return false;
                    switch (__instance.getRole())
                    {
                        case RoleId.RemoteSheriff:
                            return false;
                        case RoleId.Egoist:
                            if (!RoleClass.Egoist.UseKill) return false;
                            break;
                        case RoleId.FalseCharges:
                            target.RpcMurderPlayer(__instance);
                            RoleClass.FalseCharges.FalseChargePlayers[__instance.PlayerId] = target.PlayerId;
                            RoleClass.FalseCharges.AllTurns[__instance.PlayerId] = RoleClass.FalseCharges.DefaultTurn;
                            return false;
                        case RoleId.truelover:
                            if (!__instance.IsLovers())
                            {
                                if (target == null || target.IsLovers() || RoleClass.truelover.CreatePlayers.Contains(__instance.PlayerId)) return false;
                                RoleClass.truelover.CreatePlayers.Add(__instance.PlayerId);
                                RoleHelpers.SetLovers(__instance, target);
                                RoleHelpers.SetLoversRPC(__instance, target);
                                //__instance.RpcSetRoleDesync(RoleTypes.GuardianAngel);
                                Mode.SuperHostRoles.FixedUpdate.SetRoleName(__instance);
                                Mode.SuperHostRoles.FixedUpdate.SetRoleName(target);
                            }
                            return false;
                        case RoleId.Sheriff:
                            if (!RoleClass.Sheriff.KillCount.ContainsKey(__instance.PlayerId) || RoleClass.Sheriff.KillCount[__instance.PlayerId] >= 1)
                            {
                                if (!Sheriff.IsSheriffKill(target))
                                {
                                    FinalStatusPatch.FinalStatusData.FinalStatuses[__instance.PlayerId] = FinalStatus.SheriffMisFire;
                                    __instance.RpcMurderPlayer(__instance);
                                }
                                else
                                {
                                    FinalStatusPatch.FinalStatusData.FinalStatuses[target.PlayerId] = FinalStatus.SheriffKill;
                                    if (RoleClass.Sheriff.KillCount.ContainsKey(__instance.PlayerId))
                                    {
                                        RoleClass.Sheriff.KillCount[__instance.PlayerId]--;
                                    }
                                    else
                                    {
                                        RoleClass.Sheriff.KillCount[__instance.PlayerId] = (int)CustomOptions.SheriffKillMaxCount.getFloat() - 1;
                                    }
                                    __instance.RpcMurderPlayerCheck(target);
                                    Mode.SuperHostRoles.FixedUpdate.SetRoleName(__instance);
                                }
                            }
                            return false;
                        case RoleId.MadMaker:
                            if (!target.isImpostor())
                            {
                                if (target == null || RoleClass.MadMaker.CreatePlayers.Contains(__instance.PlayerId)) return false;
                                RoleClass.MadMaker.CreatePlayers.Add(__instance.PlayerId);
                                target.RpcSetRoleDesync(RoleTypes.GuardianAngel);
                                target.setRoleRPC(RoleId.MadMate);
                                //__instance.RpcSetRoleDesync(RoleTypes.GuardianAngel);
                                Mode.SuperHostRoles.FixedUpdate.SetRoleName(target);
                            }
                            else
                            {
                                __instance.RpcMurderPlayer(__instance);
                            }
                            return false;
                        case RoleId.Demon:
                            if (!__instance.IsCursed(target))
                            {
                                Demon.DemonCurse(target, __instance);
                                target.RpcProtectPlayerPrivate(target, 0, __instance);
                                new LateTask(() =>
                                {
                                    SyncSetting.MurderSyncSetting(__instance);
                                    __instance.RPCMurderPlayerPrivate(target);
                                }, 0.5f);
                                Mode.SuperHostRoles.FixedUpdate.SetRoleName(__instance);
                            }
                            return false;
                        case RoleId.OverKiller:
                            __instance.RpcMurderPlayerCheck(target);
                            foreach (PlayerControl p in CachedPlayer.AllPlayers)
                            {
                                if (!p.Data.Disconnected && p.PlayerId != target.PlayerId)
                                {
                                    if (p.PlayerId != 0)
                                    {
                                        for (int i = 0; i < RoleClass.OverKiller.KillCount - 1; i++)
                                        {
                                            __instance.RPCMurderPlayerPrivate(target, p);
                                        }
                                    }
                                    else
                                    {
                                        for (int i = 0; i < RoleClass.OverKiller.KillCount - 1; i++)
                                        {
                                            __instance.MurderPlayer(target);
                                        }
                                    }
                                }
                            }
                            return false;
                        case RoleId.Arsonist:
                            try
                            {
                                Arsonist.ArsonistTimer[__instance.PlayerId] =
                                        (Arsonist.ArsonistTimer[__instance.PlayerId] = RoleClass.Arsonist.DurationTime);
                                if (Arsonist.ArsonistTimer[__instance.PlayerId] <= RoleClass.Arsonist.DurationTime)//時間以上一緒にいて塗れた時
                                {
                                    if (!__instance.IsDoused(target))
                                    {
                                        Arsonist.ArsonistDouse(target, __instance);
                                        target.RpcProtectPlayerPrivate(target, 0, __instance);
                                        new LateTask(() =>
                                        {
                                            SyncSetting.MurderSyncSetting(__instance);
                                            __instance.RPCMurderPlayerPrivate(target);
                                        }, 0.5f);
                                        Mode.SuperHostRoles.FixedUpdate.SetRoleName(__instance);
                                    }
                                }
                                else
                                {
                                    float dis;
                                    dis = Vector2.Distance(__instance.transform.position, target.transform.position);//距離を出す
                                    if (dis <= 1.75f)//一定の距離にターゲットがいるならば時間をカウント
                                    {
                                        Arsonist.ArsonistTimer[__instance.PlayerId] =
                                        (Arsonist.ArsonistTimer[__instance.PlayerId] - Time.fixedDeltaTime);
                                    }
                                    else//それ以外は削除
                                    {
                                        Arsonist.ArsonistTimer.Remove(__instance.PlayerId);
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                SuperNewRolesPlugin.Logger.LogError(e);
                            }
                            return false;
                    }
                    break;
                case ModeId.Detective:
                    if (target.PlayerId == Mode.Detective.main.DetectivePlayer.PlayerId) return false;
                    break;

            }
            if (ModeHandler.isMode(ModeId.SuperHostRoles))
            {
                SyncSetting.CustomSyncSettings(__instance);
                SyncSetting.CustomSyncSettings(target);
                if (target.isRole(RoleId.StuntMan))
                {
                    if (EvilEraser.IsOKAndTryUse(EvilEraser.BlockTypes.StuntmanGuard, __instance))
                    {
                        if (!RoleClass.StuntMan.GuardCount.ContainsKey(target.PlayerId))
                        {
                            RoleClass.StuntMan.GuardCount[target.PlayerId] = (int)CustomOptions.StuntManMaxGuardCount.getFloat() - 1;
                            target.RpcProtectPlayer(target, 0);
                            new LateTask(() => __instance.RpcMurderPlayer(target), 0.5f);
                            return false;
                        }
                        else
                        {
                            if (!(RoleClass.StuntMan.GuardCount[target.PlayerId] <= 0))
                            {
                                RoleClass.StuntMan.GuardCount[target.PlayerId]--;
                                target.RpcProtectPlayer(target, 0);
                                new LateTask(() => __instance.RpcMurderPlayer(target), 0.5f);
                                return false;
                            }
                        }
                    }
                }
                else if (target.isRole(RoleId.MadStuntMan))
                {
                    if (EvilEraser.IsOKAndTryUse(EvilEraser.BlockTypes.MadStuntmanGuard, __instance))
                    {
                        if (!RoleClass.MadStuntMan.GuardCount.ContainsKey(target.PlayerId))
                        {
                            target.RpcProtectPlayer(target, 0);
                            new LateTask(() => __instance.RpcMurderPlayer(target), 0.5f);
                            return false;
                        }
                        else
                        {
                            if (!(RoleClass.MadStuntMan.GuardCount[target.PlayerId] <= 0))
                            {
                                RoleClass.MadStuntMan.GuardCount[target.PlayerId]--;
                                target.RpcProtectPlayer(target, 0);
                                new LateTask(() => __instance.RpcMurderPlayer(target), 0.5f);
                                return false;
                            }
                        }
                    }
                }
                else if (target.isRole(RoleId.Fox))
                {
                    if (EvilEraser.IsOKAndTryUse(EvilEraser.BlockTypes.FoxGuard, __instance))
                    {
                        if (!RoleClass.Fox.KillGuard.ContainsKey(target.PlayerId))
                        {
                            target.RpcProtectPlayer(target, 0);
                            new LateTask(() => __instance.RpcMurderPlayer(target), 0.5f);
                            return false;
                        }
                        else
                        {
                            if (!(RoleClass.Fox.KillGuard[target.PlayerId] <= 0))
                            {
                                RoleClass.Fox.KillGuard[target.PlayerId]--;
                                target.RpcProtectPlayer(target, 0);
                                new LateTask(() => __instance.RpcMurderPlayer(target), 0.5f);
                                return false;
                            }
                        }
                    }
                }
            }
            __instance.RpcMurderPlayerCheck(target);
            return false;
        }
        public static void RpcCheckExile(this PlayerControl __instance)
        {
            if (__instance.isRole(RoleId.Assassin))
            {
                new LateTask(() =>
                {
                    if (AmongUsClient.Instance.AmHost)
                    {
                        MeetingRoomManager.Instance.AssignSelf(__instance, null);
                        FastDestroyableSingleton<HudManager>.Instance.OpenMeetingRoom(__instance);
                        __instance.RpcStartMeeting(null);
                    }
                }, 0.5f);
                new LateTask(() =>
                {
                    __instance.RpcSetName($"<size=200%>{CustomOptions.cs(RoleClass.Marine.color, IntroDate.MarineIntro.NameKey + "Name")}は誰だ？</size>");
                }, 2f);
                new LateTask(() =>
                {
                    __instance.RpcSendChat($"\n{ModTranslation.getString("MarineWhois")}");
                }, 2.5f);
                new LateTask(() =>
                {
                    __instance.RpcSetName(__instance.getDefaultName());
                }, 2f);
                RoleClass.Assassin.TriggerPlayer = __instance;
                return;
            }
            __instance.RpcInnerExiled();
        }
        public static void RpcMurderPlayerCheck(this PlayerControl __instance, PlayerControl target)
        {
            if (target.isRole(RoleId.Assassin))
            {
                new LateTask(() =>
                {
                    if (AmongUsClient.Instance.AmHost)
                    {
                        MeetingRoomManager.Instance.AssignSelf(target, null);
                        FastDestroyableSingleton<HudManager>.Instance.OpenMeetingRoom(target);
                        target.RpcStartMeeting(null);
                    }
                    RoleClass.Assassin.TriggerPlayer = target;
                }, 0.5f);
                new LateTask(() =>
                {
                    target.RpcSetName($"<size=200%>{CustomOptions.cs(RoleClass.Marine.color, IntroDate.MarineIntro.NameKey + "Name")}は誰だ？</size>");
                }, 2f);
                new LateTask(() =>
                {
                    target.RpcSendChat($"\n{ModTranslation.getString("MarineWhois")}");
                }, 2.5f);
                new LateTask(() =>
                {
                    target.RpcSetName(target.getDefaultName());
                }, 2f);
                return;
            }
            __instance.RpcMurderPlayer(target);
        }
    }
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Die))]
    public static class DiePatch
    {
        public static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
        {
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetKillTimer))]
    static class PlayerControlSetCoolDownPatch
    {
        public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] float time)
        {
            if (PlayerControl.GameOptions.killCooldown == time && !RoleClass.IsCoolTimeSetted)
            {
                __instance.SetKillTimerUnchecked(RoleHelpers.GetEndMeetingKillCoolTime(__instance), RoleHelpers.GetEndMeetingKillCoolTime(__instance));
                RoleClass.IsCoolTimeSetted = true;
                return false;
            }
            return true;
        }
    }
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
    public static class MurderPlayerPatch
    {
        public static bool resetToCrewmate = false;
        public static bool resetToDead = false;
        public static bool Prefix(PlayerControl __instance, PlayerControl target)
        {
            EvilGambler.EvilGamblerMurder.Prefix(__instance, target);
            if (ModeHandler.isMode(ModeId.Default))
            {
                target.resetChange();
                if (target.PlayerId == CachedPlayer.LocalPlayer.PlayerId)
                {
                    if (PlayerControl.LocalPlayer.isRole(RoleId.SideKiller))
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
                else if (__instance.PlayerId == CachedPlayer.LocalPlayer.PlayerId)
                {
                    if (__instance.isRole(RoleId.EvilGambler))
                    {
                        if (RoleClass.EvilGambler.GetSuc())
                        {
                            PlayerControl.LocalPlayer.SetKillTimer(RoleClass.EvilGambler.SucCool);
                        }
                        else
                        {
                            PlayerControl.LocalPlayer.SetKillTimer(RoleClass.EvilGambler.NotSucCool);
                        }
                    }
                }

                if (AmongUsClient.Instance.AmHost && __instance.PlayerId != target.PlayerId)
                {
                    switch (target.getRole())
                    {
                        case RoleId.Fox:
                            Fox.FoxMurderPatch.Prefix(__instance, target);
                            break;
                    }
                }
            }
            return true;
        }
        public static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
        {
            // SuperNewRolesPlugin.Logger.LogInfo("MurderPlayer発生！元:" + __instance.getDefaultName() + "、ターゲット:" + target.getDefaultName());
            // Collect dead player info
            DeadPlayer deadPlayer = new DeadPlayer(target, DateTime.UtcNow, DeathReason.Kill, __instance);
            DeadPlayer.deadPlayers.Add(deadPlayer);
            FinalStatusPatch.FinalStatusData.FinalStatuses[target.PlayerId] = FinalStatus.Kill;

            SerialKiller.MurderPlayer(__instance, target);
            Seer.ExileControllerWrapUpPatch.MurderPlayerPatch.Postfix(__instance, target);

            if (ModeHandler.isMode(ModeId.SuperHostRoles))
            {
                if (AmongUsClient.Instance.AmHost)
                {
                    MurderPlayer.Postfix(__instance, target);
                }
            }
            else if (ModeHandler.isMode(ModeId.Detective))
            {
                Mode.Detective.main.MurderPatch(target);
            }
            else if (ModeHandler.isMode(ModeId.Default))
            {
                if (target.isRole(RoleId.Assassin))
                {
                    target.Revive();
                    target.Data.IsDead = false;
                    RPCProcedure.CleanBody(target.PlayerId);
                    new LateTask(() =>
                    {
                        if (AmongUsClient.Instance.AmHost)
                        {
                            MeetingRoomManager.Instance.AssignSelf(target, null);
                            FastDestroyableSingleton<HudManager>.Instance.OpenMeetingRoom(target);
                            target.RpcStartMeeting(null);
                        }
                    }, 0.5f);
                    RoleClass.Assassin.TriggerPlayer = target;
                    return;
                }
                Levelinger.MurderPlayer(__instance, target);
                if (RoleClass.Lovers.SameDie && target.IsLovers())
                {
                    if (__instance.PlayerId == CachedPlayer.LocalPlayer.PlayerId)
                    {
                        PlayerControl SideLoverPlayer = target.GetOneSideLovers();
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
                if (target.IsQuarreled())
                {
                    if (AmongUsClient.Instance.AmHost)
                    {
                        var Side = RoleHelpers.GetOneSideQuarreled(target);
                        if (Side.isDead())
                        {
                            RPCProcedure.ShareWinner(target.PlayerId);

                            MessageWriter Writer = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.ShareWinner, Hazel.SendOption.Reliable, -1);
                            Writer.Write(target.PlayerId);
                            AmongUsClient.Instance.FinishRpcImmediately(Writer);
                            Roles.RoleClass.Quarreled.IsQuarreledWin = true;
                            ShipStatus.RpcEndGame((GameOverReason)CustomGameOverReason.QuarreledWin, false);
                        }
                    }
                }
                Minimalist.MurderPatch.Postfix(__instance);
            }
            if (__instance.PlayerId == CachedPlayer.LocalPlayer.PlayerId)
            {
                if (__instance.isImpostor() && !__instance.isRole(RoleId.EvilGambler))
                {
                    PlayerControl.LocalPlayer.SetKillTimerUnchecked(RoleHelpers.getCoolTime(__instance), RoleHelpers.getCoolTime(__instance));
                }
            }
        }
    }
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Exiled))]
    public static class ExilePlayerPatch
    {
        public static void Postfix(PlayerControl __instance)
        {
            // Collect dead player info
            DeadPlayer deadPlayer = new DeadPlayer(__instance, DateTime.UtcNow, DeathReason.Exile, null);
            DeadPlayer.deadPlayers.Add(deadPlayer);
            FinalStatusPatch.FinalStatusData.FinalStatuses[__instance.PlayerId] = FinalStatus.Exiled;
            if (ModeHandler.isMode(ModeId.Default))
            {
                if (__instance.isRole(RoleId.Assassin) && !RoleClass.Assassin.MeetingEndPlayers.Contains(__instance.PlayerId))
                {
                    __instance.Revive();
                    __instance.Data.IsDead = false;
                    RPCProcedure.CleanBody(__instance.PlayerId);
                    new LateTask(() =>
                    {
                        if (AmongUsClient.Instance.AmHost)
                        {
                            MeetingRoomManager.Instance.AssignSelf(__instance, null);
                            FastDestroyableSingleton<HudManager>.Instance.OpenMeetingRoom(__instance);
                            __instance.RpcStartMeeting(null);
                        }
                    }, 0.5f);
                    RoleClass.Assassin.TriggerPlayer = __instance;
                    return;
                }
                if (RoleClass.Lovers.SameDie && __instance.IsLovers())
                {
                    if (__instance.PlayerId == CachedPlayer.LocalPlayer.PlayerId)
                    {
                        PlayerControl SideLoverPlayer = __instance.GetOneSideLovers();
                        if (SideLoverPlayer.isAlive())
                        {
                            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.ExiledRPC, Hazel.SendOption.Reliable, -1);
                            writer.Write(SideLoverPlayer.PlayerId);
                            AmongUsClient.Instance.FinishRpcImmediately(writer);
                            RPCProcedure.ExiledRPC(SideLoverPlayer.PlayerId);
                        }
                    }
                }
            }
        }
    }
    public static class PlayerControlFixedUpdatePatch
    {
        public static PlayerControl setTarget(bool onlyCrewmates = false, bool targetPlayersInVents = false, List<PlayerControl> untargetablePlayers = null, PlayerControl targetingPlayer = null)
        {
            PlayerControl result = null;
            float num = GameOptionsData.KillDistances[Mathf.Clamp(PlayerControl.GameOptions.KillDistance, 0, 2)];
            if (!MapUtilities.CachedShipStatus) return result;
            if (targetingPlayer == null) targetingPlayer = PlayerControl.LocalPlayer;
            if (targetingPlayer.Data.IsDead || targetingPlayer.inVent) return result;

            if (untargetablePlayers == null)
            {
                untargetablePlayers = new List<PlayerControl>();
            }

            Vector2 truePosition = targetingPlayer.GetTruePosition();
            Il2CppSystem.Collections.Generic.List<GameData.PlayerInfo> allPlayers = GameData.Instance.AllPlayers;
            for (int i = 0; i < allPlayers.Count; i++)
            {
                GameData.PlayerInfo playerInfo = allPlayers[i];
                if (!playerInfo.Disconnected && playerInfo.PlayerId != targetingPlayer.PlayerId && !playerInfo.IsDead && (!onlyCrewmates || !playerInfo.Role.IsImpostor))
                {
                    PlayerControl @object = playerInfo.Object;
                    if (untargetablePlayers.Any(x => x == @object))
                    {
                        // if that player is not targetable: skip check
                        continue;
                    }
                    if (@object && (!@object.inVent || targetPlayersInVents))
                    {
                        Vector2 vector = @object.GetTruePosition() - truePosition;
                        float magnitude = vector.magnitude;
                        if (magnitude <= num && !PhysicsHelpers.AnyNonTriggersBetween(truePosition, vector.normalized, magnitude, Constants.ShipAndObjectsMask))
                        {
                            result = @object;
                            num = magnitude;
                        }
                    }
                }
            }
            return result;
        }
    }
}