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
using Hazel;
using SuperNewRoles.Roles;
using SuperNewRoles.Roles.Ability;

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
    public static float preparationTime { get; private set; } = 0f;
    private static float updateTimer = 0f;
    private static float startWinCheckEnableTime = 0f;

    // チーム管理
    private static Dictionary<byte, int> playerTeams = new Dictionary<byte, int>();
    private static List<List<byte>> teams = new List<List<byte>>();
    private static bool isTeamMode = false;

    // 統計管理
    private static Dictionary<byte, int> killCounts = new Dictionary<byte, int>();
    private static int lastAliveCount = 0;
    private static int lastAllPlayerCount = 0;
    private static Dictionary<byte, string> originalPlayerNames = new();

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

    [CustomOptionBool("BattleRoyalShowKillCountSelfOnly", false, displayMode: DisplayModeId.BattleRoyal, parentFieldName: nameof(BattleRoyalShowKillCount), parentActiveValue: true)]
    public static bool BattleRoyalShowKillCountSelfOnly;

    [CustomOptionFloat("BattleRoyalPreparationTime", 0f, 45f, 2.5f, 10f, displayMode: DisplayModeId.BattleRoyal, parentFieldName: nameof(Categories.ModeOption), parentActiveValue: ModeId.BattleRoyal)]
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

        if (AmongUsClient.Instance.AmHost)
        {
            // チーム設定
            SetupTeams();

            // 全プレイヤーにキル権限を付与
            SetupPlayerRoles();

            // 元のプレイヤー名をキャッシュ（各クライアントで安定して使うため）
            originalPlayerNames.Clear();
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (pc != null && pc.Data != null)
                    originalPlayerNames[pc.PlayerId] = pc.Data.PlayerName;
            }
        }
        else
        {
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                ((ExPlayerControl)player).SetRole(RoleId.Impostor);
            }
        }

        SetupVentAbilities();

        // イベントリスナー登録
        fixedUpdateListener = FixedUpdateEvent.Instance.AddListener(FixedUpdate);

        Logger.Info($"BattleRoyalMode: Setup complete. Team mode: {isTeamMode}");
    }

    // 導入者視点でベントを無効化
    private static void SetupVentAbilities()
    {
        foreach (var player in PlayerControl.AllPlayerControls)
        {
            ((ExPlayerControl)player).AttachAbility(new CustomVentAbility(() => false), new AbilityParentPlayer(player));
        }
    }

    private static bool IsBot(PlayerControl player)
    {
        return AmongUsClient.Instance.GetClientIdFromCharacter(player) < 0;
    }

    private static PlayerControl[] GetAlivePlayers()
    {
        return PlayerControl.AllPlayerControls
            .Where(p => p != null && !p.Data.Disconnected && !p.Data.IsDead && !IsBot(p))
            .ToArray();
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
        if (!isGameStarted) return true;
        if (!AmongUsClient.Instance.AmHost) return true;

        var alivePlayers = GetAlivePlayers();

        // イントロ直後の暴発を防ぐため、一定時間は勝利判定を行わない
        if (Time.time < startWinCheckEnableTime) return true;

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

        return true;
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

        return true;
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
                EndGameSetRoles(winnerExPlayers);
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
            EndGameSetRoles(new ExPlayerControl[0]);
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

    private void EndGameSetRoles(ExPlayerControl[] winners)
    {
        List<byte> winnerIds = winners.Select(p => p.PlayerId).ToList();
        foreach (PlayerControl player in PlayerControl.AllPlayerControls)
        {
            bool isDead = player.Data.IsDead;
            if (winnerIds.Contains(player.PlayerId))
            {
                player.RpcSetRole(RoleTypes.ImpostorGhost);
            }
            else
            {
                player.RpcSetRole(RoleTypes.CrewmateGhost);
            }
            player.Data.IsDead = isDead;
        }
        GameData.Instance.DirtyAllData();
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
        var allPlayers = PlayerControl.AllPlayerControls
            .Where(p => !p.Data.Disconnected).ToList();
        new LateTask(() =>
        {
            if (isTeamMode)
            {
                // チーム戦: 複雑な役職識別システム
                SetupTeamModeRoles(allPlayers);
            }
            else
            {
                // 個人戦: Old同様、自分=Shapeshifter、他人視点=Scientistのデシンク表現
                SetupTeamModeRoles(allPlayers);
            }
        }, 3f);
    }

    private void SetupTeamModeRoles(List<PlayerControl> allPlayers)
    {
        Logger.Info($"BattleRoyalMode: SetupTeamModeRoles", "SetupTeamModeRoles");
        // チーム戦では、チーム内は味方（Shapeshifter=赤い名前）、チーム外は敵（Scientist=青い名前）として表示
        foreach (var player in allPlayers)
        {
            ((ExPlayerControl)player).SetRole(RoleId.Impostor);
            Logger.Info($"BattleRoyalMode: SetupTeamModeRoles: {player.Data.PlayerName}", "SetupTeamModeRoles");
            if (!playerTeams.TryGetValue(player.PlayerId, out int playerTeamId))
                continue;
            Logger.Info($"BattleRoyalMode: SetupTeamModeRoles: {player.Data.PlayerName} : {playerTeamId}", "SetupTeamModeRoles");

            foreach (var otherPlayer in allPlayers)
            {
                Logger.Info($"BattleRoyalMode: SetupTeamModeRoles: {player.Data.PlayerName} : {otherPlayer.Data.PlayerName}", "SetupTeamModeRoles");
                if (player.PlayerId == otherPlayer.PlayerId)
                {
                    // 自分自身
                    if (player.PlayerId != 0) // ホスト以外
                    {
                        player.RpcSetRoleDesync(RoleTypes.Impostor, player);
                    }
                    else // ホスト
                    {
                        player.StartCoroutine(player.CoSetRole(RoleTypes.Impostor, false));
                        Logger.Info($"{player.Data.PlayerName} : {player.Data.PlayerName} => {RoleTypes.Impostor}を実行(ホスト)", "RpcSetRoleDesync");
                    }
                }
                else
                {
                    // 他のプレイヤー
                    if (playerTeams.TryGetValue(otherPlayer.PlayerId, out int otherTeamId) &&
                        playerTeamId == otherTeamId)
                    {
                        // 同じチーム = 味方 = Shapeshifter (赤い名前)
                        if (player.PlayerId == 0) // ホスト以外
                        {
                            (otherPlayer).StartCoroutine(otherPlayer.CoSetRole(RoleTypes.Shapeshifter, false));
                            Logger.Info($"{player.Data.PlayerName} : {player.Data.PlayerName} => {RoleTypes.Shapeshifter}を実行(ホスト)", "RpcSetRoleDesync");
                        }
                        else
                        {
                            otherPlayer.RpcSetRoleDesync(RoleTypes.Shapeshifter, player);
                        }
                    }
                    else
                    {
                        // 違うチーム = 敵 = Scientist (青い名前)
                        if (player.PlayerId == 0) // ホスト以外
                        {
                            (otherPlayer).StartCoroutine(otherPlayer.CoSetRole(RoleTypes.Scientist, false));
                            Logger.Info($"{player.Data.PlayerName} : {player.Data.PlayerName} => {RoleTypes.Shapeshifter}を実行(ホスト)", "RpcSetRoleDesync");
                        }
                        else
                        {
                            otherPlayer.RpcSetRoleDesync(RoleTypes.Scientist, player);
                        }
                    }
                }
            }
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
                    player.RpcSetNamePrivate(message, player);
                }
            }

            updateTimer = 1f;
        }

        // 準備時間終了
        if (preparationTime <= 0f)
        {
            isPreparationPhase = false;
            isGameStarted = true;
            // 勝利判定の発火猶予
            startWinCheckEnableTime = Time.time + 1.5f;

            Logger.Info("BattleRoyalMode: Preparation phase ended, battle started!");

            // プレイヤー名をリセット（キャッシュした元名で）
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (!player.Data.Disconnected)
                {
                    var baseName = originalPlayerNames.TryGetValue(player.PlayerId, out var n) ? n : player.Data.PlayerName;
                    player.RpcSetNamePrivate(baseName, player);
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
        // 勝利条件はFixedUpdate->HandleGamePhase中にも定期チェック
        CheckWinCondition();
    }

    private void UpdatePlayerDisplayNames()
    {
        var alivePlayers = PlayerControl.AllPlayerControls
            .Where(p => !p.Data.IsDead && !p.Data.Disconnected && !IsBot(p)).ToList();
        var allPlayers = PlayerControl.AllPlayerControls
            .Where(p => !p.Data.Disconnected && !IsBot(p)).ToList();

        // 変更があった場合のみ更新（全員のSuffixを一括生成し、各viewerへ送信）
        if (lastAliveCount != alivePlayers.Count || lastAllPlayerCount != allPlayers.Count)
        {
            // まず全員分の表示名を作成（自己視点と他人視点を分けて用意）
            Dictionary<byte, string> selfViewName = new();
            Dictionary<byte, string> otherViewName = new();

            foreach (var player in allPlayers)
            {
                var baseName = originalPlayerNames.TryGetValue(player.PlayerId, out var n) ? n : player.Data.PlayerName;
                string aliveSuffix = BattleRoyalShowAliveCount ? $" ({alivePlayers.Count}/{allPlayers.Count})" : "";

                if (BattleRoyalShowKillCount)
                {
                    int kills = killCounts.GetValueOrDefault(player.PlayerId, 0);
                    var killSuffix = $" [K:{kills}]";
                    selfViewName[player.PlayerId] = baseName + aliveSuffix + killSuffix;
                    otherViewName[player.PlayerId] = baseName + aliveSuffix + (BattleRoyalShowKillCountSelfOnly ? "" : killSuffix);
                }
                else
                {
                    selfViewName[player.PlayerId] = baseName + aliveSuffix;
                    otherViewName[player.PlayerId] = baseName + aliveSuffix;
                }
            }

            // viewer毎に正しい表示名を送る（自己視点はself用、それ以外はother用）
            foreach (var viewer in allPlayers)
            {
                foreach (var player in allPlayers)
                {
                    var nameToSend = (viewer.PlayerId == player.PlayerId) ? selfViewName[player.PlayerId] : otherViewName[player.PlayerId];
                    player.RpcSetNamePrivate(nameToSend, viewer);
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

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CheckMurder))]
    public static class BattleRoyalCheckMurderPatch
    {
        public static bool Prefix(PlayerControl __instance, PlayerControl target)
        {
            if (AmongUsClient.Instance.AmHost && ModeManager.IsMode(ModeId.BattleRoyal))
            {
                if (preparationTime > 0f)
                    return false;
                if (__instance.Data.IsDead || target.Data.IsDead)
                    return false;
                __instance.isKilling = true;
                __instance.RpcMurderPlayer(target, didSucceed: true);
                return false;
            }
            return true;
        }
    }
    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.UpdateSystem), [typeof(SystemTypes), typeof(PlayerControl), typeof(byte)])]
    public static class ShipStatusUpdateSystemPatch
    {
        public static bool Prefix(ShipStatus __instance, SystemTypes systemType, PlayerControl player, byte amount)
        {
            if (ModeManager.IsMode(ModeId.BattleRoyal) && (systemType == SystemTypes.Sabotage || ModHelpers.IsSabotage(systemType)))
            {
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.RpcCloseDoorsOfType))]
    public static class ShipStatusRpcCloseDoorsOfTypePatch
    {
        public static bool Prefix(ShipStatus __instance)
        {
            if (ModeManager.IsMode(ModeId.BattleRoyal))
            {
                return false;
            }
            return true;
        }
    }
    /// <summary>
    /// バトルロイヤルモード用のベント使用禁止パッチ
    /// </summary>
    [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.CoEnterVent))]
    public static class BattleRoyalVentDisablePatch
    {
        public static bool Prefix(PlayerPhysics __instance, [HarmonyArgument(0)] int id)
        {
            // バトルロイヤルモードでない場合は通常処理
            if (!ModeManager.IsModeActive || ModeManager.CurrentMode?.Mode != ModeId.BattleRoyal)
                return true;

            // バトルロイヤルモードではベント使用を禁止
            if (AmongUsClient.Instance.AmHost)
            {
                // 強制的にベントから追い出す
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(__instance.NetId, (byte)RpcCalls.BootFromVent, SendOption.Reliable, -1);
                writer.WritePacked(127);
                AmongUsClient.Instance.FinishRpcImmediately(writer);

                // 少し遅れてプレイヤー個別にも送信
                new Modules.LateTask(() =>
                {
                    int clientId = AmongUsClient.Instance.GetClientIdFromCharacter(__instance.myPlayer);
                    MessageWriter writer2 = AmongUsClient.Instance.StartRpcImmediately(__instance.NetId, (byte)RpcCalls.BootFromVent, SendOption.Reliable, clientId);
                    writer2.Write(id);
                    AmongUsClient.Instance.FinishRpcImmediately(writer2);
                    __instance.myPlayer.inVent = false;
                }, 0.5f, "BattleRoyalAntiVent");

                return false;
            }

            return false;
        }
    }
}
