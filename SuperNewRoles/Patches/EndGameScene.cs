using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using SuperNewRoles.CustomCosmetics.CosmeticsPlayer;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles;
using SuperNewRoles.Roles.Neutral;
using SuperNewRoles.SuperTrophies;
using UnityEngine;
using SuperNewRoles.Events;
using SuperNewRoles.HelpMenus;
using UnityEngine.UI;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using SuperNewRoles.Roles.Modifiers;
using TMPro;

namespace SuperNewRoles.Patches;

public enum WinCondition
{
    None,
    CrewmateWin,
    JackalWin,
    ImpostorWin,
    Haison = CustomGameOverReason.Haison,
    NoWinner = CustomGameOverReason.NoWinner,
    // カスタム勝利条件のために予約
    TunaWin,
    TeruteruWin,
    OpportunistWin,
    WorkpersonWin,
    VultureWin,
    PavlovsWin,
    ArsonistWin,
    FalseChargesWin,
    HitmanWin,
    OwlWin,
    LoversWin,
    LoversBreakerWin,
}
public enum CustomGameOverReason
{
    None = 30,
    Haison,
    NoWinner,
    JackalWin,
    TunaWin,
    TeruteruWin,
    WorkpersonWin,
    VultureWin,
    PavlovsWin,
    ArsonistWin,
    FalseChargesWin,
    HitmanWin,
    GodWin,
    LoversWin,
    LoversBreakerWin,
    OwlWin,
    BlackHatHackerWin,
    SpelunkerWin,
}

static class AdditionalTempData
{
    private static readonly object _lock = new object();

    private static List<PlayerRoleInfo> _playerRoles = new();
    private static List<WinCondition> _additionalWinConditions = new();

    public static GameOverReason gameOverReason { get; set; }
    public static WinCondition winCondition { get; set; } = WinCondition.None;

    public static IReadOnlyList<PlayerRoleInfo> playerRoles => _playerRoles;
    public static IReadOnlyList<WinCondition> additionalWinConditions => _additionalWinConditions;

    public static void AddPlayerRole(PlayerRoleInfo roleInfo)
    {
        lock (_lock)
        {
            _playerRoles.Add(roleInfo);
        }
    }

    public static void AddAdditionalWinCondition(WinCondition condition)
    {
        lock (_lock)
        {
            if (!_additionalWinConditions.Contains(condition))
            {
                _additionalWinConditions.Add(condition);
            }
        }
    }

    public static void Clear()
    {
        lock (_lock)
        {
            _playerRoles.Clear();
            _additionalWinConditions.Clear();
            winCondition = WinCondition.None;
        }
    }

    internal class PlayerRoleInfo
    {
        public string PlayerName { get; set; }
        public string NameSuffix { get; set; }
        public IRoleBase roleBase { get; set; }
        public IGhostRoleBase ghostRoleBase { get; set; }
        public List<IModifierBase> modifierRoleBases { get; set; }
        public List<string> modifierMarks { get; set; }
        public string RoleString { get; set; }
        public int TasksCompleted { get; set; }
        public int TasksTotal { get; set; }
        public int PlayerId { get; set; }
        public int ColorId { get; set; }
        public float additionalSize { get; set; }
        public FinalStatus Status { get; internal set; }
        public RoleId RoleId { get; set; }
        public GhostRoleId GhostRoleId { get; set; }
        public ModifierRoleId ModifierRoleId { get; set; }
        public string AttributeRoleName { get; set; }
        public bool isImpostor { get; set; }
        public string Hat2Id { get; set; }
        public string Visor2Id { get; set; }
        public Color? LoversHeartColor { get; set; }
        public PlayerRoleInfo Clone()
        {
            return (PlayerRoleInfo)MemberwiseClone();
        }
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

public class EndGameCondition
{
    public GameOverReason reason;
    public List<byte> winners;
    public string UpperText;
    public Color UpperTextColor;
    public bool IsHaison;
    public string winText;
    public List<string> additionalWinTexts;
    public EndGameCondition(GameOverReason reason, List<byte> winners, string UpperText, List<string> additionalWinTexts, Color UpperTextColor, bool IsHaison, string winText = "WinText")
    {
        this.reason = reason;
        this.winners = winners;
        this.UpperText = UpperText;
        this.UpperTextColor = UpperTextColor;
        this.IsHaison = IsHaison;
        this.winText = winText;
        this.additionalWinTexts = additionalWinTexts;
    }
}
[HarmonyPatch(typeof(EndGameManager), nameof(EndGameManager.SetEverythingUp))]
public class EndGameManagerSetUpPatch
{
    public static GameObject fadeObject;
    [CustomRPC]
    public static void RpcEndGameWithCondition(GameOverReason reason, List<byte> winners, string UpperText, List<string> additionalWinTexts, Color UpperTextColor, bool IsHaison, string winText = "WinText")
    {
        if (fadeObject != null) return;
        EndGameCondition newCond = new(
            reason: reason,
            winners: winners,
            UpperText: ModTranslation.TryGetString(UpperText, out var value) ? value : "<INVALID_TEXT>",
            additionalWinTexts: additionalWinTexts.Select(x => ModTranslation.TryGetString(x, out var val) ? val : "<INVALID_TEXT>").ToList(),
            UpperTextColor: UpperTextColor,
            IsHaison: IsHaison,
            winText: ModTranslation.TryGetString(winText, out var winVal) ? winVal : "<INVALID_TEXT>"
        );
        EndGameManagerSetUpPatch.endGameCondition = newCond;

        // 黒いフェードアウト用オブジェクトを作成
        fadeObject = GameObject.Instantiate(AssetManager.GetAsset<GameObject>("EndgameOverlay"), HudManager.Instance.transform);
        fadeObject.transform.localPosition = new(0, 0, -400f);
        HudManager.Instance.StartCoroutine(FadeOutCoroutine(fadeObject.GetComponent<SpriteRenderer>(), 0.5f, () =>
        {
        }).WrapToIl2Cpp());

        if (AmongUsClient.Instance.AmHost)
            new LateTask(() => GameManager.Instance.RpcEndGame(newCond.reason, false), 0.3f);
    }

    // Imageのアルファ値を変更するコルーチン
    public static System.Collections.IEnumerator FadeOutCoroutine(SpriteRenderer image, float duration, Action onComplete)
    {
        float elapsed = 0f;
        image.color = new Color(0, 0, 0, 0); // 開始時は透明
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsed / duration);
            image.color = new Color(0, 0, 0, alpha); // 徐々に不透明に
            yield return null;
        }
        image.color = new Color(0, 0, 0, 1); // 完全に不透明に
        onComplete?.Invoke();
    }

    public static TMPro.TMP_Text textRenderer;
    public static EndGameCondition endGameCondition;
    [HarmonyPatch(typeof(EndGameNavigation), nameof(EndGameNavigation.ShowProgression))]
    public class ShowProgressionPatch
    {
        public static void Prefix()
        {
            if (textRenderer != null)
            {
                textRenderer.gameObject.SetActive(false);
            }
            foreach (var textMeshPro in textMeshPros)
            {
                textMeshPro.gameObject.SetActive(false);
            }
        }
    }
    #region ProcessWinText

    private static List<TextMeshPro> textMeshPros = new();
    static Color32 HaisonColor = new(163, 163, 162, byte.MaxValue);
    public static (string text, Color color, bool isHaison) ProcessWinText(WinCondition winCondition)
    {
        string baseText;
        Color roleColor;
        switch (winCondition)
        {
            case WinCondition.CrewmateWin:
                baseText = "Crewmate";
                roleColor = Palette.White;
                break;
            case WinCondition.ImpostorWin:
                baseText = "Impostor";
                roleColor = Palette.ImpostorRed;
                break;
            case WinCondition.Haison:
                baseText = "Haison";
                roleColor = HaisonColor;
                break;
            case WinCondition.NoWinner:
                baseText = "NoWinner";
                roleColor = Color.white;
                break;
            case WinCondition.JackalWin:
                baseText = "Jackal";
                roleColor = Jackal.Instance.RoleColor;
                break;
            case WinCondition.TunaWin:
                baseText = "Tuna";
                roleColor = Tuna.Instance.RoleColor;
                break;
            case WinCondition.TeruteruWin:
                baseText = "Teruteru";
                roleColor = Teruteru.Instance.RoleColor;
                break;
            case WinCondition.OpportunistWin:
                baseText = "Opportunist";
                roleColor = Opportunist.Instance.RoleColor;
                break;
            case WinCondition.WorkpersonWin:
                baseText = "Workperson";
                roleColor = Workperson.Instance.RoleColor;
                break;
            case WinCondition.VultureWin:
                baseText = "Vulture";
                roleColor = Vulture.Instance.RoleColor;
                break;
            case WinCondition.PavlovsWin:
                baseText = "Pavlovs";
                roleColor = PavlovsDog.Instance.RoleColor;
                break;
            case WinCondition.ArsonistWin:
                baseText = "Arsonist";
                roleColor = Arsonist.Instance.RoleColor;
                break;
            case WinCondition.FalseChargesWin:
                baseText = "FalseCharges";
                roleColor = FalseCharges.Instance.RoleColor;
                break;
            case WinCondition.HitmanWin:
                baseText = "Hitman";
                roleColor = Hitman.Instance.RoleColor;
                break;
            case WinCondition.OwlWin:
                baseText = "Owl";
                roleColor = Owl.Instance.RoleColor;
                break;
            case WinCondition.LoversWin:
                baseText = "Lovers";
                roleColor = Lovers.Instance.RoleColor;
                break;
            case WinCondition.LoversBreakerWin:
                baseText = "LoversBreaker";
                roleColor = LoversBreaker.Instance.RoleColor;
                break;
            default:
                baseText = "Unknown";
                roleColor = Color.white;
                break;
        }
        bool isHaison = winCondition == WinCondition.Haison;
        string translated = ModTranslation.GetString(baseText);

        if (isHaison)
            translated = ModTranslation.GetString("Haison");
        else if (translated == ModTranslation.GetString("NoWinner"))
            translated = ModTranslation.GetString("NoWinnerText");
        else if (translated == ModTranslation.GetString("GodName"))
            translated = $"{translated} {ModTranslation.GetString("GodWinText")}";
        else
            translated = $"{translated}";

        return (translated, roleColor, isHaison);
    }
    #endregion

    public static void Postfix(EndGameManager __instance)
    {
        CreatePlayerObjects(__instance);

        // ボーナステキストの設定
        GameObject bonusTextObject = UnityEngine.Object.Instantiate(__instance.WinText.gameObject);
        bonusTextObject.transform.position = new Vector3(__instance.WinText.transform.position.x, __instance.WinText.transform.position.y - 0.8f, __instance.WinText.transform.position.z);
        bonusTextObject.transform.localScale = new Vector3(0.7f, 0.7f, 1f);
        textRenderer = bonusTextObject.GetComponent<TMPro.TMP_Text>();
        textRenderer.text = "";

        if (AdditionalTempData.winCondition == WinCondition.Haison)
        {
            __instance.WinText.text = ModTranslation.GetString("Haison");
            __instance.WinText.color = HaisonColor;
        }
        else if (AdditionalTempData.winCondition == WinCondition.NoWinner)
        {
            __instance.WinText.text = ModTranslation.GetString("NoWinner");
            __instance.WinText.color = Color.white;
        }

        CreateRoleSummary(__instance);

        // static endGameCondition があれば UI を上書き
        if (endGameCondition != null)
        {
            if (endGameCondition.IsHaison)
            {
                __instance.WinText.text = ModTranslation.GetString("Haison");
                __instance.WinText.color = HaisonColor;
            }

            textRenderer.color = endGameCondition.IsHaison ? Color.clear : endGameCondition.UpperTextColor;
            // UpperText の後ろに追加のテキストがあれば結合
            var upperText = endGameCondition.UpperText;
            if (endGameCondition.additionalWinTexts != null && endGameCondition.additionalWinTexts.Any())
            {
                upperText += " & " + string.Join(" & ", endGameCondition.additionalWinTexts);
            }
            textRenderer.text = upperText + " " + endGameCondition.winText;
            __instance.BackgroundBar.material.SetColor("_Color", endGameCondition.UpperTextColor);
        }

        AdditionalTempData.Clear();

        // トロフィー処理を実行
        SuperTrophyManager.OnEndGame();
    }

    private static void CreatePlayerObjects(EndGameManager instance)
    {
        textMeshPros.Clear();
        // 既存のプレイヤーオブジェクトを削除
        foreach (PoolablePlayer pb in instance.transform.GetComponentsInChildren<PoolablePlayer>())
        {
            UnityEngine.Object.Destroy(pb.gameObject);
        }

        int totalCount = Mathf.CeilToInt(7.5f);
        IEnumerable<CachedPlayerData> winners = EndGameResult.CachedWinners.ToArray().OrderBy(b => !b.IsYou ? 0 : -1);
        int i = -1;
        foreach (CachedPlayerData data in winners)
        {
            i++;
            int direction = (i % 2 == 0) ? -1 : 1;
            int index = (i + 1) / 2;
            float factor = (float)index / totalCount;
            float scaleFactor = Mathf.Lerp(1f, 0.75f, factor);
            float zOffset = (i == 0) ? -8f : -1f;

            PoolablePlayer playerObj = UnityEngine.Object.Instantiate(instance.PlayerPrefab, instance.transform);
            playerObj.transform.localPosition = new Vector3(direction * index * scaleFactor, FloatRange.SpreadToEdges(-1.125f, 0f, index, totalCount), zOffset + index * 0.01f) * 0.9f;

            float playerScale = Mathf.Lerp(1f, 0.65f, factor) * 0.9f;
            Vector3 scaleVector = new Vector3(playerScale, playerScale, 1f);
            playerObj.transform.localScale = scaleVector;

            if (data.IsDead)
            {
                playerObj.SetBodyAsGhost();
                playerObj.SetDeadFlipX(i % 2 == 0);
            }
            else
            {
                playerObj.SetFlipX(i % 2 == 0);
            }

            playerObj.UpdateFromPlayerOutfit(data.Outfit, PlayerMaterial.MaskType.None, data.IsDead, true);
            playerObj.cosmetics.nameText.transform.localScale = new Vector3(1f / scaleVector.x, 1f / scaleVector.y, 1f / scaleVector.z);
            playerObj.cosmetics.nameText.transform.localPosition = new Vector3(playerObj.cosmetics.nameText.transform.localPosition.x, playerObj.cosmetics.nameText.transform.localPosition.y - 0.8f, -150f);
            TextMeshPro nameText = GameObject.Instantiate(playerObj.cosmetics.nameText, null);
            nameText.color = Color.white;
            nameText.text = data.PlayerName;
            nameText.transform.position = playerObj.cosmetics.nameText.transform.position;
            textMeshPros.Add(nameText);
            playerObj.cosmetics.nameText.gameObject.SetActive(false);

            CustomCosmeticsLayer customCosmeticsLayer = CustomCosmeticsLayers.ExistsOrInitialize(playerObj.cosmetics);

            foreach (var roleInfo in AdditionalTempData.playerRoles)
            {
                if (roleInfo.PlayerName != data.PlayerName) continue;
                string roleText = ModHelpers.CsWithTranslation(roleInfo.roleBase.RoleColor, roleInfo.roleBase.Role.ToString());
                if (roleInfo.GhostRoleId != GhostRoleId.None)
                    roleText = $"{ModHelpers.CsWithTranslation(roleInfo.ghostRoleBase.RoleColor, roleInfo.GhostRoleId.ToString())} ({roleText}) ";
                if (roleInfo.modifierMarks.Count > 0)
                    roleText += " ";
                foreach (var modifier in roleInfo.modifierMarks)
                {
                    roleText = modifier.Replace("{0}", roleText);
                }
                string playerName = ModHelpers.Cs(Palette.PlayerColors[roleInfo.ColorId], roleInfo.PlayerName);
                if (roleInfo.LoversHeartColor != null)
                    playerName += ModHelpers.Cs(roleInfo.LoversHeartColor.Value, " ♥");
                nameText.text = $"{playerName}{roleInfo.NameSuffix}\n{string.Join("\n", roleText)}";
                customCosmeticsLayer.hat2?.SetHat(roleInfo.Hat2Id, roleInfo.ColorId);
                customCosmeticsLayer.visor2?.SetVisor(roleInfo.Visor2Id, roleInfo.ColorId);
                playerObj.transform.localScale *= roleInfo.additionalSize;
                playerObj.cosmetics.nameTextContainer.transform.localScale /= roleInfo.additionalSize;

                // BodyBuilderのポージング表示
                if (roleInfo.RoleId == RoleId.BodyBuilder && roleInfo.TasksCompleted >= roleInfo.TasksTotal)
                {
                    var posingId = (byte)UnityEngine.Random.Range(1, 5); // ランダムなポーズ
                    var prefab = AssetManager.GetAsset<GameObject>($"BodyBuilderAnim0{posingId}.prefab");
                    if (prefab != null)
                    {
                        var pose = UnityEngine.Object.Instantiate(prefab, playerObj.transform);
                        pose.gameObject.transform.position = playerObj.transform.position;
                        pose.transform.localPosition = new Vector3(0f, 1f, 0f);
                        pose.transform.localScale *= 1.5f;

                        // プレイヤーを非表示にする
                        playerObj.cosmetics.gameObject.SetActive(false);
                        if (playerObj.cosmetics.currentBodySprite?.BodySprite != null)
                            playerObj.cosmetics.currentBodySprite.BodySprite.gameObject.SetActive(false);

                        // ポーズのスプライトレンダラーを設定
                        var spriteRenderer = pose.GetComponent<SpriteRenderer>();
                        if (spriteRenderer != null)
                        {
                            spriteRenderer.sharedMaterial = FastDestroyableSingleton<HatManager>.Instance.PlayerMaterial;
                            spriteRenderer.maskInteraction = SpriteMaskInteraction.None;
                            PlayerMaterial.SetMaskLayerBasedOnLocalPlayer(spriteRenderer, false);
                            PlayerMaterial.SetColors(roleInfo.ColorId, spriteRenderer);
                            spriteRenderer.color = new Color(1f, 1f, 1f, roleInfo.Status != FinalStatus.Alive ? 0.5f : 1f);
                        }
                    }
                }

                if (roleInfo.ModifierRoleId.HasFlag(ModifierRoleId.JumboModifier))
                {
                    playerObj.transform.localPosition += new Vector3(0, 0.7f, 0f);
                    playerObj.cosmetics.nameTextContainer.transform.localPosition = new(0.2f, -0.2f, 0f);
                }
            }
        }
    }

    private static void CreateRoleSummary(EndGameManager instance)
    {
        try
        {
            var position = Camera.main.ViewportToWorldPoint(new Vector3(0f, 1f, Camera.main.nearClipPlane));
            GameObject roleSummary = UnityEngine.Object.Instantiate(instance.WinText.gameObject);
            roleSummary.transform.position = new Vector3(instance.Navigation.ExitButton.transform.position.x + 0.1f, position.y - 0.1f, -14f);
            roleSummary.transform.localScale = new Vector3(1f, 1f, 1f);

            var summaryBuilder = new StringBuilder();
            summaryBuilder.AppendLine(ModTranslation.GetString("FinalResults"));

            foreach (var roleInfo in AdditionalTempData.playerRoles)
            {
                var taskInfo = roleInfo.TasksTotal > 0 ? $"<color=#FAD934FF>({roleInfo.TasksCompleted}/{roleInfo.TasksTotal})</color>" : "";
                string roleText = ModHelpers.CsWithTranslation(roleInfo.roleBase.RoleColor, roleInfo.roleBase.Role.ToString());
                if (roleInfo.modifierMarks.Count > 0)
                    roleText += " ";
                foreach (var modifier in roleInfo.modifierMarks)
                    roleText = modifier.Replace("{0}", roleText);
                string playerName = ModHelpers.Cs(Palette.PlayerColors[roleInfo.ColorId], roleInfo.PlayerName);
                if (roleInfo.LoversHeartColor != null)
                    playerName += ModHelpers.Cs(roleInfo.LoversHeartColor.Value, " ♥");
                string result = $"{playerName}{roleInfo.NameSuffix}<pos=17%>{taskInfo} - <pos=27%>{ModTranslation.GetString("FinalStatus." + roleInfo.Status)} - {roleText}";
                summaryBuilder.AppendLine(result);
            }

            TMPro.TMP_Text summaryTextMesh = roleSummary.GetComponent<TMPro.TMP_Text>();
            summaryTextMesh.alignment = TMPro.TextAlignmentOptions.TopLeft;
            summaryTextMesh.color = Color.white;
            summaryTextMesh.outlineWidth *= 1.2f;
            summaryTextMesh.fontSizeMin = 1.25f;
            summaryTextMesh.fontSizeMax = 1.25f;
            summaryTextMesh.fontSize = 1.25f;

            var rectTransform = summaryTextMesh.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = new Vector2(position.x + 3.5f, position.y - 0.1f);
            summaryTextMesh.text = summaryBuilder.ToString();
        }
        catch (Exception e)
        {
            SuperNewRolesPlugin.Logger.LogInfo("エラー:" + e);
        }
    }
}

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameEnd))]
// このクラスはゲーム終了時の処理にフックするパッチクラスです。
public static class OnGameEndPatch
{
    public static void Prefix([HarmonyArgument(0)] ref EndGameResult endGameResult)
    {
        // エンドゲームの結果からゲームオーバー理由を一時データに保存します。
        AdditionalTempData.gameOverReason = endGameResult.GameOverReason;
        if (AdditionalTempData.gameOverReason == (GameOverReason)CustomGameOverReason.Haison)
            endGameResult.GameOverReason = GameOverReason.ImpostorDisconnect;
        // ゲームオーバー理由の整数値が10以上の場合、理由をImpostorsByKillに上書きします。
        else if ((int)endGameResult.GameOverReason >= 9)
            endGameResult.GameOverReason = GameOverReason.ImpostorsByKill;
    }

    // ゲーム終了後に呼ばれるポストフィックスメソッドです。
    // このメソッドはゲーム終了後に勝者情報を確定し、各プレイヤーの状態を更新するために使用されます。
    public static void Postfix()
    {
        GameOverReason gameOverReason = AdditionalTempData.gameOverReason;
        AdditionalTempData.Clear();

        CollectPlayerRoleData(gameOverReason);

        EndGameResult.CachedWinners = new Il2CppSystem.Collections.Generic.List<CachedPlayerData>();
        foreach (var winner in EndGameManagerSetUpPatch.endGameCondition.winners)
            EndGameResult.CachedWinners.Add(new CachedPlayerData(GameData.Instance.GetPlayerById(winner)));

        AdditionalTempData.gameOverReason = gameOverReason;

        EndGameEvent.Invoke(AdditionalTempData.gameOverReason, EndGameManagerSetUpPatch.endGameCondition.winners.Select(x => ExPlayerControl.ById(x)).ToList());
    }

    private static void CollectPlayerRoleData(GameOverReason gameOverReason)
    {
        foreach (var p in GameData.Instance.AllPlayers)
        {
            if (!IsValidPlayer(p)) continue;

            var playerInfo = CreatePlayerInfo(p, gameOverReason);
            AdditionalTempData.AddPlayerRole(playerInfo);
        }
    }

    private static bool IsValidPlayer(NetworkedPlayerInfo player)
    {
        return player != null && (player.Object != null || player.Disconnected);
    }

    private static AdditionalTempData.PlayerRoleInfo CreatePlayerInfo(NetworkedPlayerInfo player, GameOverReason gameOverReason)
    {
        (int tasksCompleted, int tasksTotal) = (0, 0);
        RoleId roleId = RoleId.None;
        ModifierRoleId modifierRoleId = ModifierRoleId.None;
        GhostRoleId ghostRoleId = GhostRoleId.None;
        bool isImpostor = false;
        FinalStatus status = FinalStatus.Alive;
        string hat2Id = "";
        string visor2Id = "";
        float additionalSize = 1f;
        Color? loversHeartColor = null;
        List<string> modifierMarks = [];

        if (player.Disconnected)
        {
            DisconnectedResultSaver.DisconnectedData data = DisconnectedResultSaver.Instance.GetDisconnectedData(player.PlayerId);
            (tasksCompleted, tasksTotal) = data.Tasks;
            roleId = data.RoleId;
            modifierRoleId = data.ModifierRoleId;
            ghostRoleId = data.GhostRoleId;
            isImpostor = data.IsImpostor;
            status = FinalStatus.Disconnect;
            hat2Id = data.Hat2Id;
            visor2Id = data.Visor2Id;
        }
        else
        {
            ExPlayerControl exPlayer = player;
            (tasksCompleted, tasksTotal) = GetPlayerTaskInfo(exPlayer);
            UpdatePlayerStatusForSabotage(player, exPlayer, gameOverReason);
            roleId = exPlayer.Role;
            modifierRoleId = exPlayer.ModifierRole;
            ghostRoleId = exPlayer.GhostRole;
            isImpostor = exPlayer.IsImpostor();
            status = exPlayer.FinalStatus;
            CustomCosmeticsLayer customCosmeticsLayer = CustomCosmeticsLayers.ExistsOrInitialize(player.Object.cosmetics);
            hat2Id = customCosmeticsLayer?.hat2?.Hat?.ProdId ?? "";
            visor2Id = customCosmeticsLayer?.visor2?.Visor?.ProdId ?? "";
            loversHeartColor = exPlayer.TryGetAbility<LoversAbility>(out var loversAbility) ? loversAbility.HeartColor : null;
            foreach (var modifier in exPlayer.ModifierRoleBases)
            {
                modifierMarks.Add(modifier.ModifierMark(exPlayer));
            }
        }
        additionalSize *= modifierRoleId.HasFlag(ModifierRoleId.JumboModifier) ? 2f : 1f;

        return new AdditionalTempData.PlayerRoleInfo()
        {
            PlayerName = player.DefaultOutfit.PlayerName,
            NameSuffix = "",
            PlayerId = player.PlayerId,
            ColorId = player.DefaultOutfit.ColorId,
            TasksTotal = tasksTotal,
            TasksCompleted = tasksCompleted,
            RoleId = roleId,
            GhostRoleId = ghostRoleId,
            ModifierRoleId = modifierRoleId,
            isImpostor = isImpostor,
            Status = status,
            roleBase = CustomRoleManager.TryGetRoleById(roleId, out var role) ? role : null,
            ghostRoleBase = CustomRoleManager.TryGetGhostRoleById(ghostRoleId, out var ghostRole) ? ghostRole : null,
            modifierRoleBases = modifierRoleId != ModifierRoleId.None ? CustomRoleManager.TryGetModifierById(modifierRoleId, out var modifierRole) ? new List<IModifierBase> { modifierRole } : new List<IModifierBase>() : new List<IModifierBase>(),
            modifierMarks = modifierMarks,
            Hat2Id = hat2Id,
            Visor2Id = visor2Id,
            additionalSize = additionalSize,
            LoversHeartColor = loversHeartColor,
        };
    }

    private static (int completed, int total) GetPlayerTaskInfo(ExPlayerControl player)
    {
        if (player.IsImpostor())
        {
            return (0, 0);
        }
        return ModHelpers.TaskCompletedData(player.Data);
    }

    private static void UpdatePlayerStatusForSabotage(NetworkedPlayerInfo player, ExPlayerControl exPlayer, GameOverReason gameOverReason)
    {
        if (gameOverReason == GameOverReason.ImpostorsBySabotage && !player.IsDead && !player.Role.IsImpostor)
        {
            player.IsDead = true;
            if (exPlayer.IsAlive())
            {
                exPlayer.FinalStatus = FinalStatus.Sabotage;
            }
        }
    }
}