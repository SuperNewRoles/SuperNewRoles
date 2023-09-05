using System;
using System.Collections.Generic;
using AmongUs.GameOptions;
using Hazel;
using SuperNewRoles.Helpers;
using SuperNewRoles.Patches;
using SuperNewRoles.Roles;
using SuperNewRoles.Roles.Neutral;
using static SuperNewRoles.Patches.CheckGameEndPatch;

namespace SuperNewRoles.Mode.SuperHostRoles;

class EndGameCheck
{
    public static bool CheckEndGame(ShipStatus __instance, PlayerStatistics statistics)
    {
        if (CheckAndEndGameForCrewmateWin(__instance, statistics)) return false;
        if (!PlusModeHandler.IsMode(PlusModeId.NotTaskWin) && CheckAndEndGameForTaskWin(__instance)) return false;
        if (CheckAndEndGameForImpostorWin(__instance, statistics)) return false;
        if (CheckAndEndGameForJackalWin(__instance, statistics)) return false;
        if (CheckAndEndGameForSabotageWin(__instance)) return false;
        if (CheckAndEndGameForWorkpersonWin(__instance)) return false;
        if (CustomOptionHolder.FoxCanHouwaWin.GetBool() && CheckAndEndGameForFoxHouwaWin(__instance)) return false;
        return false;
    }
    public static bool CheckEndGameHnSs(ShipStatus __instance, PlayerStatistics statistics)
    {
        if (CheckAndEndGameForJackalWin(__instance, statistics)) return false;
        if (CheckAndEndGameForWorkpersonWin(__instance)) return false;
        return false;
    }

    public static void CustomEndGame(ShipStatus __instance, GameOverReason reason, bool showAd)
    {
        SuperNewRoles.Roles.Impostor.Camouflager.ResetCamouflageSHR();
        Chat.IsOldSHR = true;
        List<PlayerControl> WinGods = null;
        foreach (PlayerControl p in RoleClass.God.GodPlayer)
        {
            if (p.IsAlive())
            {
                var (Complete, all) = TaskCount.TaskDateNoClearCheck(p.Data);
                if (!RoleClass.God.IsTaskEndWin || Complete >= all)
                {
                    if (WinGods == null)
                    {
                        WinGods = new();
                    }
                    WinGods.Add(p);
                    Chat.WinCond = CustomGameOverReason.GodWin;
                }
            }
        }
        if (Chat.WinCond == CustomGameOverReason.GodWin)
        {
            Chat.Winner = WinGods;
        }

        /*============死亡時守護天使============*/
        List<PlayerControl> SetDeadGuardianAngel = new();
        SetDeadGuardianAngel.AddRange(RoleClass.Sheriff.SheriffPlayer);
        SetDeadGuardianAngel.AddRange(RoleClass.RemoteSheriff.RemoteSheriffPlayer);
        SetDeadGuardianAngel.AddRange(RoleClass.Arsonist.ArsonistPlayer);
        SetDeadGuardianAngel.AddRange(RoleClass.ToiletFan.ToiletFanPlayer);
        SetDeadGuardianAngel.AddRange(RoleClass.NiceButtoner.NiceButtonerPlayer);
        /*============死亡時守護天使============*/
        foreach (PlayerControl p in SetDeadGuardianAngel)
        {
            p.RpcSetRole(RoleTypes.GuardianAngel);
        }

        if (OnGameEndPatch.EndData == null && (reason == GameOverReason.ImpostorByKill || reason == GameOverReason.ImpostorBySabotage || reason == GameOverReason.ImpostorByVote || reason == GameOverReason.ImpostorDisconnect))
        {
            foreach (PlayerControl p in RoleClass.Survivor.SurvivorPlayer)
            {
                if (p.IsDead())
                {
                    p.RpcSetRole(RoleTypes.GuardianAngel);
                }
            }
        }
        else if (OnGameEndPatch.EndData == CustomGameOverReason.JackalWin)
        {
            foreach (PlayerControl p in CachedPlayer.AllPlayers)
            {
                if (!p.IsRole(RoleId.Jackal))
                {
                    p.RpcSetRole(RoleTypes.GuardianAngel);
                }
            }
        }
        FixedUpdate.SetRoleNames(true);
        __instance.enabled = false;
        GameManager.Instance.RpcEndGame(reason, showAd);
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
        if (GameData.Instance.TotalTasks <= GameData.Instance.CompletedTasks)//&& Chat.WinCond == null)
        {
            Chat.WinCond = CustomGameOverReason.CrewmateWin;
            CustomEndGame(__instance, GameOverReason.HumansByTask, false);
            return true;
        }
        return false;
    }
    public static bool CheckAndEndGameForJackalWin(ShipStatus __instance, PlayerStatistics statistics)
    {
        if (statistics.TeamJackalAlive >= statistics.TotalAlive - statistics.TeamJackalAlive && statistics.TeamImpostorsAlive == 0)
        {
            MessageWriter Writer = RPCHelper.StartRPC(CustomRPC.SetWinCond);
            Writer.Write((byte)CustomGameOverReason.JackalWin);
            Writer.EndRPC();
            RPCProcedure.SetWinCond((byte)CustomGameOverReason.JackalWin);
            __instance.enabled = false;
            CustomEndGame(__instance, GameOverReason.ImpostorByKill, false);
            return true;
        }
        return false;
    }

    public static bool CheckAndEndGameForCrewmateWin(ShipStatus __instance, PlayerStatistics statistics)
    {
        if (statistics.TeamImpostorsAlive == 0 && statistics.TeamJackalAlive == 0)
        {
            foreach (PlayerControl p in RoleClass.SideKiller.MadKillerPlayer)
            {
                if (!p.IsImpostor() && !p.Data.Disconnected)
                {
                    return false;
                }
            }
            __instance.enabled = false;
            CustomEndGame(__instance, GameOverReason.HumansByVote, false);
            return true;
        }
        return false;
    }

    public static bool CheckAndEndGameForImpostorWin(ShipStatus __instance, PlayerStatistics statistics)
    {
        if (statistics.TeamImpostorsAlive >= statistics.TotalAlive - statistics.TeamImpostorsAlive && statistics.TeamJackalAlive == 0 && !EvilEraser.IsGodWinGuard() && !EvilEraser.IsFoxWinGuard() && !EvilEraser.IsNeetWinGuard())
        {
            __instance.enabled = false;
            var endReason = TempData.LastDeathReason switch
            {
                DeathReason.Exile => GameOverReason.ImpostorByVote,
                DeathReason.Kill => GameOverReason.ImpostorByKill,
                _ => GameOverReason.ImpostorByVote,
            };
            if (Demon.IsDemonWinFlag())
            {
                MessageWriter Writer = RPCHelper.StartRPC(CustomRPC.SetWinCond);
                Writer.Write((byte)CustomGameOverReason.DemonWin);
                Writer.EndRPC();
                RPCProcedure.SetWinCond((byte)CustomGameOverReason.DemonWin);
            }

            CustomEndGame(__instance, endReason, false);
            return true;
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
    public static bool CheckAndEndGameForWorkpersonWin(ShipStatus __instance)
    {
        foreach (PlayerControl p in RoleClass.Workperson.WorkpersonPlayer)
        {
            if (!p.Data.Disconnected)
            {
                if (p.IsAlive() || !RoleClass.Workperson.IsAliveWin)
                {
                    var (playerCompleted, playerTotal) = TaskCount.TaskDate(p.Data);
                    if (playerCompleted >= playerTotal)
                    {
                        MessageWriter Writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ShareWinner, SendOption.Reliable, -1);
                        Writer.Write(p.PlayerId);
                        AmongUsClient.Instance.FinishRpcImmediately(Writer);
                        RPCProcedure.ShareWinner(p.PlayerId);
                        Writer = RPCHelper.StartRPC(CustomRPC.SetWinCond);
                        Writer.Write((byte)CustomGameOverReason.WorkpersonWin);
                        Writer.EndRPC();
                        RPCProcedure.SetWinCond((byte)CustomGameOverReason.WorkpersonWin);
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
    public static bool CheckAndEndGameForFoxHouwaWin(ShipStatus __instance)
    {
        int impostorNum = 0;
        int crewNum = 0;
        bool foxAlive = false;
        foreach (PlayerControl p in CachedPlayer.AllPlayers)
        {
            if (p.IsDead() || p.Data.Disconnected || p == null) continue;

            if (p.IsImpostor()) impostorNum++;
            else if (p.IsCrew()) crewNum++;
            else if (RoleClass.Fox.FoxPlayer.Contains(p) || FireFox.FireFoxPlayer.Contains(p)) foxAlive = true;
        }

        if (impostorNum == crewNum && foxAlive && CustomOptionHolder.FoxCanHouwaWin.GetBool())
        {
            List<PlayerControl> foxPlayers = new(RoleClass.Fox.FoxPlayer);
            foxPlayers.AddRange(FireFox.FireFoxPlayer);
            foreach (PlayerControl p in foxPlayers)
            {
                if (p.IsDead()) continue;
                MessageWriter Writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ShareWinner, SendOption.Reliable, -1);
                Writer.Write(p.PlayerId);
                AmongUsClient.Instance.FinishRpcImmediately(Writer);
                RPCProcedure.ShareWinner(p.PlayerId);

                Writer = RPCHelper.StartRPC(CustomRPC.SetWinCond);
                Writer.Write((byte)CustomGameOverReason.FoxWin);
                Writer.EndRPC();
                RPCProcedure.SetWinCond((byte)CustomGameOverReason.FoxWin);

                __instance.enabled = false;
                CustomEndGame(__instance, (GameOverReason)CustomGameOverReason.FoxWin, false);
            }
            return true;
        };
        return false;
    }
    public static void EndGameForSabotage(ShipStatus __instance)
    {
        Chat.WinCond = CustomGameOverReason.ImpostorWin;
        CustomEndGame(__instance, GameOverReason.ImpostorBySabotage, false);
    }
}