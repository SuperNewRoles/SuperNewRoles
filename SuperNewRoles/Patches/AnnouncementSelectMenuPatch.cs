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
        try
        {
            AnnouncementSelectMenuHelper.EnsureMenu(__instance);
            // 未読バッジを更新
            if (__instance != null)
                __instance.StartCoroutine(AnnouncementSelectMenuHelper.UpdateUnreadBadgesDelayed(__instance).WrapToIl2Cpp());
        }
        catch (Exception ex)
        {
            SuperNewRolesPlugin.Logger.LogWarning($"AnnouncementPopUpOnEnablePatch failed: {ex}");
        }
    }
}

[HarmonyPatch(typeof(AnnouncementPopUp), nameof(AnnouncementPopUp.OnDisable))]
public static class AnnouncementPopUpOnDisablePatch
{
    public static void Postfix(AnnouncementPopUp __instance)
    {
        try
        {
            AnnouncementSelectMenuHelper.SetMenuActive(__instance, false);
        }
        catch (Exception ex)
        {
            SuperNewRolesPlugin.Logger.LogWarning($"AnnouncementPopUpOnDisablePatch failed: {ex}");
        }
    }
}

[HarmonyPatch(typeof(AnnouncementPopUp), nameof(AnnouncementPopUp.SetMenu))]
public static class AnnouncementPopUpSetMenuPatch
{
    public static void Postfix(AnnouncementPopUp __instance)
    {
        try
        {
            AnnouncementSelectMenuHelper.OnMenuSet(__instance);
            AnnouncementSelectMenuHelper.UpdateUnreadBadges(__instance);
        }
        catch (Exception ex)
        {
            SuperNewRolesPlugin.Logger.LogWarning($"AnnouncementPopUpSetMenuPatch failed: {ex}");
        }
    }
}

[HarmonyPatch(typeof(AnnouncementPopUp), nameof(AnnouncementPopUp.UpdateAnnouncementText))]
public static class AnnouncementPopUpUpdateAnnouncementTextTitlePatch
{
    public static void Postfix(AnnouncementPopUp __instance)
    {
        try
        {
            AnnouncementSelectMenuHelper.UpdateAnnouncementPopupTitle(__instance);
        }
        catch (Exception ex)
        {
            SuperNewRolesPlugin.Logger.LogWarning($"AnnouncementPopUpUpdateAnnouncementTextTitlePatch failed: {ex}");
        }
    }
}

[HarmonyPatch(typeof(AnnouncementPopUp), nameof(AnnouncementPopUp.Update))]
public static class AnnouncementPopUpUpdateTitlePatch
{
    public static void Postfix(AnnouncementPopUp __instance)
    {
        try
        {
            AnnouncementSelectMenuHelper.UpdateAnnouncementPopupTitle(__instance);
        }
        catch (Exception ex)
        {
            SuperNewRolesPlugin.Logger.LogWarning($"AnnouncementPopUpUpdateTitlePatch failed: {ex}");
        }
    }
}

internal static class AnnouncementSelectMenuHelper
{
    private const string MenuAssetName = "AnnounceMenuSelector";
    private const string VanillaCategory = "Announce_Vanilla";
    private const string SnrCategory = "Announce_SNR";
    private const string DefaultCategory = SnrCategory;
    private const string SnrNotificationHeaderTitleFallbackSuffix = "の通知";
    private const string SnrBrandName = "SuperNewRoles";
    private const string SnrBrandColorFallback = "<color=#ffa500>Super</color><color=#ff0000>New</color><color=#00ff00>Roles</color>";
    private const float MenuScale = 0.23f;
    private const int SnrNumberOffset = 1_000_000_000;
    private const int SnrNumberModulo = 1_000_000_000;
    private const int SnrPageSize = 50;
    private const int ShortTitleMaxLength = 36;
    private static readonly Vector3 MenuFallbackPosition = new(-2.82f, 1.97f, -1.15f);
    private static readonly Vector3 SnrLogoLocalPosition = new(-1.08f, 1.93f, -2f);
    private static readonly Vector3 SnrLogoLocalScale = Vector3.one * 0.16f;
    private static readonly Dictionary<int, string> VanillaHeaderTitleByPopupId = new();
    private static Sprite FallbackUnreadBadgeSprite;

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

        if (string.IsNullOrWhiteSpace(AnnouncementSelectMenuState.CurrentCategory))
            AnnouncementSelectMenuState.CurrentCategory = DefaultCategory;
        CacheVanillaHeaderTitle(popup);
        SetCurrentTab(menuObject, AnnouncementSelectMenuState.CurrentCategory);
    }

    public static void OnMenuSet(AnnouncementPopUp popup)
    {
        if (popup == null) return;
        var menuObject = FindMenu(popup);
        if (menuObject == null) return;

        CaptureVanillaCache();

        if (string.IsNullOrWhiteSpace(AnnouncementSelectMenuState.CurrentCategory))
            AnnouncementSelectMenuState.CurrentCategory = DefaultCategory;

        if (AnnouncementSelectMenuState.CurrentCategory == SnrCategory)
            SetCurrentTab(menuObject, AnnouncementSelectMenuState.CurrentCategory);
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

        // SNRタブの場合
        if (AnnouncementSelectMenuState.CurrentCategory == SnrCategory)
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
            if (announcement == null)
                continue;
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
                renderer.sprite = GetFallbackUnreadBadgeSprite();
                renderer.color = new Color(1f, 0.2f, 0.2f, 1f);
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

    private static Sprite GetFallbackUnreadBadgeSprite()
    {
        if (FallbackUnreadBadgeSprite != null)
            return FallbackUnreadBadgeSprite;

        const int texSize = 32;
        var texture = new Texture2D(texSize, texSize, TextureFormat.ARGB32, false);
        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;

        float center = (texSize - 1) * 0.5f;
        float radius = texSize * 0.5f;
        float radiusSqr = radius * radius;
        for (int y = 0; y < texSize; y++)
        {
            for (int x = 0; x < texSize; x++)
            {
                float dx = x - center;
                float dy = y - center;
                bool inside = (dx * dx + dy * dy) <= radiusSqr;
                texture.SetPixel(x, y, inside ? Color.white : Color.clear);
            }
        }

        texture.Apply(false, false);
        FallbackUnreadBadgeSprite = Sprite.Create(texture, new Rect(0f, 0f, texSize, texSize), new Vector2(0.5f, 0.5f), texSize);
        return FallbackUnreadBadgeSprite;
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

        var header = parent.Find("Sizer/Header");
        menuObject.transform.SetParent(header != null ? header : parent, false);
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
            AnnouncementSelectMenuState.CurrentCategory = categoryName;
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

        AnnouncementSelectMenuState.CurrentCategory = categoryName;

        switch (categoryName)
        {
            case VanillaCategory:
                ApplyVanillaAnnouncements(popup);
                break;
            case SnrCategory:
                ApplySnrAnnouncements(popup);
                break;
        }
    }

    private static void ApplyVanillaAnnouncements(AnnouncementPopUp popup)
    {
        if (popup == null)
            return;

        ApplyAnnouncementHeaderState(popup, isSnrTab: false);

        if (AnnouncementSelectMenuState.VanillaCache == null)
            CaptureVanillaCache();

        if (AnnouncementSelectMenuState.VanillaCache == null || AnnouncementSelectMenuState.VanillaCache.Count == 0)
        {
            // バニラキャッシュがない場合は、バニラの実際のアナウンスを使用
            var announcements = DataManager.Player?.Announcements?.AllAnnouncements.ToSystemList();
            if (announcements != null && announcements.Count > 0 && !IsSnrAnnouncementList(announcements))
            {
                ApplyAnnouncements(popup, announcements);
                return;
            }
            return;
        }

        ApplyAnnouncements(popup, AnnouncementSelectMenuState.VanillaCache);
    }

    private static void ApplySnrAnnouncements(AnnouncementPopUp popup)
    {
        if (popup == null)
            return;

        ApplyAnnouncementHeaderState(popup, isSnrTab: true);

        if (AnnouncementSelectMenuState.VanillaCache == null)
            CaptureVanillaCache();

        string lang = GetApiLanguage();
        if (string.IsNullOrWhiteSpace(lang))
            return;

        // メモリキャッシュをチェック
        if (AnnouncementSelectMenuState.SnrCache != null &&
            AnnouncementSelectMenuState.SnrCache.Count > 0 &&
            string.Equals(AnnouncementSelectMenuState.SnrLang, lang, StringComparison.Ordinal))
        {
            ApplyAnnouncements(popup, AnnouncementSelectMenuState.SnrCache);

            // メモリキャッシュがある場合でも、バックグラウンドで更新をチェック
            if (!AnnouncementSelectMenuState.SnrLoading)
            {
                var cachedListForUpdate = AnnounceCache.GetArticlesList(lang);
                AnnouncementSelectMenuState.SnrRequestToken++;
                int token = AnnouncementSelectMenuState.SnrRequestToken;
                popup.StartCoroutine(UpdateSnrAnnouncementsInBackground(popup, lang, token, cachedListForUpdate?.ETag).WrapToIl2Cpp());
            }
            return;
        }

        if (AnnouncementSelectMenuState.SnrLoading)
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
                AnnouncementSelectMenuState.SnrCache = cachedAnnouncements;
                AnnouncementSelectMenuState.SnrLang = lang;
                ApplyAnnouncements(popup, cachedAnnouncements);

                // バックグラウンドで更新をチェック（常に実行して最新内容を取得）
                AnnouncementSelectMenuState.SnrRequestToken++;
                int token = AnnouncementSelectMenuState.SnrRequestToken;
                popup.StartCoroutine(UpdateSnrAnnouncementsInBackground(popup, lang, token, cachedList.ETag).WrapToIl2Cpp());
                return;
            }
        }

        // キャッシュがない場合、ローディング表示して取得
        AnnouncementSelectMenuState.SnrLoading = true;
        AnnouncementSelectMenuState.SnrLang = lang;
        AnnouncementSelectMenuState.SnrRequestToken++;
        int loadToken = AnnouncementSelectMenuState.SnrRequestToken;

        ApplyAnnouncements(popup, CreateLoadingAnnouncements());
        popup.StartCoroutine(LoadSnrAnnouncements(popup, lang, loadToken).WrapToIl2Cpp());
    }

    private static IEnumerator LoadSnrAnnouncements(AnnouncementPopUp popup, string lang, int requestToken)
    {
        ApiResult<ArticlesResponse> listResult = null;
        yield return SuperNewAnnounceApi.ListArticles(lang, result => listResult = result, page: 1, pageSize: SnrPageSize, fallback: true);

        if (AnnouncementSelectMenuState.SnrRequestToken != requestToken)
            yield break;

        List<Announcement> announcements = new();
        if (listResult == null || !listResult.IsSuccess || listResult.Data == null)
        {
            AnnouncementSelectMenuState.SnrLoading = false;
            AnnouncementSelectMenuState.SnrCache = announcements;
            AnnouncementSelectMenuState.SnrLang = lang;
            if (AnnouncementSelectMenuState.CurrentCategory == SnrCategory)
                ApplyAnnouncements(popup, announcements);
            yield break;
        }

        // キャッシュに保存
        AnnounceCache.SaveArticlesList(lang, listResult.Data, listResult.ETag);

        // 個別記事を取得してアナウンスに変換
        int fallbackIndex = 0;
        foreach (var item in listResult.Data.Items)
        {
            if (AnnouncementSelectMenuState.SnrRequestToken != requestToken)
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

        AnnouncementSelectMenuState.SnrLoading = false;
        AnnouncementSelectMenuState.SnrCache = announcements;
        AnnouncementSelectMenuState.SnrLang = lang;

        if (AnnouncementSelectMenuState.CurrentCategory == SnrCategory)
            ApplyAnnouncements(popup, announcements);
    }

    private static IEnumerator UpdateSnrAnnouncementsInBackground(AnnouncementPopUp popup, string lang, int requestToken, string etag)
    {
        ApiResult<ArticlesResponse> listResult = null;
        // リスト取得時はETagを使って更新チェック
        yield return SuperNewAnnounceApi.ListArticles(lang, result => listResult = result, page: 1, pageSize: SnrPageSize, fallback: true, etag: etag);

        if (AnnouncementSelectMenuState.SnrRequestToken != requestToken)
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
            if (AnnouncementSelectMenuState.SnrRequestToken != requestToken)
                yield break;

            // キャッシュから取得
            var cachedArticle = AnnounceCache.GetArticle(item.Id, lang);
            Article article = cachedArticle?.Article;
            string articleEtag = cachedArticle?.ETag;

            // 個別記事もETagで更新チェック
            ApiResult<Article> articleResult = null;
            yield return SuperNewAnnounceApi.GetArticle(item.Id, lang, result => articleResult = result, fallback: true, etag: articleEtag);

            if (articleResult != null && articleResult.IsSuccess && articleResult.Data != null)
            {
                bool articleChanged = !AreArticlesEquivalent(cachedArticle?.Article, articleResult.Data)
                    || !string.Equals(articleEtag ?? string.Empty, articleResult.ETag ?? string.Empty, StringComparison.Ordinal);

                article = articleResult.Data;
                AnnounceCache.SaveArticle(item.Id, lang, article, articleResult.ETag);
                if (articleChanged)
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
        bool cacheChanged = !AreAnnouncementListsEquivalent(AnnouncementSelectMenuState.SnrCache, announcements);
        if (hasUpdates || cacheChanged)
        {
            // メモリキャッシュと画面を更新
            AnnouncementSelectMenuState.SnrCache = announcements;
            AnnouncementSelectMenuState.SnrLang = lang;

            if (AnnouncementSelectMenuState.CurrentCategory == SnrCategory && popup != null)
                ApplyAnnouncements(popup, announcements);
        }
    }

    private static bool AreAnnouncementListsEquivalent(List<Announcement> left, List<Announcement> right)
    {
        if (ReferenceEquals(left, right))
            return true;
        if (left == null || right == null || left.Count != right.Count)
            return false;

        for (int i = 0; i < left.Count; i++)
        {
            if (!AreAnnouncementsEquivalent(left[i], right[i]))
                return false;
        }

        return true;
    }

    private static bool AreAnnouncementsEquivalent(Announcement left, Announcement right)
    {
        if (left == null || right == null)
            return left == right;

        return left.Number == right.Number
            && left.PinState == right.PinState
            && left.Language == right.Language
            && string.Equals(left.Id, right.Id, StringComparison.Ordinal)
            && string.Equals(left.Title, right.Title, StringComparison.Ordinal)
            && string.Equals(left.SubTitle, right.SubTitle, StringComparison.Ordinal)
            && string.Equals(left.ShortTitle, right.ShortTitle, StringComparison.Ordinal)
            && string.Equals(left.Text, right.Text, StringComparison.Ordinal)
            && string.Equals(left.Date, right.Date, StringComparison.Ordinal);
    }

    private static bool AreArticlesEquivalent(Article left, Article right)
    {
        if (left == null || right == null)
            return left == right;

        return string.Equals(left.Id, right.Id, StringComparison.Ordinal)
            && string.Equals(left.Title, right.Title, StringComparison.Ordinal)
            && string.Equals(left.Url, right.Url, StringComparison.Ordinal)
            && string.Equals(left.Lang, right.Lang, StringComparison.Ordinal)
            && string.Equals(left.RequestedLang, right.RequestedLang, StringComparison.Ordinal)
            && left.IsFallback == right.IsFallback
            && string.Equals(left.CreatedAt, right.CreatedAt, StringComparison.Ordinal)
            && string.Equals(left.UpdatedAt, right.UpdatedAt, StringComparison.Ordinal)
            && string.Equals(left.Body, right.Body, StringComparison.Ordinal)
            && AreTagListsEquivalent(left.Tags, right.Tags);
    }

    private static bool AreTagListsEquivalent(List<Tag> left, List<Tag> right)
    {
        if (ReferenceEquals(left, right))
            return true;
        if (left == null || right == null || left.Count != right.Count)
            return false;

        for (int i = 0; i < left.Count; i++)
        {
            var l = left[i];
            var r = right[i];
            if (l == null || r == null)
            {
                if (l != r)
                    return false;
                continue;
            }

            if (!string.Equals(l.Id, r.Id, StringComparison.Ordinal)
                || !string.Equals(l.Name, r.Name, StringComparison.Ordinal)
                || !string.Equals(l.Lang, r.Lang, StringComparison.Ordinal)
                || !string.Equals(l.Color, r.Color, StringComparison.Ordinal))
            {
                return false;
            }
        }

        return true;
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
        try
        {
            if (list.Count > 0)
            {
                var filtered = new List<Announcement>(list.Count);
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i] != null)
                        filtered.Add(list[i]);
                }
                playerAnnouncements.SetAnnouncements(filtered.ToArray());
            }
            else
            {
                playerAnnouncements.SetAnnouncements(Array.Empty<Announcement>());
            }
            RefreshPopup(popup);
        }
        catch (Exception ex)
        {
            SuperNewRolesPlugin.Logger.LogWarning($"ApplyAnnouncements failed: {ex}");
        }
    }

    private static void RefreshPopup(AnnouncementPopUp popup)
    {
        if (popup == null)
            return;

        popup.CreateAnnouncementList();

        // スクロール位置をリセット
        ResetScrollPosition(popup);

        var announcements = DataManager.Player?.Announcements?.AllAnnouncements;
        if (announcements == null || announcements.Count == 0 || announcements[0] == null)
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

    private static void CaptureVanillaCache()
    {
        var announcements = DataManager.Player?.Announcements?.AllAnnouncements.ToSystemList();
        if (announcements == null || announcements.Count == 0 || IsSnrAnnouncementList(announcements))
            return;

        var filtered = new List<Announcement>();
        for (int i = 0; i < announcements.Count; i++)
        {
            if (announcements[i] != null)
                filtered.Add(announcements[i]);
        }

        if (filtered.Count == 0)
            return;

        AnnouncementSelectMenuState.VanillaCache = filtered;
    }

    private static bool IsSnrAnnouncementList(List<Announcement> announcements)
    {
        if (announcements == null)
            return false;

        foreach (var announcement in announcements)
        {
            if (announcement == null)
                continue;
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
        AnnouncementImageCache.SetAnnouncementId(number, article?.Id);
        var images = new List<AnnouncementImageInfo>();
        string bodyWithoutImages = AnnouncementImageCache.StripMarkdownImages(body, images);
        AnnouncementImageCache.SetImages(number, images);

        // マークダウンをUnityタグに変換
        string shortTitleSource = MakeShortTitle(title);
        string convertedTitle = MarkdownToUnityTag.Convert(title);
        string convertedBody = MarkdownToUnityTag.Convert(bodyWithoutImages);
        string shortTitle = MarkdownToUnityTag.Convert(shortTitleSource);

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

    public static void UpdateAnnouncementPopupTitle(AnnouncementPopUp popup)
    {
        if (popup == null) return;
        ApplyAnnouncementHeaderState(popup, AnnouncementSelectMenuState.CurrentCategory == SnrCategory);
    }

    private static void ApplyAnnouncementHeaderState(AnnouncementPopUp popup, bool isSnrTab)
    {
        if (popup == null)
            return;

        UpdateAnnouncementHeaderTitle(popup, isSnrTab);
        HideAnnouncementLogo(popup);
    }

    private static void CacheVanillaHeaderTitle(AnnouncementPopUp popup)
    {
        if (popup == null)
            return;

        int popupId = popup.GetInstanceID();
        if (VanillaHeaderTitleByPopupId.ContainsKey(popupId))
            return;

        var titleText = FindAnnouncementHeaderTitleText(popup);
        if (titleText == null)
            return;

        string currentTitle = titleText.text;
        if (string.IsNullOrWhiteSpace(currentTitle))
            return;
        string snrHeaderTitle = GetSnrNotificationHeaderTitle(popup, currentTitle);
        if (string.Equals(currentTitle, snrHeaderTitle, StringComparison.Ordinal))
            return;

        VanillaHeaderTitleByPopupId[popupId] = currentTitle;
    }

    private static void UpdateAnnouncementHeaderTitle(AnnouncementPopUp popup, bool isSnrTab)
    {
        if (popup == null)
            return;

        CacheVanillaHeaderTitle(popup);
        var titleText = FindAnnouncementHeaderTitleText(popup);
        if (titleText == null)
            return;

        if (isSnrTab)
        {
            string snrHeaderTitle = GetSnrNotificationHeaderTitle(popup, titleText.text);
            titleText.richText = true;
            if (!string.Equals(titleText.text, snrHeaderTitle, StringComparison.Ordinal))
                titleText.text = snrHeaderTitle;
            return;
        }

        string vanillaTitle = GetVanillaAnnouncementHeaderTitle(popup, titleText.text);
        if (!string.IsNullOrWhiteSpace(vanillaTitle) && !string.Equals(titleText.text, vanillaTitle, StringComparison.Ordinal))
        {
            titleText.text = vanillaTitle;
        }
    }

    private static TMP_Text FindAnnouncementHeaderTitleText(AnnouncementPopUp popup)
    {
        if (popup == null)
            return null;

        var titleTransform = popup.transform.Find("Sizer/Header/Title_Text");
        if (titleTransform == null)
            return null;

        return titleTransform.GetComponent<TMP_Text>() ?? titleTransform.GetComponentInChildren<TMP_Text>(true);
    }

    private static string GetSnrNotificationHeaderTitle(AnnouncementPopUp popup, string currentTitle)
    {
        string vanillaTitle = GetVanillaAnnouncementHeaderTitle(popup, currentTitle);
        string coloredBrand = GetColoredSnrBrandText();
        if (!string.IsNullOrWhiteSpace(vanillaTitle))
        {
            string replaced = ReplaceAmongUsBrand(vanillaTitle, coloredBrand);
            replaced = ReplaceIgnoreCase(replaced, SnrBrandName, coloredBrand);
            if (!string.Equals(replaced, vanillaTitle, StringComparison.Ordinal))
                return replaced;
        }

        return $"{coloredBrand}{SnrNotificationHeaderTitleFallbackSuffix}";
    }

    private static string GetVanillaAnnouncementHeaderTitle(AnnouncementPopUp popup, string currentTitle)
    {
        string translated = null;
        try
        {
            var controller = DestroyableSingleton<TranslationController>.Instance;
            if (controller != null)
                translated = controller.GetString(StringNames.AmongUsAnnouncements);
        }
        catch
        {
        }

        if (!string.IsNullOrWhiteSpace(translated))
            return translated;

        if (popup != null && VanillaHeaderTitleByPopupId.TryGetValue(popup.GetInstanceID(), out var cached) && !string.IsNullOrWhiteSpace(cached))
            return cached;

        return currentTitle;
    }

    private static string ReplaceAmongUsBrand(string source, string replacement)
    {
        if (string.IsNullOrWhiteSpace(source))
            return source;

        string replaced = source;
        replaced = ReplaceIgnoreCase(replaced, "AMONG US", replacement);
        replaced = ReplaceIgnoreCase(replaced, "Among Us", replacement);
        replaced = ReplaceIgnoreCase(replaced, "AmongUs", replacement);
        replaced = ReplaceIgnoreCase(replaced, "AMONGUS", replacement);
        replaced = ReplaceIgnoreCase(replaced, "among us", replacement);
        return replaced;
    }

    private static string GetColoredSnrBrandText()
    {
        return string.IsNullOrWhiteSpace(UIConfig.ColorModName) ? SnrBrandColorFallback : UIConfig.ColorModName;
    }

    private static string ReplaceIgnoreCase(string source, string oldValue, string newValue)
    {
        if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(oldValue))
            return source;

        int startIndex = 0;
        while (true)
        {
            int index = source.IndexOf(oldValue, startIndex, StringComparison.OrdinalIgnoreCase);
            if (index < 0)
                return source;

            source = source.Substring(0, index) + newValue + source.Substring(index + oldValue.Length);
            startIndex = index + newValue.Length;
        }
    }

    private static void UpdateAnnouncementLogo(AnnouncementPopUp popup)
    {
        if (popup == null) return;

        Transform logoTransform = popup.transform.Find("SNRLogo");
        if (logoTransform == null)
        {
            var logoSprite = AssetManager.GetAsset<Sprite>("banner", AssetManager.AssetBundleType.Sprite);
            if (logoSprite == null) return;

            GameObject logoObject = new GameObject("SNRLogo");
            logoObject.transform.SetParent(popup.transform, false);

            var spriteRenderer = logoObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = logoSprite;
            spriteRenderer.sortingOrder = 10;
            logoTransform = logoObject.transform;
        }

        logoTransform.localPosition = SnrLogoLocalPosition;
        logoTransform.localScale = SnrLogoLocalScale;
        logoTransform.localRotation = Quaternion.identity;
        if (!logoTransform.gameObject.activeSelf)
            logoTransform.gameObject.SetActive(true);
    }

    private static void HideAnnouncementLogo(AnnouncementPopUp popup)
    {
        if (popup == null) return;

        Transform logoTransform = popup.transform.Find("SNRLogo");
        if (logoTransform != null)
        {
            logoTransform.gameObject.SetActive(false);
        }
    }
}

public class AnnouncementSelectMenuMarker : MonoBehaviour
{
}

internal static class AnnouncementSelectMenuState
{
    public static string CurrentCategory = string.Empty;
    public static List<Announcement> VanillaCache;
    public static List<Announcement> SnrCache;
    public static string SnrLang = string.Empty;
    public static bool SnrLoading;
    public static int SnrRequestToken;
}
