using UnityEngine;
using TMPro;
using SuperNewRoles.Modules;
using SuperNewRoles.Patches;
using UnityEngine.Events;
using System;
using System.Reflection;
using System.Linq;
using HarmonyLib;
using System.Collections.Generic;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Roles;
using InnerNet;

namespace SuperNewRoles.HelpMenus;

public static class HelpMenuObjectManager
{
    private static GameObject helpMenuObject;
    private static GameObject directRoleDetailObject;
    private static GameObject helpMenuBackdropObject;
    private static bool isDirectRoleDetailMode;
    private static bool isWaitingForIntroDisplay;
    public static FadeCoroutine fadeCoroutine;
    public static HelpMenuCategoryBase[] categories;
    public static Dictionary<string, GameObject> selectedButtons;
    public static HelpMenuCategoryBase? CurrentCategory;
    public const HelpMenuCategory DEFAULT_MENU_GAME = HelpMenuCategory.MyRoleInfomation;
    public const HelpMenuCategory DEFAULT_MENU_LOBBY = HelpMenuCategory.AssignmentsSettingInfomation;
    public static bool IsHelpMenuActive => helpMenuObject != null && fadeCoroutine != null && fadeCoroutine.isActive;

    public static bool CanToggleHelpMenu()
    {
        if (AmongUsClient.Instance == null || HudManager.Instance == null)
            return false;

        if (HudManager.Instance.IsIntroDisplayed)
            return false;

        return !(AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started && isWaitingForIntroDisplay);
    }

    private static void Initialize()
    {
        if (categories == null || categories.Length == 0)
            SetUpCategories();
        CurrentCategory = null;
        var parent = HudManager.Instance.transform;
        helpMenuObject = GameObject.Instantiate(AssetManager.GetAsset<GameObject>("HelpMenuObject"), parent);
        helpMenuObject.transform.localPosition = new Vector3(0f, 0f, -500f);
        helpMenuObject.transform.localScale = Vector3.one;
        helpMenuObject.transform.localRotation = Quaternion.identity;
        helpMenuObject.SetActive(true);

        helpMenuObject.AddComponent<HelpMenuObjectComponent>();
        // フェードイン処理
        fadeCoroutine = helpMenuObject.AddComponent<FadeCoroutine>();
        fadeCoroutine.StartFadeIn(helpMenuObject, 0.115f);

        // 左側のボタンをセットアップ
        SetUpLeftButtons();

        // AirColliderにPassiveButtonを設定して後ろの判定をクリックできないようにする。
        var airCollider = helpMenuObject.transform.Find("AirCollider").gameObject;
        var airPassiveButton = airCollider.AddComponent<PassiveButton>();
        airPassiveButton.Colliders = new Collider2D[1];
        airPassiveButton.Colliders[0] = airCollider.GetComponent<BoxCollider2D>();
        airPassiveButton.OnClick = new();
        airPassiveButton.OnMouseOver = new();
        airPassiveButton.OnMouseOut = new();

        // 外側クリックで閉じられる透明オーバーレイを置く。
        // 通常ヘルプでは見た目だけ出さず、役職説明時のみ暗転する。
        // メニュー本体のフェード対象外にするため、ヘルプメニューの兄弟として生成する。
        SetHelpMenuBackdropState(menuOpen: true, dimmed: false);

        var defaultNow =
            AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started
            ? DEFAULT_MENU_GAME : DEFAULT_MENU_LOBBY;
        CurrentCategory = categories.FirstOrDefault(c => c.Category == defaultNow);
        CurrentCategory.Show(helpMenuObject.transform.Find("RightContainer").gameObject);
        CurrentCategory.UpdateShow();
    }
    private static void SetUpCategories()
    {
        // HelpMenuCategoryBaseを継承した全ての型を取得
        var categoryTypes = SuperNewRolesPlugin.Assembly.GetTypes()
            .Where(type => type.IsSubclassOf(typeof(HelpMenuCategoryBase)) && !type.IsAbstract)
            .ToArray();

        // 各カテゴリのインスタンスを作成して配列に格納
        categories = new HelpMenuCategoryBase[categoryTypes.Length];
        for (int i = 0; i < categoryTypes.Length; i++)
        {
            categories[i] = (HelpMenuCategoryBase)Activator.CreateInstance(categoryTypes[i]);
        }
        categories = categories.OrderBy(x => (int)x.Category).ToArray();
    }
    private static void SetUpLeftButtons()
    {
        // LeftButtonsを取得
        var leftButtons = helpMenuObject.transform.Find("LeftButtons").gameObject;

        // BulkRoleButtonをアセットから取得
        var bulkRoleButtonAsset = AssetManager.GetAsset<GameObject>("BulkRoleButton");

        var rightContainer = helpMenuObject.transform.Find("RightContainer").gameObject;

        selectedButtons = new();

        // ボタンを設定
        for (int i = 0; i < categories.Length; i++)
        {
            // BulkRoleButtonを複製して設置
            var bulkRoleButton = GameObject.Instantiate(bulkRoleButtonAsset, leftButtons.transform);
            bulkRoleButton.name = $"HelpMenuButton_{categories[i].Name}";
            bulkRoleButton.transform.localScale = Vector3.one * 0.36f;
            bulkRoleButton.transform.localPosition = new Vector3(-2.83f, (categories.Length - 1) * 0.25f - i * 0.5f, 0f); // Y座標を0を中心に0.5ずつずらして配置

            // テキストを設定
            bulkRoleButton.transform.Find("Text").GetComponent<TextMeshPro>().text =
            $"{ModTranslation.GetString($"HelpMenu.{categories[i].Name}")}";

            // Selectedオブジェクトを取得
            GameObject selectedObject = bulkRoleButton.transform.Find("Selected").gameObject;
            selectedButtons[categories[i].Name] = selectedObject;
            if (CurrentCategory == categories[i])
            {
                selectedObject.SetActive(true);
            }

            // PassiveButtonを追加
            var passiveButton = bulkRoleButton.AddComponent<PassiveButton>();
            passiveButton.Colliders = new Collider2D[1];
            passiveButton.Colliders[0] = bulkRoleButton.GetComponent<BoxCollider2D>();
            passiveButton.OnClick = new();

            // クリック時の処理
            int index = i; // ループ変数をキャプチャしないようにコピー
            passiveButton.OnClick.AddListener((UnityAction)(() =>
            {
                if (CurrentCategory != null && CurrentCategory != categories[index])
                {
                    CurrentCategory.Hide(rightContainer);
                    if (selectedButtons.TryGetValue(CurrentCategory.Name, out var oldSelectedObject))
                        oldSelectedObject.SetActive(false);
                }
                CurrentCategory = categories[index];
                CurrentCategory.Show(rightContainer);
                CurrentCategory.UpdateShow();

                // 選択されたボタンのSelectedを表示する
                selectedObject.SetActive(true);
            }));

            // 初期状態でCurrentCategoryと一致する場合のみSelectedを表示
            if (CurrentCategory == categories[i])
            {
                selectedObject.SetActive(true);
            }
            else
            {
                selectedObject.SetActive(false);
            }

            // マウスオーバー時の処理
            passiveButton.OnMouseOver = new();
            passiveButton.OnMouseOver.AddListener((UnityAction)(() =>
            {
                selectedObject.SetActive(true);
            }));

            // マウスアウト時の処理
            passiveButton.OnMouseOut = new();
            passiveButton.OnMouseOut.AddListener((UnityAction)(() =>
            {
                if (CurrentCategory != categories[index]) // CurrentCategoryと異なる場合のみ非表示にする
                {
                    selectedObject.SetActive(false);
                }
            }));
        }
    }

    /// <summary>
    /// カテゴリ一覧を表示せず、指定した役職の説明をヘルプメニュー全体に表示する。
    /// </summary>
    public static void ShowRoleDetail(RoleId roleId)
    {
        if (!IsHelpMenuActive && !CanToggleHelpMenu())
            return;

        bool needsFadeIn = helpMenuObject == null || fadeCoroutine == null || !fadeCoroutine.isActive;
        if (helpMenuObject == null)
            Initialize();

        var rightContainer = helpMenuObject.transform.Find("RightContainer")?.gameObject;
        if (rightContainer == null)
        {
            Logger.Error("HelpMenuObject/RightContainer が見つかりませんでした。");
            return;
        }

        if (isDirectRoleDetailMode && directRoleDetailObject != null)
            GameObject.Destroy(directRoleDetailObject);
        else
            CurrentCategory?.Hide(rightContainer);

        SetCategoryNavigationActive(false);
        isDirectRoleDetailMode = true;
        SetHelpMenuBackdropState(menuOpen: true, dimmed: true);
        directRoleDetailObject = RoleDetailHelper.ShowRoleDetail(
            roleId,
            rightContainer,
            null,
            HideHelpMenu,
            useDirectRoleDetailLayout: true);

        // 非表示中の既存メニューだけでなく、Initialize直後に追加した詳細画面も
        // フェード対象へ含めるため、ここで改めてフェードインを開始する。
        if (needsFadeIn)
            fadeCoroutine.StartFadeIn(helpMenuObject, 0.115f);

        var activeIndicator = HelpMenusHudManagerStartPatch.helpMenuButton?.transform.Find("active");
        if (activeIndicator != null)
            activeIndicator.gameObject.SetActive(true);
    }

    private static void SetCategoryNavigationActive(bool active)
    {
        var leftButtons = helpMenuObject?.transform.Find("LeftButtons");
        if (leftButtons != null)
            leftButtons.gameObject.SetActive(active);

        var categorySeparator = helpMenuObject?.transform.Find("Line");
        if (categorySeparator != null)
            categorySeparator.gameObject.SetActive(active);
    }

    private static void EnsureHelpMenuBackdrop()
    {
        if (helpMenuBackdropObject != null || helpMenuObject == null || HudManager.Instance == null)
            return;

        var backdropPrefab = AssetManager.GetAsset<GameObject>("GreenBack");
        if (backdropPrefab == null)
        {
            Logger.Warning("ヘルプメニュー用の画面外オーバーレイを生成できませんでした。");
            return;
        }

        // helpMenuObject の子にするとフェード対象に含まれて開閉が不自然になるため、兄弟として配置する。
        helpMenuBackdropObject = GameObject.Instantiate(backdropPrefab, HudManager.Instance.transform);
        helpMenuBackdropObject.name = "HelpMenuBackdrop";
        helpMenuBackdropObject.layer = 5;
        // HelpMenuObject は z=-500。少し後ろに置いてメニュー本体の背面に回す。
        helpMenuBackdropObject.transform.localPosition = new Vector3(0f, 0f, -498f);
        helpMenuBackdropObject.transform.localRotation = Quaternion.identity;
        helpMenuBackdropObject.transform.localScale = new Vector3(100f, 100f, 1f);

        foreach (var renderer in helpMenuBackdropObject.GetComponentsInChildren<SpriteRenderer>(true))
        {
            renderer.color = new Color(0f, 0f, 0f, 0f);
            renderer.maskInteraction = SpriteMaskInteraction.None;
        }

        var collider = helpMenuBackdropObject.GetComponent<BoxCollider2D>()
            ?? helpMenuBackdropObject.AddComponent<BoxCollider2D>();
        collider.size = Vector2.one;
        collider.enabled = true;

        var passiveButton = helpMenuBackdropObject.GetComponent<PassiveButton>()
            ?? helpMenuBackdropObject.AddComponent<PassiveButton>();
        passiveButton.Colliders = new Collider2D[] { collider };
        passiveButton.OnClick = new();
        passiveButton.OnClick.AddListener((UnityAction)HideHelpMenu);
        passiveButton.OnMouseOver = new();
        passiveButton.OnMouseOut = new();
    }

    /// <summary>
    /// ヘルプメニュー背面オーバーレイの状態を更新する。
    /// 通常ヘルプでは暗転せず alpha=0 のまま当たり判定だけ残し、役職説明時のみ暗転する。
    /// メニュー閉鎖時のみオブジェクト自体を非アクティブにする。
    /// </summary>
    private static void SetHelpMenuBackdropState(bool menuOpen, bool dimmed)
    {
        if (menuOpen)
            EnsureHelpMenuBackdrop();
        if (helpMenuBackdropObject == null)
            return;

        helpMenuBackdropObject.SetActive(menuOpen);
        if (!menuOpen)
            return;

        float alpha = dimmed ? 0.7f : 0f;
        foreach (var renderer in helpMenuBackdropObject.GetComponentsInChildren<SpriteRenderer>(true))
        {
            renderer.color = new Color(0f, 0f, 0f, alpha);
            renderer.maskInteraction = SpriteMaskInteraction.None;
        }

        foreach (var collider in helpMenuBackdropObject.GetComponentsInChildren<Collider2D>(true))
            collider.enabled = true;
    }

    private static void RestoreCategoryMenu()
    {
        if (!isDirectRoleDetailMode || helpMenuObject == null)
            return;

        if (directRoleDetailObject != null)
            GameObject.Destroy(directRoleDetailObject);
        directRoleDetailObject = null;
        isDirectRoleDetailMode = false;
        SetCategoryNavigationActive(true);
        SetHelpMenuBackdropState(menuOpen: true, dimmed: false);

        var rightContainer = helpMenuObject.transform.Find("RightContainer")?.gameObject;
        if (rightContainer == null || CurrentCategory == null)
            return;

        CurrentCategory.Show(rightContainer);
        CurrentCategory.UpdateShow();
        if (selectedButtons != null
            && selectedButtons.TryGetValue(CurrentCategory.Name, out var selectedObject))
        {
            selectedObject.SetActive(true);
        }
    }

    public static void ShowOrHideHelpMenu()
    {
        if (!IsHelpMenuActive && !CanToggleHelpMenu())
            return;

        if (isDirectRoleDetailMode && IsHelpMenuActive)
        {
            HideHelpMenu();
            return;
        }

        RestoreCategoryMenu();

        if (helpMenuObject == null)
        {
            Initialize();
        }
        else
        {
            // ロビーの場合は毎回「配役情報」カテゴリに戻す
            bool inLobby = AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Joined;
            if (inLobby)
            {
                // 配役情報カテゴリに切り替える
                var rightContainer = helpMenuObject.transform.Find("RightContainer").gameObject;
                if (CurrentCategory != null && CurrentCategory.Category != DEFAULT_MENU_LOBBY)
                {
                    CurrentCategory.Hide(rightContainer);
                    if (selectedButtons.TryGetValue(CurrentCategory.Name, out var oldSelectedObject))
                        oldSelectedObject.SetActive(false);

                    CurrentCategory = categories.FirstOrDefault(c => c.Category == DEFAULT_MENU_LOBBY);
                    if (CurrentCategory != null)
                    {
                        CurrentCategory.Show(rightContainer);
                        if (selectedButtons.TryGetValue(CurrentCategory.Name, out var newSelectedObject))
                            newSelectedObject.SetActive(true);
                    }
                }
            }
            else
            {
                // ゲーム中は自分の役職情報に戻す
                var rightContainer = helpMenuObject.transform.Find("RightContainer").gameObject;
                if (CurrentCategory != null && CurrentCategory.Category != DEFAULT_MENU_GAME)
                {
                    CurrentCategory.Hide(rightContainer);
                    if (selectedButtons.TryGetValue(CurrentCategory.Name, out var oldSelectedObject))
                        oldSelectedObject.SetActive(false);

                    CurrentCategory = categories.FirstOrDefault(c => c.Category == DEFAULT_MENU_GAME);
                    if (CurrentCategory != null)
                    {
                        CurrentCategory.Show(rightContainer);
                        if (selectedButtons.TryGetValue(CurrentCategory.Name, out var newSelectedObject))
                            newSelectedObject.SetActive(true);
                    }
                }
            }

            fadeCoroutine.ReverseFade();

        }
        if (fadeCoroutine.isActive)
        {
            CurrentCategory?.UpdateShow();
            SetHelpMenuBackdropState(menuOpen: true, dimmed: isDirectRoleDetailMode);
        }
        else
        {
            SetHelpMenuBackdropState(menuOpen: false, dimmed: false);
        }
        HelpMenusHudManagerStartPatch.helpMenuButton?.transform.Find("active").gameObject.SetActive(fadeCoroutine.isActive);
    }
    public static void HideHelpMenu()
    {
        if (helpMenuObject == null || fadeCoroutine == null) return;
        fadeCoroutine.StartFadeOut(helpMenuObject, 0.115f);
        SetHelpMenuBackdropState(menuOpen: false, dimmed: false);

        var activeIndicator = HelpMenusHudManagerStartPatch.helpMenuButton?.transform.Find("active");
        if (activeIndicator != null)
            activeIndicator.gameObject.SetActive(false);
    }
    // overlayを閉じる時。
    [HarmonyPatch(typeof(KeyboardJoystick), nameof(KeyboardJoystick.Update))]
    public static class KeyboardJoystickUpdatePatch
    {
        public static void Postfix(KeyboardJoystick __instance)
        {
            // Overlayが非表示なら処理を省略する
            if (helpMenuObject == null || fadeCoroutine == null || !fadeCoroutine.isActive)
            {
                return;
            }

            // チャットがアクティブ、またはEsc/Tabキーのいずれかが押された場合、
            // Overlayが表示されているなら非表示に切り替える
            bool isChatActive = FastDestroyableSingleton<HudManager>.Instance.Chat.IsOpenOrOpening;
            bool isCancelKeyPressed = Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Tab);
            if (isChatActive || isCancelKeyPressed)
            {
                HideHelpMenu();
            }
        }
    }
    [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Update))]
    public static class GameStartManagerUpdatePatch
    {
        public static void Postfix(GameStartManager __instance)
        {
            bool enabled = helpMenuObject == null || fadeCoroutine == null || !fadeCoroutine.isActive;
            if (AmongUsClient.Instance.AmHost && GameData.Instance != null)
            {
                bool minOk = GameData.Instance.PlayerCount >= __instance.MinPlayers;
                bool startEnabled = enabled && minOk;
                if (AmongUsClient.Instance.NetworkMode == NetworkModes.OnlineGame)
                    startEnabled = startEnabled && SyncVersion.CanHostStartGame();
                __instance.StartButton.SetButtonEnableState(startEnabled);
                if (__instance.StartButtonGlyph != null)
                    __instance.StartButtonGlyph.SetColor(startEnabled ? Palette.EnabledColor : Palette.DisabledClear);
            }
            else
            {
                __instance.StartButton.enabled = enabled;
            }
            __instance.LobbyInfoPane.EditButton.enabled = enabled;
            PassiveButton p = null;
            if (__instance.LobbyInfoPane.HostViewButton != null)
                __instance.LobbyInfoPane.HostViewButton.enabled = enabled;
            if (__instance.LobbyInfoPane.ClientViewButton != null)
                __instance.LobbyInfoPane.ClientViewButton.enabled = enabled;
        }
    }

    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.CoStartGame))]
    public static class OnGameStartedPatch
    {
        public static void Postfix()
        {
            isWaitingForIntroDisplay = true;
            GhostAssignRole.ClearAndReloads();
            // ヘルプメニューが表示されている場合は破棄する
            if (helpMenuObject != null)
            {
                GameObject.Destroy(helpMenuObject);
                helpMenuObject = null;
                directRoleDetailObject = null;
                if (helpMenuBackdropObject != null)
                {
                    GameObject.Destroy(helpMenuBackdropObject);
                    helpMenuBackdropObject = null;
                }
                isDirectRoleDetailMode = false;
                fadeCoroutine = null;
                CurrentCategory = null;
                selectedButtons?.Clear();
            }
        }
    }

    [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.CoBegin))]
    public static class IntroCutsceneCoBeginPatch
    {
        public static void Postfix()
        {
            isWaitingForIntroDisplay = false;
        }
    }
}
public class HelpMenuObjectComponent : MonoBehaviour
{
    public void Start()
    {
        HelpMenuClipMaterialController.Refresh(gameObject, retryShaderLoad: true);
    }

    public void LateUpdate()
    {
        HelpMenuClipMaterialController.Refresh(gameObject);
    }

    public void OnClick()
    {
        HelpMenuObjectManager.CurrentCategory?.OnUpdate();
    }

    public void OnDestroy()
    {
        HelpMenuClipMaterialController.Release();
    }
}
