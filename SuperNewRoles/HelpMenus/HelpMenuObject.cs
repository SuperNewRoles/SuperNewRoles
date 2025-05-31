using UnityEngine;
using TMPro;
using SuperNewRoles.Modules;
using UnityEngine.Events;
using System;
using System.Reflection;
using System.Linq;
using HarmonyLib;
using System.Collections.Generic;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Roles;

namespace SuperNewRoles.HelpMenus;

public static class HelpMenuObjectManager
{
    private static GameObject helpMenuObject;
    public static FadeCoroutine fadeCoroutine;
    public static HelpMenuCategoryBase[] categories;
    public static Dictionary<string, GameObject> selectedButtons;
    public static HelpMenuCategoryBase? CurrentCategory;
    public const HelpMenuCategory DEFAULT_MENU_GAME = HelpMenuCategory.MyRoleInfomation;
    public const HelpMenuCategory DEFAULT_MENU_LOBBY = HelpMenuCategory.AssignmentsSettingInfomation;
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

        // ヘルプメニューを表示するときにホスト情報のマスクエリアを非表示にする
        RoleOptionMenu.UpdateHostInfoMaskArea(false);

        // 会議中の場合、playerStatesのMaskAreaを非表示にする
        ModHelpers.UpdateMeetingHudMaskAreas(false);

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

    public static void ShowOrHideHelpMenu()
    {
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

            // ヘルプメニューの表示状態によってマスクエリアの表示を切り替える
            // fadeCoroutine.isActiveが反転する前に呼ばれるため、現在の状態の逆を設定
            bool shouldShowMaskAreas = !fadeCoroutine.isActive;
            RoleOptionMenu.UpdateHostInfoMaskArea(shouldShowMaskAreas);
            ModHelpers.UpdateMeetingHudMaskAreas(shouldShowMaskAreas);
        }
        if (fadeCoroutine.isActive)
        {
            CurrentCategory?.UpdateShow();
        }
        HelpMenusHudManagerStartPatch.helpMenuButton?.transform.Find("active").gameObject.SetActive(fadeCoroutine.isActive);
    }
    public static void HideHelpMenu()
    {
        if (helpMenuObject == null || fadeCoroutine == null) return;
        fadeCoroutine.StartFadeOut(helpMenuObject, 0.115f);

        // ヘルプメニューを非表示にするときにホスト情報とMeetingHudのマスクエリアを表示する
        RoleOptionMenu.UpdateHostInfoMaskArea(true);
        ModHelpers.UpdateMeetingHudMaskAreas(true);
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

            // チャットがアクティブ、またはEsc, Tab, Hキーのいずれかが押された場合、
            // Overlayが表示されているなら非表示に切り替える
            bool isChatActive = FastDestroyableSingleton<HudManager>.Instance.Chat.IsOpenOrOpening;
            bool isCancelKeyPressed = Input.GetKeyDown(KeyCode.Escape);
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
            __instance.StartButton.enabled = enabled;
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
            GhostAssignRole.ClearAndReloads();
            // ヘルプメニューが表示されている場合は破棄する
            if (helpMenuObject != null)
            {
                GameObject.Destroy(helpMenuObject);
                helpMenuObject = null;
                fadeCoroutine = null;
                CurrentCategory = null;
                selectedButtons?.Clear();
            }
        }
    }
}
public class HelpMenuObjectComponent : MonoBehaviour
{
    public void OnClick()
    {
        HelpMenuObjectManager.CurrentCategory?.OnUpdate();
    }
}