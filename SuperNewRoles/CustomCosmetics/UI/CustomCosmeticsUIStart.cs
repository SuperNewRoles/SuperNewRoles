using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AmongUs.Data;
using HarmonyLib;
using Rewired;
using SuperNewRoles.CustomCosmetics.CosmeticsPlayer;
using SuperNewRoles.Modules;
using UnityEngine;
using UnityEngine.Events;

namespace SuperNewRoles.CustomCosmetics.UI;

public static class CustomCosmeticsUIStart
{
    public enum FrameType
    {
        Main,
        Category,
    }
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
    private static GameObject MenuObject = null;
    public static Dictionary<FrameType, GameObject> FrameObjects = new();
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
        HandleCategoryClick($"cosmetic_{CustomCosmeticsMenuType.costume}", menuObject);
        MenuObject = menuObject;
    }
    public static void Update(PlayerCustomizationMenu menu)
    {
        (int width, int height) = ModHelpers.GetAspectRatio(Screen.width, Screen.height);
        if (Math.Abs(width - height) <= 2)
            menu.transform.localScale = Vector3.one * 0.82f;
        else
            menu.transform.localScale = Vector3.one;

        // 現在のメニューが存在する場合は更新
        CurrentMenu?.Update();

        var player = ReInput.players.GetPlayer(0);

        // 進む: ボタンID 34
        if (player.GetButtonDown(34))
        {
            SwitchMenu(menu, 1);
        }
        // 戻る: ボタンID 35
        else if (player.GetButtonDown(35))
        {
            SwitchMenu(menu, -1);
        }
        else if (player.GetButtonDown(29) && CurrentMenu?.MenuType == CustomCosmeticsMenuType.cube)
        {
            menu.ViewCube();
        }
    }

    /// <summary>
    /// オフセットに応じたメニュー切替処理を行うヘルパー
    /// </summary>
    private static void SwitchMenu(PlayerCustomizationMenu menu, int offset)
    {
        if (Menus == null || Menus.Count == 0)
        {
            Logger.Warning("No menus available.");
            return;
        }

        int currentIndex = Menus.IndexOf(CurrentMenu);
        int newIndex = (currentIndex + offset + Menus.Count) % Menus.Count;
        var selectedMenu = Menus[newIndex];

        HandleCategoryClick("cosmetic_" + selectedMenu.MenuType.ToString(), MenuObject);
    }

    private static void MenusInitialize()
    {
        if (Menus != null)
        {
            return;
        }

        // AssemblyからCustomCosmeticsMenuBase<>を継承しているクラスを探す
        var customMenuTypes = SuperNewRolesPlugin.Assembly
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
            .OrderBy(menu => menu.MenuType)
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
        obj.transform.localScale = Vector3.one * MenuPositions.SCALE;
        var aspectPosition = obj.AddComponent<AspectPosition>();
        aspectPosition.Alignment = AspectPosition.EdgeAlignments.Center;
        aspectPosition.DistanceFromEdge = new(MenuPositions.X_POSITION, MenuPositions.Y_POSITION, MenuPositions.Z_POSITION);
        aspectPosition.OnEnable();
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

        PlayerCustomizationMenu.Instance.PreviewArea.UpdateFromDataManager(PlayerMaterial.MaskType.None);
        SetFrameType(FrameType.Main);

        var menu = Menus.Find(m => "cosmetic_" + m.MenuType.ToString() == categoryName);
        if (CurrentMenu != null)
            CurrentMenu.Hide();
        CurrentMenu = menu;
        if (menu != null)
            menu.Initialize();
    }

    public static void SetFrameType(FrameType frameType)
    {
        foreach (var frame in FrameObjects)
        {
            frame.Value.SetActive(frame.Key == frameType);
        }
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
        foreach (FrameType frameType in Enum.GetValues(typeof(FrameType)))
        {
            string frameName = $"CosmeticMenuFrame{frameType}";
            var frame = GameObject.Instantiate(AssetManager.GetAsset<GameObject>(frameName), menu.transform);
            frame.transform.localScale = Vector3.one * 0.28f;
            var aspectPosition = frame.AddComponent<AspectPosition>();
            aspectPosition.Alignment = AspectPosition.EdgeAlignments.Center;
            aspectPosition.DistanceFromEdge = new(0, -0.1f, -11.5f);
            aspectPosition.OnEnable();

            var closeButton = frame.transform.Find("CloseButtonCosmetics");
            PassiveButton closeButtonButton = closeButton.gameObject.AddComponent<PassiveButton>();
            closeButtonButton.Colliders = new Collider2D[] { closeButton.gameObject.GetComponent<Collider2D>() };
            closeButtonButton.OnClick = new();
            closeButtonButton.OnMouseOut = new();
            closeButtonButton.OnMouseOver = new();
            closeButtonButton.OnClick.AddListener((UnityAction)(() =>
            {
                menu.Close(true);
            }));

            frame.SetActive(false);
            FrameObjects[frameType] = frame;
        }
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
        __instance.PreviewArea.SetPetIdle(PlayerControl.LocalPlayer != null ? PlayerControl.LocalPlayer.Data.DefaultOutfit.PetId : DataManager.Player.Customization.Pet, PlayerControl.LocalPlayer != null ? PlayerControl.LocalPlayer.CurrentOutfit.ColorId : DataManager.Player.Customization.Color);
        __instance.PreviewArea.ToggleName(active: false);
        __instance.PreviewArea.TogglePet(active: true);
        __instance.PreviewArea.gameObject.SetActive(true);
        __instance.nameplateMaskArea.SetActive(false);
        __instance.cubeArea.SetActive(false);
        __instance.equipButton.SetActive(false);
    }
    [HarmonyPatch(typeof(PlayerCustomizationMenu), nameof(PlayerCustomizationMenu.OnDestroy))]
    public static class PlayerCustomizationMenu_OnDestroy_Patch
    {
        public static void Postfix(PlayerCustomizationMenu __instance)
        {
            if (PlayerControl.LocalPlayer == null) return;
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                CustomCosmeticsLayer customCosmeticsLayer = CustomCosmeticsLayers.ExistsOrInitialize(player.cosmetics);
                bool oldActive = customCosmeticsLayer?.ModdedCosmetics?.activeInHierarchy ?? true;
                customCosmeticsLayer?.ModdedCosmetics?.SetActive(false);
                customCosmeticsLayer.ModdedCosmetics?.SetActive(oldActive);
            }
        }
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