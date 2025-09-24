using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SuperNewRoles.Modules;
using SuperNewRoles.CustomOptions.Categories;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Modules.Events.Bases;
using SuperNewRoles.Events;
using AmongUs.GameOptions;
using HarmonyLib;
using SuperNewRoles.Extensions;

namespace SuperNewRoles.Mode;

/// <summary>
/// バトルロイヤルモード - 全員がキル能力を持ち、最後の1人/チームまで戦うモード
/// </summary>
public class BattleRoyalMode : ModeBase<BattleRoyalMode>, IModeBase
{
    public override ModeId Mode => ModeId.BattleRoyal;
    public override string ModeName => ModTranslation.GetString("BattleRoyalMode");
    public override Color32 ModeColor => new Color32(255, 64, 64, 255);

    // ゲーム状態管理
    private static bool isGameStarted = false;
    private static bool isPreparationPhase = true;
    private static float preparationTime = 0f;
    private static float updateTimer = 0f;

    // チーム管理
    private static Dictionary<byte, int> playerTeams = new Dictionary<byte, int>();
    private static List<List<byte>> teams = new List<List<byte>>();
    private static bool isTeamMode = false;

    // 統計管理
    private static Dictionary<byte, int> killCounts = new Dictionary<byte, int>();
    private static int lastAliveCount = 0;
    private static int lastAllPlayerCount = 0;

    // イベントリスナー
    private EventListener fixedUpdateListener;

    // 設定オプション
    [CustomOptionBool("BattleRoyalTeamMode", false, displayMode: DisplayModeId.BattleRoyal, parentFieldName: nameof(Categories.ModeOption), parentActiveValue: ModeId.BattleRoyal)]
    public static bool BattleRoyalTeamMode;

    [CustomOptionInt("BattleRoyalTeamCount", 2, 10, 1, 2, displayMode: DisplayModeId.BattleRoyal, parentFieldName: nameof(BattleRoyalTeamMode), parentActiveValue: true)]
    public static int BattleRoyalTeamCount;

    [CustomOptionBool("BattleRoyalShowAliveCount", true, displayMode: DisplayModeId.BattleRoyal, parentFieldName: nameof(Categories.ModeOption), parentActiveValue: ModeId.BattleRoyal)]
    public static bool BattleRoyalShowAliveCount;

    [CustomOptionBool("BattleRoyalShowKillCount", true, displayMode: DisplayModeId.BattleRoyal, parentFieldName: nameof(Categories.ModeOption), parentActiveValue: ModeId.BattleRoyal)]
    public static bool BattleRoyalShowKillCount;

    [CustomOptionFloat("BattleRoyalPreparationTime", 0f, 60f, 2.5f, 10f, displayMode: DisplayModeId.BattleRoyal, parentFieldName: nameof(Categories.ModeOption), parentActiveValue: ModeId.BattleRoyal)]
    public static float BattleRoyalPreparationTime;

    public override void OnGameStart()
    {
        Logger.Info("BattleRoyalMode: Game Started");

        // 初期化
        isGameStarted = false;
        isPreparationPhase = true;
        preparationTime = BattleRoyalPreparationTime;
        updateTimer = 0f;
        isTeamMode = BattleRoyalTeamMode;

        killCounts.Clear();
        playerTeams.Clear();
        teams.Clear();

        lastAliveCount = 0;
        lastAllPlayerCount = 0;

        // チーム設定
        SetupTeams();

        // 全プレイヤーにキル権限を付与
        SetupPlayerRoles();

        // イベントリスナー登録
        fixedUpdateListener = FixedUpdateEvent.Instance.AddListener(FixedUpdate);

        Logger.Info($"BattleRoyalMode: Setup complete. Team mode: {isTeamMode}");
    }

    public override void OnGameEnd()
    {
        Logger.Info("BattleRoyalMode: Game Ended");

        // クリーンアップ
        isGameStarted = false;
        isPreparationPhase = true;

        killCounts.Clear();
        playerTeams.Clear();
        teams.Clear();

        // イベントリスナー解除
        if (fixedUpdateListener != null)
        {
            FixedUpdateEvent.Instance.RemoveListener(fixedUpdateListener);
            fixedUpdateListener = null;
        }
    }

    public override void OnPlayerDeath(PlayerControl player, PlayerControl killer)
    {
        if (!isGameStarted) return;

        Logger.Info($"BattleRoyalMode: Player {player.name} killed by {killer?.name ?? "unknown"}");

        // キル数カウント
        if (killer != null && killer != player)
        {
            if (!killCounts.ContainsKey(killer.PlayerId))
                killCounts[killer.PlayerId] = 0;
            killCounts[killer.PlayerId]++;
        }

        // 勝利条件チェック
        CheckWinCondition();
    }

    public override bool CheckWinCondition()
    {
        if (!isGameStarted) return false;

        var alivePlayers = PlayerControl.AllPlayerControls
            .Where(p => !p.Data.IsDead && !p.Data.Disconnected).ToArray();

        if (isTeamMode)
        {
            return CheckTeamWinCondition(alivePlayers);
        }
        else
        {
            return CheckSoloWinCondition(alivePlayers);
        }
    }

    private bool CheckTeamWinCondition(PlayerControl[] alivePlayers)
    {
        if (alivePlayers.Length == 0)
        {
            // 全員死亡 - 引き分け
            EndGame(GameOverReason.CrewmatesByVote, null);
            return true;
        }

        // 生存チーム数をカウント
        var aliveTeams = new HashSet<int>();
        foreach (var player in alivePlayers)
        {
            if (playerTeams.TryGetValue(player.PlayerId, out int teamId))
            {
                aliveTeams.Add(teamId);
            }
        }

        if (aliveTeams.Count <= 1)
        {
            // 1チーム以下 - 勝利
            var winnerTeamId = aliveTeams.FirstOrDefault();
            var winners = alivePlayers.Where(p => playerTeams.GetValueOrDefault(p.PlayerId) == winnerTeamId).ToList();
            EndGame(GameOverReason.CrewmatesByVote, winners);
            return true;
        }

        return false;
    }

    private bool CheckSoloWinCondition(PlayerControl[] alivePlayers)
    {
        if (alivePlayers.Length <= 1)
        {
            if (alivePlayers.Length == 1)
            {
                // 1人生存 - 勝利
                EndGame(GameOverReason.ImpostorsByVote, new List<PlayerControl> { alivePlayers[0] });
            }
            else
            {
                // 全員死亡 - 引き分け
                EndGame(GameOverReason.CrewmatesByVote, null);
            }
            return true;
        }

        return false;
    }

    private void EndGame(GameOverReason reason, List<PlayerControl> winners)
    {
        Logger.Info($"BattleRoyalMode: Game ending with reason {reason}");

        if (winners != null && winners.Count > 0)
        {
            // 勝者がいる場合
            var winnerExPlayers = winners.Select(p => (ExPlayerControl)p).Where(p => p != null).ToArray();
            if (winnerExPlayers.Length > 0)
            {
                EndGamer.RpcEndGameWithWinner(
                    reason: (SuperNewRoles.Patches.CustomGameOverReason)reason,
                    winType: WinType.Default,
                    winners: winnerExPlayers,
                    color: ModeColor,
                    upperText: "BattleRoyalWin",
                    winText: "WinText"
                );
            }
        }
        else
        {
            // 勝者がいない場合（引き分け）
            EndGamer.RpcEndGameWithWinner(
                reason: (SuperNewRoles.Patches.CustomGameOverReason)reason,
                winType: WinType.NoWinner,
                winners: new ExPlayerControl[0],
                color: Color.gray,
                upperText: "Draw",
                winText: "WinText"
            );
        }
    }

    private void SetupTeams()
    {
        var allPlayers = PlayerControl.AllPlayerControls
            .Where(p => !p.Data.Disconnected).ToList();

        if (isTeamMode)
        {
            // チーム戦の場合
            int teamCount = BattleRoyalTeamCount;
            int playersPerTeam = Mathf.CeilToInt((float)allPlayers.Count / teamCount);

            var shuffledPlayers = allPlayers.OrderBy(_ => UnityEngine.Random.value).ToList();

            for (int teamId = 0; teamId < teamCount; teamId++)
            {
                var teamPlayers = new List<byte>();

                for (int i = 0; i < playersPerTeam && teamId * playersPerTeam + i < shuffledPlayers.Count; i++)
                {
                    var player = shuffledPlayers[teamId * playersPerTeam + i];
                    playerTeams[player.PlayerId] = teamId;
                    teamPlayers.Add(player.PlayerId);
                }

                if (teamPlayers.Count > 0)
                {
                    teams.Add(teamPlayers);
                }
            }

            Logger.Info($"BattleRoyalMode: Created {teams.Count} teams");
        }
        else
        {
            // 個人戦の場合
            for (int i = 0; i < allPlayers.Count; i++)
            {
                playerTeams[allPlayers[i].PlayerId] = i;
                teams.Add(new List<byte> { allPlayers[i].PlayerId });
            }

            Logger.Info($"BattleRoyalMode: Created {teams.Count} solo teams");
        }
    }

    private void SetupPlayerRoles()
    {
        // 全プレイヤーにキル能力を付与
        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (player.Data.Disconnected) continue;

            // Shapeshifterとして設定してキル能力を付与
            player.RpcSetRole(RoleTypes.Shapeshifter);
        }
    }

    /// <summary>
    /// FixedUpdateで呼ばれる処理
    /// </summary>
    public void FixedUpdate()
    {
        if (!AmongUsClient.Instance.AmHost) return;
        if (HudManager.Instance.IsIntroDisplayed) return;

        if (isPreparationPhase)
        {
            HandlePreparationPhase();
        }
        else if (isGameStarted)
        {
            HandleGamePhase();
        }
    }

    private void HandlePreparationPhase()
    {
        preparationTime -= Time.fixedDeltaTime;
        updateTimer -= Time.fixedDeltaTime;

        // 1秒ごとに名前を更新
        if (updateTimer <= 0f)
        {
            var remainingSeconds = Mathf.CeilToInt(preparationTime);
            var message = ModTranslation.GetString("BattleRoyalPreparationRemaining") + remainingSeconds + ModTranslation.GetString("seconds");

            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (!player.Data.Disconnected)
                {
                    player.RpcSetName(message);
                }
            }

            updateTimer = 1f;
        }

        // 準備時間終了
        if (preparationTime <= 0f)
        {
            isPreparationPhase = false;
            isGameStarted = true;

            Logger.Info("BattleRoyalMode: Preparation phase ended, battle started!");

            // プレイヤー名をリセット
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (!player.Data.Disconnected)
                {
                    player.RpcSetName(player.Data.PlayerName);
                }
            }
        }
    }

    private void HandleGamePhase()
    {
        // 生存人数とキル数の表示更新
        if (BattleRoyalShowAliveCount || BattleRoyalShowKillCount)
        {
            UpdatePlayerDisplayNames();
        }
    }

    private void UpdatePlayerDisplayNames()
    {
        var alivePlayers = PlayerControl.AllPlayerControls
            .Where(p => !p.Data.IsDead && !p.Data.Disconnected).ToList();
        var allPlayers = PlayerControl.AllPlayerControls
            .Where(p => !p.Data.Disconnected).ToList();

        // 変更があった場合のみ更新
        if (lastAliveCount != alivePlayers.Count || lastAllPlayerCount != allPlayers.Count)
        {
            foreach (var player in allPlayers)
            {
                string displaySuffix = "";

                if (BattleRoyalShowAliveCount)
                {
                    displaySuffix += $" ({alivePlayers.Count}/{allPlayers.Count})";
                }

                if (BattleRoyalShowKillCount)
                {
                    int kills = killCounts.GetValueOrDefault(player.PlayerId, 0);
                    displaySuffix += $" [K:{kills}]";
                }

                if (!string.IsNullOrEmpty(displaySuffix))
                {
                    string newName = player.Data.PlayerName + displaySuffix;
                    player.RpcSetName(newName);
                }
            }

            lastAliveCount = alivePlayers.Count;
            lastAllPlayerCount = allPlayers.Count;
        }
    }


    public override ModeIntroInfo GetIntroInfo(PlayerControl player)
    {
        string teamInfo = "";
        if (isTeamMode && playerTeams.TryGetValue(player.PlayerId, out int teamId))
        {
            teamInfo = ModTranslation.GetString("Team") + " " + (char)('A' + teamId);
        }

        return new ModeIntroInfo
        {
            RoleTitle = ModeName,
            RoleSubTitle = teamInfo,
            IntroMessage = ModTranslation.GetString("BattleRoyalModeDescription"),
            RoleColor = ModeColor,
            TeamMembers = GetTeamMembers(player)
        };
    }

    public override List<PlayerControl> GetTeamMembers(PlayerControl player)
    {
        if (!isTeamMode || !playerTeams.TryGetValue(player.PlayerId, out int teamId))
        {
            return new List<PlayerControl> { player };
        }

        return PlayerControl.AllPlayerControls
            .Where(p => playerTeams.GetValueOrDefault(p.PlayerId) == teamId)
            .ToList();
    }

    public override bool HasCustomIntro => true;
}
