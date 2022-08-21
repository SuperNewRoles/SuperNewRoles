using System;
using System.Collections.Generic;
using System.Linq;
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
using UnityEngine;
using static SuperNewRoles.Helpers.DesyncHelpers;
using static SuperNewRoles.ModHelpers;

namespace SuperNewRoles.Patches
{
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcUsePlatform))]
    public class UsePlatformPlayerControlPatch
    {
        public static bool Prefix(PlayerControl __instance)
        {
            Logger.Info("来た");
            if (AmongUsClient.Instance.AmHost)
            {
                AirshipStatus airshipStatus = GameObject.FindObjectOfType<AirshipStatus>();
                if (airshipStatus)
                {
                    airshipStatus.GapPlatform.Use(__instance);
                }
            }
            else
            {
                AmongUsClient.Instance.StartRpc(__instance.NetId, 32, (SendOption)1).EndMessage();
            }
            return false;
        }
    }
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Shapeshift))]
    class RpcShapesihftPatch
    {
        public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
        {
            SyncSetting.CustomSyncSettings();
            if (RoleClass.Assassin.TriggerPlayer != null) return false;
            if (target.IsBot()) return true;
            if (__instance.PlayerId == target.PlayerId)
            {
                if (ModeHandler.IsMode(ModeId.SuperHostRoles) && AmongUsClient.Instance.AmHost)
                {
                    if (__instance.IsRole(RoleId.RemoteSheriff))
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
            if (ModeHandler.IsMode(ModeId.SuperHostRoles))
            {
                switch (__instance.GetRole())
                {
                    case RoleId.RemoteSheriff:
                        if (AmongUsClient.Instance.AmHost)
                        {
                            if (target.IsDead()) return true;
                            if (!RoleClass.RemoteSheriff.KillCount.ContainsKey(__instance.PlayerId) || RoleClass.RemoteSheriff.KillCount[__instance.PlayerId] >= 1)
                            {
                                if (!Sheriff.IsRemoteSheriffKill(target) || target.IsRole(RoleId.RemoteSheriff))
                                {
                                    FinalStatusPatch.FinalStatusData.FinalStatuses[__instance.PlayerId] = FinalStatus.SheriffMisFire;
                                    __instance.RpcMurderPlayer(__instance);
                                    return true;
                                }
                                else
                                {
                                    FinalStatusPatch.FinalStatusData.FinalStatuses[target.PlayerId] = FinalStatus.SheriffKill;
                                    if (RoleClass.RemoteSheriff.KillCount.ContainsKey(__instance.PlayerId))
                                        RoleClass.RemoteSheriff.KillCount[__instance.PlayerId]--;
                                    else
                                        RoleClass.RemoteSheriff.KillCount[__instance.PlayerId] = CustomOptions.RemoteSheriffKillMaxCount.GetInt() - 1;
                                    if (RoleClass.RemoteSheriff.IsKillTeleport)
                                        __instance.RpcMurderPlayerCheck(target);
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
                                if (p.IsAlive() && p.PlayerId != __instance.PlayerId)
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
                                    if (p.IsAlive() && p.PlayerId != __instance.PlayerId)
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
                                    MessageWriter Writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.ShareWinner, SendOption.Reliable, -1);
                                    Writer.Write(p.PlayerId);
                                    AmongUsClient.Instance.FinishRpcImmediately(Writer);

                                    Writer = RPCHelper.StartRPC(CustomRPC.CustomRPC.SetWinCond);
                                    Writer.Write((byte)CustomGameOverReason.ArsonistWin);
                                    Writer.EndRPC();
                                    RPCProcedure.SetWinCond((byte)CustomGameOverReason.ArsonistWin);

                                    RoleClass.Arsonist.TriggerArsonistWin = true;
                                    AdditionalTempData.winCondition = WinCondition.ArsonistWin;
                                    __instance.enabled = false;
                                    ShipStatus.RpcEndGame((GameOverReason)CustomGameOverReason.ArsonistWin, false);
                                    return true;
                                }
                            }
                        }
                        return false;
                    case RoleId.SuicideWisher:
                        if (AmongUsClient.Instance.AmHost)
                        {
                            __instance.RpcMurderPlayer(__instance);
                        }
                        return false;
                    case RoleId.ToiletFan:
                        ShipStatus.Instance.RpcRepairSystem(SystemTypes.Doors, 79);
                        ShipStatus.Instance.RpcRepairSystem(SystemTypes.Doors, 80);
                        ShipStatus.Instance.RpcRepairSystem(SystemTypes.Doors, 81);
                        ShipStatus.Instance.RpcRepairSystem(SystemTypes.Doors, 82);
                        return false;
                    case RoleId.NiceButtoner:
                        if (RoleClass.NiceButtoner.SkillCountSHR.ContainsKey(__instance.PlayerId))
                            RoleClass.NiceButtoner.SkillCountSHR[__instance.PlayerId]--;
                        else
                            RoleClass.NiceButtoner.SkillCountSHR[__instance.PlayerId] = CustomOptions.NiceButtonerCount.GetInt() - 1;
                        if (AmongUsClient.Instance.AmHost && RoleClass.NiceButtoner.SkillCountSHR[__instance.PlayerId] + 1 >= 1) EvilButtoner.EvilButtonerStartMeetingSHR(__instance);
                        return false;
                    case RoleId.EvilButtoner:
                        if (RoleClass.EvilButtoner.SkillCountSHR.ContainsKey(__instance.PlayerId))
                            RoleClass.EvilButtoner.SkillCountSHR[__instance.PlayerId]--;
                        else
                            RoleClass.EvilButtoner.SkillCountSHR[__instance.PlayerId] = CustomOptions.EvilButtonerCount.GetInt() - 1;
                        if (AmongUsClient.Instance.AmHost && RoleClass.EvilButtoner.SkillCountSHR[__instance.PlayerId] + 1 >= 1) EvilButtoner.EvilButtonerStartMeetingSHR(__instance);
                        return false;
                }
            }
            return true;
        }
    }
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CheckProtect))]
    class CheckProtectPatch
    {
        public static bool Prefix()
        {
            return !ModeHandler.IsMode(ModeId.SuperHostRoles);
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
            if (PlayerControl.LocalPlayer.IsRole(RoleId.RemoteSheriff))
            {
                if (RoleClass.RemoteSheriff.KillMaxCount > 0)
                {
                    if (ModeHandler.IsMode(ModeId.SuperHostRoles))
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
                    else if (ModeHandler.IsMode(ModeId.Default))
                    {
                        if (player.IsAlive())
                        {
                            var Target = player;
                            var misfire = !Sheriff.IsRemoteSheriffKill(Target);
                            var TargetID = Target.PlayerId;
                            var LocalID = CachedPlayer.LocalPlayer.PlayerId;

                            PlayerControl.LocalPlayer.RpcShapeshift(PlayerControl.LocalPlayer, true);

                            RPCProcedure.SheriffKill(LocalID, TargetID, misfire);
                            MessageWriter killWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.SheriffKill, SendOption.Reliable, -1);
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
            if (!ModeHandler.IsMode(ModeId.Default))
            {
                if (ModeHandler.IsMode(ModeId.SuperHostRoles))
                {
                    if (PlayerControl.LocalPlayer.IsRole(RoleId.RemoteSheriff))
                    {
                        if (__instance.isActiveAndEnabled && PlayerControl.LocalPlayer.IsAlive() && PlayerControl.LocalPlayer.CanMove && !__instance.isCoolingDown && RoleClass.RemoteSheriff.KillMaxCount > 0)
                        {
                            DestroyableSingleton<RoleManager>.Instance.SetRole(PlayerControl.LocalPlayer, RoleTypes.Shapeshifter);
                            foreach (PlayerControl p in CachedPlayer.AllPlayers)
                            {
                                p.Data.Role.NameColor = Color.white;
                            }
                            CachedPlayer.LocalPlayer.Data.Role.TryCast<ShapeshifterRole>().UseAbility();
                            foreach (PlayerControl p in CachedPlayer.AllPlayers)
                            {
                                if (p.IsImpostor())
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
            if (__instance.isActiveAndEnabled && __instance.currentTarget && !__instance.isCoolingDown && PlayerControl.LocalPlayer.IsAlive() && PlayerControl.LocalPlayer.CanMove)
            {
                if (PlayerControl.LocalPlayer.IsRole(RoleId.Kunoichi))
                {
                    Kunoichi.KillButtonClick();
                    return false;
                }
                if (!(__instance.currentTarget.IsRole(RoleId.Bait) || __instance.currentTarget.IsRole(RoleId.NiceRedRidingHood)) && PlayerControl.LocalPlayer.IsRole(RoleId.Vampire))
                {
                    PlayerControl.LocalPlayer.killTimer = RoleHelpers.GetCoolTime(PlayerControl.LocalPlayer);
                    RoleClass.Vampire.target = __instance.currentTarget;
                    RoleClass.Vampire.KillTimer = DateTime.Now;
                    RoleClass.Vampire.Timer = RoleClass.Vampire.KillDelay;
                    return false;
                }
                bool showAnimation = true;
                /*
                if (PlayerControl.LocalPlayer.IsRole(RoleType.Ninja) && Ninja.isStealthed(PlayerControl.LocalPlayer))
                {
                    showAnimation = false;
                }
                */

                // Use an unchecked kill command, to allow shorter kill cooldowns etc. without getting kicked
                MurderAttemptResult res = CheckMuderAttemptAndKill(PlayerControl.LocalPlayer, __instance.currentTarget, showAnimation: showAnimation);
                // Handle blank kill
                if (res == MurderAttemptResult.BlankKill)
                {
                    PlayerControl.LocalPlayer.killTimer = RoleHelpers.GetCoolTime(PlayerControl.LocalPlayer);
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
            SuperNewRolesPlugin.Logger.LogInfo("a(Murder)" + __instance.Data.PlayerName + " => " + target.Data.PlayerName);
            if (__instance.IsBot() || target.IsBot()
                || __instance.IsDead()
                || target.IsDead()
                || (!RoleClass.IsStart && AmongUsClient.Instance.GameMode != GameModes.FreePlay)) return false;
            if (__instance.PlayerId == target.PlayerId) { __instance.RpcMurderPlayer(target); return false; }
            SuperNewRolesPlugin.Logger.LogInfo("b(Murder)" + __instance.Data.PlayerName + " => " + target.Data.PlayerName);
            if (!AmongUsClient.Instance.AmHost)
                return true;
            SuperNewRolesPlugin.Logger.LogInfo("c(Murder)" + __instance.Data.PlayerName + " => " + target.Data.PlayerName);
            switch (ModeHandler.GetMode())
            {
                case ModeId.Zombie:
                    return false;
                case ModeId.BattleRoyal:
                    if (isKill)
                    {
                        return false;
                    }
                    if (Mode.BattleRoyal.Main.StartSeconds <= 0)
                    {
                        if (Mode.BattleRoyal.Main.IsTeamBattle)
                        {
                            foreach (List<PlayerControl> teams in Mode.BattleRoyal.Main.Teams)
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
                        SuperNewRolesPlugin.Logger.LogInfo("[CheckMurder]RateTask:" + (AmongUsClient.Instance.Ping / 1000f) * 2f);
                        isKill = true;
                        if (__instance.PlayerId != 0)
                        {
                            target.Data.IsDead = true;
                            __instance.RpcMurderPlayer(target);
                            isKill = false;
                        }
                        else
                        {
                            new LateTask(() =>
                            {
                                if (__instance.IsAlive() && target.IsAlive())
                                {
                                    __instance.RpcMurderPlayer(target);
                                }
                                isKill = false;
                            }, AmongUsClient.Instance.Ping / 1000f * 1.1f);
                        }
                    }
                    return false;
                case ModeId.Default://通常モード
                    switch (__instance.GetRole())
                    {
                        case RoleId.FastMaker:
                            if (!RoleClass.FastMaker.IsCreatedMadMate)//まだ作ってなくて、設定が有効の時
                            {
                                if (target == null || RoleClass.FastMaker.CreatePlayers.Contains(__instance.PlayerId)) return false;
                                target.RpcProtectPlayer(target, 0);//キルを無効にする為守護をかける
                                                                   //守護がかかるのを待つためのLateTask
                                new LateTask(() =>
                                    {
                                        RoleClass.FastMaker.CreatePlayers.Add(__instance.PlayerId);
                                        target.RpcSetRoleDesync(RoleTypes.GuardianAngel);//守護天使にして
                                        target.SetRoleRPC(RoleId.MadMate);//マッドにする
                                        Mode.SuperHostRoles.FixedUpdate.SetRoleName(target);//名前も変える
                                        RoleClass.FastMaker.IsCreatedMadMate = true;//作ったことにする
                                        SuperNewRolesPlugin.Logger.LogInfo("[FastMakerSNR]マッドを作ったよ");
                                    }, 0.5f);
                            }
                            else
                            {
                                //作ってたら普通のキル(此処にMurderPlayerを使用すると2回キルされる為ログのみ表示)
                                SuperNewRolesPlugin.Logger.LogInfo("[FastMakerSNR]作ったので普通のキル");
                            }
                            return false;
                    }
                    break;
                case ModeId.SuperHostRoles:
                    SuperNewRolesPlugin.Logger.LogInfo("d(Murder)" + __instance.Data.PlayerName + " => " + target.Data.PlayerName);
                    if (RoleClass.Assassin.TriggerPlayer != null) return false;
                    SuperNewRolesPlugin.Logger.LogInfo("e(Murder)" + __instance.Data.PlayerName + " => " + target.Data.PlayerName);
                    switch (__instance.GetRole())
                    {
                        case RoleId.RemoteSheriff:
                        case RoleId.ToiletFan:
                        case RoleId.NiceButtoner:
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
                                if (target == null || target.IsLovers() || RoleClass.Truelover.CreatePlayers.Contains(__instance.PlayerId)) return false;
                                RoleClass.Truelover.CreatePlayers.Add(__instance.PlayerId);
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
                                        RoleClass.Sheriff.KillCount[__instance.PlayerId] = CustomOptions.SheriffKillMaxCount.GetInt() - 1;
                                    }
                                    __instance.RpcMurderPlayerCheck(target);
                                    Mode.SuperHostRoles.FixedUpdate.SetRoleName(__instance);
                                }
                            }
                            return false;
                        case RoleId.MadMaker:
                            if (!target.IsImpostor())
                            {
                                if (target == null || RoleClass.MadMaker.CreatePlayers.Contains(__instance.PlayerId)) return false;
                                RoleClass.MadMaker.CreatePlayers.Add(__instance.PlayerId);
                                target.RpcSetRoleDesync(RoleTypes.GuardianAngel);
                                target.SetRoleRPC(RoleId.MadMate);
                                //__instance.RpcSetRoleDesync(RoleTypes.GuardianAngel);
                                Mode.SuperHostRoles.FixedUpdate.SetRoleName(target);
                            }
                            else
                            {
                                if (AmongUsClient.Instance.AmHost)
                                {
                                    foreach (PlayerControl p in RoleClass.MadMaker.MadMakerPlayer)
                                    {
                                        p.RpcMurderPlayer(p);
                                    }
                                }
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
                                        Arsonist.ArsonistTimer[__instance.PlayerId] = RoleClass.Arsonist.DurationTime;
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
                                        Arsonist.ArsonistTimer[__instance.PlayerId] - Time.fixedDeltaTime;
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
                        case RoleId.Mafia:
                            if (!Mafia.IsKillFlag()) return false;
                            break;
                        case RoleId.FastMaker:
                            if (!RoleClass.FastMaker.IsCreatedMadMate)//まだ作ってなくて、設定が有効の時
                            {
                                if (target == null || RoleClass.FastMaker.CreatePlayers.Contains(__instance.PlayerId)) return false;
                                target.RpcProtectPlayer(target, 0);//キルを無効にする為守護をかける
                                //守護がかかるのを待つためのLateTask
                                new LateTask(() =>
                                    {
                                        RoleClass.FastMaker.CreatePlayers.Add(__instance.PlayerId);
                                        target.RpcSetRoleDesync(RoleTypes.GuardianAngel);//守護天使にして
                                        target.SetRoleRPC(RoleId.MadMate);//マッドにする
                                        Mode.SuperHostRoles.FixedUpdate.SetRoleName(target);//名前も変える
                                        RoleClass.FastMaker.IsCreatedMadMate = true;//作ったことにする
                                        SuperNewRolesPlugin.Logger.LogInfo("[FastMakerSHR]マッドを作ったよ");
                                    }, 0.5f);
                            }
                            else
                            {
                                //作ってたら普通のキル(此処にMurderPlayerを使用すると2回キルされる為ログのみ表示)
                                SuperNewRolesPlugin.Logger.LogInfo("[FastMakerSHR]作ったので普通のキル");
                            }
                            break;
                        case RoleId.Jackal:
                            if (!RoleClass.Jackal.IsCreatedFriend && RoleClass.Jackal.CanCreateFriend)//まだ作ってなくて、設定が有効の時
                            {
                                SuperNewRolesPlugin.Logger.LogInfo("まだ作ってなくて、設定が有効の時なんでフレンズ作成");
                                if (target == null || RoleClass.Jackal.CreatePlayers.Contains(__instance.PlayerId)) return false;
                                target.RpcProtectPlayer(target, 0);//キルを無効にする為守護をかける
                                //守護がかかるのを待つためのLateTask
                                new LateTask(() =>
                                    {
                                        RoleClass.Jackal.CreatePlayers.Add(__instance.PlayerId);
                                        target.RpcSetRoleDesync(RoleTypes.GuardianAngel);//守護天使にして
                                        target.RPCSetRoleUnchecked(RoleTypes.Crewmate);//クルーにして
                                        target.SetRoleRPC(RoleId.JackalFriends);//フレンズにする
                                        Mode.SuperHostRoles.FixedUpdate.SetRoleName(target);//名前も変える
                                        RoleClass.Jackal.IsCreatedFriend = true;//作ったことにする
                                        SuperNewRolesPlugin.Logger.LogInfo("[JackalSHR]フレンズを作ったよ");
                                    }, 0.5f);
                            }
                            else
                            {
                                // キルができた理由のログを表示する(此処にMurderPlayerを使用すると2回キルされる為ログのみ表示)
                                if (!RoleClass.Jackal.CanCreateFriend) SuperNewRolesPlugin.Logger.LogInfo("[JackalSHR] フレンズを作る設定ではない為 普通のキル");
                                else if (RoleClass.Jackal.CanCreateFriend && RoleClass.Jackal.IsCreatedFriend) SuperNewRolesPlugin.Logger.LogInfo("[JackalSHR] 作ったので 普通のキル");
                                else SuperNewRolesPlugin.Logger.LogInfo("[JackalSHR] 不正なキル");
                            }
                            break;
                    }
                    break;
                case ModeId.Detective:
                    if (target.PlayerId == Mode.Detective.Main.DetectivePlayer.PlayerId) return false;
                    break;
            }
            SuperNewRolesPlugin.Logger.LogInfo("f(Murder)" + __instance.Data.PlayerName + " => " + target.Data.PlayerName);
            if (ModeHandler.IsMode(ModeId.SuperHostRoles))
            {
                SyncSetting.CustomSyncSettings(__instance);
                SyncSetting.CustomSyncSettings(target);
                if (target.IsRole(RoleId.StuntMan))
                {
                    if (EvilEraser.IsOKAndTryUse(EvilEraser.BlockTypes.StuntmanGuard, __instance))
                    {
                        if (!RoleClass.StuntMan.GuardCount.ContainsKey(target.PlayerId))
                        {
                            RoleClass.StuntMan.GuardCount[target.PlayerId] = CustomOptions.StuntManMaxGuardCount.GetInt() - 1;
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
                else if (target.IsRole(RoleId.MadStuntMan))
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
                else if (target.IsRole(RoleId.Fox))
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
            SuperNewRolesPlugin.Logger.LogInfo("g(Murder)" + __instance.Data.PlayerName + " => " + target.Data.PlayerName);
            __instance.RpcMurderPlayerCheck(target);
            SuperNewRolesPlugin.Logger.LogInfo("h(Murder)" + __instance.Data.PlayerName + " => " + target.Data.PlayerName);
            return false;
        }
        public static void RpcCheckExile(this PlayerControl __instance)
        {
            if (__instance.IsRole(RoleId.Assassin) && __instance.IsAlive())
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
                    __instance.RpcSetName($"<size=200%>{CustomOptions.Cs(RoleClass.Marine.color, IntroDate.MarineIntro.NameKey + "Name")}は誰だ？</size>");
                }, 2f);
                new LateTask(() =>
                {
                    __instance.RpcSendChat($"\n{ModTranslation.GetString("MarineWhois")}");
                }, 2.5f);
                new LateTask(() =>
                {
                    __instance.RpcSetName(__instance.GetDefaultName());
                }, 2f);
                RoleClass.Assassin.TriggerPlayer = __instance;
                return;
            }
            __instance.RpcInnerExiled();
        }
        public static void RpcMurderPlayerCheck(this PlayerControl __instance, PlayerControl target)
        {
            if (target.IsRole(RoleId.Assassin) && target.IsAlive())
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
                    target.RpcSetName($"<size=200%>{CustomOptions.Cs(RoleClass.Marine.color, IntroDate.MarineIntro.NameKey + "Name")}は誰だ？</size>");
                }, 2f);
                new LateTask(() =>
                {
                    target.RpcSendChat($"\n{ModTranslation.GetString("MarineWhois")}");
                }, 2.5f);
                new LateTask(() =>
                {
                    target.RpcSetName(target.GetDefaultName());
                }, 2f);
                return;
            }
            SuperNewRolesPlugin.Logger.LogInfo("i(Murder)" + __instance.Data.PlayerName + " => " + target.Data.PlayerName);
            __instance.RpcMurderPlayer(target);
            SuperNewRolesPlugin.Logger.LogInfo("j(Murder)" + __instance.Data.PlayerName + " => " + target.Data.PlayerName);
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
            if (ModeHandler.IsMode(ModeId.Default))
            {
                target.resetChange();
                if (target.PlayerId == CachedPlayer.LocalPlayer.PlayerId)
                {
                    if (PlayerControl.LocalPlayer.IsRole(RoleId.SideKiller))
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
                else if (__instance.PlayerId == CachedPlayer.LocalPlayer.PlayerId)
                {
                    if (__instance.IsRole(RoleId.EvilGambler))
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
                    switch (target.GetRole())
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
            // SuperNewRolesPlugin.Logger.LogInfo("MurderPlayer発生！元:" + __instance.GetDefaultName() + "、ターゲット:" + target.GetDefaultName());
            // Collect dead player info
            Logger.Info("追加");
            DeadPlayer deadPlayer = new(target, target.PlayerId, DateTime.UtcNow, DeathReason.Kill, __instance);
            DeadPlayer.deadPlayers.Add(deadPlayer);
            FinalStatusPatch.FinalStatusData.FinalStatuses[target.PlayerId] = FinalStatus.Kill;

            SerialKiller.MurderPlayer(__instance, target);
            Seer.ExileControllerWrapUpPatch.MurderPlayerPatch.Postfix(target);

            if (ModeHandler.IsMode(ModeId.SuperHostRoles))
            {
                if (AmongUsClient.Instance.AmHost)
                {
                    MurderPlayer.Postfix(__instance, target);
                }
            }
            else if (ModeHandler.IsMode(ModeId.Detective))
            {
                Mode.Detective.Main.MurderPatch(target);
            }
            else if (ModeHandler.IsMode(ModeId.Default))
            {
                if (__instance.PlayerId == CachedPlayer.LocalPlayer.PlayerId && PlayerControl.LocalPlayer.IsRole(RoleId.Finder))
                {
                    RoleClass.Finder.KillCount++;
                }
                if (PlayerControl.LocalPlayer.IsRole(RoleId.Painter) && RoleClass.Painter.CurrentTarget != null && RoleClass.Painter.CurrentTarget.PlayerId == target.PlayerId) Roles.CrewMate.Painter.Handle(Roles.CrewMate.Painter.ActionType.Death);
                if (target.IsRole(RoleId.Assassin))
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
                if (PlayerControl.LocalPlayer.IsRole(RoleId.Psychometrist))
                {
                    Roles.CrewMate.Psychometrist.MurderPlayer(__instance, target);
                }
                if (target.IsDead())
                {
                    if (target.IsRole(RoleId.Hitman))
                    {
                        Roles.Neutral.Hitman.Death();
                    }
                }
                Levelinger.MurderPlayer(__instance, target);
                if (RoleClass.Lovers.SameDie && target.IsLovers())
                {
                    if (__instance.PlayerId == CachedPlayer.LocalPlayer.PlayerId)
                    {
                        PlayerControl SideLoverPlayer = target.GetOneSideLovers();
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
                if (target.IsQuarreled())
                {
                    if (AmongUsClient.Instance.AmHost)
                    {
                        var Side = RoleHelpers.GetOneSideQuarreled(target);
                        if (Side.IsDead())
                        {
                            RPCProcedure.ShareWinner(target.PlayerId);
                            MessageWriter Writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.ShareWinner, SendOption.Reliable, -1);
                            Writer.Write(target.PlayerId);
                            AmongUsClient.Instance.FinishRpcImmediately(Writer);
                            RoleClass.Quarreled.IsQuarreledWin = true;
                            ShipStatus.RpcEndGame((GameOverReason)CustomGameOverReason.QuarreledWin, false);
                        }
                    }
                }
                Minimalist.MurderPatch.Postfix(__instance);
            }
            if (__instance.PlayerId == CachedPlayer.LocalPlayer.PlayerId)
            {
                if (__instance.IsImpostor() && !__instance.IsRole(RoleId.EvilGambler))
                {
                    PlayerControl.LocalPlayer.SetKillTimerUnchecked(RoleHelpers.GetCoolTime(__instance), RoleHelpers.GetCoolTime(__instance));
                }
            }
        }
    }
    [HarmonyPatch(typeof(PlayerControl),nameof(PlayerControl.CompleteTask))]
    class CompleteTask
    {
        public static void Postfix(PlayerControl __instance, uint idx)
        {
            if (PlayerControl.LocalPlayer.IsRole(RoleId.Painter) && RoleClass.Painter.CurrentTarget != null && RoleClass.Painter.CurrentTarget.PlayerId == __instance.PlayerId) Roles.CrewMate.Painter.Handle(Roles.CrewMate.Painter.ActionType.TaskComplete);
        }
    }
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Exiled))]
    public static class ExilePlayerPatch
    {
        public static void Postfix(PlayerControl __instance)
        {
            // Collect dead player info
            DeadPlayer deadPlayer = new(__instance, __instance.PlayerId, DateTime.UtcNow, DeathReason.Exile, null);
            DeadPlayer.deadPlayers.Add(deadPlayer);
            FinalStatusPatch.FinalStatusData.FinalStatuses[__instance.PlayerId] = FinalStatus.Exiled;
            if (ModeHandler.IsMode(ModeId.Default))
            {
                if (__instance.IsRole(RoleId.Assassin) && !RoleClass.Assassin.MeetingEndPlayers.Contains(__instance.PlayerId))
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
                if (__instance.IsRole(RoleId.Hitman)) {
                    Roles.Neutral.Hitman.Death();
                }
                if (RoleClass.Lovers.SameDie && __instance.IsLovers())
                {
                    if (__instance.PlayerId == CachedPlayer.LocalPlayer.PlayerId)
                    {
                        PlayerControl SideLoverPlayer = __instance.GetOneSideLovers();
                        if (SideLoverPlayer.IsAlive())
                        {
                            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.ExiledRPC, SendOption.Reliable, -1);
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
        public static PlayerControl SetTarget(bool onlyCrewmates = false, bool targetPlayersInVents = false, List<PlayerControl> untargetablePlayers = null, PlayerControl targetingPlayer = null)
        {
            PlayerControl result = null;
            float num = GameOptionsData.KillDistances[Mathf.Clamp(PlayerControl.GameOptions.KillDistance, 0, 2)];
            if (!MapUtilities.CachedShipStatus) return result;
            if (targetingPlayer == null) targetingPlayer = PlayerControl.LocalPlayer;
            if (targetingPlayer.Data.IsDead || targetingPlayer.inVent) return result;

            if (untargetablePlayers == null)
            {
                untargetablePlayers = new();
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
            return result.IsDead() ? null : result;
        }
    }
}