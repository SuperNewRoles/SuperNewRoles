using System.Collections.Generic;
using HarmonyLib;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;
using System;
using SuperNewRoles.CustomOptions.Data;

namespace SuperNewRoles.CustomOptions;

/// <summary>
/// ロールオプションメニューのタイプを定義する列挙型
/// </summary>
public enum RoleOptionMenuType
{
    Crewmate,
    Impostor,
    Neutral,
    Hidden,
    Ghost,
    Modifier,
}

/// <summary>
/// ロールオプションメニューのオブジェクトデータを管理するクラス
/// メニューのオブジェクト、タイトルテキスト、スクローラーなどの情報を保持
/// </summary>
public class RoleOptionMenuObjectData : OptionMenuBase
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

    public RoleId CurrentRoleId { get; set; }
    public TextMeshPro CurrentRoleNumbersOfCrewsText { get; set; }

    /// <summary>
    /// 現在選択中のロールの確率表示用TextMeshPro
    /// </summary>
    public TextMeshPro CurrentRolePercentageText { get; set; }

    /// <summary>
    /// メニューのBoxCollider2Dをキャッシュ
    /// </summary>
    public BoxCollider2D MenuObjectCollider { get; private set; }

    /// <summary>
    /// 設定メニューのスクローラーをキャッシュ
    /// </summary>
    public Scroller SettingsScroller { get; set; }
    public Transform SettingsInner { get; set; }
    public GameObject BulkRoleSettingsMenu { get; set; }

    /// <summary>
    /// 一括設定メニューのスクローラーとその内部コンテンツ
    /// </summary>
    public Scroller BulkSettingsScroller { get; set; }
    public int CurrentBulkSettingsIndex { get; set; }
    public Transform BulkSettingsInner { get; set; }
    public GameObject CurrentBulkSettingsParent { get; set; }

    /// <summary>
    /// 標準設定メニューのGameObject
    /// </summary>
    public GameObject StandardOptionMenu { get; set; }

    /// <summary>
    /// RoleIdとRoleDetailButtonの対応を保存するDictionary
    /// </summary>
    public Dictionary<RoleId, GameObject> RoleDetailButtonDictionary { get; } = new();

    /// <summary>
    /// 現在表示中の設定とそのテキストコンポーネントのリスト
    /// </summary>
    public List<(TextMeshPro Text, CustomOption Option)> CurrentOptionDisplays { get; } = new();

    /// <summary>
    /// コンストラクター：メニューオブジェクトからデータを初期化
    /// </summary>
    public RoleOptionMenuObjectData(GameObject roleOptionMenuObject, RoleOptionMenuType currentRoleType) : base()
    {
        MenuObject = roleOptionMenuObject;
        TitleText = roleOptionMenuObject.transform.Find("TitleText").GetComponent<TextMeshPro>();
        CurrentRoleType = currentRoleType;

        // BoxCollider2Dをキャッシュ
        MenuObjectCollider = roleOptionMenuObject.GetComponent<BoxCollider2D>();
    }
    public override void Hide()
    {
        if (MenuObject != null)
            MenuObject.SetActive(false);
        BulkRoleSettings.HideBulkRoleSettings();
    }

    public override void UpdateOptionDisplay()
    {
        if (RoleDetailButtonDictionary == null)
        {
            return;
        }
        foreach (var roleButton in RoleDetailButtonDictionary)
        {
            var roleOption = RoleOptionManager.RoleOptions.FirstOrDefault(x => x.RoleId == roleButton.Key);
            if (roleOption != null)
            {
                RoleOptionMenu.UpdateRoleDetailButtonColor(roleButton.Value.GetComponent<SpriteRenderer>(), roleOption);
            }
        }

        // RoleOptionSettingsの内容も更新
        if (CurrentRoleId != RoleId.None)
        {
            var currentRoleOption = RoleOptionManager.RoleOptions.FirstOrDefault(x => x.RoleId == CurrentRoleId);
            if (currentRoleOption != null)
            {
                RoleOptionMenu.UpdateNumOfCrewsSelect(currentRoleOption);
                RoleOptionSettings.UpdateSettingsDisplay(currentRoleOption);
            }
        }
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
    public const string ROLE_DETAIL_BUTTON_ASSET_NAME = "RoleDetailButton";

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
    public static GameSettingMenu GetGameSettingMenu()
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

        // CurrentRoleTypeを更新
        RoleOptionMenuObjectData.CurrentRoleType = type;

        // 一括設定メニューを非表示にする
        BulkRoleSettings.HideBulkRoleSettings();

        // 右側の設定を破棄
        if (RoleOptionMenuObjectData.CurrentRoleId != RoleId.None)
        {
            RoleOptionSettings.HideRoleSettings();
            RoleOptionMenuObjectData.CurrentRoleId = RoleId.None;
            RoleOptionMenuObjectData.CurrentRoleNumbersOfCrewsText = null;
            RoleOptionMenuObjectData.CurrentRolePercentageText = null;
        }

        // スクロール位置とUI状態のリセット
        ResetScrollUIState(targetScroll, type);
    }

    private static void InitializeMenuIfNeeded(RoleOptionMenuType type)
    {
        if (IsRoleOptionMenuNull())
        {
            RoleOptionMenuObjectData = InitializeRoleOptionMenuObject(type);
            // 役職数一括設定を開くボタンを初期化
            BulkRoleSettings.InitializeBulkRoleButton();
            // Scroll生成部分
            RoleOptionSettings.SetupScroll(RoleOptionMenu.RoleOptionMenuObjectData.MenuObject.transform);
        }
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

    /// <summary>
    /// ロールオプションメニューのスクロール位置と表示状態をリセットする
    /// </summary>
    private static void ResetScrollUIState(GameObject targetScroll, RoleOptionMenuType type)
    {
        RoleOptionMenuObjectData.CurrentScrollParent = targetScroll.transform;
        UpdateMenuTitle(type);
        // 子オブジェクト数に基づいてスクロール範囲を再計算
        int count = targetScroll.transform.childCount;
        RoleOptionMenuObjectData.Scroller.ContentYBounds.max = CalculateContentYBounds(count);
        // スクロール位置の前回位置に移動
        ReSyncScrollbarPosition(type);
        // メニューを表示
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
        RoleOptionMenuObjectData?.MenuObject == null;

    /// <summary>
    /// メニューのタイトルを指定されたタイプに更新する
    /// </summary>
    private static void UpdateMenuTitle(RoleOptionMenuType type)
    {
        RoleOptionMenuObjectData.TitleText.text = $"{ModTranslation.GetString($"RoleOptionMenuType.{type}")}";
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
        var mask = obj.transform.FindChild("Mask");
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
    public static int GenCount = 500;
    /// <summary>
    /// 初期コンテンツを生成する
    /// </summary>
    private static GameObject GenerateInitialContent(RoleOptionMenuObjectData data, RoleOptionMenuType type)
    {
        var parent = new GameObject($"{type}_parent");
        parent.transform.SetParent(data.InnerScroll.transform);
        parent.transform.localScale = Vector3.one;
        parent.transform.localPosition = Vector3.zero;

        // RoleOptionsからボタンを生成
        var roleOptions = RoleOptionManager.RoleOptions
            .Where(ro =>
            {
                var roleInfo = CustomRoleManager.AllRoles.FirstOrDefault(r => r.Role == ro.RoleId);
                return roleInfo != null && roleInfo.OptionTeam == type;
            })
            .ToArray();

        int index = 0;
        foreach (var roleOption in roleOptions)
        {
            var roleInfo = CustomRoleManager.AllRoles.FirstOrDefault(r => r.Role == roleOption.RoleId);
            if (roleInfo == null) continue;

            string roleName = ModTranslation.GetString($"{roleInfo.Role}");
            GenerateRoleDetailButton(roleName, parent.transform, index, roleOption);
            index++;
        }
        /*
                if (index < 25)
                {
                    for (int i = 0; i < 250; i++)
                    {
                        GenerateRoleDetailButton("ロールを追加", parent.transform, index, RoleOptionManager.RoleOptions.FirstOrDefault());
                        index++;
                    }
                }
        */
        // スクロール範囲の調整
        data.Scroller.ContentYBounds.max = CalculateContentYBounds(index);
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
            Gradient.transform.localPosition = new(5.77f, -3.98f, -20f);
            Gradient.transform.localScale = new(0.774f, 0.6f, 7.2125f);
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
    public static void UpdateRoleDetailButtonColor(SpriteRenderer spriteRenderer, RoleOptionManager.RoleOption roleOption)
    {
        if (roleOption == null) throw new Exception("roleOption is null");
        if (spriteRenderer == null) throw new Exception("spriteRenderer is null");
        if (roleOption.NumberOfCrews > 0 && roleOption.Percentage > 0)
            spriteRenderer.color = Color.white;
        else
            spriteRenderer.color = new Color(1, 1f, 1f, 0.6f);
    }
    /// <summary>
    /// ロール詳細ボタンを生成する
    /// </summary>
    private static GameObject GenerateRoleDetailButton(string roleName, Transform innerscroll, int index, RoleOptionManager.RoleOption roleOption)
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
        obj.transform.localPosition = new Vector3(posX, posY, -0.21f);
        obj.transform.Find("Text").GetComponent<TextMeshPro>().text = $"<b><color=#{ColorUtility.ToHtmlStringRGB(roleOption.RoleColor)}>{roleName}</color></b>";
        var passiveButton = obj.AddComponent<PassiveButton>();
        passiveButton.Colliders = new Collider2D[1];
        passiveButton.Colliders[0] = obj.GetComponent<BoxCollider2D>();
        passiveButton.OnClick = new();
        GameObject SelectedObject = null;
        var spriteRenderer = obj.GetComponent<SpriteRenderer>();
        UpdateRoleDetailButtonColor(spriteRenderer, roleOption);
        passiveButton.OnClick.AddListener((UnityAction)(() =>
        {
            // TODO: ロールの設定画面を表示する処理を追加
            RoleOptionSettings.ClickedRole(roleOption);
        }));
        passiveButton.OnMouseOut = new();
        passiveButton.OnMouseOut.AddListener((UnityAction)(() =>
        {
            if (SelectedObject == null)
                SelectedObject = obj.transform.FindChild("Selected").gameObject;
            SelectedObject.SetActive(false);
        }));
        passiveButton.OnMouseOver = new();
        passiveButton.OnMouseOver.AddListener((UnityAction)(() =>
        {
            if (SelectedObject == null)
                SelectedObject = obj.transform.FindChild("Selected").gameObject;
            SelectedObject.SetActive(true);
        }));

        // 右クリック検知用のコンポーネントを追加し、イベントを登録
        var rightClickDetector = obj.AddComponent<RightClickDetector>();
        rightClickDetector.OnRightClick.AddListener((UnityAction)(() =>
        {
            // 対象が有効のとき
            if (roleOption.NumberOfCrews >= 1)
            {
                roleOption.NumberOfCrews = 0;
                roleOption.Percentage = 0;
            }
            else
            {
                roleOption.NumberOfCrews = 1;
                roleOption.Percentage = 100;
            }
            UpdateRoleDetailButtonColor(spriteRenderer, roleOption);
            UpdateNumOfCrewsSelect(roleOption);
            RoleOptionManager.RpcSyncRoleOptionDelay(roleOption.RoleId, roleOption.NumberOfCrews, roleOption.Percentage);
        }));

        // RoleDetailButtonDictionaryに追加
        if (roleOption != null)
            RoleOptionMenuObjectData.RoleDetailButtonDictionary[roleOption.RoleId] = obj;

        return obj;
    }
    [HarmonyPatch(typeof(GameSettingMenu), nameof(GameSettingMenu.Start))]
    public static class RoleOptionMenuStartPatch
    {
        public static void Postfix()
        {
            CustomOptionsMenu.ShowOptionsMenu();
            // マスクが邪魔されるので非表示に
            UpdateHostInfoMaskArea(false);
        }
    }
    [HarmonyPatch(typeof(GameSettingMenu), nameof(GameSettingMenu.Close))]
    public static class RoleOptionMenuClosePatch
    {
        public static void Postfix()
        {
            UpdateHostInfoMaskArea(true);
            // 設定を保存
            if (CustomOptionSaver.IsLoaded)
                CustomOptionSaver.Save();
        }
    }
    public static void UpdateHostInfoMaskArea(bool active)
    {
        if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Joined)
            return;
        var maskArea = GameStartManager.Instance.transform.FindChild("StartGameArea/Host Info/Content/Player Area/MaskArea");
        if (maskArea != null)
            maskArea.gameObject.SetActive(active);
    }
    public static void UpdateNumOfCrewsSelect(RoleOptionManager.RoleOption roleOption)
    {
        if (RoleOptionMenuObjectData == null) return;
        if (RoleOptionMenuObjectData.CurrentRoleNumbersOfCrewsText != null && RoleOptionMenuObjectData.CurrentRoleId == roleOption.RoleId)
        {
            RoleOptionMenuObjectData.CurrentRoleNumbersOfCrewsText.text = ModTranslation.GetString("NumberOfCrewsSelected", roleOption.NumberOfCrews);
            if (RoleOptionMenuObjectData.CurrentRolePercentageText != null)
            {
                RoleOptionMenuObjectData.CurrentRolePercentageText.text = roleOption.Percentage + "%";
            }
        }
    }
    [HarmonyPatch(typeof(ModManager), nameof(ModManager.LateUpdate))]
    public static class RoleOptionMenuLateUpdatePatch
    {
        private static float DISPLAY_UPPER_LIMIT = 1.6f;
        private static float DISPLAY_LOWER_LIMIT = -2.3f;

        private static void Postfix()
        {
            // RoleOptionMenuObjectDataが存在し、InnerScrollが設定されている場合のみ処理を行う
            var data = RoleOptionMenuObjectData;
            if (data != null && data.InnerScroll != null && cachedGameSettingMenu != null)
            {
                Transform currentScrollParent = data.CurrentScrollParent;
                Transform scrollerTransform = data.Scroller.transform;
                int childCount = currentScrollParent.childCount;
                for (int i = 0; i < childCount; i++)
                {
                    Transform child = currentScrollParent.GetChild(i);
                    // Scrollerの座標空間におけるchildの相対位置を取得（Transformのキャッシュによる最適化）
                    Vector3 relativePos = scrollerTransform.InverseTransformPoint(child.position);
                    // Scrollerの表示範囲に基づいて表示/非表示を決定
                    bool shouldDisplay = (relativePos.y < DISPLAY_UPPER_LIMIT && relativePos.y > DISPLAY_LOWER_LIMIT);
                    // 現在の状態と比較して変更が必要な場合のみSetActiveを呼び出す
                    if (child.gameObject.activeSelf != shouldDisplay)
                    {
                        child.gameObject.SetActive(shouldDisplay);
                    }
                }
                // InnerScrollのTransformもキャッシュして辞書を更新
                Transform innerScrollTransform = data.InnerScroll.transform;
                data.ScrollPositionDictionary[data.CurrentRoleType] = innerScrollTransform.localPosition.y;
                Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                // キャッシュしたColliderを使用
                if (data.MenuObjectCollider != null && data.MenuObjectCollider.OverlapPoint(mousePos))
                {
                    // メニュー上にマウスがある場合、メニューのスクロールを有効化し、設定メニューのスクロールを無効化
                    data.Scroller.enabled = true;
                    if (data.SettingsScroller != null)
                    {
                        data.SettingsScroller.enabled = false;
                    }
                }
                else
                {
                    // メニュー外にマウスがある場合、メニューのスクロールを無効化し、設定メニューのスクロールを有効化
                    data.Scroller.enabled = false;
                    if (data.SettingsScroller != null)
                    {
                        data.SettingsScroller.enabled = true;
                    }
                }
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

    // 新規関数: 指定された子オブジェクト数からContentYBounds.max値を計算する
    private static float CalculateContentYBounds(int count)
    {
        return count < 25 ? 0f : (0.38f * ((count - 24) / 4 + 1)) - 0.25f;
    }
}
