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

public static class GhostOptionMenu
{
    private const string GHOST_OPTION_MENU_ASSET_NAME = "RoleOptionMenu";
    public const string GHOST_ROLE_DETAIL_BUTTON_ASSET_NAME = "RoleDetailButton";

    public static void ShowGhostOptionMenu()
    {
        if (GhostOptionMenuObjectData.Instance?.MenuObject == null)
            InitializeGhostOptionMenu();
        GhostOptionMenuObjectData.Instance.MenuObject?.SetActive(true);
        UpdateMenuTitle();
        UpdateGhostRoleButtons();
        HideRightSettings();
    }

    public static void HideGhostOptionMenu()
    {
        GhostOptionMenuObjectData.Instance?.MenuObject.SetActive(false);
    }

    private static void InitializeGhostOptionMenu()
    {
        var gameSettingMenu = GameObject.FindObjectOfType<GameSettingMenu>();
        var menuObject = GameObject.Instantiate(AssetManager.GetAsset<GameObject>(GHOST_OPTION_MENU_ASSET_NAME));
        menuObject.transform.SetParent(gameSettingMenu.transform);
        menuObject.transform.localScale = Vector3.one * 0.2f;
        menuObject.transform.localPosition = new(0, 0, -1f);
        var data = new GhostOptionMenuObjectData(menuObject);
        (data.Scroller, data.InnerScroll) = CreateScrollbar(menuObject.transform);
        data.InnerScroll.transform.localPosition = new Vector3(data.InnerScroll.transform.localPosition.x, 0f, 0.2f);
        // 右側設定エリアのキャッシュ
        data.SettingsScroller = menuObject.transform.Find("SettingsScroller")?.GetComponent<Scroller>();
        data.SettingsInner = menuObject.transform.Find("SettingsScroller/InnerContent");
    }

    private static (Scroller, GameObject) CreateScrollbar(Transform parent)
    {
        var gameSettingMenu = GameObject.FindObjectOfType<GameSettingMenu>();
        if (gameSettingMenu == null) return (null, null);
        var gameSettingsTab = gameSettingMenu.GameSettingsTab;
        if (gameSettingsTab == null) return (null, null);
        var tabCopy = GameObject.Instantiate(gameSettingsTab.gameObject, parent, false);
        tabCopy.transform.localScale = Vector3.one * 5.3f;
        tabCopy.transform.localPosition = new Vector3(-30.7f, 12f, 0f);
        tabCopy.gameObject.SetActive(true);
        var Gradient = tabCopy.transform.Find("Gradient");
        if (Gradient != null)
        {
            Gradient.transform.localPosition = new(5.77f, -3.98f, -20f);
            Gradient.transform.localScale = new(0.774f, 0.6f, 7.2125f);
        }
        var closeButton = tabCopy.transform.Find("CloseButton");
        if (closeButton != null)
            GameObject.Destroy(closeButton.gameObject);
        var scrollInner = tabCopy.transform.Find("Scroller/SliderInner");
        if (scrollInner != null)
        {
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
        var scroller = tabCopy.GetComponentInChildren<Scroller>();
        var innerscroll = tabCopy.transform.Find("Scroller/SliderInner").gameObject;
        return (scroller, innerscroll);
    }

    private static void UpdateMenuTitle()
    {
        var data = GhostOptionMenuObjectData.Instance;
        if (data?.TitleText != null)
            data.TitleText.text = $"{ModTranslation.GetString($"RoleOptionMenuType.Ghost")}";
    }

    private static void UpdateGhostRoleButtons()
    {
        var data = GhostOptionMenuObjectData.Instance;
        for (int i = data.InnerScroll.transform.childCount - 1; i >= 0; i--)
        {
            GameObject.Destroy(data.InnerScroll.transform.GetChild(i).gameObject);
        }
        data.GhostRoleButtonDictionary.Clear();
        var ghostRoleOptions = RoleOptionManager.GhostRoleOptions;
        int index = 0;
        foreach (var ghostRoleOption in ghostRoleOptions)
        {
            var roleBase = CustomRoleManager.AllGhostRoles.FirstOrDefault(r => r.Role == ghostRoleOption.RoleId);
            if (roleBase == null) continue;
            if (roleBase.HiddenOption) continue;
            string roleName = ModTranslation.GetString($"{roleBase.Role}");
            var obj = GameObject.Instantiate(AssetManager.GetAsset<GameObject>(GHOST_ROLE_DETAIL_BUTTON_ASSET_NAME));
            obj.transform.SetParent(data.InnerScroll.transform);
            obj.transform.localScale = Vector3.one * 0.31f;
            int col = index % 4;
            int row = index / 4;
            float posX = -1.27f + col * 1.63f;
            float posY = 0.85f - row * 0.38f;
            obj.transform.localPosition = new Vector3(posX, posY, -0.21f);
            obj.transform.Find("Text").GetComponent<TextMeshPro>().text = $"<b><color=#{ColorUtility.ToHtmlStringRGB(ghostRoleOption.RoleColor)}>{roleName}</color></b>";
            var passiveButton = obj.AddComponent<PassiveButton>();
            passiveButton.Colliders = new Collider2D[1];
            passiveButton.Colliders[0] = obj.GetComponent<BoxCollider2D>();
            passiveButton.OnClick = new();
            GameObject SelectedObject = null;
            var spriteRenderer = obj.GetComponent<SpriteRenderer>();
            UpdateRoleDetailButtonColor(spriteRenderer, ghostRoleOption);
            passiveButton.OnClick.AddListener((UnityAction)(() =>
            {
                ClickedGhostRole(spriteRenderer, ghostRoleOption);
            }));
            // 右クリック検知用のコンポーネントを追加し、イベントを登録
            var rightClickDetector = obj.AddComponent<RightClickDetector>();
            rightClickDetector.OnRightClick.AddListener((UnityAction)(() =>
            {
                // 対象が有効のとき
                if (ghostRoleOption.NumberOfCrews >= 1)
                {
                    ghostRoleOption.NumberOfCrews = 0;
                    ghostRoleOption.Percentage = 0;
                }
                else
                {
                    // デフォルト値を設定（必要に応じて調整してください）
                    ghostRoleOption.NumberOfCrews = 1;
                    ghostRoleOption.Percentage = 100;
                }
                UpdateRoleDetailButtonColor(spriteRenderer, ghostRoleOption);
                // 変更を同期
                RoleOptionManager.RpcSyncGhostRoleOptionDelay(ghostRoleOption.RoleId, ghostRoleOption.NumberOfCrews, ghostRoleOption.Percentage);

                // 右側の設定も更新
                if (data.CurrentGhostRoleId == ghostRoleOption.RoleId)
                {
                    ShowRightSettings(spriteRenderer, ghostRoleOption); // 右側の設定UIを再描画
                }
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
            data.GhostRoleButtonDictionary[ghostRoleOption.RoleId] = obj;
            index++;
        }
        data.Scroller.ContentYBounds.max = index < 25 ? 0f : (0.38f * ((index - 24) / 4 + 1)) - 0.5f;
        data.Scroller.UpdateScrollBars();
    }

    private static void UpdateRoleDetailButtonColor(SpriteRenderer spriteRenderer, RoleOptionManager.GhostRoleOption ghostRoleOption)
    {
        if (ghostRoleOption == null) throw new Exception("ghostRoleOption is null");
        if (spriteRenderer == null) throw new Exception("spriteRenderer is null");
        if (ghostRoleOption.NumberOfCrews > 0 && ghostRoleOption.Percentage > 0)
            spriteRenderer.color = Color.white;
        else
            spriteRenderer.color = new Color(1, 1f, 1f, 0.6f);
    }

    private static void ClickedGhostRole(SpriteRenderer spriteRenderer, RoleOptionManager.GhostRoleOption ghostRoleOption)
    {
        var data = GhostOptionMenuObjectData.Instance;
        data.CurrentGhostRoleId = ghostRoleOption.RoleId;
        ShowRightSettings(spriteRenderer, ghostRoleOption);
    }

    private static void ShowRightSettings(SpriteRenderer spriteRenderer, RoleOptionManager.GhostRoleOption ghostRoleOption)
    {
        var data = GhostOptionMenuObjectData.Instance;
        // 既存の設定UIを破棄
        if (data.CurrentSettingsParent != null)
            GameObject.Destroy(data.CurrentSettingsParent);
        data.CurrentOptionDisplays.Clear();
        float lastY = 4f;
        // 親オブジェクト生成
        data.CurrentSettingsParent = new GameObject("Parent");
        data.CurrentSettingsParent.transform.SetParent(data.SettingsInner);
        data.CurrentSettingsParent.transform.localScale = Vector3.one;
        data.CurrentSettingsParent.transform.localPosition = Vector3.zero;
        data.CurrentSettingsParent.layer = 5;
        // 人数・確率セレクタ
        CreateNumberOfCrewsSelectAndPerSelect(spriteRenderer, data.CurrentSettingsParent.transform, ghostRoleOption, ref lastY);
        int index = 2;
        // カスタムオプション
        foreach (var option in ghostRoleOption.Options)
        {
            if (option.IsBooleanOption)
                CreateCheckBox(data.CurrentSettingsParent.transform, option, ref lastY);
            else
                CreateSelect(data.CurrentSettingsParent.transform, option, ref lastY);
            index++;
        }
        foreach (var option in ghostRoleOption.Options)
        {
            if (option.ParentOption == null)
                UpdateOptionsActive(data.CurrentSettingsParent.transform, option);
        }
        // スクロール位置リセット
        if (data.SettingsInner != null && data.SettingsScroller != null)
        {
            data.SettingsInner.localPosition = new Vector3(data.SettingsInner.localPosition.x, 0f, data.SettingsInner.localPosition.z);
            data.SettingsScroller.UpdateScrollBars();
        }
        // スクロールバー範囲調整
        if (data.SettingsScroller != null)
        {
            data.SettingsScroller.ContentYBounds.max = index < 4 ? 0.1f : index == 4 ? 2.1f : (index - 4) * 4.5f + 2f;
            data.SettingsScroller.UpdateScrollBars();
        }
    }

    private static void HideRightSettings()
    {
        var data = GhostOptionMenuObjectData.Instance;
        if (data.CurrentSettingsParent != null)
            GameObject.Destroy(data.CurrentSettingsParent);
        data.CurrentOptionDisplays.Clear();
    }

    // --- 以下、UI生成ヘルパー ---
    private static GameObject CreateCheckBox(Transform parent, CustomOption option, ref float lastY)
    {
        var data = GhostOptionMenuObjectData.Instance;
        GameObject optionInstance = CreateOptionElement(parent, ModTranslation.GetString(option.Name), ref lastY, "Option_Check");
        var passiveButton = optionInstance.AddComponent<PassiveButton>();
        var checkMark = optionInstance.transform.Find("CheckMark");
        SetupCheckBoxButton(passiveButton, checkMark, option);
        passiveButton.Colliders = new[] { optionInstance.GetComponent<BoxCollider2D>() };
        var checkMarkTMP = checkMark.gameObject.AddComponent<TextMeshPro>();
        data.CurrentOptionDisplays.Add((checkMarkTMP, option));
        return optionInstance;
    }
    private static void SetupCheckBoxButton(PassiveButton passiveButton, Transform checkMark, CustomOption option)
    {
        checkMark.gameObject.SetActive((bool)option.Value);
        var spriteRenderer = passiveButton.GetComponent<SpriteRenderer>();
        ConfigurePassiveButton(passiveButton, () =>
        {
            bool newValue = !checkMark.gameObject.activeSelf;
            checkMark.gameObject.SetActive(newValue);
            option.UpdateSelection(newValue ? (byte)1 : (byte)0);
            UpdateDisplayAfterOptionChange(passiveButton.transform.parent, option);
            if (AmongUsClient.Instance.AmHost)
                CustomOptionManager.RpcSyncOption(option.Id, newValue ? (byte)1 : (byte)0);
        }, spriteRenderer);
    }
    private static GameObject CreateSelect(Transform parent, CustomOption option, ref float lastY)
    {
        var data = GhostOptionMenuObjectData.Instance;
        GameObject optionInstance = CreateOptionElement(parent, ModTranslation.GetString(option.Name), ref lastY, "Option_Select");
        var selectedText = optionInstance.transform.Find("SelectedText").GetComponent<TextMeshPro>();
        selectedText.text = UIHelper.FormatOptionValue(option.Selections[option.Selection], option);
        SetupSelectButtons(optionInstance, selectedText, option);
        data.CurrentOptionDisplays.Add((selectedText, option));
        return optionInstance;
    }
    private static void SetupSelectButtons(GameObject optionInstance, TextMeshPro selectedText, CustomOption option)
    {
        SetupMinusButton(optionInstance, selectedText, option);
        SetupPlusButton(optionInstance, selectedText, option);
    }
    private static void SetupMinusButton(GameObject optionInstance, TextMeshPro selectedText, CustomOption option)
    {
        var minusButton = optionInstance.transform.Find("Button_Minus").gameObject;
        var passiveButton = minusButton.AddComponent<PassiveButton>();
        var spriteRenderer = passiveButton.GetComponent<SpriteRenderer>();
        ConfigurePassiveButton(passiveButton, () =>
        {
            byte newSelection = option.Selection > 0 ? (byte)(option.Selection - 1) : (byte)(option.Selections.Length - 1);
            option.UpdateSelection(newSelection);
            selectedText.text = UIHelper.FormatOptionValue(option.Selections[option.Selection], option);
            UpdateDisplayAfterOptionChange(selectedText.transform.parent.parent, option);
            if (AmongUsClient.Instance.AmHost)
                CustomOptionManager.RpcSyncOption(option.Id, newSelection);
        }, spriteRenderer);
    }
    private static void SetupPlusButton(GameObject optionInstance, TextMeshPro selectedText, CustomOption option)
    {
        var plusButton = optionInstance.transform.Find("Button_Plus").gameObject;
        var passiveButton = plusButton.AddComponent<PassiveButton>();
        var spriteRenderer = passiveButton.GetComponent<SpriteRenderer>();
        ConfigurePassiveButton(passiveButton, () =>
        {
            byte newSelection = 0;
            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            {
                newSelection = (byte)(option.Selections.Length - 1);
            }
            else
            {
                int additional = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) ? RoleOptionSettings.ShiftSelection : 1;
                newSelection = option.Selection + additional < option.Selections.Length ? (byte)(option.Selection + additional) : (byte)0;
            }
            option.UpdateSelection(newSelection);
            selectedText.text = UIHelper.FormatOptionValue(option.Selections[option.Selection], option);
            UpdateDisplayAfterOptionChange(selectedText.transform.parent.parent, option);
            if (AmongUsClient.Instance.AmHost)
                CustomOptionManager.RpcSyncOption(option.Id, newSelection);
        }, spriteRenderer);
    }
    private static void UpdateDisplayAfterOptionChange(Transform parentTransform, CustomOption changedOption)
    {
        // 子オプションを持つ場合のみ表示状態を更新
        if ((changedOption.ChildrenOption != null && changedOption.ChildrenOption.Count > 0) || changedOption.ParentOption != null)
        {
            Transform topParent = FindTopParent(parentTransform);
            UpdateOptionsActive(topParent, changedOption);
            RecalculateOptionsPosition(topParent, GhostOptionMenuObjectData.Instance.SettingsScroller);
        }
    }
    private static Transform FindTopParent(Transform transform)
    {
        Transform current = transform;
        while (current.parent != null && current.parent.name != "InnerContent" && current.parent.name != "SettingsScroller")
        {
            current = current.parent;
        }
        return current;
    }
    private static void UpdateOptionsActive(Transform parentTransform, CustomOption changedOption)
    {
        for (int i = 0; i < parentTransform.childCount; i++)
        {
            var child = parentTransform.GetChild(i);
            var textComp = child.Find("Text")?.GetComponent<TextMeshPro>();
            if (textComp != null && ModTranslation.GetString(changedOption.Name) == textComp.text)
            {
                child.gameObject.SetActive(changedOption.ShouldDisplay());
            }
        }
        if (changedOption.ChildrenOption != null)
        {
            foreach (var childOption in changedOption.ChildrenOption)
            {
                UpdateOptionsActive(parentTransform, childOption);
            }
        }
    }
    private static void RecalculateOptionsPosition(Transform parentTransform, Scroller scroller)
    {
        float lastY = 4f;
        int activeCount = 0;
        for (int i = 0; i < parentTransform.childCount; i++)
        {
            Transform child = parentTransform.GetChild(i);
            if (!child.gameObject.activeSelf)
                continue;
            child.localPosition = new Vector3(-0.22f, lastY, -5f);
            lastY -= 4.5f;
            activeCount++;
        }
        if (scroller != null)
        {
            scroller.ContentYBounds.max = activeCount <= 4 ? 0.1f : (activeCount - 4) * 4.5f + 2f;
            scroller.UpdateScrollBars();
        }
    }
    private static GameObject CreateOptionElement(Transform parent, string optionName, ref float lastY, string prefabName)
    {
        var optionPrefab = AssetManager.GetAsset<GameObject>(prefabName);
        var optionInstance = UnityEngine.Object.Instantiate(optionPrefab, parent);
        optionInstance.transform.localPosition = new Vector3(-0.22f, lastY, -5f);
        lastY -= 4.5f;
        optionInstance.transform.localScale = Vector3.one * 2f;
        optionInstance.transform.Find("Text").GetComponent<TextMeshPro>().text = optionName;
        return optionInstance;
    }
    private static GameObject CreateNumberOfCrewsSelectAndPerSelect(SpriteRenderer spriteRenderer, Transform parent, RoleOptionManager.GhostRoleOption ghostRoleOption, ref float lastY)
    {
        var data = GhostOptionMenuObjectData.Instance;
        GameObject optionInstance = CreateOptionElement(parent, ModTranslation.GetString("NumberOfCrews"), ref lastY, "Option_Select");
        var selectedText = optionInstance.transform.Find("SelectedText").GetComponent<TextMeshPro>();
        selectedText.text = ModTranslation.GetString("NumberOfCrewsSelected", ghostRoleOption.NumberOfCrews);
        var minusButton = optionInstance.transform.Find("Button_Minus").gameObject;
        var minusPassiveButton = minusButton.AddComponent<PassiveButton>();
        var minusSpriteRenderer = minusPassiveButton.GetComponent<SpriteRenderer>();
        ConfigurePassiveButton(minusPassiveButton, () =>
        {
            byte playerCount = 15;
            ghostRoleOption.NumberOfCrews--;
            if (ghostRoleOption.NumberOfCrews < 0)
                ghostRoleOption.NumberOfCrews = playerCount;
            if (ghostRoleOption.NumberOfCrews > playerCount)
                ghostRoleOption.NumberOfCrews = playerCount;
            selectedText.text = ModTranslation.GetString("NumberOfCrewsSelected", ghostRoleOption.NumberOfCrews);
            UpdateRoleDetailButtonColor(spriteRenderer, ghostRoleOption);
            RoleOptionManager.RpcSyncGhostRoleOptionDelay(ghostRoleOption.RoleId, ghostRoleOption.NumberOfCrews, ghostRoleOption.Percentage);
        }, minusSpriteRenderer);
        var plusButton = optionInstance.transform.Find("Button_Plus").gameObject;
        var plusPassiveButton = plusButton.AddComponent<PassiveButton>();
        var plusSpriteRenderer = plusPassiveButton.GetComponent<SpriteRenderer>();
        ConfigurePassiveButton(plusPassiveButton, () =>
        {
            byte playerCount = 15;
            byte oldValue = ghostRoleOption.NumberOfCrews;
            ghostRoleOption.NumberOfCrews++;
            if (ghostRoleOption.NumberOfCrews > playerCount)
                ghostRoleOption.NumberOfCrews = 0;
            if (oldValue == 0 && ghostRoleOption.NumberOfCrews > 0 && ghostRoleOption.Percentage == 0)
            {
                ghostRoleOption.Percentage = 100;
            }
            selectedText.text = ModTranslation.GetString("NumberOfCrewsSelected", ghostRoleOption.NumberOfCrews);
            UpdateRoleDetailButtonColor(spriteRenderer, ghostRoleOption);
            RoleOptionManager.RpcSyncGhostRoleOptionDelay(ghostRoleOption.RoleId, ghostRoleOption.NumberOfCrews, ghostRoleOption.Percentage);
        }, plusSpriteRenderer);
        // 確率設定
        var perOption = CreateAssignPerSelect(parent, ghostRoleOption, ref lastY);
        var percentageText = perOption.transform.Find("SelectedText").GetComponent<TextMeshPro>();
        data.CurrentRoleNumbersOfCrewsText = selectedText;
        data.CurrentRolePercentageText = percentageText;
        return optionInstance;
    }
    private static GameObject CreateAssignPerSelect(Transform parent, RoleOptionManager.GhostRoleOption ghostRoleOption, ref float lastY)
    {
        GameObject optionInstance = CreateOptionElement(parent, ModTranslation.GetString("AssignPer"), ref lastY, "Option_Select");
        var selectedText = optionInstance.transform.Find("SelectedText").GetComponent<TextMeshPro>();
        selectedText.text = ghostRoleOption.Percentage + "%";
        var minusButton = optionInstance.transform.Find("Button_Minus").gameObject;
        var minusPassiveButton = minusButton.AddComponent<PassiveButton>();
        var minusSpriteRenderer = minusPassiveButton.GetComponent<SpriteRenderer>();
        ConfigurePassiveButton(minusPassiveButton, () =>
        {
            ghostRoleOption.Percentage -= 10;
            if (ghostRoleOption.Percentage < 0)
                ghostRoleOption.Percentage = 100;
            if (ghostRoleOption.Percentage > 100)
                ghostRoleOption.Percentage = 100;
            selectedText.text = ghostRoleOption.Percentage + "%";
            RoleOptionManager.RpcSyncGhostRoleOptionDelay(ghostRoleOption.RoleId, ghostRoleOption.NumberOfCrews, ghostRoleOption.Percentage);
        }, minusSpriteRenderer);
        var plusButton = optionInstance.transform.Find("Button_Plus").gameObject;
        var plusPassiveButton = plusButton.AddComponent<PassiveButton>();
        var plusSpriteRenderer = plusPassiveButton.GetComponent<SpriteRenderer>();
        ConfigurePassiveButton(plusPassiveButton, () =>
        {
            ghostRoleOption.Percentage += 10;
            if (ghostRoleOption.Percentage > 100)
                ghostRoleOption.Percentage = 0;
            selectedText.text = ghostRoleOption.Percentage + "%";
            RoleOptionManager.RpcSyncGhostRoleOptionDelay(ghostRoleOption.RoleId, ghostRoleOption.NumberOfCrews, ghostRoleOption.Percentage);
        }, plusSpriteRenderer);
        return optionInstance;
    }
    private static void ConfigurePassiveButton(PassiveButton button, Action onClick, SpriteRenderer spriteRenderer = null)
    {
        button.OnClick = new();
        button.OnClick.AddListener(onClick);
        button.OnMouseOver = new();
        button.OnMouseOver.AddListener((UnityAction)(() =>
        {
            if (spriteRenderer != null)
                spriteRenderer.color = new Color32(45, 235, 198, 255);
        }));
        button.OnMouseOut = new();
        button.OnMouseOut.AddListener((UnityAction)(() =>
        {
            if (spriteRenderer != null)
                spriteRenderer.color = Color.white;
        }));
    }
}