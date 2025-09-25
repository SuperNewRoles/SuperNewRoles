using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles;
using SuperNewRoles.Roles.Neutral;
using SuperNewRoles.CustomOptions.Categories;
using SuperNewRoles.CustomOptions;
using HarmonyLib;
using AmongUs.GameOptions;
using SuperNewRoles.Events;
using SuperNewRoles.Modules.Events.Bases;
using SuperNewRoles.Patches;
using SuperNewRoles.Extensions;

namespace SuperNewRoles.Mode;

public class WCBattleRoyalMode : ModeBase<WCBattleRoyalMode>, IModeBase
{
    public override ModeId Mode => ModeId.WCBattleRoyal;
    public override string ModeName => "BattleRoyalMode";
    public override Color32 ModeColor => new Color32(255, 128, 0, 255);

    // チーム管理
    private Dictionary<byte, int> playerTeams = new Dictionary<byte, int>();
    private List<List<byte>> teams = new List<List<byte>>();
    private int totalTeams = 1;
    private bool isTeamMode = false;

    // 公開プロパティ
    public bool IsTeamMode => isTeamMode;
    public int TotalTeams => totalTeams;
    private EventListener<NameTextUpdateEventData> nameTextListener;

    // チーム名（A, B, C...）
    private static readonly string[] TeamNames = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".Select(c => c.ToString()).ToArray();

    // バトルロワイヤルモード設定
    [CustomOptionBool("WaveCannonBattleRoyalTeamMode", false, displayMode: DisplayModeId.WCBattleRoyal, parentFieldName: nameof(Categories.ModeOption), parentActiveValue: ModeId.WCBattleRoyal)]
    public static bool WaveCannonBattleRoyalTeamMode;

    [CustomOptionInt("WaveCannonBattleRoyalTeamCount", 2, 10, 1, 2, displayMode: DisplayModeId.WCBattleRoyal, parentFieldName: nameof(WaveCannonBattleRoyalTeamMode), parentActiveValue: true)]
    public static int WaveCannonBattleRoyalTeamCount;

    [CustomOptionBool("WaveCannonBattleRoyalFriendlyFire", true, displayMode: DisplayModeId.WCBattleRoyal, parentFieldName: nameof(WaveCannonBattleRoyalTeamMode), parentActiveValue: true)]
    public static bool WaveCannonBattleRoyalFriendlyFire;

    [CustomOptionFloat("WaveCannonBattleRoyalCooldown", 2.5f, 60f, 2.5f, 15f, displayMode: DisplayModeId.WCBattleRoyal, parentFieldName: nameof(Categories.ModeOption), parentActiveValue: ModeId.WCBattleRoyal)]
    public static float WaveCannonBattleRoyalCooldown;

    [CustomOptionFloat("WaveCannonBattleRoyalChargeTime", 0f, 10f, 0.5f, 3f, displayMode: DisplayModeId.WCBattleRoyal, parentFieldName: nameof(Categories.ModeOption), parentActiveValue: ModeId.WCBattleRoyal)]
    public static float WaveCannonBattleRoyalChargeTime;

    public override void OnGameStart()
    {
        Logger.Info("WCBattleRoyalMode.OnGameStart");

        // 全プレイヤーを取得してチーム分け
        var allPlayers = PlayerControl.AllPlayerControls.ToArray().Where(p => p != null).ToList();

        // モード設定を取得
        isTeamMode = WaveCannonBattleRoyalTeamMode;
        // チーム数をプレイヤー数に制限（空のチームを防ぐ）
        totalTeams = isTeamMode ? Math.Min(WaveCannonBattleRoyalTeamCount, allPlayers.Count) : 1;

        if (isTeamMode)
        {
            AssignTeams(allPlayers);
        }
        else
        {
            // 個人戦の場合
            teams.Clear();
            foreach (var player in allPlayers)
            {
                teams.Add(new List<byte> { player.PlayerId });
                playerTeams[player.PlayerId] = teams.Count - 1;
            }
        }

        nameTextListener = NameTextUpdateEvent.Instance.AddListener(OnNameTextUpdate);

        // 全員をBattleRoyalWaveCannon役職に設定
        foreach (var player in allPlayers)
        {
            // 既存の役職をクリア
            var exPlayer = (ExPlayerControl)player;

            // BattleRoyalWaveCannonを設定
            var battleRoyalRole = BattleRoyalWaveCannon.Instance;
            exPlayer.SetRole(battleRoyalRole.Role);
            RoleManager.Instance.SetRole(player, RoleTypes.Crewmate);

            // チームカラーを設定
            if (isTeamMode)
            {
                var teamIndex = playerTeams[player.PlayerId];
                var teamColorId = GetTeamColorId(teamIndex);
                player.cosmetics.SetBodyColor(teamColorId);
            }
        }
    }

    private void AssignTeams(List<PlayerControl> players)
    {
        teams.Clear();
        playerTeams.Clear();

        // チームを初期化
        for (int i = 0; i < totalTeams; i++)
        {
            teams.Add(new List<byte>());
        }

        // プレイヤーをランダムにチームに割り当て
        var shuffledPlayers = players.OrderBy(x => Guid.NewGuid()).ToList();
        for (int i = 0; i < shuffledPlayers.Count; i++)
        {
            var teamIndex = i % totalTeams;
            var playerId = shuffledPlayers[i].PlayerId;
            teams[teamIndex].Add(playerId);
            playerTeams[playerId] = teamIndex;
        }
    }

    private Color GetTeamColor(int teamIndex)
    {
        // HSVで色を計算
        float hue = (360f / totalTeams) * teamIndex;
        return Color.HSVToRGB(hue / 360f, 1f, 1f);
    }

    private byte GetTeamColorId(int teamIndex)
    {
        // 既存の色のIDを使用（0-17の範囲）
        // チームの数に応じて色を分配
        var availableColors = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17 };
        return availableColors[teamIndex % availableColors.Length];
    }

    public override void OnGameEnd()
    {
        Logger.Info("WCBattleRoyalMode.OnGameEnd");
        playerTeams.Clear();
        teams.Clear();
        nameTextListener?.RemoveListener();
    }

    private void OnNameTextUpdate(NameTextUpdateEventData data)
    {
        if (!isTeamMode) return;
        if (ExPlayerControl.LocalPlayer.IsDead() || IsOnSameTeam(ExPlayerControl.LocalPlayer.Player, data.Player))
            NameText.SetNameTextColor(data.Player, GetTeamColor(GetPlayerTeam(data.Player)));
    }

    public override void OnPlayerDeath(PlayerControl player, PlayerControl killer)
    {
        Logger.Info($"BattleRoyalMode.OnPlayerDeath: {player?.name} killed by {killer?.name}");

        // 勝利条件をチェック
        if (CanWin())
        {
            // ゲーム終了処理を呼び出す
            EndGame();
        }
    }

    public override bool CheckWinCondition()
    {
        if (ExPlayerControl.ExPlayerControls.Where(x => x.IsAlive()).Count() <= 1)
        {
            EndGame();
        }
        return true;
    }

    private bool CanWin()
    {
        if (isTeamMode)
        {
            // チーム戦：生き残っているチームが1つだけ
            var aliveTeams = new HashSet<int>();
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (player != null && !player.Data.IsDead && !player.Data.Disconnected)
                {
                    if (playerTeams.TryGetValue(player.PlayerId, out var teamIndex))
                    {
                        aliveTeams.Add(teamIndex);
                    }
                }
            }
            return aliveTeams.Count <= 1;
        }
        else
        {
            // 個人戦：生き残っているプレイヤーが1人だけ
            var alivePlayers = PlayerControl.AllPlayerControls.ToArray()
                .Where(p => p != null && !p.Data.IsDead && !p.Data.Disconnected)
                .Count();
            return alivePlayers <= 1;
        }
    }

    private void EndGame()
    {
        Logger.Info("BattleRoyalMode.EndGame");

        // 勝者チーム/プレイヤーを決定
        int winnerTeam = -1;

        if (isTeamMode)
        {
            // チーム戦モードの場合
            var aliveTeams = PlayerControl.AllPlayerControls
                .Where(p => p != null && !p.Data.IsDead && !p.Data.Disconnected)
                .Select(p => playerTeams.TryGetValue(p.PlayerId, out var team) ? team : -1)
                .Where(team => team != -1)
                .Distinct()
                .ToHashSet();

            if (aliveTeams.Count == 1)
            {
                winnerTeam = aliveTeams.First();
                Logger.Info($"Team {GetTeamName(winnerTeam)} wins!");
            }
        }
        else
        {
            var winner = ExPlayerControl.ExPlayerControls.FirstOrDefault(p =>
                p != null && !p.IsDead() && !p.Data.Disconnected);
            if (winner != null)
            {
                winnerTeam = GetPlayerTeam(winner.Player);
                Logger.Info($"Player {winner.Player.name} wins!");
            }
        }

        // ゲーム終了RPCを呼び出し
        RpcWCBattleRoyalEndGame(winnerTeam);
    }
    [CustomRPC]
    public static void RpcWCBattleRoyalEndGame(int winnerTeam)
    {
        var winners = new List<byte>();
        if (winnerTeam == -1)
        {
            winners = PlayerControl.AllPlayerControls.ToArray().Where(p => p != null && !p.Data.IsDead && !p.Data.Disconnected).Select(p => p.PlayerId).ToList();
        }
        else
        {
            // チームインデックスの境界チェックを追加
            if (winnerTeam >= 0 && winnerTeam < WCBattleRoyalMode.Instance.teams.Count)
            {
                winners = WCBattleRoyalMode.Instance.teams[winnerTeam];
            }
            else
            {
                Logger.Error($"Invalid winnerTeam index: {winnerTeam}");
                return;
            }
        }
        string winnerName = string.Empty;
        if (WCBattleRoyalMode.Instance.isTeamMode)
        {
            winnerName = WCBattleRoyalMode.Instance.GetTeamName(winnerTeam);
        }
        else
        {
            // チームメンバーが存在するかチェック
            if (WCBattleRoyalMode.Instance.teams[winnerTeam].Count > 0)
            {
                winnerName = ExPlayerControl.ById(WCBattleRoyalMode.Instance.teams[winnerTeam].First()).Player.name;
            }
            else
            {
                Logger.Error($"No players in winner team: {winnerTeam}");
                return;
            }
        }
        EndGameManagerSetUpPatch.EndGameWithCondition(
            GameOverReason.ImpostorsByKill,
            winners,
            "",
            new List<string>(),
            new Color32(255, 128, 0, 255),
            false,
            string.Format(ModTranslation.GetString("BattleRoyalWinText"), winnerName),
            validTranslation: false
        );
    }

    public string GetTeamName(int teamIndex)
    {
        if (teamIndex >= 0 && teamIndex < TeamNames.Length)
        {
            return TeamNames[teamIndex];
        }
        return $"Team{teamIndex + 1}";
    }

    public bool IsOnSameTeam(PlayerControl player1, PlayerControl player2)
    {
        if (!isTeamMode) return false;

        if (playerTeams.TryGetValue(player1.PlayerId, out var team1) &&
            playerTeams.TryGetValue(player2.PlayerId, out var team2))
        {
            return team1 == team2;
        }
        return false;
    }

    public int GetPlayerTeam(PlayerControl player)
    {
        if (playerTeams.TryGetValue(player.PlayerId, out var team))
        {
            return team;
        }
        return -1;
    }

    /// <summary>
    /// モードがイントロをカスタマイズするかどうか
    /// </summary>
    public override bool HasCustomIntro => true;

    /// <summary>
    /// イントロ情報を取得する
    /// </summary>
    /// <param name="player">プレイヤー</param>
    /// <returns>イントロ情報</returns>
    public override ModeIntroInfo GetIntroInfo(PlayerControl player)
    {
        var info = new ModeIntroInfo();

        // 基本色設定
        info.RoleColor = ModeColor;

        // チーム戦かどうかで表示を変更
        if (isTeamMode)
        {
            var teamIndex = GetPlayerTeam(player);
            var teamName = GetTeamName(teamIndex);

            info.RoleTitle = ModTranslation.GetString("WCBattleRoyalIntroTeamTitle", teamName);
            info.RoleSubTitle = ModTranslation.GetString("BattleRoyalIntroTeamSubTitle");
            info.RoleColor = GetTeamColor(teamIndex);

            // チームメイトリストを取得
            info.TeamMembers = GetTeamMembers(player);
        }
        else
        {
            info.RoleTitle = ModTranslation.GetString("WCBattleRoyalIntroTitle");
            info.RoleSubTitle = ModTranslation.GetString("BattleRoyalIntroSubTitle");
            info.TeamMembers = new List<PlayerControl> { player };
        }

        // ランダムなイントロメッセージを選択
        info.IntroMessage = GetRandomIntroMessage();

        // バトルロワイヤルらしい緊張感のあるサウンドを設定
        info.IntroSoundType = AmongUs.GameOptions.RoleTypes.Impostor;

        return info;
    }

    /// <summary>
    /// チームメンバーのリストを取得する
    /// </summary>
    /// <param name="player">プレイヤー</param>
    /// <returns>チームメンバーのリスト</returns>
    public override List<PlayerControl> GetTeamMembers(PlayerControl player)
    {
        if (!isTeamMode)
        {
            return new List<PlayerControl> { player };
        }

        var teamIndex = GetPlayerTeam(player);
        var teamMembers = new List<PlayerControl>();

        foreach (var p in PlayerControl.AllPlayerControls)
        {
            if (p != null && GetPlayerTeam(p) == teamIndex)
            {
                teamMembers.Add(p);
            }
        }

        return teamMembers;
    }

    /// <summary>
    /// ランダムなイントロメッセージを取得する
    /// </summary>
    /// <returns>イントロメッセージ</returns>
    private string GetRandomIntroMessage()
    {
        var messages = new List<string>
        {
            "BattleRoyalIntroMessage1",
            "BattleRoyalIntroMessage2"
        };

        if (isTeamMode)
        {
            messages.Add("BattleRoyalIntroMessage3");
        }

        return ModTranslation.GetString(ModHelpers.GetRandom(messages));
    }
}