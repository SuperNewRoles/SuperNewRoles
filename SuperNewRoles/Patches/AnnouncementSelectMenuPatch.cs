using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using AmongUs.Data;
using Assets.InnerNet;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using HarmonyLib;
using SuperNewRoles;
using SuperNewRoles.Modules;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

namespace SuperNewRoles.Patches;

[HarmonyPatch(typeof(AnnouncementPopUp), nameof(AnnouncementPopUp.OnEnable))]
public static class AnnouncementPopUpOnEnablePatch
{
    public static void Postfix(AnnouncementPopUp __instance)
    {
        AnnouncementSelectMenuHelper.EnsureMenu(__instance);
        // 未読バッジを更新
        __instance.StartCoroutine(AnnouncementSelectMenuHelper.UpdateUnreadBadgesDelayed(__instance).WrapToIl2Cpp());
    }
}

[HarmonyPatch(typeof(AnnouncementPopUp), nameof(AnnouncementPopUp.OnDisable))]
public static class AnnouncementPopUpOnDisablePatch
{
    public static void Postfix(AnnouncementPopUp __instance)
    {
        AnnouncementSelectMenuHelper.SetMenuActive(__instance, false);
    }
}

[HarmonyPatch(typeof(AnnouncementPopUp), nameof(AnnouncementPopUp.SetMenu))]
public static class AnnouncementPopUpSetMenuPatch
{
    public static void Postfix(AnnouncementPopUp __instance)
    {
        AnnouncementSelectMenuHelper.OnMenuSet(__instance);
        AnnouncementSelectMenuHelper.UpdateUnreadBadges(__instance);
    }
}

internal static class AnnouncementSelectMenuHelper
{
    private const string MenuAssetName = "AnnounceMenuSelector";
    private const string VanillaCategory = "Announce_Vanilla";
    private const string SnrCategory = "Announce_SNR";
    private const string DefaultCategory = SnrCategory;
    private const float MenuScale = 0.23f;
    private const int SnrNumberOffset = 1_000_000_000;
    private const int SnrNumberModulo = 1_000_000_000;
    private const int SnrPageSize = 50;
    private const int ShortTitleMaxLength = 36;
    private static readonly Vector3 MenuFallbackPosition = new(-2.82f, 1.97f, -1.15f);

    private static readonly string[] CategoryNames =
    {
        VanillaCategory,
        SnrCategory
    };

    public static void EnsureMenu(AnnouncementPopUp popup)
    {
        if (popup == null) return;

        var menuObject = FindMenu(popup);
        if (menuObject == null)
        {
            var asset = AssetManager.GetAsset<GameObject>(MenuAssetName);
            if (asset == null)
                return;

            menuObject = UnityEngine.Object.Instantiate(asset);
            menuObject.AddComponent<AnnouncementSelectMenuMarker>();
            PlaceMenu(popup, menuObject);
            SetupCategoryButtons(menuObject);
        }
        else
        {
            menuObject.SetActive(true);
        }

        var state = menuObject.GetComponent<AnnouncementSelectMenuState>();
        if (state == null)
            state = menuObject.AddComponent<AnnouncementSelectMenuState>();
        if (string.IsNullOrWhiteSpace(state.CurrentCategory))
            state.CurrentCategory = DefaultCategory;
        SetCurrentTab(menuObject, state.CurrentCategory);
    }

    public static void OnMenuSet(AnnouncementPopUp popup)
    {
        if (popup == null) return;
        var menuObject = FindMenu(popup);
        if (menuObject == null) return;

        var state = menuObject.GetComponent<AnnouncementSelectMenuState>();
        if (state == null) return;

        CaptureVanillaCache(state);

        if (string.IsNullOrWhiteSpace(state.CurrentCategory))
            state.CurrentCategory = DefaultCategory;

        if (state.CurrentCategory == SnrCategory)
            SetCurrentTab(menuObject, state.CurrentCategory);
    }

    public static void SetMenuActive(AnnouncementPopUp popup, bool active)
    {
        var menuObject = FindMenu(popup);
        if (menuObject != null)
            menuObject.SetActive(active);
    }

    public static void UpdateUnreadBadges(AnnouncementPopUp popup)
    {
        if (popup == null) return;

        // 現在のタブを確認
        var menuObject = FindMenu(popup);
        if (menuObject == null) return;

        var state = menuObject.GetComponent<AnnouncementSelectMenuState>();
        if (state == null) return;

        // SNRタブの場合
        if (state.CurrentCategory == SnrCategory)
        {
            UpdateSnrUnreadBadges(popup);
        }
        else
        {
            // Vanillaタブの場合は何もしない(デフォルトの動作に任せる)
        }

        // 選択されたアナウンスを既読にする
        MarkCurrentAnnouncementAsRead(popup);
    }

    private static void UpdateSnrUnreadBadges(AnnouncementPopUp popup)
    {
        if (popup == null) return;

        // 現在表示中のアナウンスリストから未読バッジを更新
        var announcementList = popup.transform.Find("AnnouncementList");
        if (announcementList == null) return;

        var announcements = DataManager.Player?.Announcements?.AllAnnouncements;
        if (announcements == null) return;

        for (int i = 0; i < announcements.Count && i < announcementList.childCount; i++)
        {
            var item = announcementList.GetChild(i);
            if (item == null) continue;

            var announcement = announcements[i];
            bool isUnread = !AnnounceNotificationManager.IsRead(announcement.Id);

            // 未読バッジを探して表示/非表示
            var badge = item.Find("UnreadBadge");
            if (badge == null)
            {
                // バッジがない場合は作成
                badge = CreateUnreadBadge(item);
            }

            if (badge != null)
                badge.gameObject.SetActive(isUnread);
        }
    }

    public static IEnumerator UpdateUnreadBadgesDelayed(AnnouncementPopUp popup)
    {
        // AnnouncementListが作成されるまで待つ
        yield return new WaitForSeconds(0.1f);
        UpdateUnreadBadges(popup);
    }

    private static void MarkCurrentAnnouncementAsRead(AnnouncementPopUp popup)
    {
        if (popup == null) return;

        try
        {
            var selectedAnnouncement = popup.selectedPanel.announcement;
            if (selectedAnnouncement != null && !string.IsNullOrWhiteSpace(selectedAnnouncement.Id))
            {
                AnnounceNotificationManager.MarkAsRead(selectedAnnouncement.Id);
            }
        }
        catch (Exception ex)
        {
            SuperNewRolesPlugin.Logger.LogWarning($"Failed to mark announcement as read: {ex.Message}");
        }
    }

    private static Transform CreateUnreadBadge(Transform parent)
    {
        try
        {
            var badgeObj = new GameObject("UnreadBadge");
            badgeObj.transform.SetParent(parent, false);
            badgeObj.transform.localPosition = new Vector3(1.5f, 0f, -1f);
            badgeObj.transform.localScale = Vector3.one * 0.5f;

            var renderer = badgeObj.AddComponent<SpriteRenderer>();
            renderer.sprite = AssetManager.GetAsset<Sprite>("badge");
            if (renderer.sprite == null)
            {
                // スプライトがない場合は円を描画
                var circle = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                GameObject.Destroy(circle.GetComponent<Collider>());
                renderer = circle.GetComponent<SpriteRenderer>();
                if (renderer != null)
                    renderer.color = new Color(1f, 0.2f, 0.2f); // 赤色
                badgeObj = circle;
                badgeObj.name = "UnreadBadge";
                badgeObj.transform.SetParent(parent, false);
                badgeObj.transform.localPosition = new Vector3(1.5f, 0f, -1f);
                badgeObj.transform.localScale = Vector3.one * 0.3f;
            }

            return badgeObj.transform;
        }
        catch (Exception ex)
        {
            SuperNewRolesPlugin.Logger.LogWarning($"Failed to create unread badge: {ex.Message}");
            return null;
        }
    }

    private static GameObject FindMenu(AnnouncementPopUp popup)
    {
        if (popup == null) return null;
        foreach (var marker in popup.GetComponentsInChildren<AnnouncementSelectMenuMarker>(true))
        {
            if (marker != null)
                return marker.gameObject;
        }
        return null;
    }

    private static void PlaceMenu(AnnouncementPopUp popup, GameObject menuObject)
    {
        Transform parent = popup.transform;
        Vector3 position = MenuFallbackPosition;

        menuObject.transform.SetParent(parent.transform.Find("Sizer/Header"), false);
        menuObject.transform.localPosition = position;
        menuObject.transform.localScale = Vector3.one * MenuScale;
    }

    private static void SetupCategoryButtons(GameObject menuObject)
    {
        foreach (var name in CategoryNames)
        {
            var category = menuObject.transform.Find(name)?.gameObject;
            if (category == null) continue;
            ConfigureButton(menuObject, category);
        }
    }

    private static void ConfigureButton(GameObject menuObject, GameObject category)
    {
        var button = category.GetComponent<PassiveButton>() ?? category.AddComponent<PassiveButton>();
        button.OnClick = new();
        button.OnMouseOver = new();
        button.OnMouseOut = new();

        string categoryName = category.name;
        button.OnClick.AddListener((UnityAction)(() =>
        {
            var state = menuObject.GetComponent<AnnouncementSelectMenuState>();
            if (state != null)
                state.CurrentCategory = categoryName;
            SetCurrentTab(menuObject, categoryName);
        }));
        button.OnMouseOver.AddListener((UnityAction)(() => SetCategoryHighlight(category, true)));
        button.OnMouseOut.AddListener((UnityAction)(() => SetCategoryHighlight(category, false)));
    }

    private static void SetCurrentTab(GameObject menuObject, string categoryName)
    {
        if (menuObject == null || string.IsNullOrWhiteSpace(categoryName))
            return;

        var popup = menuObject.GetComponentInParent<AnnouncementPopUp>();
        if (popup == null)
            return;

        var state = menuObject.GetComponent<AnnouncementSelectMenuState>();
        if (state == null)
            return;

        state.CurrentCategory = categoryName;

        switch (categoryName)
        {
            case VanillaCategory:
                ApplyVanillaAnnouncements(popup, state);
                break;
            case SnrCategory:
                ApplySnrAnnouncements(popup, state);
                break;
        }
    }

    private static void ApplyVanillaAnnouncements(AnnouncementPopUp popup, AnnouncementSelectMenuState state)
    {
        if (popup == null || state == null)
            return;

        if (state.VanillaCache == null)
            CaptureVanillaCache(state);

        if (state.VanillaCache == null)
            return;

        ApplyAnnouncements(popup, state.VanillaCache);
    }

    private static void ApplySnrAnnouncements(AnnouncementPopUp popup, AnnouncementSelectMenuState state)
    {
        if (popup == null || state == null)
            return;

        if (state.VanillaCache == null)
            CaptureVanillaCache(state);

        string lang = GetApiLanguage();

        // メモリキャッシュをチェック
        if (state.SnrCache != null &&
            state.SnrCache.Count > 0 &&
            string.Equals(state.SnrLang, lang, StringComparison.Ordinal))
        {
            ApplyAnnouncements(popup, state.SnrCache);

            // メモリキャッシュがある場合でも、バックグラウンドで更新をチェック
            if (!state.SnrLoading)
            {
                var cachedListForUpdate = AnnounceCache.GetArticlesList(lang);
                state.SnrRequestToken++;
                int token = state.SnrRequestToken;
                popup.StartCoroutine(UpdateSnrAnnouncementsInBackground(popup, state, lang, token, cachedListForUpdate?.ETag).WrapToIl2Cpp());
            }
            return;
        }

        if (state.SnrLoading)
        {
            ApplyAnnouncements(popup, CreateLoadingAnnouncements());
            return;
        }

        // ディスクキャッシュをチェック
        var cachedList = AnnounceCache.GetArticlesList(lang);
        if (cachedList != null && cachedList.Response != null && cachedList.Response.Items.Count > 0)
        {
            // キャッシュから個別記事も含めてアナウンスを作成
            var cachedAnnouncements = ConvertCachedArticlesListToAnnouncements(cachedList.Response, lang);

            // アナウンスがある場合はキャッシュを表示（本文なしでも表示）
            if (cachedAnnouncements.Count > 0)
            {
                state.SnrCache = cachedAnnouncements;
                state.SnrLang = lang;
                ApplyAnnouncements(popup, cachedAnnouncements);

                // バックグラウンドで更新をチェック（常に実行して最新内容を取得）
                state.SnrRequestToken++;
                int token = state.SnrRequestToken;
                popup.StartCoroutine(UpdateSnrAnnouncementsInBackground(popup, state, lang, token, cachedList.ETag).WrapToIl2Cpp());
                return;
            }
        }

        // キャッシュがない場合、ローディング表示して取得
        state.SnrLoading = true;
        state.SnrLang = lang;
        state.SnrRequestToken++;
        int loadToken = state.SnrRequestToken;

        ApplyAnnouncements(popup, CreateLoadingAnnouncements());
        popup.StartCoroutine(LoadSnrAnnouncements(popup, state, lang, loadToken).WrapToIl2Cpp());
    }

    private static IEnumerator LoadSnrAnnouncements(AnnouncementPopUp popup, AnnouncementSelectMenuState state, string lang, int requestToken)
    {
        ApiResult<ArticlesResponse> listResult = null;
        yield return SuperNewAnnounceApi.ListArticles(lang, result => listResult = result, page: 1, pageSize: SnrPageSize, fallback: true);

        if (state == null || state.SnrRequestToken != requestToken)
            yield break;

        List<Announcement> announcements = new();
        if (listResult == null || !listResult.IsSuccess || listResult.Data == null)
        {
            state.SnrLoading = false;
            state.SnrCache = announcements;
            state.SnrLang = lang;
            if (state.CurrentCategory == SnrCategory)
                ApplyAnnouncements(popup, announcements);
            yield break;
        }

        // キャッシュに保存
        AnnounceCache.SaveArticlesList(lang, listResult.Data, listResult.ETag);

        // 個別記事を取得してアナウンスに変換
        int fallbackIndex = 0;
        foreach (var item in listResult.Data.Items)
        {
            if (state == null || state.SnrRequestToken != requestToken)
                yield break;

            // キャッシュから取得
            var cachedArticle = AnnounceCache.GetArticle(item.Id, lang);
            Article article = cachedArticle?.Article;

            // キャッシュにない場合はAPIから取得
            if (article == null)
            {
                ApiResult<Article> articleResult = null;
                yield return SuperNewAnnounceApi.GetArticle(item.Id, lang, result => articleResult = result, fallback: true);

                if (articleResult != null && articleResult.IsSuccess && articleResult.Data != null)
                {
                    article = articleResult.Data;
                    // キャッシュに保存
                    AnnounceCache.SaveArticle(item.Id, lang, article, articleResult.ETag);
                }
            }

            if (article != null)
            {
                announcements.Add(ConvertToAnnouncement(article, fallbackIndex));
            }
            fallbackIndex++;
        }

        state.SnrLoading = false;
        state.SnrCache = announcements;
        state.SnrLang = lang;

        if (state.CurrentCategory == SnrCategory)
            ApplyAnnouncements(popup, announcements);
    }

    private static IEnumerator UpdateSnrAnnouncementsInBackground(AnnouncementPopUp popup, AnnouncementSelectMenuState state, string lang, int requestToken, string etag)
    {
        ApiResult<ArticlesResponse> listResult = null;
        // リスト取得時はETagを使って更新チェック
        yield return SuperNewAnnounceApi.ListArticles(lang, result => listResult = result, page: 1, pageSize: SnrPageSize, fallback: true, etag: etag);

        if (state == null || state.SnrRequestToken != requestToken)
            yield break;

        // 304 Not Modified の場合でも、個別記事の更新をチェック
        bool listUpdated = listResult != null && listResult.IsSuccess && listResult.Data != null;
        bool shouldCheckArticles = listResult != null && (listResult.IsNotModified || listUpdated);

        if (!shouldCheckArticles)
            yield break;

        // リストが更新された場合はキャッシュを保存
        if (listUpdated)
            AnnounceCache.SaveArticlesList(lang, listResult.Data, listResult.ETag);

        // 現在表示中のリストから記事IDを取得
        var cachedList = AnnounceCache.GetArticlesList(lang);
        if (cachedList == null || cachedList.Response == null || cachedList.Response.Items.Count == 0)
            yield break;

        // 個別記事を取得してアナウンスに変換
        List<Announcement> announcements = new();
        bool hasUpdates = false;
        int fallbackIndex = 0;

        foreach (var item in cachedList.Response.Items)
        {
            if (state == null || state.SnrRequestToken != requestToken)
                yield break;

            // キャッシュから取得
            var cachedArticle = AnnounceCache.GetArticle(item.Id, lang);
            Article article = cachedArticle?.Article;

            // 常に最新の記事を取得（ETagなし）して更新をチェック
            ApiResult<Article> articleResult = null;
            yield return SuperNewAnnounceApi.GetArticle(item.Id, lang, result => articleResult = result, fallback: true);

            if (articleResult != null && articleResult.IsSuccess && articleResult.Data != null)
            {
                article = articleResult.Data;
                AnnounceCache.SaveArticle(item.Id, lang, article, articleResult.ETag);
                hasUpdates = true;
            }
            else if (article != null)
            {
                // API取得失敗時はキャッシュを使用
            }

            if (article != null)
            {
                announcements.Add(ConvertToAnnouncement(article, fallbackIndex));
            }
            fallbackIndex++;
        }

        // 更新があった場合、または記事数が変わった場合は画面を更新
        if (hasUpdates || state.SnrCache == null || state.SnrCache.Count != announcements.Count)
        {
            // メモリキャッシュと画面を更新
            state.SnrCache = announcements;
            state.SnrLang = lang;

            if (state.CurrentCategory == SnrCategory && popup != null)
                ApplyAnnouncements(popup, announcements);
        }
    }

    private static List<Announcement> ConvertCachedArticlesListToAnnouncements(ArticlesResponse response, string lang)
    {
        List<Announcement> announcements = new();
        if (response == null || response.Items == null)
            return announcements;

        int fallbackIndex = 0;
        foreach (var item in response.Items)
        {
            // キャッシュから個別記事を取得
            var cachedArticle = AnnounceCache.GetArticle(item.Id, lang);
            Article article = cachedArticle?.Article;

            // キャッシュがない場合は、リストの情報から仮の記事を作成
            if (article == null)
            {
                article = new Article(item, string.Empty);
            }

            announcements.Add(ConvertToAnnouncement(article, fallbackIndex));
            fallbackIndex++;
        }

        return announcements;
    }

    private static void ApplyAnnouncements(AnnouncementPopUp popup, List<Announcement> announcements)
    {
        if (popup == null)
            return;

        var playerAnnouncements = DataManager.Player?.Announcements;
        if (playerAnnouncements == null)
            return;

        var list = announcements ?? new List<Announcement>();
        playerAnnouncements.SetAnnouncements(list.ToArray());
        RefreshPopup(popup);
    }

    private static void RefreshPopup(AnnouncementPopUp popup)
    {
        if (popup == null)
            return;

        popup.CreateAnnouncementList();

        // スクロール位置をリセット
        ResetScrollPosition(popup);

        var announcements = DataManager.Player?.Announcements?.AllAnnouncements;
        if (announcements == null || announcements.Count == 0)
            return;

        bool previewOnly = ActiveInputManager.currentControlType == ActiveInputManager.InputType.Joystick;
        popup.UpdateAnnouncementText(announcements[0].Number, previewOnly);

        // 未読バッジを更新（少し遅延させてUIが完全に構築されてから）
        popup.StartCoroutine(UpdateUnreadBadgesDelayed(popup).WrapToIl2Cpp());
    }

    private static void ResetScrollPosition(AnnouncementPopUp popup)
    {
        if (popup == null)
            return;

        try
        {
            // AnnouncementListのScrollerを探してリセット
            var scroller = popup.GetComponentInChildren<Scroller>();
            if (scroller != null)
            {
                scroller.ScrollToTop();
            }
        }
        catch (Exception ex)
        {
            SuperNewRolesPlugin.Logger.LogWarning($"Failed to reset scroll position: {ex.Message}");
        }
    }

    private static void CaptureVanillaCache(AnnouncementSelectMenuState state)
    {
        if (state == null)
            return;

        var announcements = DataManager.Player?.Announcements?.AllAnnouncements.ToSystemList();
        if (announcements == null || IsSnrAnnouncementList(announcements))
            return;

        state.VanillaCache = new List<Announcement>(announcements);
    }

    private static bool IsSnrAnnouncementList(List<Announcement> announcements)
    {
        if (announcements == null)
            return false;

        foreach (var announcement in announcements)
        {
            if (announcement.Number >= SnrNumberOffset)
                return true;
        }
        return false;
    }

    private static Announcement ConvertToAnnouncement(Article article, int fallbackIndex)
    {
        string title = string.IsNullOrWhiteSpace(article?.Title) ? "Announcement" : article.Title;
        string body = string.IsNullOrWhiteSpace(article?.Body) ? string.Empty : article.Body;
        if (string.IsNullOrWhiteSpace(body))
            body = string.IsNullOrWhiteSpace(article?.Url) ? title : article.Url;

        int number = BuildSnrNumber(article?.Id, fallbackIndex);
        var images = new List<AnnouncementImageInfo>();
        string bodyWithoutImages = AnnouncementImageCache.StripMarkdownImages(body, images);
        AnnouncementImageCache.SetImages(number, images);

        // マークダウンをUnityタグに変換
        string convertedTitle = MarkdownToUnityTag.Convert(title);
        string convertedBody = MarkdownToUnityTag.Convert(bodyWithoutImages);
        string shortTitle = MakeShortTitle(convertedTitle);

        return new Announcement
        {
            Id = article?.Id ?? string.Empty,
            Language = GetCurrentLanguageId(),
            Number = number,
            Title = convertedTitle,
            SubTitle = string.Empty,
            ShortTitle = shortTitle,
            PinState = false,
            Text = convertedBody,
            Date = NormalizeDate(article)
        };
    }

    private static string NormalizeDate(Article article)
    {
        string raw = article?.UpdatedAt;
        if (string.IsNullOrWhiteSpace(raw))
            raw = article?.CreatedAt;

        if (!string.IsNullOrWhiteSpace(raw) &&
            DateTime.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out var parsed))
        {
            return parsed.ToString("o", CultureInfo.InvariantCulture);
        }

        return DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture);
    }

    private static int BuildSnrNumber(string id, int fallbackIndex)
    {
        if (string.IsNullOrWhiteSpace(id))
            return SnrNumberOffset + (fallbackIndex % SnrNumberModulo);

        string hash = ModHelpers.HashMD5(id);
        if (hash.Length >= 8 &&
            uint.TryParse(hash.Substring(0, 8), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var value))
        {
            return SnrNumberOffset + (int)(value % SnrNumberModulo);
        }

        return SnrNumberOffset + (fallbackIndex % SnrNumberModulo);
    }

    private static string MakeShortTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            return string.Empty;

        if (title.Length <= ShortTitleMaxLength)
            return title;

        return title.Substring(0, ShortTitleMaxLength - 3) + "...";
    }

    private static uint GetCurrentLanguageId()
    {
        try
        {
            return (uint)DataManager.Settings.Language.CurrentLanguage;
        }
        catch
        {
            return 0;
        }
    }

    private static string GetApiLanguage()
    {
        try
        {
            return DataManager.Settings.Language.CurrentLanguage switch
            {
                SupportedLangs.Japanese => "ja",
                SupportedLangs.SChinese => "zh-CN",
                SupportedLangs.TChinese => "zh-TW",
                _ => "en"
            };
        }
        catch
        {
            return "en";
        }
    }

    private static List<Announcement> CreateLoadingAnnouncements()
    {
        string loadingText = GetLoadingText();
        AnnouncementImageCache.SetImages(SnrNumberOffset, null);
        return new List<Announcement>
        {
            new Announcement
            {
                Id = "snr-loading",
                Language = GetCurrentLanguageId(),
                Number = SnrNumberOffset,
                Title = loadingText,
                SubTitle = string.Empty,
                ShortTitle = loadingText,
                PinState = false,
                Text = loadingText,
                Date = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture)
            }
        };
    }

    private static string GetLoadingText()
    {
        try
        {
            var controller = DestroyableSingleton<TranslationController>.Instance;
            if (controller != null)
                return controller.GetString(StringNames.Loading);
        }
        catch
        {
        }
        return "Loading...";
    }

    private static void SetCategoryHighlight(GameObject category, bool active)
    {
        if (category == null) return;
        var highlight = category.transform.Find("Highlight")?.gameObject;
        if (highlight != null && highlight.activeSelf != active)
            highlight.SetActive(active);
    }

    private static void ConfigureEmptyButton(GameObject target)
    {
        if (target == null) return;
        var button = target.GetComponent<PassiveButton>() ?? target.AddComponent<PassiveButton>();
        if (button.Colliders == null || button.Colliders.Length == 0)
            button.Colliders = new Collider2D[] { target.GetComponent<Collider2D>() };
        button.OnClick = new();
        button.OnMouseOver = new();
        button.OnMouseOut = new();
    }
}

internal sealed class AnnouncementSelectMenuMarker : MonoBehaviour
{
}

internal sealed class AnnouncementSelectMenuState : MonoBehaviour
{
    public string CurrentCategory = string.Empty;
    public List<Announcement> VanillaCache;
    public List<Announcement> SnrCache;
    public string SnrLang = string.Empty;
    public bool SnrLoading;
    public int SnrRequestToken;
}
