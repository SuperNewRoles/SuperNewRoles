
using HarmonyLib;
using Hazel;
using SuperNewRoles.EndGame;
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
            IsImpostorWin = false;
            IsNeutralWin = false;
            IsCrewmateWin = false;
            if (CheckAndEndGameForSabotageWin(__instance)) return false;
            if (CheckAndEndGameForImpostorWin(__instance, statistics)) return false;
            if (CheckAndEndGameForCrewmateWin(__instance, statistics)) return false;
            if (CheckAndEndGameForWorkpersonWin(__instance)) return false;
            if (CheckAndEndGameForTaskWin(__instance)) return false;
            return false;
        }
        private static bool IsImpostorWin = false;
        private static bool IsNeutralWin = false;
        private static bool IsCrewmateWin = false;
        public static void WinNeutral(List<PlayerControl> players)
        {
            /**
            foreach (PlayerControl p in PlayerControl.AllPlayerControls)
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
                        if (WinGods == null)
                        {
                            WinGods = new List<PlayerControl>();
                        }
                        WinGods.Add(p);
                        Chat.WinCond = CustomGameOverReason.GodWin;
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
            /*
                foreach (PlayerControl p in RoleClass.Opportunist.OpportunistPlayer)
                {
                    if (p.isAlive())
                    {
                        if (IsCrewmateWin)
                        {
                            p.RpcSetRoleDesync(RoleTypes.Crewmate);
                        }
                        else
                        {
                            p.RpcSetRoleDesync(RoleTypes.Impostor);
                        }
                    }
                    else
                    {
                        if (IsCrewmateWin)
                        {
                            p.RpcSetRoleDesync(RoleTypes.Impostor);
                        }
                        else
                        {
                            p.RpcSetRoleDesync(RoleTypes.Crewmate);
                        }
                    }
                }
            */
                    __instance.enabled = false;
                    ShipStatus.RpcEndGame(reason, showAd);

            //変更した設定を直す
            /*
            PlayerControl.GameOptions = ChangeGameOptions.DefaultGameOption;
            PlayerControl.LocalPlayer.RpcSyncSettings(PlayerControl.GameOptions);
            */
            //終わり
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
                IsCrewmateWin = true;
                Chat.WinCond = CustomGameOverReason.CrewmateWin;
                CustomEndGame(__instance,GameOverReason.HumansByTask, false);
                return true;
            }
            return false;
        }

        public static bool CheckAndEndGameForImpostorWin(ShipStatus __instance, PlayerStatistics statistics)
        {
            if (statistics.TeamImpostorsAlive >= statistics.TotalAlive - statistics.TeamImpostorsAlive && statistics.TeamImpostorsAlive != 0)//&& Chat.WinCond == null)
            {
                IsImpostorWin = true;
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
                Chat.WinCond = CustomGameOverReason.ImpostorWin;
                CustomEndGame(__instance, endReason, false);
                return true;
            }
            return false;
        }

        public static bool CheckAndEndGameForCrewmateWin(ShipStatus __instance, PlayerStatistics statistics)
        {
            if (statistics.CrewAlive > 0 && statistics.TeamImpostorsAlive == 0)// && Chat.WinCond == null)
            {
                IsCrewmateWin = true;
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
                    var (playerCompleted, playerTotal) = TaskCount.TaskDate(p.Data);
                    if (playerCompleted >= playerTotal)
                    {
                        MessageWriter Writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.ShareWinner, Hazel.SendOption.Reliable, -1);
                        Writer.Write(p.PlayerId);
                        AmongUsClient.Instance.FinishRpcImmediately(Writer);
                        CustomRPC.RPCProcedure.ShareWinner(p.PlayerId);
                        Chat.WinCond = CustomGameOverReason.WorkpersonWin;
                        __instance.enabled = false;
                        CustomEndGame(__instance,(GameOverReason)CustomGameOverReason.CrewmateWin, false);
                        return true;
                    }
                }
            }
            return false;
        }
        public static void EndGameForSabotage(ShipStatus __instance)
        {
            if (true)//Chat.WinCond == null)
            {
                IsImpostorWin = true;
                Chat.WinCond = CustomGameOverReason.ImpostorWin;
                CustomEndGame(__instance, GameOverReason.ImpostorBySabotage, false);
                return;
            }
        }
    }
}
