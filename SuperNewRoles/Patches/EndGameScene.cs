using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles;
using UnityEngine;

namespace SuperNewRoles.Patches;

public enum WinCondition
{
    None,
    CrewmateWin,
    ImpostorWin,
    Haison = CustomGameOverReason.Haison,
    NoWinner = CustomGameOverReason.NoWinner,
    // カスタム勝利条件のために予約
    CustomWin
}
public enum CustomGameOverReason
{
    None = 30,
    Haison = 31,
    NoWinner = 32,
}

static class AdditionalTempData
{
    // Should be implemented using a proper GameOverReason in the future
    public static List<PlayerRoleInfo> playerRoles = new();
    public static GameOverReason gameOverReason;
    public static WinCondition winCondition = WinCondition.None;
    public static List<WinCondition> additionalWinConditions = new();

    public static Dictionary<int, PlayerControl> plagueDoctorInfected = new();
    public static Dictionary<int, float> plagueDoctorProgress = new();

    public static void Clear()
    {
        playerRoles.Clear();
        additionalWinConditions.Clear();
        winCondition = WinCondition.None;
    }
    internal class PlayerRoleInfo
    {
        public string PlayerName { get; set; }
        public string NameSuffix { get; set; }
        public IRoleBase roleBase { get; set; }
        public string RoleString { get; set; }
        public int TasksCompleted { get; set; }
        public int TasksTotal { get; set; }
        public int PlayerId { get; set; }
        public int ColorId { get; set; }
        public FinalStatus Status { get; internal set; }
        public RoleId RoleId { get; set; }
        public RoleId GhostRoleId { get; set; }
        public string AttributeRoleName { get; set; }
        public bool isImpostor { get; set; }
    }
}

[HarmonyPatch(typeof(LogicGameFlowNormal), nameof(LogicGameFlowNormal.IsGameOverDueToDeath))]
public static class ShipStatusPatch
{
    public static bool Prefix(ref bool __result)
    {
        __result = false;
        return false;
    }
}

[HarmonyPatch(typeof(EndGameManager), nameof(EndGameManager.SetEverythingUp))]
public class EndGameManagerSetUpPatch
{
    public static bool IsHaison = false;
    public static TMPro.TMP_Text textRenderer;
    [HarmonyPatch(typeof(EndGameNavigation), nameof(EndGameNavigation.ShowProgression))]
    public class ShowProgressionPatch
    {
        public static void Prefix()
        {
            if (textRenderer != null)
            {
                textRenderer.gameObject.SetActive(false);
            }
        }
    }
    #region ProcessWinText
    static Color32 HaisonColor = new(163, 163, 162, byte.MaxValue);
    public static (string text, Color color, bool Haison) ProcessWinText(GameOverReason gameOverReason, WinCondition winCondition)
    {
        string text = "";
        Color RoleColor = Color.white;
        switch (winCondition)
        {
            case WinCondition.CrewmateWin:
                text = "Crewmate";
                RoleColor = Palette.White;
                break;
            case WinCondition.ImpostorWin:
                text = "Impostor";
                RoleColor = Palette.ImpostorRed;
                break;
            case WinCondition.Haison:
                text = "Haison";
                RoleColor = HaisonColor;
                break;
            case WinCondition.NoWinner:
                text = "NoWinner";
                RoleColor = Color.white;
                break;
        }
        bool haison = false;
        text = ModTranslation.GetString(text);


        bool IsOpptexton = false;

        // オポチュニスト

        bool IsLovetexton = false;
        bool Temp1;

        if (haison) text = ModTranslation.GetString("HaisonName");
        else if (text == ModTranslation.GetString("NoWinner")) text = ModTranslation.GetString("NoWinnerText");
        else if (text == ModTranslation.GetString("GodName")) text += " " + ModTranslation.GetString("GodWinText");
        else text = string.Format(text + " " + ModTranslation.GetString("WinName"));

        return (text, RoleColor, haison);
    }
    #endregion

    public static void Postfix(EndGameManager __instance)
    {
        // AprilFoolsManager.SetRandomModMode();

        foreach (PoolablePlayer pb in __instance.transform.GetComponentsInChildren<PoolablePlayer>())
        {
            UnityEngine.Object.Destroy(pb.gameObject);
        }
        int num = Mathf.CeilToInt(7.5f);
        IEnumerable<CachedPlayerData> list = EndGameResult.CachedWinners.ToArray().OrderBy(b => !b.IsYou ? 0 : -1);
        int i = -1;
        foreach (CachedPlayerData CachedPlayerData2 in list)
        {
            i++;
            int num2 = (i % 2 == 0) ? -1 : 1;
            int num3 = (i + 1) / 2;
            float num4 = (float)num3 / (float)num;
            float num5 = Mathf.Lerp(1f, 0.75f, num4);
            float num6 = (float)((i == 0) ? -8 : -1);
            PoolablePlayer poolablePlayer = UnityEngine.Object.Instantiate<PoolablePlayer>(__instance.PlayerPrefab, __instance.transform);
            poolablePlayer.transform.localPosition = new Vector3(1f * (float)num2 * (float)num3 * num5, FloatRange.SpreadToEdges(-1.125f, 0f, num3, num), num6 + (float)num3 * 0.01f) * 0.9f;
            float num7 = Mathf.Lerp(1f, 0.65f, num4) * 0.9f;
            Vector3 vector = new(num7, num7, 1f);
            poolablePlayer.transform.localScale = vector;
            if (CachedPlayerData2.IsDead)
            {
                poolablePlayer.SetBodyAsGhost();
                poolablePlayer.SetDeadFlipX(i % 2 == 0);
            }
            else
            {
                poolablePlayer.SetFlipX(i % 2 == 0);
            }
            poolablePlayer.UpdateFromPlayerOutfit(CachedPlayerData2.Outfit, PlayerMaterial.MaskType.None, CachedPlayerData2.IsDead, true);
            poolablePlayer.cosmetics.nameText.color = Color.white;
            poolablePlayer.cosmetics.nameText.transform.localScale = new Vector3(1f / vector.x, 1f / vector.y, 1f / vector.z);
            poolablePlayer.cosmetics.nameText.transform.localPosition = new Vector3(poolablePlayer.cosmetics.nameText.transform.localPosition.x, poolablePlayer.cosmetics.nameText.transform.localPosition.y - 0.8f, -15f);
            poolablePlayer.cosmetics.nameText.text = CachedPlayerData2.PlayerName;

            foreach (var data in AdditionalTempData.playerRoles)
            {
                if (data.PlayerName != CachedPlayerData2.PlayerName) continue;
                poolablePlayer.cosmetics.nameText.text = $"{data.PlayerName}{data.NameSuffix}\n{string.Join("\n", ModHelpers.CsWithTranslation(data.roleBase.RoleColor, data.roleBase.Role.ToString()))}";
            }
        }

        GameObject bonusTextObject = UnityEngine.Object.Instantiate(__instance.WinText.gameObject);
        bonusTextObject.transform.position = new Vector3(__instance.WinText.transform.position.x, __instance.WinText.transform.position.y - 0.8f, __instance.WinText.transform.position.z);
        bonusTextObject.transform.localScale = new Vector3(0.7f, 0.7f, 1f);
        textRenderer = bonusTextObject.GetComponent<TMPro.TMP_Text>();
        textRenderer.text = "";
        (string text, Color RoleColor, bool haison) = ProcessWinText(AdditionalTempData.gameOverReason, AdditionalTempData.winCondition);

        textRenderer.color = AdditionalTempData.winCondition is WinCondition.Haison ? Color.clear : RoleColor;

        __instance.BackgroundBar.material.SetColor("_Color", RoleColor);

        if (AdditionalTempData.winCondition == WinCondition.Haison)
        {
            __instance.WinText.text = ModTranslation.GetString("HaisonName");
            __instance.WinText.color = HaisonColor;
        }
        else if (AdditionalTempData.winCondition == WinCondition.NoWinner)
        {
            __instance.WinText.text = ModTranslation.GetString("NoWinner");
            __instance.WinText.color = Color.white;
            RoleColor = Color.white;
        }

        if (AdditionalTempData.winCondition != WinCondition.Haison)
            textRenderer.text = text;
        try
        {
            var position = Camera.main.ViewportToWorldPoint(new Vector3(0f, 1f, Camera.main.nearClipPlane));
            GameObject roleSummary = UnityEngine.Object.Instantiate(__instance.WinText.gameObject);
            roleSummary.transform.position = new Vector3(__instance.Navigation.ExitButton.transform.position.x + 0.1f, position.y - 0.1f, -14f);
            roleSummary.transform.localScale = new Vector3(1f, 1f, 1f);

            var roleSummaryText = new StringBuilder();
            roleSummaryText.AppendLine(ModTranslation.GetString("FinalResults"));

            foreach (var data in AdditionalTempData.playerRoles)
            {
                var taskInfo = data.TasksTotal > 0 ? $"<color=#FAD934FF>({data.TasksCompleted}/{data.TasksTotal})</color>" : "";
                string roleText = ModHelpers.CsWithTranslation(data.roleBase.RoleColor, data.roleBase.Role.ToString());
                //位置調整:ExR参考  by 漢方
                string result = $"{ModHelpers.Cs(Palette.PlayerColors[data.ColorId], data.PlayerName)}{data.NameSuffix}<pos=17%>{taskInfo} - <pos=27%>{ModTranslation.GetString("FinalStatus." + data.Status.ToString())} - {roleText}";
                roleSummaryText.AppendLine(result);
            }

            TMPro.TMP_Text roleSummaryTextMesh = roleSummary.GetComponent<TMPro.TMP_Text>();
            roleSummaryTextMesh.alignment = TMPro.TextAlignmentOptions.TopLeft;
            roleSummaryTextMesh.color = Color.white;
            roleSummaryTextMesh.outlineWidth *= 1.2f;
            roleSummaryTextMesh.fontSizeMin = 1.25f;
            roleSummaryTextMesh.fontSizeMax = 1.25f;
            roleSummaryTextMesh.fontSize = 1.25f;

            var roleSummaryTextMeshRectTransform = roleSummaryTextMesh.GetComponent<RectTransform>();
            roleSummaryTextMeshRectTransform.anchoredPosition = new Vector2(position.x + 3.5f, position.y - 0.1f);
            roleSummaryTextMesh.text = roleSummaryText.ToString();

        }
        catch (Exception e)
        {
            SuperNewRolesPlugin.Logger.LogInfo("エラー:" + e);
        }
        AdditionalTempData.Clear();
        OnGameEndPatch.WinText = ModHelpers.Cs(RoleColor, text);
        IsHaison = false;
    }
}

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameEnd))]
// このクラスはゲーム終了時の処理にフックするパッチクラスです。
public static class OnGameEndPatch
{
    // 勝者のプレイヤーコントロールを保持します。
    public static PlayerControl WinnerPlayer;
    // カスタムゲームオーバー理由を保持するための変数（null許容型）。
    public static CustomGameOverReason? EndData = null;
    // 勝利時に表示するテキスト（カスタムの勝利メッセージなど）。
    public static string WinText;

    // ゲーム終了直前に呼ばれるプレフィックスメソッドです。
    // このメソッドはHarmonyによって、ゲーム終了処理が実行される前に呼ばれ、
    // endGameResultの内容を加工または解析するために使用されます。
    public static void Prefix([HarmonyArgument(0)] ref EndGameResult endGameResult)
    {
        // エンドゲームの結果からゲームオーバー理由を一時データに保存します。
        AdditionalTempData.gameOverReason = endGameResult.GameOverReason;

        // ゲームオーバー理由の整数値が10以上の場合、理由をImpostorByKillに上書きします。
        if ((int)endGameResult.GameOverReason >= 10)
        {
            endGameResult.GameOverReason = GameOverReason.ImpostorByKill;
        }
    }

    public static (IEnumerable<ExPlayerControl> Winners, WinCondition winCondition, List<NetworkedPlayerInfo> WillRevivePlayers) HandleEndGameProcess(GameOverReason gameOverReason)
    {
        switch ((CustomGameOverReason)gameOverReason)
        {
            case (CustomGameOverReason)GameOverReason.HumansByTask:
            case (CustomGameOverReason)GameOverReason.HumansByVote:
            case (CustomGameOverReason)GameOverReason.HumansDisconnect:
                return (ExPlayerControl.ExPlayerControls.Where(p => p.IsAlive() && p.IsCrewmate()), WinCondition.CrewmateWin, new());
            case (CustomGameOverReason)GameOverReason.ImpostorByKill:
            case (CustomGameOverReason)GameOverReason.ImpostorBySabotage:
            case (CustomGameOverReason)GameOverReason.ImpostorByVote:
            case (CustomGameOverReason)GameOverReason.ImpostorDisconnect:
                return (ExPlayerControl.ExPlayerControls.Where(p => p.IsAlive() && p.IsImpostorWinTeam()), WinCondition.ImpostorWin, new());
            case CustomGameOverReason.Haison:
                return (ExPlayerControl.ExPlayerControls, WinCondition.Haison, new());
        }
        Logger.Error("不明なゲームオーバー理由:" + gameOverReason);
        return (null, WinCondition.None, new());
    }

    // ゲーム終了後に呼ばれるポストフィックスメソッドです。
    // このメソッドはゲーム終了後に勝者情報を確定し、各プレイヤーの状態を更新するために使用されます。
    public static void Postfix()
    {
        // 一時データからゲームオーバー理由を取得。
        var gameOverReason = AdditionalTempData.gameOverReason;
        // 次回のゲーム用に一時データをクリアします。
        AdditionalTempData.Clear();

        // ゲーム内のすべてのプレイヤーについて処理を行います。
        foreach (var p in GameData.Instance.AllPlayers)
        {
            // プレイヤーまたはそのオブジェクトがnullの場合はスキップ。
            if (p == null || p.Object == null)
                continue;

            // ExPlayerControlにキャストして、拡張された情報にアクセス可能にします。
            ExPlayerControl exPlayer = p;
            // 現在のプレイヤーのロールを取得。
            RoleId playerRole = exPlayer.Role;
            // プレイヤーのタスク進捗（完了タスク数と総タスク数）を取得。
            var (tasksCompleted, tasksTotal) = ModHelpers.TaskCompletedData(p);

            // Impostorである場合、タスク進捗をリセットして0に設定。
            if (exPlayer.IsImpostor())
            {
                tasksCompleted = 0;
                tasksTotal = 0;
            }

            // サボタージュによる死の場合：非インポスターで生存しているプレイヤーは死亡処理を実行。
            if (gameOverReason == GameOverReason.ImpostorBySabotage && !p.IsDead && !p.Role.IsImpostor)
            {
                p.IsDead = true;
                if (exPlayer.IsAlive())
                    exPlayer.FinalStatus = FinalStatus.Sabotage;
            }

            // プレイヤーの名前の接尾辞（現状は空文字）を設定し、追加の一時データにプレイヤーロール情報を保存。
            string nameSuffix = "";
            AdditionalTempData.playerRoles.Add(new AdditionalTempData.PlayerRoleInfo()
            {
                PlayerName = p.DefaultOutfit.PlayerName,
                NameSuffix = nameSuffix,
                PlayerId = p.PlayerId,
                ColorId = p.DefaultOutfit.ColorId,
                TasksTotal = tasksTotal,
                TasksCompleted = tasksCompleted,
                RoleId = playerRole,
                isImpostor = exPlayer.IsImpostor(),
                Status = exPlayer.FinalStatus,
                roleBase = exPlayer.roleBase
            });
        }

        // ゲーム終了処理を実行し、勝者情報、勝利条件、及び蘇生対象プレイヤーを取得。
        var (winners, winCondition, willRevivePlayers) = HandleEndGameProcess(gameOverReason);

        // 勝者情報をキャッシュ用変数に格納し、以降の処理や表示に利用されます。
        EndGameResult.CachedWinners = new();
        foreach (var winner in winners)
            EndGameResult.CachedWinners.Add(new(winner.Data));

        // 蘇生処理：蘇生対象のプレイヤーは死亡状態を解除する。
        foreach (NetworkedPlayerInfo player in willRevivePlayers)
            player.IsDead = false;

        // 追加データに最終的な勝利条件を保存して終了。
        AdditionalTempData.winCondition = winCondition;
    }
}