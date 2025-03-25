using System;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using Hazel;
using Il2CppInterop.Generator.Extensions;
using SuperNewRoles.Helpers;
using SuperNewRoles.Patches;
using SuperNewRoles.Roles;
using SuperNewRoles.Roles.Neutral;
using UnityEngine;
using static SuperNewRoles.Patches.CheckGameEndPatch;

namespace SuperNewRoles.Mode.SuperHostRoles;

class EndGameCheck
{
    public static bool CheckEndGame(ShipStatus __instance, PlayerStatistics statistics)
    {
        // 暗転対策の途中にゲーム終了処理が入ると終了されるかもしれないからパス。
        if (AntiBlackOut.GamePlayers != null) return false;
        if (CheckAndEndGameForCrewmateWin(__instance, statistics)) return false;
        if (!PlusModeHandler.IsMode(PlusModeId.NotTaskWin) && CheckAndEndGameForTaskWin(__instance)) return false;
        if (CheckAndEndGameForImpostorWin(__instance, statistics)) return false;
        if (CheckAndEndGameForJackalWin(__instance, statistics)) return false;
        if (CheckAndEndGameForPavlovsWin(__instance, statistics)) return false;
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

    public static void CustomEndGame(ShipStatus __instance, CustomGameOverReason reason, bool showAd)
    {
        if (Chat.IsOldSHR)
            return;
        if (reason == CustomGameOverReason.HAISON)
        {
            Chat.WinCond = CustomGameOverReason.HAISON;

            MessageWriter writer = RPCHelper.StartRPC(CustomRPC.SetWinCond);
            writer.Write((byte)reason);
            writer.EndRPC();
            RPCProcedure.SetWinCond((byte)reason);

            GameManager.Instance.RpcEndGame(GameOverReason.ImpostorDisconnect, showAd);
            return;
        }
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

        /*
        foreach (PlayerControl p in SetDeadGuardianAngel)
        {
            p.RpcSetRole(RoleTypes.GuardianAngel);
        }

        if (OnGameEndPatch.EndData == null && (reason == GameOverReason.ImpostorsByKill || reason == GameOverReason.ImpostorsBySabotage || reason == GameOverReason.ImpostorsByVote || reason == GameOverReason.ImpostorDisconnect))
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
        }*/

        var (winners, winCondition, WillRevivePlayers) = OnGameEndPatch.HandleEndGameProcess((GameOverReason)reason);
        var winnersByte = winners.Select((p) => p?.PlayerId ?? byte.MaxValue).ToArray();
        foreach (PlayerControl player in PlayerControl.AllPlayerControls)
        {
            bool IsDead = player.Data.IsDead;
            RoleTypes RealRole = player.Data.Role.Role;
            if (winnersByte.Contains(player.PlayerId))
                player.RpcSetRole(RoleTypes.ImpostorGhost);
            else
                player.RpcSetRole(RoleTypes.CrewmateGhost);
            player.Data.IsDead = IsDead;
            _ = new LateTask(() => player.RPCSetRoleUnchecked(RealRole), 0.1f);
            if (!IsDead)
                player.Revive();
        }
        Dictionary<PlayerControl, byte> ShowTargets = new();
        foreach (PlayerControl player in PlayerControl.AllPlayerControls)
        {
            if (player.IsMod())
                continue;
            if (winnersByte.Contains(player.PlayerId) && player.IsAlive())
                continue;
            PlayerControl SeeTarget = null;
            foreach (PlayerControl seeTarget in PlayerControl.AllPlayerControls)
            {
                if (player.PlayerId == seeTarget.PlayerId)
                    continue;
                if (!winnersByte.Contains(seeTarget.PlayerId))
                    continue;
                SeeTarget = seeTarget;
                break;
            }
            if (SeeTarget != null)
                ShowTargets.Add(player, SeeTarget.PlayerId);
            else
                Logger.Error($"No winner to show. {player.PlayerId}", "EndGameCheck.CustomEndGame");
        }

        MessageWriter Writer = RPCHelper.StartRPC(CustomRPC.SetWinCond);
        Writer.Write((byte)reason);
        Writer.EndRPC();
        RPCProcedure.SetWinCond((byte)reason);

        (string text, Color color, _) = EndGameManagerSetUpPatch.ProcessWinText((GameOverReason)reason, winCondition);
        EndGameDetail.SetEndGameDetail(ModHelpers.Cs(color, text), ShowTargets);
        _ = new LateTask(() => RPCHelper.RpcSyncAllNetworkedPlayer(), 0.2f);
        _ = new LateTask(() => ChangeName.UpdateRoleNames(ChangeNameType.EndGame), 0.25f);
        _ = new LateTask(() => GameManager.Instance.RpcEndGame(GameOverReason.ImpostorsByVote, showAd), 0.4f);
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
        ISystemType systemType2 = __instance.Systems.ContainsKey(SystemTypes.HeliSabotage) ? __instance.Systems[SystemTypes.HeliSabotage] : null;
        if (systemType2 == null)
        {
            systemType2 = __instance.Systems.ContainsKey(SystemTypes.Laboratory) ? __instance.Systems[SystemTypes.Laboratory] : null;
        }
        if (systemType2 == null)
        {
            systemType2 = __instance.Systems.ContainsKey(SystemTypes.Reactor) ? __instance.Systems[SystemTypes.Reactor] : null;
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
            CustomEndGame(__instance, (CustomGameOverReason)GameOverReason.CrewmatesByTask, false);
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
            CustomEndGame(__instance, CustomGameOverReason.JackalWin, false);
            return true;
        }
        return false;
    }

    public static bool CheckAndEndGameForCrewmateWin(ShipStatus __instance, PlayerStatistics statistics)
    {
        if (statistics.TeamImpostorsAlive == 0 && statistics.TeamJackalAlive == 0 && !statistics.IsGuardPavlovs)
        {
            foreach (PlayerControl p in RoleClass.SideKiller.MadKillerPlayer)
            {
                if (!p.IsImpostor() && !p.Data.Disconnected)
                {
                    return false;
                }
            }
            __instance.enabled = false;
            CustomEndGame(__instance, (CustomGameOverReason)GameOverReason.CrewmatesByVote, false);
            return true;
        }
        return false;
    }

    public static bool CheckAndEndGameForImpostorWin(ShipStatus __instance, PlayerStatistics statistics)
    {
        if (statistics.TeamImpostorsAlive >= statistics.TotalAlive - statistics.TeamImpostorsAlive && statistics.TeamJackalAlive == 0 && !EvilEraser.IsGodWinGuard() && !EvilEraser.IsFoxWinGuard() && !statistics.IsGuardPavlovs && !EvilEraser.IsNeetWinGuard())
        {
            __instance.enabled = false;
            var endReason = GameData.LastDeathReason switch
            {
                DeathReason.Exile => GameOverReason.ImpostorsByVote,
                DeathReason.Kill => GameOverReason.ImpostorsByKill,
                _ => GameOverReason.ImpostorsByVote,
            };
            if (Demon.IsDemonWinFlag())
            {
                MessageWriter Writer = RPCHelper.StartRPC(CustomRPC.SetWinCond);
                Writer.Write((byte)CustomGameOverReason.DemonWin);
                Writer.EndRPC();
                RPCProcedure.SetWinCond((byte)CustomGameOverReason.DemonWin);
            }

            CustomEndGame(__instance, (CustomGameOverReason)endReason, false);
            return true;
        }
        return false;
    }
    public static bool CheckAndEndGameForPavlovsWin(ShipStatus __instance, PlayerStatistics statistics)
    {
        if (statistics.PavlovsTeamAlive >= statistics.TotalAlive - statistics.PavlovsTeamAlive && statistics.TeamImpostorsAlive == 0 && statistics.TeamJackalAlive == 0)
        {
            __instance.enabled = false;
            CustomEndGame(__instance, CustomGameOverReason.PavlovsTeamWin, false);
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
                        CustomEndGame(__instance, CustomGameOverReason.WorkpersonWin, false);
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
                CustomEndGame(__instance, CustomGameOverReason.FoxWin, false);
            }
            return true;
        }
        ;
        return false;
    }
    public static void EndGameForSabotage(ShipStatus __instance)
    {
        Chat.WinCond = CustomGameOverReason.ImpostorWin;
        CustomEndGame(__instance, (CustomGameOverReason)GameOverReason.ImpostorsBySabotage, false);
    }
}
public static class EndGameDetail
{
    public static string EndGameTitle { get; private set; }
    public static Dictionary<PlayerControl, byte> ShowTargets { get; private set; }
    public static void SetEndGameDetail(string title, Dictionary<PlayerControl, byte> targets)
    {
        EndGameTitle = title;
        ShowTargets = targets;
    }
    public static void Reset()
    {
        EndGameTitle = "None Detail.";
        ShowTargets = new();
    }
}