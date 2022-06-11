
using HarmonyLib;
using Hazel;
using SuperNewRoles.CustomRPC;
using SuperNewRoles.EndGame;
using SuperNewRoles.Helpers;
using SuperNewRoles.Patch;
using SuperNewRoles.Roles;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static SuperNewRoles.EndGame.CheckGameEndPatch;

namespace SuperNewRoles.Mode.SuperHostRoles
{
    class EndGameCheck
    {
        public static bool CheckEndGame(ShipStatus __instance, PlayerStatistics statistics)
        {
            if (CheckAndEndGameForDefaultWin(__instance,statistics)) return false;
            if (CheckAndEndGameForJackalWin(__instance, statistics)) return false;
            if (CheckAndEndGameForSabotageWin(__instance)) return false;
            if (!PlusModeHandler.isMode(PlusModeId.NotTaskWin) && CheckAndEndGameForTaskWin(__instance)) return false;
            if (CheckAndEndGameForWorkpersonWin(__instance)) return false;
            return false;
        }
        public static void WinNeutral(List<PlayerControl> players)
        {
            /**
            foreach (PlayerControl p in CachedPlayer.AllPlayers)
            {
                if (players.IsCheckListPlayerControl(p))
                {
                    p.UnCheckedRpcSetRole(RoleTypes.Impostor);
                } else
                {
                    p.UnCheckedRpcSetRole(RoleTypes.Crewmate);
                }
            }
            **/
        }
        public static void CustomEndGame(ShipStatus __instance,GameOverReason reason, bool showAd)
        {
                Chat.IsOldSHR = true;
                List<PlayerControl>? WinGods = null;
                foreach (PlayerControl p in RoleClass.God.GodPlayer)
                {
                    if (p.isAlive())
                {
                    var (complate, all) = TaskCount.TaskDateNoClearCheck(p.Data);
                    if (!RoleClass.God.IsTaskEndWin || complate >= all)
                    {
                        if (WinGods == null)
                        {
                            WinGods = new List<PlayerControl>();
                        }
                        WinGods.Add(p);
                        Chat.WinCond = CustomGameOverReason.GodWin;
                    }
                    }
                }
            if (Chat.WinCond == CustomGameOverReason.GodWin)
            {
                WinNeutral(WinGods);
                Chat.Winner = WinGods;
            }
            foreach (PlayerControl p in RoleClass.Sheriff.SheriffPlayer) {
                p.RpcSetRole(RoleTypes.GuardianAngel);
            }
            foreach (PlayerControl p in RoleClass.RemoteSheriff.RemoteSheriffPlayer)
            {
                p.RpcSetRole(RoleTypes.GuardianAngel);
            }
            foreach (PlayerControl p in RoleClass.Arsonist.ArsonistPlayer)
            {
                p.RpcSetRole(RoleTypes.GuardianAngel);
            }
            if (OnGameEndPatch.EndData == null && (reason == GameOverReason.ImpostorByKill || reason == GameOverReason.ImpostorBySabotage || reason == GameOverReason.ImpostorByVote || reason == GameOverReason.ImpostorDisconnect))
            {
                foreach (PlayerControl p in RoleClass.Survivor.SurvivorPlayer)
                {
                    if (p.isDead())
                    {
                        p.RpcSetRole(RoleTypes.GuardianAngel);
                    }
                }
            }
            else if (OnGameEndPatch.EndData == CustomGameOverReason.JackalWin)
            {
                foreach (PlayerControl p in CachedPlayer.AllPlayers)
                {
                    if (!p.isRole(RoleId.Jackal)) {
                        p.RpcSetRole(RoleTypes.GuardianAngel);
                    }
                }
            }
            FixedUpdate.SetRoleNames(true);
            __instance.enabled = false;
            ShipStatus.RpcEndGame(reason, showAd);
        }
        public static bool CheckAndEndGameForSabotageWin(ShipStatus __instance)
        {
            if (__instance.Systems == null) return false;
            ISystemType systemType = __instance.Systems.ContainsKey(SystemTypes.LifeSupp) ? __instance.Systems[SystemTypes.LifeSupp] : null;
            if (systemType != null)
            {
                LifeSuppSystemType lifeSuppSystemType = systemType.TryCast<LifeSuppSystemType>();
                if (lifeSuppSystemType != null && lifeSuppSystemType.Countdown < 0f)
                {
                    EndGameForSabotage(__instance);
                    lifeSuppSystemType.Countdown = 10000f;
                    return true;
                }
            }
            ISystemType systemType2 = __instance.Systems.ContainsKey(SystemTypes.Reactor) ? __instance.Systems[SystemTypes.Reactor] : null;
            if (systemType2 == null)
            {
                systemType2 = __instance.Systems.ContainsKey(SystemTypes.Laboratory) ? __instance.Systems[SystemTypes.Laboratory] : null;
            }
            if (systemType2 != null)
            {
                ICriticalSabotage criticalSystem = systemType2.TryCast<ICriticalSabotage>();
                if (criticalSystem != null && criticalSystem.Countdown < 0f)
                {
                    EndGameForSabotage(__instance);
                    criticalSystem.ClearSabotage();
                    return true;
                }
            }
            return false;
        }

        public static bool CheckAndEndGameForTaskWin(ShipStatus __instance)
        {
            if (GameData.Instance.TotalTasks <= GameData.Instance.CompletedTasks )//&& Chat.WinCond == null)
            {
                Chat.WinCond = CustomGameOverReason.CrewmateWin;
                CustomEndGame(__instance,GameOverReason.HumansByTask, false);
                return true;
            }
            return false;
        }
        public static bool CheckAndEndGameForJackalWin(ShipStatus __instance, PlayerStatistics statistics)
        {
            if (statistics.TeamJackalAlive >= statistics.TotalAlive - statistics.TeamJackalAlive && statistics.TeamImpostorsAlive == 0)
            {
                MessageWriter Writer = RPCHelper.StartRPC(CustomRPC.CustomRPC.SetWinCond);
                Writer.Write((byte)CustomGameOverReason.JackalWin);
                Writer.EndRPC();
                CustomRPC.RPCProcedure.SetWinCond((byte)CustomGameOverReason.JackalWin);
                __instance.enabled = false;
                CustomEndGame(__instance,GameOverReason.ImpostorByKill, false);
                return true;
            }
            return false;
        }

        public static bool CheckAndEndGameForDefaultWin(ShipStatus __instance, PlayerStatistics statistics)
        {
            int num1 = 0;
            int num2 = 0;
            int num3 = 0;
            for (int index = 0; index < GameData.Instance.PlayerCount; ++index)
            {
                GameData.PlayerInfo allPlayer = GameData.Instance.AllPlayers[index];
                if (!allPlayer.Disconnected && allPlayer.Object.IsPlayer())
                {
                    //インポスター判定ならnum3にカウント
                    if (allPlayer.Object.isImpostor() || allPlayer.Object.isRole(RoleId.Egoist))
                        ++num3;
                    //生存しているかつ
                    if (!allPlayer.IsDead)
                    {
                        //インポスターならnum2に追加
                        if (allPlayer.Object.isImpostor() || allPlayer.Object.isRole(RoleId.Egoist))
                            ++num2;
                        //違うならnum1に追加
                        else
                            ++num1;
                    }
                }
            }
            if (num2 <= 0 && statistics.TeamJackalAlive <= 0 && (!DestroyableSingleton<TutorialManager>.InstanceExists || num3 > 0))
            {
                __instance.BeginCalled = false;
                CustomEndGame(__instance,GameOverReason.HumansByVote, !SaveManager.BoughtNoAds);
            }
            else if (num1 <= num2 && statistics.TeamJackalAlive < 1)
            {
                if (!DestroyableSingleton<TutorialManager>.InstanceExists)
                {
                    __instance.BeginCalled = false;
                    GameOverReason endReason;
                    switch (TempData.LastDeathReason)
                    {
                        case DeathReason.Exile:
                            endReason = GameOverReason.ImpostorByVote;
                            break;
                        case DeathReason.Kill:
                            endReason = GameOverReason.ImpostorByKill;
                            break;
                        default:
                            endReason = GameOverReason.ImpostorByVote;
                            break;
                    }
                    int impostorplayer = 0;
                    int egoistplayer = 0;
                    foreach (PlayerControl p in CachedPlayer.AllPlayers)
                    {
                        if (p.isAlive())
                        {
                            if (p.isImpostor()) impostorplayer++;
                            else if (p.isRole(RoleId.Egoist)) egoistplayer++;
                        }
                    }
                    if (impostorplayer <= 0 && egoistplayer >= 1)
                    {
                        MessageWriter Writer = RPCHelper.StartRPC(CustomRPC.CustomRPC.SetWinCond);
                        Writer.Write((byte)CustomGameOverReason.EgoistWin);
                        Writer.EndRPC();
                        RPCProcedure.SetWinCond((byte)CustomGameOverReason.EgoistWin);
                    }
                    if (Demon.IsDemonWinFlag())
                    {
                        MessageWriter Writer = RPCHelper.StartRPC(CustomRPC.CustomRPC.SetWinCond);
                        Writer.Write((byte)CustomGameOverReason.DemonWin);
                        Writer.EndRPC();
                        RPCProcedure.SetWinCond((byte)CustomGameOverReason.DemonWin);
                    }
                    CustomEndGame(__instance,endReason, !SaveManager.BoughtNoAds);
                    return true;
                }
            }
            return false;
        }
        public static bool CheckAndEndGameForArsonistWin(ShipStatus __instance, PlayerStatistics statistics)
        {
            if (Arsonist.IsArsonistWinFlag())
            {
                Chat.WinCond = CustomGameOverReason.ArsonistWin;
                RPCProcedure.SetWinCond((byte)CustomGameOverReason.ArsonistWin);
                return true;
            }
            return false;
        }


        public static bool CheckAndEndGameForCrewmateWin(ShipStatus __instance, PlayerStatistics statistics)
        {
            if (statistics.CrewAlive > 0 && statistics.TeamImpostorsAlive < 1 && statistics.TeamJackalAlive < 1)// && Chat.WinCond == null)
            {
                Chat.WinCond = CustomGameOverReason.CrewmateWin;
                CustomEndGame(__instance, GameOverReason.HumansByVote, false);
                return true;
            }
            return false;
        }
        public static bool CheckAndEndGameForWorkpersonWin(ShipStatus __instance)
        {
            foreach (PlayerControl p in RoleClass.Workperson.WorkpersonPlayer)
            {
                if (!p.Data.Disconnected)
                {
                    if (p.isAlive() || !RoleClass.Workperson.IsAliveWin)
                    {
                        var (playerCompleted, playerTotal) = TaskCount.TaskDate(p.Data);
                        if (playerCompleted >= playerTotal)
                        {
                            MessageWriter Writer = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.ShareWinner, Hazel.SendOption.Reliable, -1);
                            Writer.Write(p.PlayerId);
                            AmongUsClient.Instance.FinishRpcImmediately(Writer);
                            CustomRPC.RPCProcedure.ShareWinner(p.PlayerId);
                            Writer = RPCHelper.StartRPC(CustomRPC.CustomRPC.SetWinCond);
                            Writer.Write((byte)CustomGameOverReason.WorkpersonWin);
                            Writer.EndRPC();
                            CustomRPC.RPCProcedure.SetWinCond((byte)CustomGameOverReason.WorkpersonWin);
                            Chat.WinCond = CustomGameOverReason.WorkpersonWin;
                            __instance.enabled = false;
                            CustomEndGame(__instance, (GameOverReason)CustomGameOverReason.CrewmateWin, false);
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        public static void EndGameForSabotage(ShipStatus __instance)
        {
            if (true)//Chat.WinCond == null)
            {
                Chat.WinCond = CustomGameOverReason.ImpostorWin;
                CustomEndGame(__instance, GameOverReason.ImpostorBySabotage, false);
                return;
            }
        }
    }
}
