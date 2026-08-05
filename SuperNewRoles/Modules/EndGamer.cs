using System.Collections.Generic;
using System.Linq;
using SuperNewRoles.Patches;
using SuperNewRoles.Roles;
using SuperNewRoles.Roles.Modifiers;
using SuperNewRoles.Roles.Neutral;
using SuperNewRoles.Roles.Impostor;
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
    // additionalWinTexts の1要素に「翻訳キー」と「表示色」を両方載せるためのエンコード区切り文字。
    // [CustomRPC] は List<string> しか安全に運べないため、
    // "翻訳キー\x1FRRGGBB" の形式に文字列エンコードして EndGameScene.cs 側でデコードする。
    public const char ColorEncodeSeparator = '\x1F';
    public static string EncodeWithColor(string key, Color32 color)
        => key + ColorEncodeSeparator + ColorUtility.ToHtmlStringRGB(color);

    public static void EndGame(GameOverReason reason, WinType winType, HashSet<ExPlayerControl> winners, Color32 color, string upperText, string winText = null)
    {
        if (CustomOptionManager.DebugMode && CustomOptionManager.DebugModeNoGameEnd && reason != (GameOverReason)CustomGameOverReason.Haison)
        {
            Logger.Info("EndGame called but skipped due to DebugModeNoGameEnd. reason: " + reason);
            return;
        }
        HashSet<string> addWinners = new();
        List<string> hijackAddWinners = new();

        // サボタージュ勝ちの時はインポスター以外死んだ判定で判定していく
        if (reason == GameOverReason.ImpostorsBySabotage)
        {
            foreach (ExPlayerControl player in ExPlayerControl.ExPlayerControls)
            {
                if (!player.IsImpostorWinTeam())
                {
                    // IsDead = true にする前に、まだ生存していたプレイヤーの死因をサボタージュとして記録する。
                    // ここで先に IsDead を立ててしまうと、後段の UpdatePlayerStatusForSabotage
                    // （EndGameScene.cs）が「!player.IsDead」を条件にしているため発動せず、
                    // 死因がサボタージュとして表示されなくなるバグがあった。
                    if (player.IsAlive())
                        player.FinalStatus = FinalStatus.Sabotage;
                    player.Data.IsDead = true;
                }
            }
        }

        if (winType != WinType.NoWinner)
        {
            if (winType != WinType.SingleNeutral && reason != (GameOverReason)CustomGameOverReason.LoversWin)
                UpdateHijackers(ref reason, ref winners, ref color, ref upperText, ref winText, ref winType, hijackAddWinners);
            // 独自単独勝利とは同時勝利できない
            UpdateAdditionalWinners(reason, ref winners, out addWinners, ref winText, winType == WinType.SingleNeutral);
        }
        // Hijackers勝利で複数役職が同時成立した場合の & 表示用リストをマージする
        HashSet<string> allAddWinners = new(addWinners);
        foreach (var text in hijackAddWinners)
            allAddWinners.Add(text);

        Logger.Info("----------- Finished EndGame Start -----------");
        Logger.Info("reason: " + reason);
        Logger.Info("winners: " + winners.Count);
        Logger.Info("color: " + color);
        Logger.Info("upperText: " + upperText);
        Logger.Info("winText: " + winText);
        Logger.Info("----------- Finished EndGame End -----------");
        RpcSyncAlive(ExPlayerControl.ExPlayerControls.ToDictionary(x => x.PlayerId, x => x.IsDead()));
        // FinalStatus（サボタージュ死亡等）はホストのローカル状態にしか反映されず、
        // 非ホストクライアントでは常に Alive のまま表示されてしまうバグがあった
        // （死因:サボタージュがホスト視点にしか表示されない）ため、同期する。
        RpcSyncFinalStatus(ExPlayerControl.ExPlayerControls
            .Where(x => x.FinalStatus != FinalStatus.Alive)
            .ToDictionary(x => x.PlayerId, x => (byte)x.FinalStatus));
        string resolvedWinText = winText;
        // 単独勝利の場合、三人称単数になるので「wins」にする
        if (winType == WinType.SingleNeutral
            && reason != (GameOverReason)CustomGameOverReason.LoversWin
            && (string.IsNullOrEmpty(resolvedWinText) || resolvedWinText == "WinText"))
        {
            resolvedWinText = "SingleNeutralWinText";
        }
        resolvedWinText ??= "WinText";
        EndGameManagerSetUpPatch.RpcEndGameWithCondition(reason, winners.Select(x => x.PlayerId).ToList(), upperText ?? reason.ToString(), allAddWinners.ToList(), color, false, resolvedWinText);
    }
    public static void RpcHaison()
    {
        EndGameManagerSetUpPatch.RpcEndGameWithCondition((GameOverReason)CustomGameOverReason.Haison, ExPlayerControl.ExPlayerControls.Select(x => x.PlayerId).ToList(), "廃 of the 村", new List<string>(), Color.white, true);
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
    public static void RpcSyncFinalStatus(Dictionary<byte, byte> finalStatus)
    {
        foreach (ExPlayerControl player in ExPlayerControl.ExPlayerControls)
        {
            if (finalStatus.TryGetValue(player.PlayerId, out byte status))
                player.FinalStatus = (FinalStatus)status;  // 受け取り側でキャスト
        }
    }
    [CustomRPC]
    public static void RpcEndGameWithWinner(CustomGameOverReason reason, WinType winType, ExPlayerControl[] winners, Color32 color, string upperText, string winText = "")
    {
        if (CustomOptionManager.DebugMode && CustomOptionManager.DebugModeNoGameEnd)
            return;
        ShipStatus.Instance.enabled = false;
        if (!AmongUsClient.Instance.AmHost) return;
        EndGame((GameOverReason)reason, winType, winners.ToHashSet(), color, upperText, string.IsNullOrEmpty(winText) || winText == "" ? null : winText);
    }
    [CustomRPC]
    public static void RpcEndGameImpostorWin()
    {
        if (!AmongUsClient.Instance.AmHost) return;
        EndGameImpostorWin();
    }
    public static void EndGameImpostorWin()
    {
        if (!AmongUsClient.Instance.AmHost) return;
        EndGame(GameOverReason.ImpostorsByKill, WinType.Default, ExPlayerControl.ExPlayerControls.Where(x => x.IsImpostorWinTeam()).ToHashSet(), Palette.ImpostorRed, "ImpostorWin");
    }
    private static void UpdateHijackers(ref GameOverReason reason, ref HashSet<ExPlayerControl> winners, ref Color32 color, ref string upperText, ref string winText, ref WinType winType, List<string> hijackAddWinners)
    {
        if (GameSettingOptions.DisableHijackTaskWin && reason == GameOverReason.CrewmatesByTask) return;
        if (Tasker.DisableHijackTaskerWin && reason == (GameOverReason)CustomGameOverReason.TaskerWin) return;

        // ========================= 優先度(最高) =========================
        // 三匹の仔豚勝利
        // 旧仕様:
        // - チーム全員が生存していれば勝利
        // - そうでなくても、生存キラー(インポスター/ジャッカル/その他キラー)が全滅していれば勝利
        // - 同時勝利は禁止
        foreach (var team in Roles.Neutral.TheThreeLittlePigs.Teams)
        {
            if (team == null || team.Count != 3) continue;
            var members = team.Select(id => ExPlayerControl.ById(id)).Where(p => p != null && Roles.Neutral.TheThreeLittlePigs.IsLittlePig(p)).ToList();
            if (members.Count != 3) continue;

            bool allAlive = members.All(p => p.IsAlive());
            if (!members.Any(p => p.IsAlive())) continue;

            bool allKillerDead = ExPlayerControl.ExPlayerControls
                .Where(p => p != null && p.IsAlive())
                .All(p => !p.IsNonCrewKiller() && !p.IsJackalTeam());

            if (allAlive || allKillerDead)
            {
                reason = (GameOverReason)CustomGameOverReason.TheThreeLittlePigsWin;
                winners = members.ToHashSet();
                color = TheThreeLittlePigs.Instance.RoleColor;
                upperText = "TheThreeLittlePigs";
                winText = null;
                winType = WinType.Hijackers;
                return;
            }
        }

        // ======================= 優先度(高) ===========================
        // 条件付き生存横取り勝利 — モイラ / フランケンシュタイン
        bool hasConditionalWon = false;
        void AddConditionalWinner(ref HashSet<ExPlayerControl> winners, ref List<string> hijackAddWinners, ref string upperText, ref Color32 color, ref bool hasConditionalWon, ref GameOverReason reason, ref string winText, ref WinType winType, ExPlayerControl player, string key, CustomGameOverReason customReason, Color32 roleColor)
        {
            if (!hasConditionalWon)
            {
                winners = new HashSet<ExPlayerControl> { player };
                hijackAddWinners.Clear();
                upperText = key;
                color = roleColor;
                hasConditionalWon = true;
            }
            else
            {
                winners.Add(player);
                if (upperText != key && !hijackAddWinners.Any(x => x.StartsWith(key + ColorEncodeSeparator)))
                    hijackAddWinners.Add(EncodeWithColor(key, roleColor));
            }
            reason = (GameOverReason)customReason;
            winText = null;
            winType = WinType.SingleNeutral;
            winType = WinType.SingleNeutral;
        }

        foreach (ExPlayerControl player in ExPlayerControl.ExPlayerControls)
        {
            if (player.Role != RoleId.Moira || player.IsDead()) continue;
            if (!player.TryGetAbility<MoiraMeetingAbility>(out var moiraAbility) || moiraAbility.HasCount) continue;
            AddConditionalWinner(ref winners, ref hijackAddWinners, ref upperText, ref color, ref hasConditionalWon, ref reason, ref winText, ref winType, player, "Moira", CustomGameOverReason.MoiraWin, Moira.Instance.RoleColor);
        }
        foreach (ExPlayerControl player in ExPlayerControl.ExPlayerControls)
        {
            if (player.Role != RoleId.Frankenstein || player.IsDead()) continue;
            if (!player.TryGetAbility<FrankensteinAbility>(out var frankensteinAbility) || frankensteinAbility.RemainingKillsToWin > 0) continue;
            AddConditionalWinner(ref winners, ref hijackAddWinners, ref upperText, ref color, ref hasConditionalWon, ref reason, ref winText, ref winType, player, "Frankenstein", CustomGameOverReason.FrankensteinWin, Frankenstein.Instance.RoleColor);
        }
        if (hasConditionalWon) return;

        // ========================= 優先度(中) =========================
        // 単純生存横取り勝利 - スペランカー / マグロ / 陰陽師
        bool hasHijackWon = false;
        void AddHijackWinner(ref HashSet<ExPlayerControl> winners, ref List<string> hijackAddWinners, ref string upperText, ref Color32 color, ref bool hasHijackWon, ref GameOverReason reason, ref string winText, ref WinType winType, ExPlayerControl player, string key, CustomGameOverReason customReason, Color32 roleColor)
        {
            if (!hasHijackWon)
            {
                winners.Clear();
                upperText = key;
                color = roleColor;
                hasHijackWon = true;
            }
            else if (upperText != key && !hijackAddWinners.Any(x => x.StartsWith(key + ColorEncodeSeparator)))
            {
                // 同じ役職(key)が複数人いる場合、"&" 表示に同じ役職名を何度も追加しない。
                // winners への追加自体は毎回行うため、勝利プレイヤーとしては正しく全員入る。
                hijackAddWinners.Add(EncodeWithColor(key, roleColor));
            }
            winners.Add(player);
            reason = (GameOverReason)customReason;
            winText = null;
            winType = WinType.Hijackers;
        }

        // スペランカー
        if (!Spelunker.SpelunkerIsAdditionalWin)
        {
            foreach (ExPlayerControl player in ExPlayerControl.ExPlayerControls)
            {
                if (player.Role == RoleId.Spelunker && player.IsAlive())
                    AddHijackWinner(ref winners, ref hijackAddWinners, ref upperText, ref color, ref hasHijackWon, ref reason, ref winText, ref winType, player, "Spelunker", CustomGameOverReason.SpelunkerWin, Spelunker.Instance.RoleColor);
            }
        }

        // マグロ
        if (Tuna.EnableTunaSoloWin)
        {
            foreach (ExPlayerControl player in ExPlayerControl.ExPlayerControls)
            {
                if (player.Role == RoleId.Tuna && player.IsAlive())
                    AddHijackWinner(ref winners, ref hijackAddWinners, ref upperText, ref color, ref hasHijackWon, ref reason, ref winText, ref winType, player, "Tuna", CustomGameOverReason.TunaWin, Tuna.Instance.RoleColor);
            }
        }

        // 陰陽師 / 式神
        // CustomGameOverReason.OrientalShamanWinを追加
        foreach (ExPlayerControl player in ExPlayerControl.ExPlayerControls)
        {
            if (player.Role != RoleId.OrientalShaman || player.IsDead()) continue;
            if (OrientalShaman.OrientalShamanNeededTaskComplete && !player.IsTaskComplete()) continue;
            if (player.TryGetAbility<OrientalShamanAbility>(out var orientalShamanAbility))
            {
                AddHijackWinner(ref winners, ref hijackAddWinners, ref upperText, ref color, ref hasHijackWon, ref reason, ref winText, ref winType, player, "OrientalShaman", CustomGameOverReason.OrientalShamanWin, OrientalShaman.Instance.RoleColor);
                if (orientalShamanAbility._servant?.Player != null)
                    winners.Add(orientalShamanAbility._servant.Player);
            }
        }

        // ========================= 優先度(低) =========================
        // 神
        if (!hasHijackWon)
        {
            foreach (ExPlayerControl player in ExPlayerControl.ExPlayerControls)
            {
                if (player.Role == RoleId.God && player.IsAlive())
                {
                    if (God.GodNeededTask && !player.IsTaskComplete()) continue;
                    AddHijackWinner(ref winners, ref hijackAddWinners, ref upperText, ref color, ref hasHijackWon, ref reason, ref winText, ref winType, player, "God", CustomGameOverReason.GodWin, God.Instance.RoleColor);
                    winText = "GodDescends";
                }
            }
        }
    }

    private static void UpdateAdditionalWinners(GameOverReason reason, ref HashSet<ExPlayerControl> winners, out HashSet<string> addWinners, ref string winText, bool cantWinSixAdditionalWinners)
    {
        addWinners = new();
        // 三匹の仔豚勝利は同時勝利しない（旧仕様に合わせる）
        if (reason == (GameOverReason)CustomGameOverReason.TheThreeLittlePigsWin)
        {
            return;
        }
        // ラバーズじゃない人がいる場合
        if (Lovers.LoversWinType == LoversWinType.Single && winners.Any(x => !x.IsLovers()))
        {
            winners.RemoveWhere(x => x.IsLovers());
        }
        if (!cantWinSixAdditionalWinners)
        {
            foreach (ExPlayerControl player in ExPlayerControl.ExPlayerControls)
            {
                switch (player.Role)
                {
                    case RoleId.Opportunist:
                        if (player.IsAlive())
                        {
                            winners.Add(player);
                            addWinners.Add(EncodeWithColor(player.Role.ToString(), Opportunist.Instance.RoleColor));
                        }
                        break;
                    case RoleId.Tuna when !Tuna.EnableTunaSoloWin:
                        if (player.IsAlive())
                        {
                            winners.Add(player);
                            addWinners.Add(EncodeWithColor(player.Role.ToString(), Tuna.Instance.RoleColor));
                        }
                        break;
                    case RoleId.Spelunker when Spelunker.SpelunkerIsAdditionalWin:
                        if (player.IsAlive())
                        {
                            winners.Add(player);
                            addWinners.Add(EncodeWithColor(player.Role.ToString(), Spelunker.Instance.RoleColor));
                        }
                        break;
                }
            }
        }
        foreach (ExPlayerControl winner in winners.ToArray())
        {
            if (Lovers.LoversWinType == LoversWinType.Shared && winner.IsLovers())
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
                    addWinners.Add(EncodeWithColor(cupid.Role.ToString(), Cupid.Instance.RoleColor));
                }
            }
        }
        if (reason == (GameOverReason)CustomGameOverReason.LoversWin)
        {
            List<ExPlayerControl> creatorCupid = getCreatorCupid(winners.First());
            foreach (ExPlayerControl cupid in creatorCupid)
            {
                winners.Add(cupid);
                addWinners.Add(EncodeWithColor(cupid.Role.ToString(), Cupid.Instance.RoleColor));
            }
        }
        foreach (ExPlayerControl player in ExPlayerControl.ExPlayerControls)
        {
            if (player.Role == RoleId.PartTimer)
            {
                PartTimerAbility partTimerAbility = player.GetAbility<PartTimerAbility>();
                if (partTimerAbility != null && partTimerAbility._employer != null && winners.Contains(partTimerAbility._employer))
                {
                    // 生存勝利設定がONで死んでいる場合は勝利しない
                    if (partTimerAbility._data.needAliveToWin && player.IsDead()) continue;
                    winners.Add(player);
                    addWinners.Add(EncodeWithColor(player.Role.ToString(), PartTimer.Instance.RoleColor));
                }
            }
        }
        if (addWinners.Count != 0)
        {
            winText = null;
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
