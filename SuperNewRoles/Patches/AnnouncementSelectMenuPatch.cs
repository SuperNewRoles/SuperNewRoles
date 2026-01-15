using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using AmongUs.Data;
using Assets.InnerNet;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using HarmonyLib;
using SuperNewRoles;
using SuperNewRoles.Modules;
using UnityEngine;
using UnityEngine.Events;

namespace SuperNewRoles.Patches;

[HarmonyPatch(typeof(AnnouncementPopUp), "OnEnable")]
public static class AnnouncementPopUpOnEnablePatch
{
    public static void Postfix(AnnouncementPopUp __instance)
    {
        AnnouncementSelectMenuHelper.EnsureMenu(__instance);
    }
}

[HarmonyPatch(typeof(AnnouncementPopUp), "OnDisable")]
public static class AnnouncementPopUpOnDisablePatch
{
    public static void Postfix(AnnouncementPopUp __instance)
    {
        AnnouncementSelectMenuHelper.SetMenuActive(__instance, false);
    }
}

[HarmonyPatch(typeof(AnnouncementPopUp), "SetMenu")]
public static class AnnouncementPopUpSetMenuPatch
{
    public static void Postfix(AnnouncementPopUp __instance)
    {
        AnnouncementSelectMenuHelper.OnMenuSet(__instance);
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

    private static readonly System.Reflection.MethodInfo CreateAnnouncementListMethod =
        AccessTools.Method(typeof(AnnouncementPopUp), "CreateAnnouncementList");
    private static readonly System.Reflection.MethodInfo UpdateAnnouncementTextMethod =
        AccessTools.Method(typeof(AnnouncementPopUp), "UpdateAnnouncementText");

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
        if (state.SnrCache != null &&
            state.SnrCache.Count > 0 &&
            string.Equals(state.SnrLang, lang, StringComparison.Ordinal))
        {
            ApplyAnnouncements(popup, state.SnrCache);
            return;
        }

        if (state.SnrLoading)
        {
            ApplyAnnouncements(popup, CreateLoadingAnnouncements());
            return;
        }

        state.SnrLoading = true;
        state.SnrLang = lang;
        state.SnrRequestToken++;
        int token = state.SnrRequestToken;

        ApplyAnnouncements(popup, CreateLoadingAnnouncements());
        popup.StartCoroutine(LoadSnrAnnouncements(popup, state, lang, token).WrapToIl2Cpp());
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

        int fallbackIndex = 0;
        foreach (var item in listResult.Data.Items)
        {
            ApiResult<Article> articleResult = null;
            yield return SuperNewAnnounceApi.GetArticle(item.Id, lang, result => articleResult = result, fallback: true);

            if (state == null || state.SnrRequestToken != requestToken)
                yield break;

            if (articleResult != null && articleResult.IsSuccess && articleResult.Data != null)
            {
                announcements.Add(ConvertToAnnouncement(articleResult.Data, fallbackIndex));
            }
            fallbackIndex++;
        }

        state.SnrLoading = false;
        state.SnrCache = announcements;
        state.SnrLang = lang;

        if (state.CurrentCategory == SnrCategory)
            ApplyAnnouncements(popup, announcements);
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

        CreateAnnouncementListMethod?.Invoke(popup, null);

        var announcements = DataManager.Player?.Announcements?.AllAnnouncements;
        if (announcements == null || announcements.Count == 0)
            return;

        bool previewOnly = ActiveInputManager.currentControlType == ActiveInputManager.InputType.Joystick;
        UpdateAnnouncementTextMethod?.Invoke(popup, new object[] { announcements[0].Number, previewOnly });
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
        string shortTitle = MakeShortTitle(title);
        string body = string.IsNullOrWhiteSpace(article?.Body) ? string.Empty : article.Body;
        if (string.IsNullOrWhiteSpace(body))
            body = string.IsNullOrWhiteSpace(article?.Url) ? title : article.Url;

        return new Announcement
        {
            Id = article?.Id ?? string.Empty,
            Language = GetCurrentLanguageId(),
            Number = BuildSnrNumber(article?.Id, fallbackIndex),
            Title = title,
            SubTitle = string.Empty,
            ShortTitle = shortTitle,
            PinState = false,
            Text = body,
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
