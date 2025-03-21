using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using SuperNewRoles.Modules;
using UnityEngine;
using UnityEngine.Events;

namespace SuperNewRoles.CustomCosmetics.UI;

public static class CustomCosmeticsUIStart
{
    private static class MenuPositions
    {
        public const float X_POSITION = -3.07f;
        public const float Y_POSITION = 2.153f;
        public const float Z_POSITION = -15f;
        public const float SCALE = 0.26f;
    }

    private const string MENU_SELECTOR_ASSET_NAME = "CosmeticMenuSelector";
    private static List<ICustomCosmeticsMenu> Menus = null;
    private static ICustomCosmeticsMenu CurrentMenu = null;

    public static void Start(PlayerCustomizationMenu menu)
    {
        Logger.Info("CustomCosmeticsUIStart Start");
        CurrentMenu = null;
        MenusInitialize();
        HideDefaultUI(menu);
        var menuObject = CreateMenuObject(menu);
        if (menuObject != null)
            SetupCategoryButtons(menuObject);
        CreateMenuFrame(menuObject, menu);
        HandleCategoryClick("cosmetic_costume", menuObject);
    }
    public static void Update(PlayerCustomizationMenu menu)
    {
        if (CurrentMenu != null)
            CurrentMenu.Update();
    }
    private static void MenusInitialize()
    {
        if (Menus != null)
        {
            return;
        }

        // AssemblyからCustomCosmeticsMenuBase<>を継承しているクラスを探す
        var customMenuTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => typeof(ICustomCosmeticsMenu).IsAssignableFrom(t)
                        && t != typeof(CustomCosmeticsMenuBase<>)
                        && IsDerivedFromCustomCosmeticsMenuBase(t))
            .ToList();

        Logger.Info($"Found {customMenuTypes.Count} custom cosmetics menus");

        BindingFlags flags = BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy;
        Menus = customMenuTypes
            .Select(t => t.GetProperty("Instance", flags)?.GetValue(null) as ICustomCosmeticsMenu)
            .Where(menu => menu != null)
            .ToList();

        Logger.Info($"Menus: {string.Join(", ", Menus.Select(m => m.MenuType))}");

        // ローカル関数：タイプがCustomCosmeticsMenuBase<>を継承しているか確認する
        bool IsDerivedFromCustomCosmeticsMenuBase(Type type)
        {
            while (type != null && type != typeof(object))
            {
                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(CustomCosmeticsMenuBase<>))
                {
                    return true;
                }
                type = type.BaseType;
            }
            return false;
        }
    }

    private static void HideDefaultUI(PlayerCustomizationMenu menu)
    {
        ModHelpers.SetActiveAllObject(menu.gameObject.GetChildren(), false);
        menu.transform.Find("Tint").gameObject.SetActive(true);
        menu.transform.Find("Background").gameObject.SetActive(true);
        menu.transform.Find("Background/RightPanel").localPosition = new(2.688f, 0, -15f);
        GameObject.Destroy(menu.transform.Find("Background/RightPanel").GetComponent<AspectPosition>());
        // ModHelpers.SetActiveAllObject(menu.transform.Find("Background").gameObject.GetChildren(), true);
        // menu.transform.Find("Background/RightPanel").gameObject.SetActive(false);
    }

    private static GameObject CreateMenuObject(PlayerCustomizationMenu menu)
    {
        var obj = GameObject.Instantiate(AssetManager.GetAsset<GameObject>(MENU_SELECTOR_ASSET_NAME));
        obj.transform.SetParent(menu.transform, false);
        obj.transform.localPosition = new Vector3(MenuPositions.X_POSITION, MenuPositions.Y_POSITION, MenuPositions.Z_POSITION);
        obj.transform.localScale = Vector3.one * MenuPositions.SCALE;
        return obj;
    }

    private static void SetupCategoryButtons(GameObject menuObject)
    {
        var categories = GetCategories(menuObject);
        foreach (var category in categories)
        {
            ConfigureButton(category, menuObject);
        }
    }

    private static GameObject[] GetCategories(GameObject menuObject)
    {
        if (menuObject == null) return new GameObject[0];

        var categories = new System.Collections.Generic.List<GameObject>();
        var transforms = menuObject.GetComponentsInChildren<Transform>();

        foreach (var transform in transforms)
        {
            if (transform.name.StartsWith("cosmetic_"))
            {
                categories.Add(transform.gameObject);
            }
        }

        return categories.ToArray();
    }

    private static void ConfigureButton(GameObject category, GameObject menuObject)
    {
        var button = category.AddComponent<PassiveButton>();
        SetupButtonEvents(button, category.name, menuObject);
    }

    private static void SetupButtonEvents(PassiveButton button, string categoryName, GameObject menuObject)
    {
        InitializeButtonEvents(button);
        ConfigureClickEvent(button, categoryName, menuObject);
        ConfigureHoverEvents(button, button.gameObject);
    }

    private static void InitializeButtonEvents(PassiveButton button)
    {
        button.OnClick = new();
        button.OnMouseOut = new();
        button.OnMouseOver = new();
    }

    private static void ConfigureClickEvent(PassiveButton button, string categoryName, GameObject menuObject)
    {
        button.OnClick.AddListener((UnityAction)(() =>
        {
            HandleCategoryClick(categoryName, menuObject);
        }));
    }

    private static void ConfigureHoverEvents(PassiveButton button, GameObject category)
    {
        button.OnMouseOver.AddListener((UnityAction)(() => SetCategoryHighlight(category, true)));
        button.OnMouseOut.AddListener((UnityAction)(() => SetCategoryHighlight(category, false)));
    }

    private static void HandleCategoryClick(string categoryName, GameObject menuObject)
    {
        Logger.Info($"Cosmetic Category Clicked: {categoryName}");
        SetCurrentTab(categoryName, menuObject);
        var menu = Menus.Find(m => "cosmetic_" + m.MenuType.ToString() == categoryName);
        if (CurrentMenu != null)
            CurrentMenu.Hide();
        CurrentMenu = menu;
        if (menu != null)
            menu.Initialize();
    }

    private static void SetCurrentTab(string categoryName, GameObject menuObject)
    {
        var selectedObject = menuObject.transform.Find("Selected")?.gameObject;
        if (selectedObject == null)
        {
            Logger.Error($"selectedObject is null: {categoryName}");
            return;
        }

        var categoryObject = menuObject.transform.Find(categoryName)?.gameObject;
        if (categoryObject == null)
        {
            Logger.Error($"categoryObject is null: {categoryName}");
            return;
        }

        selectedObject.SetActive(true);
        var position = selectedObject.transform.localPosition;
        position.x = categoryObject.transform.localPosition.x + 4.05f;
        selectedObject.transform.localPosition = position;
    }

    private static void SetCategoryHighlight(GameObject category, bool active)
    {
        if (category == null) return;

        var highlight = category.transform.Find("Highlight")?.gameObject;
        if (highlight != null && highlight.activeSelf != active)
        {
            highlight.SetActive(active);
        }
    }
    private static void CreateMenuFrame(GameObject menuObject, PlayerCustomizationMenu menu)
    {
        var frame = GameObject.Instantiate(AssetManager.GetAsset<GameObject>("CosmeticMenuFrame"), menu.transform);
        frame.transform.localPosition = new(0, -0.1f, -11.5f);
        frame.transform.localScale = Vector3.one * 0.28f;
    }


    [HarmonyPatch(typeof(PlayerCustomizationMenu), nameof(PlayerCustomizationMenu.Start))]
    public static class PlayerCustomizationMenu_Start_Patch
    {
        public static bool Prefix(PlayerCustomizationMenu __instance)
        {
            if ((bool)PlayerCustomizationMenu.Instance && PlayerCustomizationMenu.Instance != __instance)
            {
                UnityEngine.Object.Destroy(PlayerCustomizationMenu.Instance.gameObject);
            }
            else
            {
                PlayerCustomizationMenu.Instance = __instance;
            }
            ControllerManager.Instance.OpenOverlayMenu(__instance.name, __instance.BackButton, __instance.DefaultButtonSelected, __instance.ControllerSelectable);
            try
            {
                UpdateToDefault(__instance);
            }
            catch (Exception e)
            {
                Logger.Error("PlayerCustomizationMenu::Start: OpenTab failed");
                Logger.Error(e.Message);
            }
            if (DestroyableSingleton<HudManager>.InstanceExists)
            {
                DestroyableSingleton<HudManager>.Instance.PlayerCam.OverrideScreenShakeEnabled = false;
            }
            if (DestroyableSingleton<GameStartManager>.InstanceExists)
            {
                DestroyableSingleton<GameStartManager>.Instance.CloseGameOptionsMenus();
            }
            Start(__instance);
            return false;
        }
    }
    public static void UpdateToDefault(PlayerCustomizationMenu __instance)
    {
        bool flag = ActiveInputManager.currentControlType == ActiveInputManager.InputType.Joystick;
        __instance.glyphL.SetActive(false);
        __instance.glyphR.SetActive(false);
        ControllerManager.Instance.ClearDestroyedSelectableUiElements();
        __instance.PreviewArea.UpdateFromDataManager(PlayerMaterial.MaskType.None);
        __instance.PreviewArea.ToggleName(active: false);
        __instance.PreviewArea.gameObject.SetActive(true);
        __instance.nameplateMaskArea.SetActive(false);
        __instance.cubeArea.SetActive(false);
        __instance.equipButton.SetActive(false);
    }
    [HarmonyPatch(typeof(PlayerCustomizationMenu), nameof(PlayerCustomizationMenu.Update))]
    public static class PlayerCustomizationMenu_Update_Patch
    {
        public static bool Prefix(PlayerCustomizationMenu __instance)
        {
            if (ShipStatus.Instance)
                __instance.DestroyObj();
            Update(__instance);
            return false;
        }
    }
}