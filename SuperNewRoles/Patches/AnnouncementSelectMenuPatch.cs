using System;
using HarmonyLib;
using SuperNewRoles.HelpMenus;
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

internal static class AnnouncementSelectMenuHelper
{
    private const string MenuAssetName = "AnnounceMenuSelector";
    private const string DefaultCategory = "Announce_SNR";
    private const string AnnounceBgAssetName = "AnnounceBG";
    private const string AnnounceBgObjectName = "SNR_AnnounceBG";
    private const float MenuScale = 0.2f;
    private const float AnnounceBgScale = 0.55f;
    private const float AnnounceBgZ = -14f;
    private const float AnnounceBgFadeDuration = 0.06f;
    private static GameObject currentAnnounceBg;
    private static readonly Vector3 MenuFallbackPosition = new(-2.96f, 2.65f, 1.4382f);

    private static readonly string[] CategoryNames =
    {
        "Announce_Vanilla",
        "Announce_SNR"
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

    public static void SetMenuActive(AnnouncementPopUp popup, bool active)
    {
        var menuObject = FindMenu(popup);
        if (menuObject != null)
            menuObject.SetActive(active);
        if (!active)
            SetAnnounceBgActive(false);
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

        menuObject.transform.SetParent(parent, false);
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

        var selectedObject = menuObject.transform.Find("Selected")?.gameObject;
        var categoryObject = menuObject.transform.Find(categoryName)?.gameObject;
        if (selectedObject != null && categoryObject != null)
        {
            selectedObject.SetActive(true);
            var position = selectedObject.transform.localPosition;
            position.x = categoryObject.transform.localPosition.x + 4.02f;
            selectedObject.transform.localPosition = position;
        }

        var sizer = GameObject.FindObjectOfType<AnnouncementPopUp>()?.transform.Find("Sizer")?.gameObject;
        switch (categoryName)
        {
            case "Announce_Vanilla":
                sizer?.SetActive(true);
                SetAnnounceBgActive(false);
                break;
            case "Announce_SNR":
                sizer?.SetActive(false);
                EnsureAnnounceBg(menuObject);
                SetAnnounceBgActive(true);
                break;
        }
    }

    private static void SetCategoryHighlight(GameObject category, bool active)
    {
        if (category == null) return;
        var highlight = category.transform.Find("Highlight")?.gameObject;
        if (highlight != null && highlight.activeSelf != active)
            highlight.SetActive(active);
    }

    private static void SetAnnounceBgActive(bool active)
    {
        var bg = currentAnnounceBg;
        if (bg != null)
        {
            bool wasActive = bg.activeSelf;
            bg.SetActive(active);
            if (active && !wasActive)
            {
                var fade = bg.GetComponent<FadeCoroutine>();
                if (fade != null)
                    fade.StartFadeIn(bg, AnnounceBgFadeDuration);
            }
        }
    }

    private static GameObject EnsureAnnounceBg(GameObject menuObject)
    {
        if (currentAnnounceBg != null)
            return currentAnnounceBg;
        var asset = AssetManager.GetAsset<GameObject>(AnnounceBgAssetName);
        if (asset == null)
            return null;

        currentAnnounceBg = UnityEngine.Object.Instantiate(asset);
        currentAnnounceBg.name = AnnounceBgObjectName;
        currentAnnounceBg.transform.localPosition = new Vector3(0f, 0f, AnnounceBgZ);
        currentAnnounceBg.transform.localScale = Vector3.one * AnnounceBgScale;
        ConfigureBackgroundBlocker(currentAnnounceBg);
        SetupCloseButtons(currentAnnounceBg, menuObject);
        var fade = currentAnnounceBg.AddComponent<FadeCoroutine>();
        fade.StartFadeIn(currentAnnounceBg, AnnounceBgFadeDuration);
        return currentAnnounceBg;
    }

    private static void SetupCloseButtons(GameObject bg, GameObject menuObject)
    {
        ConfigureCloseButton(bg.transform.Find("CloseBox")?.gameObject, menuObject);
        ConfigureCloseButton(bg.transform.Find("CloseButton")?.gameObject, menuObject);
    }

    private static void ConfigureBackgroundBlocker(GameObject bg)
    {
        ConfigureEmptyButton(bg);
        ConfigureEmptyButton(bg.transform.Find("BG")?.gameObject);
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

    private static void ConfigureCloseButton(GameObject target, GameObject menuObject)
    {
        if (target == null) return;
        var button = target.GetComponent<PassiveButton>() ?? target.AddComponent<PassiveButton>();
        button.Colliders = new Collider2D[] { target.GetComponent<Collider2D>() };
        button.OnClick = new();
        button.OnMouseOver = new();
        button.OnMouseOut = new();
        button.OnClick.AddListener((UnityAction)(() => CloseAnnounceBg(menuObject)));
    }

    private static void CloseAnnounceBg(GameObject menuObject)
    {
        var bg = currentAnnounceBg;
        if (bg == null) return;

        var fade = bg.GetComponent<FadeCoroutine>() ?? bg.AddComponent<FadeCoroutine>();
        fade.StartFadeOut(bg, AnnounceBgFadeDuration, true);
        currentAnnounceBg = null;

        if (menuObject != null)
        {
            var state = menuObject.GetComponent<AnnouncementSelectMenuState>();
            if (state != null)
                state.CurrentCategory = DefaultCategory;
            GameObject.FindObjectOfType<AnnouncementPopUp>()?.Close();
        }
    }
}

internal sealed class AnnouncementSelectMenuMarker : MonoBehaviour
{
}

internal sealed class AnnouncementSelectMenuState : MonoBehaviour
{
    public string CurrentCategory = string.Empty;
}
