using System;
using System.Collections.Generic;
using System.Linq;
using SuperNewRoles.Modules;
using UnityEngine;
using UnityEngine.Events;

namespace SuperNewRoles.CustomOptions;

/// <summary>
/// カスタムオプションメニューの管理を行うクラス
/// ゲーム設定のUIとインタラクションを制御します
/// </summary>
public static class CustomOptionsMenu
{
    /// <summary>
    /// メニューカテゴリーの定数定義
    /// </summary>
    private static class MenuCategories
    {
        public const string VANILLA = "Setting_Vanilla";
        public const string CREWMATE = "Setting_Crewmate";
        public const string IMPOSTOR = "Setting_Impostor";
        public const string NEUTRAL = "Setting_Neutral";
        public const string STANDARD = "Setting_Standard";
        public const string EXCLUSIVITY = "Setting_Exclusivity";
        public const string Setting_Modifier = "Setting_Modifier";
        public const string Setting_Ghost = "Setting_Ghost";
    }

    /// <summary>
    /// メニューの位置とスケールの定数定義
    /// </summary>
    private static class MenuPositions
    {
        public const float X_POSITION = -3.042f;
        public const float Y_POSITION = 2.65f;
        public const float Z_POSITION = -1f;
        public const float SCALE = 0.31f;
    }

    private const string MENU_SELECTOR_ASSET_NAME = "OptionsMenuSelector";

    /// <summary>
    /// GameSettingMenuのキャッシュ
    /// </summary>
    private static GameSettingMenu cachedGameSettingMenu;

    /// <summary>
    /// GameSettingMenuを取得またはキャッシュから返す
    /// </summary>
    private static GameSettingMenu GetGameSettingMenu()
    {
        if (cachedGameSettingMenu == null)
        {
            cachedGameSettingMenu = GameObject.FindObjectOfType<GameSettingMenu>();
        }
        return cachedGameSettingMenu;
    }

    /// <summary>
    /// オプションメニューを表示する
    /// パフォーマンス改善：キャッシュを活用
    /// </summary>
    public static void ShowOptionsMenu()
    {
        var menuObject = CreateMenuObject();
        if (menuObject != null)
        {
            SetupCategoryButtons(menuObject);
        }
    }

    /// <summary>
    /// メニューオブジェクトを生成し、適切な位置に配置する
    /// パフォーマンス改善：キャッシュされたGameSettingMenuを使用
    /// </summary>
    private static GameObject CreateMenuObject()
    {
        var gameSettingMenu = GetGameSettingMenu();
        if (gameSettingMenu == null) return null;

        var obj = GameObject.Instantiate(AssetManager.GetAsset<GameObject>(MENU_SELECTOR_ASSET_NAME));
        obj.transform.SetParent(gameSettingMenu.transform, false);
        obj.transform.localPosition = new Vector3(MenuPositions.X_POSITION, MenuPositions.Y_POSITION, MenuPositions.Z_POSITION);
        obj.transform.localScale = Vector3.one * MenuPositions.SCALE;
        return obj;
    }

    /// <summary>
    /// カテゴリーボタンの設定を行う
    /// </summary>
    private static void SetupCategoryButtons(GameObject menuObject)
    {
        var categories = GetSettingCategories(menuObject);
        foreach (var category in categories)
        {
            ConfigureButton(category);
        }
    }

    /// <summary>
    /// 設定カテゴリーのGameObjectを取得する
    /// </summary>
    private static GameObject[] GetSettingCategories(GameObject menuObject)
    {
        if (menuObject == null) return new GameObject[0];

        var categories = new List<GameObject>();
        var transforms = menuObject.GetComponentsInChildren<Transform>();

        foreach (var transform in transforms)
        {
            if (transform.name.StartsWith("Setting_"))
            {
                categories.Add(transform.gameObject);
            }
        }

        return categories.ToArray();
    }

    /// <summary>
    /// ボタンの基本設定を行う
    /// </summary>
    private static void ConfigureButton(GameObject category)
    {
        var button = category.AddComponent<PassiveButton>();
        SetupButtonEvents(button, category.name);
    }

    /// <summary>
    /// ボタンのイベントを設定する
    /// </summary>
    private static void SetupButtonEvents(PassiveButton button, string categoryName)
    {
        InitializeButtonEvents(button);
        ConfigureClickEvent(button, categoryName);
        ConfigureHoverEvents(button, button.gameObject);
    }

    /// <summary>
    /// ボタンのイベントを初期化する
    /// </summary>
    private static void InitializeButtonEvents(PassiveButton button)
    {
        button.OnClick = new();
        button.OnMouseOut = new();
        button.OnMouseOver = new();
    }

    /// <summary>
    /// クリックイベントを設定する
    /// </summary>
    private static void ConfigureClickEvent(PassiveButton button, string categoryName)
    {
        button.OnClick.AddListener((UnityAction)(() =>
        {
            HandleCategoryClick(categoryName);
        }));
    }

    /// <summary>
    /// ホバーイベントを設定する
    /// </summary>
    private static void ConfigureHoverEvents(PassiveButton button, GameObject category)
    {
        button.OnMouseOver.AddListener((UnityAction)(() => SetCategoryHighlight(category, true)));
        button.OnMouseOut.AddListener((UnityAction)(() => SetCategoryHighlight(category, false)));
    }

    /// <summary>
    /// カテゴリークリック時の処理を行う
    /// </summary>
    private static void HandleCategoryClick(string categoryName)
    {
        Logger.Info($"Category Clicked: {categoryName}");

        OptionMenuBase.HideAll();
        SetVanillaTabActive(false);
        SetCurrentTab(categoryName);

        switch (categoryName)
        {
            case MenuCategories.VANILLA:
                SetVanillaTabActive(true);
                break;
            case MenuCategories.STANDARD:
                StandardOptionMenu.ShowStandardOptionMenu();
                break;
            case MenuCategories.EXCLUSIVITY:
                ExclusivityOptionMenu.ShowExclusivityOptionMenu();
                break;
            case MenuCategories.Setting_Modifier:
                ModifierOptionMenu.ShowModifierOptionMenu();
                break;
            case MenuCategories.Setting_Ghost:
                GhostOptionMenu.ShowGhostOptionMenu();
                break;
            default:
                if (IsRoleOptionMenuCategory(categoryName))
                {
                    RoleOptionMenu.ShowRoleOptionMenu(ConvertToRoleOptionMenuType(categoryName));
                }
                break;
        }
    }

    private static void SetCurrentTab(string categoryName)
    {
        var menuObject = GetGameSettingMenu()?.transform.Find(MENU_SELECTOR_ASSET_NAME + "(Clone)")?.gameObject;
        if (menuObject == null)
        {
            Logger.Error($"menuObject is null: {categoryName}");
            return;
        }

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
        position.x = categoryObject.transform.localPosition.x + 4.02f;
        selectedObject.transform.localPosition = position;
    }

    /// <summary>
    /// カテゴリーがロールオプションメニューに属するかチェックする
    /// </summary>
    private static bool IsRoleOptionMenuCategory(string categoryName) =>
        categoryName is MenuCategories.CREWMATE or MenuCategories.IMPOSTOR or MenuCategories.NEUTRAL;

    /// <summary>
    /// カテゴリー名をRoleOptionMenuTypeに変換する
    /// </summary>
    private static RoleOptionMenuType ConvertToRoleOptionMenuType(string categoryName) =>
        categoryName switch
        {
            MenuCategories.CREWMATE => RoleOptionMenuType.Crewmate,
            MenuCategories.IMPOSTOR => RoleOptionMenuType.Impostor,
            MenuCategories.NEUTRAL => RoleOptionMenuType.Neutral,
            _ => RoleOptionMenuType.Crewmate,
        };

    /// <summary>
    /// バニラタブの表示/非表示を切り替える
    /// パフォーマンス改善：キャッシュとnullチェックの強化
    /// </summary>
    private static void SetVanillaTabActive(bool active)
    {
        var gameSettingsMenu = GetGameSettingMenu();
        if (gameSettingsMenu == null) return;

        var components = new[]
        {
            "MainArea",
            "LeftPanel",
            "What Is This?",
            "GameSettingsLabel",
            "PanelSprite/LeftSideTint"
        };

        foreach (var component in components)
        {
            var obj = gameSettingsMenu.transform.Find(component)?.gameObject;
            if (obj != null)
            {
                // パフォーマンス改善：現在の状態と異なる場合のみSetActiveを呼び出す
                if (obj.activeSelf != active)
                {
                    obj.SetActive(active);
                }
            }
        }
    }

    /// <summary>
    /// カテゴリーのハイライト表示を制御する
    /// パフォーマンス改善：nullチェックと状態チェックの強化
    /// </summary>
    private static void SetCategoryHighlight(GameObject category, bool active)
    {
        if (category == null) return;

        var highlight = category.transform.Find("Highlight")?.gameObject;
        if (highlight != null && highlight.activeSelf != active)
        {
            highlight.SetActive(active);
        }
    }
}
