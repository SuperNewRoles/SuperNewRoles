using System.Collections.Generic;
using System.Linq;
using SuperNewRoles.Patches;
using SuperNewRoles.Roles;
using SuperNewRoles.Roles.Modifiers;
using SuperNewRoles.Roles.Neutral;
using UnityEngine;
using SuperNewRoles.CustomOptions.Categories;
using SuperNewRoles.Roles.Ability;

namespace SuperNewRoles.Modules;

public enum WinType
{
    // クルーとかの普通のやつ
    Default,
    // 単独勝利
    SingleNeutral,
    // 乗っ取り勝利
    Hijackers,
    // ノー勝者
    NoWinner
}
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
    public static void EndGame(GameOverReason reason, WinType winType, HashSet<ExPlayerControl> winners, Color32 color, string upperText, string winText = null)
    {
        HashSet<ExPlayerControl> additionalWinners = new();

        // サボタージュ勝ちの時はインポスター以外死んだ判定で判定していく
        if (reason == GameOverReason.ImpostorsBySabotage)
        {
            foreach (ExPlayerControl player in ExPlayerControl.ExPlayerControls)
            {
                if (!player.IsImpostorWinTeam())
                    player.Data.IsDead = true;
            }
        }

        if (winType != WinType.NoWinner)
        {
            if (winType != WinType.SingleNeutral)
                UpdateHijackers(ref reason, ref winners, ref color, ref upperText, ref winText, ref winType);
            // 独自単独勝利とは同時勝利できない
            UpdateAdditionalWinners(reason, winners, out additionalWinners, winType == WinType.SingleNeutral);
            winners.UnionWith(additionalWinners);
        }
        Logger.Info("----------- Finished EndGame Start -----------");
        Logger.Info("reason: " + reason);
        Logger.Info("winners: " + winners.Count);
        Logger.Info("color: " + color);
        Logger.Info("upperText: " + upperText);
        Logger.Info("winText: " + winText);
        Logger.Info("----------- Finished EndGame End -----------");
        RpcSyncAlive(ExPlayerControl.ExPlayerControls.ToDictionary(x => x.PlayerId, x => x.IsDead()));
        EndGameManagerSetUpPatch.RpcEndGameWithCondition(reason, winners.Select(x => x.PlayerId).ToList(), upperText ?? reason.ToString(), additionalWinners.Select(x => x.Role.ToString()).ToHashSet().ToList(), color, false, winText ?? "WinText");
    }
    [CustomRPC]
    public static void RpcSyncAlive(Dictionary<byte, bool> dead)
    {
        foreach (ExPlayerControl player in ExPlayerControl.ExPlayerControls)
        {
            if (dead.TryGetValue(player.PlayerId, out bool isDead))
                player.Data.IsDead = isDead;
        }
    }
    [CustomRPC]
    public static void RpcEndGameWithWinner(CustomGameOverReason reason, WinType winType, ExPlayerControl[] winners, Color32 color, string upperText, string winText = "")
    {
        ShipStatus.Instance.enabled = false;
        if (!AmongUsClient.Instance.AmHost) return;
        EndGame((GameOverReason)reason, winType, winners.ToHashSet(), color, upperText, string.IsNullOrEmpty(winText) || winText == "" ? null : winText);
    }
    [CustomRPC]
    public static void RpcEndGameImpostorWin()
    {
        if (!AmongUsClient.Instance.AmHost) return;
        EndGame(GameOverReason.ImpostorsByKill, WinType.Default, ExPlayerControl.ExPlayerControls.Where(x => x.IsImpostorWinTeam()).ToHashSet(), Palette.ImpostorRed, "ImpostorWin");
    }
    private static void UpdateHijackers(ref GameOverReason reason, ref HashSet<ExPlayerControl> winners, ref Color32 color, ref string upperText, ref string winText, ref WinType winType)
    {
        if (GameSettingOptions.DisableHijackTaskWin && reason == GameOverReason.CrewmatesByTask) return;

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
                winType = WinType.Hijackers;
            }
        }
        if (Tuna.EnableTunaSoloWin)
        {
            foreach (ExPlayerControl player in ExPlayerControl.ExPlayerControls)
            {
                if (player.Role == RoleId.Tuna && player.IsAlive())
                {
                    reason = (GameOverReason)CustomGameOverReason.TunaWin;
                    winners = [player];
                    color = Tuna.Instance.RoleColor;
                    upperText = "Tuna";
                    winType = WinType.Hijackers;
                }
            }
        }
    }
    private static void UpdateAdditionalWinners(GameOverReason reason, HashSet<ExPlayerControl> nowWinners, out HashSet<ExPlayerControl> winners, bool cantWinSixAdditionalWinners)
    {
        winners = new();
        // ラバーズじゃない人がいる場合
        if (Lovers.LoversOriginalTeamCannotWin && winners.Any(x => !x.IsLovers()))
        {
            foreach (ExPlayerControl winner in winners)
            {
                if (winner.IsLovers())
                {
                    winners.Remove(winner);
                }
            }
        }
        if (!cantWinSixAdditionalWinners)
        {
            foreach (ExPlayerControl player in ExPlayerControl.ExPlayerControls)
            {
                switch (player.Role)
                {
                    case RoleId.Opportunist:
                        if (player.IsAlive())
                            winners.Add(player);
                        break;
                    case RoleId.Tuna when !Tuna.EnableTunaSoloWin:
                        if (player.IsAlive())
                            winners.Add(player);
                        break;
                }
            }
        }
        foreach (ExPlayerControl winner in winners)
        {
            if (Lovers.LoversAdditionalWinCondition && winner.IsLovers())
            {
                foreach (LoversAbility lovers in winner.GetAbility<LoversAbility>()?.couple?.lovers)
                {
                    if (lovers.Player.IsDead()) continue;
                    winners.Add(lovers.Player);
                }
                List<ExPlayerControl> creatorCupid = getCreatorCupid(winner);
                foreach (ExPlayerControl cupid in creatorCupid)
                {
                    winners.Add(cupid);
                }
            }
        }
        if (reason == (GameOverReason)CustomGameOverReason.LoversWin)
        {
            List<ExPlayerControl> creatorCupid = getCreatorCupid(nowWinners.First());
            foreach (ExPlayerControl cupid in creatorCupid)
            {
                winners.Add(cupid);
            }
        }
    }

    // Helper
    private static List<ExPlayerControl> getCreatorCupid(ExPlayerControl winner)
    {
        return ExPlayerControl.ExPlayerControls.Where(x =>
                x.Role == RoleId.Cupid &&
                x.TryGetAbility<CupidAbility>(out var cupidAbility) &&
                (cupidAbility.Lovers1 == winner.PlayerId || cupidAbility.Lovers2 == winner.PlayerId)).ToList();
    }
}
