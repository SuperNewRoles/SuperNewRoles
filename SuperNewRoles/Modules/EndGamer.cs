using System.Collections.Generic;
using System.Linq;
using SuperNewRoles.Patches;
using SuperNewRoles.Roles;
using SuperNewRoles.Roles.Neutral;
using UnityEngine;

namespace SuperNewRoles.Modules;

public static class EndGamer
{/*
    public static void EndGame(GameOverReason reason)
    {
        List<ExPlayerControl> winners = new();
        Color32 color = Color.white;
        string upperText = null;
        switch (reason)
        {
            case GameOverReason.ImpostorsByKill:
            case GameOverReason.ImpostorsByVote:
            case GameOverReason.ImpostorsBySabotage:
                winners = ExPlayerControl.ExPlayerControls.Where(x => x.IsImpostorWinTeam()).ToList();
                color = Palette.ImpostorRed;
                upperText = "ImpostorWin";
                break;
            case GameOverReason.CrewmatesByTask:
            case GameOverReason.CrewmatesByVote:
                winners = ExPlayerControl.ExPlayerControls.Where(x => x.IsCrewmate()).ToList();
                color = Palette.CrewmateBlue;
                upperText = "CrewmateWin";
                break;
        }
        EndGame(reason, winners, color, upperText);
    }*/
    public static void EndGame(GameOverReason reason, HashSet<ExPlayerControl> winners, Color32 color, string upperText, string winText = null)
    {
        UpdateHijackers(ref reason, ref winners, ref color, ref upperText, ref winText);
        UpdateAdditionalWinners(out HashSet<ExPlayerControl> additionalWinners);
        winners.UnionWith(additionalWinners);
        Logger.Info("----------- Finished EndGame Start -----------");
        Logger.Info("reason: " + reason);
        Logger.Info("winners: " + winners.Count);
        Logger.Info("color: " + color);
        Logger.Info("upperText: " + upperText);
        Logger.Info("winText: " + winText);
        Logger.Info("----------- Finished EndGame End -----------");
        EndGameManagerSetUpPatch.RpcEndGameWithCondition(reason, winners.Select(x => x.PlayerId).ToList(), upperText ?? reason.ToString(), additionalWinners.Select(x => x.Role.ToString()).ToList(), color, false, winText ?? "WinText");
    }
    [CustomRPC]
    public static void RpcEndGameWithWinner(CustomGameOverReason reason, ExPlayerControl[] winners, Color32 color, string upperText, string winText = null)
    {
        if (!AmongUsClient.Instance.AmHost) return;
        EndGame((GameOverReason)reason, winners.ToHashSet(), color, upperText, string.IsNullOrEmpty(winText) ? null : winText);
    }
    [CustomRPC]
    public static void RpcEndGameImpostorWin()
    {
        if (!AmongUsClient.Instance.AmHost) return;
        EndGame(GameOverReason.ImpostorsByKill, ExPlayerControl.ExPlayerControls.Where(x => x.IsImpostorWinTeam()).ToHashSet(), Palette.ImpostorRed, "ImpostorWin");
    }
    private static void UpdateHijackers(ref GameOverReason reason, ref HashSet<ExPlayerControl> winners, ref Color32 color, ref string upperText, ref string winText)
    {
        winners = new();
        foreach (ExPlayerControl player in ExPlayerControl.ExPlayerControls)
        {
            if (player.Role == RoleId.God && player.IsAlive())
            {
                if (God.GodNeededTask && !player.IsTaskComplete()) continue;
                reason = (GameOverReason)CustomGameOverReason.GodWin;
                winners = [player];
                color = God.Instance.RoleColor;
                upperText = "God";
                winText = "GodDescends";
            }
        }
        foreach (ExPlayerControl player in ExPlayerControl.ExPlayerControls)
        {
            if (player.Role == RoleId.Tuna && player.IsAlive())
            {
                reason = (GameOverReason)CustomGameOverReason.TunaWin;
                winners = [player];
                color = Tuna.Instance.RoleColor;
                upperText = "TunaWin";
            }
        }
    }
    private static void UpdateAdditionalWinners(out HashSet<ExPlayerControl> winners)
    {
        winners = new();
        foreach (ExPlayerControl player in ExPlayerControl.ExPlayerControls)
        {
            switch (player.Role)
            {
                case RoleId.Opportunist:
                    if (player.IsAlive())
                        winners.Add(player);
                    break;
            }
        }
    }
}
