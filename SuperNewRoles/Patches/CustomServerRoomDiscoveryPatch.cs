using System;
using System.Collections;
using AmongUs.Data;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using HarmonyLib;
using SuperNewRoles.Modules;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace SuperNewRoles.Patches;

/// <summary>
/// SNR カスタムサーバーの「部屋を探す」画面に、公開ルーム状況と部屋作成導線を足す。
/// </summary>
[HarmonyPatch]
public static class CustomServerRoomDiscoveryPatch
{
    private const float UiUpdateInterval = 0.25f;
    private const float RefreshInterval = 10f;
    private const int RequestTimeoutSeconds = 5;
    private const int CreatePromptCount = 4;
    private const string GamesApiPath = "/api/games/all_for_web";

    // バニラのルーム一覧が SNR パネルと重ならないよう、縦方向に少し詰める。
    private const float ListingShiftX = 0.1f;
    private const float ListingCompressY = 0.85f;
    private const float ListingOffsetY = 0.54f;
    private const float ListingScale = 0.82f;

    // FindAGameManager は画面を開き直すたびに作り直されるので、所有者と UI を静的に保持する。
    private static FindAGameManager owner;
    private static GameObject panel;
    private static TextMeshPro summaryText;
    private static TextMeshPro recommendationText;
    private static PassiveButton regionButton;
    private static PassiveButton createButton;
    private static TextMeshPro createPromptText;
    private static string regionUrl;
    private static bool fetching;
    private static bool roomInfoReady;
    private static int lastJoinable = -1;
    private static bool listingsReady;
    private static FindAGameManager listingsOwner;
    private static float nextRefresh;
    private static bool openCreateOnReturn;
    private static IRegionInfo recommendedRegion;
    private static Vector3[] listingPositions;
    private static Vector3[] listingScales;
    private static bool listingsCompacted;
    private static float nextUiUpdate;
    private static bool panelCreateFailed;

    /// <summary>
    /// 現在のリージョンが SNR サーバーなら API のベース URL を返す。それ以外は null。
    /// </summary>
    private static string CurrentUrl()
    {
        var region = FastDestroyableSingleton<ServerManager>.Instance?.CurrentRegion;
        if (region == null)
            return null;
        if (CustomServer.SNRRegionJP != null && region.Name == CustomServer.SNRRegionJP.Name)
            return SNRURLs.SNRCS_JP;
        if (CustomServer.SNRRegionUSEast != null && region.Name == CustomServer.SNRRegionUSEast.Name)
            return SNRURLs.SNRCS_USEast;
        return null;
    }

    [HarmonyPatch(typeof(FindAGameManager), nameof(FindAGameManager.Update)), HarmonyPostfix]
    public static void Update(FindAGameManager __instance)
    {
        string url = CurrentUrl();
        ResetIfOwnerChanged(__instance);

        if (url == null)
        {
            HidePanel(__instance);
            return;
        }

        if (panel == null)
            CreatePanel(__instance);
        if (panel == null)
            return;

        // 作成導線は検索アニメのON/OFFにすぐ追従させる。それ以外の UI は間引く。
        if (Time.unscaledTime >= nextUiUpdate || regionUrl != url)
        {
            nextUiUpdate = Time.unscaledTime + UiUpdateInterval;
            panel.SetActive(true);
            SetListingLayout(__instance, compact: true);
            if (regionUrl != url)
                OnRegionChanged(url);

            UpdateRegionRecommendation(url);
            StartRefreshIfDue(__instance, url);
        }

        UpdateCreateOffer(__instance);
    }

    private static void ResetIfOwnerChanged(FindAGameManager manager)
    {
        if (owner == manager)
            return;

        DestroyPanel();
        owner = manager;
        regionUrl = null;
        fetching = false;
        roomInfoReady = false;
        lastJoinable = -1;
        listingsReady = listingsOwner == manager;
        listingPositions = null;
        listingScales = null;
        listingsCompacted = false;
        nextUiUpdate = 0;
        panelCreateFailed = false;
    }

    private static void HidePanel(FindAGameManager manager)
    {
        if (panel != null)
            panel.SetActive(false);
        SetListingLayout(manager, compact: false);
        regionUrl = null;
        roomInfoReady = false;
        lastJoinable = -1;
        HideCreateOffer();
    }

    private static void OnRegionChanged(string url)
    {
        regionUrl = url;
        nextRefresh = 0;
        roomInfoReady = false;
        lastJoinable = -1;
        listingsReady = false;
        summaryText.text = ModTranslation.GetString("RoomDiscoveryLoading");
        HideCreateOffer();
    }

    private static void UpdateRegionRecommendation(string url)
    {
        bool japanese = DataManager.Settings.Language.CurrentLanguage == SupportedLangs.Japanese;
        recommendedRegion = GetRecommendedRegion(url, japanese);
        bool show = recommendedRegion != null;
        recommendationText.gameObject.SetActive(show);
        regionButton.gameObject.SetActive(show);
        if (!show)
            return;

        string name = japanese ? "Tokyo" : "US-East";
        recommendationText.text = ModTranslation.GetString("RoomDiscoveryRecommend", name);
        regionButton.GetComponentInChildren<TextMeshPro>().text = ModTranslation.GetString("RoomDiscoverySwitch", name);
    }

    // 表示言語とサーバーが食い違っているときだけ、もう一方の SNR リージョンを勧める。
    private static IRegionInfo GetRecommendedRegion(string url, bool japanese)
    {
        if (japanese && url == SNRURLs.SNRCS_USEast)
            return CustomServer.SNRRegionJP;
        if (!japanese && url == SNRURLs.SNRCS_JP)
            return CustomServer.SNRRegionUSEast;
        return null;
    }

    private static void StartRefreshIfDue(FindAGameManager manager, string url)
    {
        if (fetching || Time.unscaledTime < nextRefresh)
            return;

        // 定期更新中は前回の件数と作成導線を残す。失敗時だけ ShowUnavailable で消す。
        fetching = true;
        nextRefresh = Time.unscaledTime + RefreshInterval;
        manager.StartCoroutine(Refresh(manager, url).WrapToIl2Cpp());
    }

    // バニラの更新ボタンの上に2行分の余白を確保する。SNR 以外では元の配置へ戻す。
    private static void SetListingLayout(FindAGameManager manager, bool compact)
    {
        if (listingsCompacted == compact || manager.gameContainers == null)
            return;

        int count = manager.gameContainers.Length;
        if (listingPositions == null)
        {
            listingPositions = new Vector3[count];
            listingScales = new Vector3[count];
            for (int i = 0; i < count; i++)
            {
                listingPositions[i] = manager.gameContainers[i].transform.localPosition;
                listingScales[i] = manager.gameContainers[i].transform.localScale;
            }
        }

        for (int i = 0; i < count; i++)
        {
            var item = manager.gameContainers[i].transform;
            var position = listingPositions[i];
            if (compact)
            {
                position.x = listingPositions[0].x + ListingShiftX;
                position.y = listingPositions[0].y + (position.y - listingPositions[0].y) * ListingCompressY + ListingOffsetY;
            }
            item.localPosition = position;
            item.localScale = compact ? listingScales[i] * ListingScale : listingScales[i];
        }
        listingsCompacted = compact;
    }

    private static IEnumerator Refresh(FindAGameManager manager, string url)
    {
        var request = UnityWebRequest.Get(url + GamesApiPath);
        request.timeout = RequestTimeoutSeconds;
        try
        {
            yield return request.SendWebRequest();
            if (manager == null || owner != manager || regionUrl != url || panel == null)
                yield break;

            if (owner == manager)
                fetching = false;

            if (request.result == UnityWebRequest.Result.Success &&
                PublicRoomSummary.TryParse(request.downloadHandler.text, out var summary))
                ShowSummary(manager, summary);
            else
                ShowUnavailable();
        }
        finally
        {
            request.Dispose();
            if (owner == manager)
                fetching = false;
        }
    }

    private static void ShowSummary(FindAGameManager manager, PublicRoomSummary summary)
    {
        roomInfoReady = true;
        lastJoinable = summary.Joinable;
        summaryText.text = ModTranslation.GetString("RoomDiscoveryBusy", summary.Busy);
        UpdateCreateOffer(manager);
    }

    private static void ShowUnavailable()
    {
        roomInfoReady = false;
        lastJoinable = -1;
        summaryText.text = ModTranslation.GetString("RoomDiscoveryUnavailable");
        HideCreateOffer();
    }

    // バニラの部屋検索中や、一覧に部屋が出ている間は作成導線を出さない。
    // RefreshList 自体はクールダウンで空振りすることがあるので、実際の検索中は animLoad を見る。
    private static bool ShouldShowCreate(FindAGameManager manager)
    {
        if (!roomInfoReady || lastJoinable != 0 || !listingsReady)
            return false;
        if (manager == null || IsVanillaSearching(manager) || HasVisibleListings(manager))
            return false;
        return true;
    }

    private static bool IsVanillaSearching(FindAGameManager manager)
    {
        return manager.animLoad != null && manager.animLoad.activeSelf;
    }

    private static bool HasVisibleListings(FindAGameManager manager)
    {
        if (manager.gameContainers == null)
            return false;
        foreach (var container in manager.gameContainers)
        {
            if (container != null && container.gameObject.activeSelf)
                return true;
        }
        return false;
    }

    private static void UpdateCreateOffer(FindAGameManager manager)
    {
        if (createButton == null)
            return;

        bool show = ShouldShowCreate(manager);
        bool wasShown = createButton.gameObject.activeSelf;
        createButton.gameObject.SetActive(show);
        if (createPromptText == null)
            return;

        createPromptText.gameObject.SetActive(show);
        if (!show || wasShown)
            return;

        int messageIndex = ModHelpers.GetRandomInt(1, CreatePromptCount);
        createPromptText.text = ModTranslation.GetString($"RoomDiscoveryCreatePrompt{messageIndex}");
    }

    private static void HideCreateOffer()
    {
        if (createButton != null)
            createButton.gameObject.SetActive(false);
        if (createPromptText != null)
            createPromptText.gameObject.SetActive(false);
    }

    [HarmonyPatch(typeof(FindAGameManager), nameof(FindAGameManager.HandleList)), HarmonyPostfix]
    public static void HandleListPostfix(FindAGameManager __instance)
    {
        listingsOwner = __instance;
        listingsReady = true;
        if (owner == __instance)
            UpdateCreateOffer(__instance);
    }

    private static void CreatePanel(FindAGameManager manager)
    {
        if (panelCreateFailed || manager.TotalText == null || manager.container == null)
            return;

        try
        {
            panel = new GameObject("SNRRoomDiscovery");
            // バニラのコンテナに付けて、スライド演出と表示状態を追従させる。
            panel.transform.SetParent(manager.container, false);

            summaryText = AddText(manager, "StartedRooms", new Vector3(0.1f, -2.12f, -5), 1.6f);
            summaryText.rectTransform.sizeDelta = new Vector2(4.8f, 0.4f);

            recommendationText = AddText(manager, "RegionRecommendation", new Vector3(0.1f, -1.34f, -5), 1.4f);
            recommendationText.rectTransform.sizeDelta = new Vector2(5.6f, 0.4f);

            regionButton = AddButton("SwitchRegion", new Vector3(0.1f, -1.69f, -5), SwitchToRecommendedRegion, scaleOverride: 0.35f);
            createButton = AddButton("CreateRoom", new Vector3(0.1f, 0, -5), OpenCreateGame, 2.4f);
            createButton.GetComponentInChildren<TextMeshPro>().text = ModTranslation.GetString("RoomDiscoveryCreate");
            createButton.gameObject.SetActive(false);

            createPromptText = AddText(manager, "CreateRoomPrompt", new Vector3(0.1f, 0.5f, -5), 1.2f);
            createPromptText.transform.localScale = Vector3.one * 2f;
            createPromptText.rectTransform.sizeDelta = new Vector2(5.8f, 0.5f);
            createPromptText.gameObject.SetActive(false);

            var discord = AddButton(
                "MatchmakingDiscord",
                new Vector3(2.9f, -2.62f, -5),
                () => Constants.OpenURL(SocialLinks.DiscordServer),
                2.4f,
                useDiscordTemplate: true);
            discord.GetComponentInChildren<TextMeshPro>().text = ModTranslation.GetString("RoomDiscoveryDiscord");
        }
        catch (Exception e)
        {
            Logger.Error($"CustomServerRoomDiscovery UI construction failed: {e}");
            DestroyPanel();
            panelCreateFailed = true;
        }
    }

    private static void DestroyPanel()
    {
        if (panel != null)
            UnityEngine.Object.Destroy(panel);
        panel = null;
        summaryText = null;
        recommendationText = null;
        regionButton = null;
        createButton = null;
        createPromptText = null;
    }

    private static void SwitchToRecommendedRegion()
    {
        if (recommendedRegion == null || owner == null)
            return;

        FastDestroyableSingleton<ServerManager>.Instance.SetRegion(recommendedRegion);
        owner.SetCurrentServer();
        // マッチメイキングの待ち時間を捨てて、切り替え先の参加可能ルームをすぐ取る。
        owner.SetRefresh(true);
        owner.ResetTimer();
        owner.RefreshList();
    }

    private static void OpenCreateGame()
    {
        if (owner == null || owner.animating)
            return;

        // FindAGame から直接部屋作成画面へは戻れないので、メインメニュー経由で開く。
        openCreateOnReturn = true;
        owner.ExitGame();
    }

    private static TextMeshPro AddText(FindAGameManager manager, string name, Vector3 position, float size)
    {
        var text = UnityEngine.Object.Instantiate(manager.TotalText, panel.transform);
        text.name = name;

        // バニラの翻訳・アスペクト追従が SNR 文言を上書きしないよう外す。
        var translator = text.GetComponent<TextTranslatorTMP>();
        if (translator != null)
            UnityEngine.Object.Destroy(translator);
        var aspect = text.GetComponent<AspectPosition>();
        if (aspect != null)
        {
            aspect.enabled = false;
            UnityEngine.Object.Destroy(aspect);
        }

        text.transform.localPosition = position;
        text.transform.localScale = Vector3.one;
        text.rectTransform.sizeDelta = new Vector2(8, 0.5f);
        text.fontSize = size;
        text.enableAutoSizing = false;
        text.alignment = TextAlignmentOptions.Center;
        text.text = string.Empty;
        text.gameObject.SetActive(true);
        return text;
    }

    private static PassiveButton AddButton(
        string name,
        Vector3 position,
        Action action,
        float width = 2.4f,
        float height = 0.44f,
        bool useDiscordTemplate = false,
        float? scaleOverride = null)
    {
        Transform template = useDiscordTemplate
            ? AssetManager.GetAsset<GameObject>("BugReport_Top").transform.Find("Buttons/Button_Discord")
            : AssetManager.GetAsset<GameObject>("AnalyticsBG").transform.Find("AnalyticsButton");
        var clone = UnityEngine.Object.Instantiate(template.gameObject, panel.transform);
        clone.name = name;
        clone.transform.localPosition = position;

        var collider = clone.GetComponent<BoxCollider2D>();
        float scale = scaleOverride ?? Mathf.Min(width / collider.size.x, height / collider.size.y);
        clone.transform.localScale = Vector3.one * scale;

        var label = clone.transform.Find("Text").GetComponent<TextMeshPro>();
        label.enableAutoSizing = true;
        float labelScale = scale * label.transform.localScale.x;
        label.fontSizeMin = 1.1f / labelScale;
        label.fontSizeMax = 1.5f / labelScale;
        label.color = Color.white;

        var selected = clone.transform.Find("Selected").gameObject;
        selected.SetActive(false);

        var button = clone.GetComponent<PassiveButton>() ?? clone.AddComponent<PassiveButton>();
        button.Colliders = new Collider2D[] { collider };
        button.OnClick = new();
        button.OnClick.AddListener((UnityAction)(() => action()));
        button.OnMouseOver = new();
        button.OnMouseOver.AddListener((UnityAction)(() => selected.SetActive(true)));
        button.OnMouseOut = new();
        button.OnMouseOut.AddListener((UnityAction)(() => selected.SetActive(false)));
        clone.SetActive(true);
        return button;
    }

    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.LateUpdate)), HarmonyPostfix]
    public static void OpenCreate(MainMenuManager __instance)
    {
        if (!openCreateOnReturn || !__instance.finishStartup || __instance.animating)
            return;

        openCreateOnReturn = false;
        __instance.OpenOnlineMenu();
        __instance.OpenCreateGame();
    }
}
