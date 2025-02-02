using System.Collections.Generic;
using HarmonyLib;
using SuperNewRoles.Modules;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace SuperNewRoles.CustomOptions;

/// <summary>
/// ロールオプションメニューのタイプを定義する列挙型
/// </summary>
public enum RoleOptionMenuType
{
    Impostor,
    Crewmate,
    Neutral,
}

/// <summary>
/// ロールオプションメニューのオブジェクトデータを管理するクラス
/// メニューのオブジェクト、タイトルテキスト、スクローラーなどの情報を保持
/// </summary>
public class RoleOptionMenuObjectData
{
    /// <summary>
    /// デフォルトのメニュースケール
    /// </summary>
    public const float DEFAULT_SCALE = 0.2f;

    /// <summary>
    /// タイトルテキストのスケール
    /// </summary>
    public const float TITLE_TEXT_SCALE = 0.7f;

    /// <summary>
    /// メニューのZ位置
    /// </summary>
    public const float Z_POSITION = -1f;

    /// <summary>
    /// メニューのGameObjectを取得
    /// </summary>
    public GameObject MenuObject { get; }

    /// <summary>
    /// タイトルテキストのコンポーネントを取得
    /// </summary>
    public TextMeshPro TitleText { get; }

    /// <summary>
    /// スクローラーコンポーネント
    /// </summary>
    public Scroller Scroller { get; set; }

    /// <summary>
    /// スクロール内部のGameObject
    /// </summary>
    public GameObject InnerScroll { get; set; }

    public Transform CurrentScrollParent { get; set; }
    public Dictionary<RoleOptionMenuType, GameObject> RoleScrollDictionary { get; } = new();
    public Dictionary<RoleOptionMenuType, float> ScrollPositionDictionary { get; } = new();
    public RoleOptionMenuType CurrentRoleType { get; set; }
    /// <summary>
    /// コンストラクター：メニューオブジェクトからデータを初期化
    /// </summary>
    public RoleOptionMenuObjectData(GameObject roleOptionMenuObject, RoleOptionMenuType currentRoleType)
    {
        MenuObject = roleOptionMenuObject;
        TitleText = roleOptionMenuObject.transform.Find("TitleText").GetComponent<TextMeshPro>();
        CurrentRoleType = currentRoleType;
    }
}

/// <summary>
/// ロールオプションメニューの管理と生成を行う静的クラス
/// </summary>
public static class RoleOptionMenu
{
    /// <summary>
    /// ロールオプションメニューのアセット名
    /// </summary>
    private const string ROLE_OPTION_MENU_ASSET_NAME = "RoleOptionMenu";

    /// <summary>
    /// ロール詳細ボタンのアセット名
    /// </summary>
    private const string ROLE_DETAIL_BUTTON_ASSET_NAME = "RoleDetailButton";

    /// <summary>
    /// スクロールバーの設定を定義する内部クラス
    /// </summary>
    private static class ScrollbarSettings
    {
        public const float SCALE = 5.3f;
        public const float X_POSITION = -30.7f;
        public const float Y_POSITION = 12f;
        public const float Z_POSITION = 0f;
    }

    /// <summary>
    /// 現在のロールオプションメニューのオブジェクトデータ
    /// </summary>
    public static RoleOptionMenuObjectData RoleOptionMenuObjectData;

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
    /// 指定されたタイプのロールオプションメニューを表示する
    /// </summary>
    /// <param name="type">表示するロールオプションメニューのタイプ</param>
    public static void ShowRoleOptionMenu(RoleOptionMenuType type)
    {
        // メニュー初期化チェック
        InitializeMenuIfNeeded(type);

        // 対象スクロールコンテンツの取得・生成
        GameObject targetScroll = GetOrCreateRoleScrollContent(type);

        // スクロール表示制御
        UpdateScrollVisibility(targetScroll);

        // スクロール位置とUI状態のリセット
        ResetScrollUIState(targetScroll, type);
    }

    private static void InitializeMenuIfNeeded(RoleOptionMenuType type)
    {
        if (IsRoleOptionMenuNull())
            RoleOptionMenuObjectData = InitializeRoleOptionMenuObject(type);
    }

    private static GameObject GetOrCreateRoleScrollContent(RoleOptionMenuType type)
    {
        if (!RoleOptionMenuObjectData.RoleScrollDictionary.TryGetValue(type, out GameObject scroll) || scroll == null)
            scroll = GenerateInitialContent(RoleOptionMenuObjectData, type);
        return scroll;
    }

    private static void UpdateScrollVisibility(GameObject targetScroll)
    {
        foreach (var entry in RoleOptionMenuObjectData.RoleScrollDictionary)
            entry.Value.SetActive(false);

        targetScroll.SetActive(true);
    }

    private static void ResetScrollUIState(GameObject targetScroll, RoleOptionMenuType type)
    {
        ReSyncScrollbarPosition(type);
        RoleOptionMenuObjectData.CurrentScrollParent = targetScroll.transform;
        UpdateMenuTitle(type);
        ShowMenu();
    }

    /// <summary>
    /// ロールオプションメニューを非表示にする
    /// </summary>
    public static void HideRoleOptionMenu()
    {
        if (IsRoleOptionMenuNull()) return;
        HideMenu();
    }

    /// <summary>
    /// ロールオプションメニューがnullかどうかをチェックする
    /// </summary>
    private static bool IsRoleOptionMenuNull() =>
        RoleOptionMenuObjectData == null || RoleOptionMenuObjectData.MenuObject == null;

    /// <summary>
    /// メニューのタイトルを指定されたタイプに更新する
    /// </summary>
    private static void UpdateMenuTitle(RoleOptionMenuType type)
    {
        RoleOptionMenuObjectData.TitleText.text = $"<b>{ModTranslation.GetString($"RoleOptionMenuType.{type}")}</b>";
    }

    /// <summary>
    /// メニューを表示する
    /// </summary>
    private static void ShowMenu() =>
        RoleOptionMenuObjectData.MenuObject.SetActive(true);

    /// <summary>
    /// メニューを非表示にする
    /// </summary>
    private static void HideMenu() =>
        RoleOptionMenuObjectData.MenuObject.SetActive(false);

    /// <summary>
    /// ロールオプションメニューオブジェクトを初期化する
    /// </summary>
    private static RoleOptionMenuObjectData InitializeRoleOptionMenuObject(RoleOptionMenuType type)
    {
        var menuObject = CreateBaseMenuObject();
        var data = new RoleOptionMenuObjectData(menuObject, type);
        ConfigureTitleText(data);
        ConfigureScrollbar(data);
        return data;
    }

    /// <summary>
    /// ベースとなるメニューオブジェクトを作成する
    /// パフォーマンス改善：キャッシュされたGameSettingMenuを使用
    /// </summary>
    private static GameObject CreateBaseMenuObject()
    {
        var obj = GameObject.Instantiate(AssetManager.GetAsset<GameObject>(ROLE_OPTION_MENU_ASSET_NAME));
        var gameSettingMenu = GetGameSettingMenu();
        if (gameSettingMenu != null)
        {
            obj.transform.SetParent(gameSettingMenu.transform);
            obj.transform.localScale = Vector3.one * RoleOptionMenuObjectData.DEFAULT_SCALE;
            obj.transform.localPosition = new(0, 0, RoleOptionMenuObjectData.Z_POSITION);
        }
        return obj;
    }

    /// <summary>
    /// タイトルテキストを設定する
    /// </summary>
    private static void ConfigureTitleText(RoleOptionMenuObjectData data)
    {
        data.TitleText.transform.localScale = Vector3.one * RoleOptionMenuObjectData.TITLE_TEXT_SCALE;
    }

    /// <summary>
    /// スクロールバーを設定する
    /// </summary>
    private static void ConfigureScrollbar(RoleOptionMenuObjectData data)
    {
        (data.Scroller, data.InnerScroll) = CreateScrollbar(data.MenuObject.transform);
        data.InnerScroll.transform.localPosition = new Vector3(data.InnerScroll.transform.localPosition.x, 0f, 0.2f);
        // Scrollerの位置を調整
        var scrollerParent = data.Scroller.transform.parent;
        var scrollbar = scrollerParent.transform.Find("UI_Scrollbar");
        var scrollbarTrack = scrollerParent.transform.Find("UI_ScrollbarTrack");
        scrollbar.localPosition = new Vector3(scrollbar.localPosition.x + 0.21f, scrollbar.localPosition.y, scrollbar.localPosition.z);
        scrollbarTrack.localPosition = new Vector3(scrollbarTrack.localPosition.x + 0.21f, scrollbarTrack.localPosition.y, scrollbarTrack.localPosition.z);
    }
    public static int GenCount = 30;
    /// <summary>
    /// 初期コンテンツを生成する
    /// </summary>
    private static GameObject GenerateInitialContent(RoleOptionMenuObjectData data, RoleOptionMenuType type)
    {
        var parent = new GameObject($"{type}_parent");
        parent.transform.SetParent(data.InnerScroll.transform);
        parent.transform.localScale = Vector3.one;
        parent.transform.localPosition = Vector3.zero;
        int index = 0;
        for (index = 0; index < GenCount; index++)
        {
            GenerateRoleDetailButton($"Role_{index + 1}_{type}", parent.transform, index);
        }
        data.Scroller.ContentYBounds.max = index < 25 ? 0 : (0.38f * ((index - 24) / 4 + 1)) - 0.5f;
        data.RoleScrollDictionary[type] = parent;
        return parent;
    }
    /// <summary>
    /// スクロールバーを作成する
    /// </summary>
    private static (Scroller scroller, GameObject innerscroll) CreateScrollbar(Transform parent)
    {
        var gameSettingMenu = GetGameSettingMenu();
        if (gameSettingMenu == null) return (null, null);

        var gameSettingsTab = gameSettingMenu.GameSettingsTab;
        if (gameSettingsTab == null) return (null, null);

        var tabCopy = CopyGameSettingsTab(gameSettingsTab.gameObject, parent);
        if (tabCopy == null) return (null, null);

        RemoveCloseButton(tabCopy);
        ClearScrollInnerContents(tabCopy);

        return GetScrollComponents(tabCopy);
    }

    /// <summary>
    /// ゲーム設定タブをコピーする
    /// </summary>
    private static GameObject CopyGameSettingsTab(GameObject gameSettingsTab, Transform parent)
    {
        if (gameSettingsTab == null || parent == null) return null;

        var tabCopy = GameObject.Instantiate(gameSettingsTab, parent, false);
        var localScale = Vector3.one * ScrollbarSettings.SCALE;
        var localPosition = new Vector3(ScrollbarSettings.X_POSITION, ScrollbarSettings.Y_POSITION, ScrollbarSettings.Z_POSITION);

        tabCopy.transform.localScale = localScale;
        tabCopy.transform.localPosition = localPosition;
        tabCopy.gameObject.SetActive(true);

        var Gradient = tabCopy.transform.Find("Gradient");
        if (Gradient != null)
        {
            Gradient.transform.localPosition = new(4.45f, -3.98f, -20f);
            Gradient.transform.localScale = new(0.552f, 0.6f, 7.2125f);
        }
        return tabCopy;
    }

    /// <summary>
    /// 閉じるボタンを削除する
    /// </summary>
    private static void RemoveCloseButton(GameObject tabCopy)
    {
        var closeButton = tabCopy.transform.Find("CloseButton");
        if (closeButton != null)
        {
            GameObject.Destroy(closeButton.gameObject);
        }
    }

    /// <summary>
    /// スクロール内部のコンテンツをクリアする
    /// </summary>
    private static void ClearScrollInnerContents(GameObject tabCopy)
    {
        if (tabCopy == null) return;

        var scrollInner = tabCopy.transform.Find("Scroller/SliderInner");
        if (scrollInner == null) return;

        // 一時的に親を無効化してパフォーマンスを向上
        var wasActive = scrollInner.gameObject.activeSelf;
        scrollInner.gameObject.SetActive(false);

        for (int i = scrollInner.childCount - 1; i >= 0; i--)
        {
            var child = scrollInner.GetChild(i);
            child.SetParent(null);
            GameObject.Destroy(child.gameObject);
        }

        scrollInner.gameObject.SetActive(wasActive);
    }

    /// <summary>
    /// スクロールコンポーネントを取得する
    /// </summary>
    private static (Scroller scroller, GameObject innerscroll) GetScrollComponents(GameObject tabCopy)
    {
        var scroller = tabCopy.GetComponentInChildren<Scroller>();
        var innerscroll = tabCopy.transform.Find("Scroller/SliderInner").gameObject;
        return (scroller, innerscroll);
    }

    /// <summary>
    /// ロール詳細ボタンを生成する
    /// </summary>
    private static GameObject GenerateRoleDetailButton(string roleName, Transform innerscroll, int index)
    {
        var obj = GameObject.Instantiate(AssetManager.GetAsset<GameObject>(ROLE_DETAIL_BUTTON_ASSET_NAME));
        obj.transform.SetParent(innerscroll);
        obj.transform.localScale = Vector3.one * 0.31f;

        // indexに基づいてボタンの位置を計算する
        // 横方向：初期x座標は-1.28で、1列ごとに1.05増加。4列まで表示する。
        // 縦方向：初期y座標は0.85で、1行ごとに0.4減少。
        int col = index % 4;   // 列番号を計算
        int row = index / 4;   // 行番号を計算
        float posX = -1.27f + col * 1.63f;
        float posY = 0.85f - row * 0.38f;
        obj.transform.localPosition = new Vector3(posX, posY, 0f);

        obj.transform.Find("Text").GetComponent<TextMeshPro>().text = roleName;
        var passiveButton = obj.AddComponent<PassiveButton>();
        passiveButton.OnClick = new();
        passiveButton.OnClick.AddListener((UnityAction)(() =>
        {
            Logger.Info($"Clicked {roleName}");
        }));
        passiveButton.OnMouseOut = new();
        passiveButton.OnMouseOver = new();
        return obj;
    }

    [HarmonyPatch(typeof(ModManager), nameof(ModManager.LateUpdate))]
    public static class RoleOptionMenuLateUpdatePatch
    {
        private static float DISPLAY_UPPER_LIMIT = 1.6f;
        private static float DISPLAY_LOWER_LIMIT = -2.06f;

        private static void Postfix()
        {
            // RoleOptionMenuObjectDataが存在し、InnerScrollが設定されている場合のみ処理を行う
            if (RoleOptionMenuObjectData != null && RoleOptionMenuObjectData.InnerScroll != null && cachedGameSettingMenu != null)
            {
                // InnerScroll内の各子オブジェクトについて、Scrollerから見た相対座標を基に表示/非表示を判定する
                for (int i = 0; i < RoleOptionMenuObjectData.CurrentScrollParent.childCount; i++)
                {
                    Transform child = RoleOptionMenuObjectData.CurrentScrollParent.GetChild(i);
                    // Scrollerの座標空間におけるchildの相対位置を取得
                    Vector3 relativePos = RoleOptionMenuObjectData.Scroller.transform.InverseTransformPoint(child.position);
                    if (i == 0)
                        Logger.Info($"child relative Y: {relativePos.y}");
                    // Scrollerの表示範囲に基づいて表示/非表示を決定
                    bool shouldDisplay = (relativePos.y < DISPLAY_UPPER_LIMIT && relativePos.y > DISPLAY_LOWER_LIMIT);
                    child.gameObject.SetActive(shouldDisplay);
                }
                RoleOptionMenuObjectData.ScrollPositionDictionary[RoleOptionMenuObjectData.CurrentRoleType] = RoleOptionMenuObjectData.InnerScroll.transform.localPosition.y;
            }
        }
    }
    private static void ReSyncScrollbarPosition(RoleOptionMenuType type)
    {
        if (RoleOptionMenuObjectData == null || RoleOptionMenuObjectData.InnerScroll == null || RoleOptionMenuObjectData.Scroller == null) return;
        var innerPos = RoleOptionMenuObjectData.InnerScroll.transform.localPosition;
        float targetY = RoleOptionMenuObjectData.ScrollPositionDictionary.TryGetValue(type, out float y) ? y : 0;
        RoleOptionMenuObjectData.InnerScroll.transform.localPosition = new(innerPos.x, targetY, innerPos.z);
        RoleOptionMenuObjectData.Scroller.UpdateScrollBars();
    }
}
