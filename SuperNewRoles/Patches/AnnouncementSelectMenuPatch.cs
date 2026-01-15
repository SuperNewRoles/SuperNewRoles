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
    private const float MenuScale = 0.23f;
    private static readonly Vector3 MenuFallbackPosition = new(-2.82f, 1.97f, -1.15f);

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

        switch (categoryName)
        {
            case "Announce_Vanilla":
                break;
            case "Announce_SNR":
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
}
