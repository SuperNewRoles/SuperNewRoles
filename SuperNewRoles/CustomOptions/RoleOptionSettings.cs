using System;
using System.Linq;
using SuperNewRoles.Modules;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace SuperNewRoles.CustomOptions;
public class RoleOptionSettings
{
    private const int GenCount = 50;
    private const float DefaultRate = 4.5f;
    private const float DefaultLastY = 4f;
    private const float ElementXPosition = -0.22f;
    private const float ElementZPosition = -5f;
    private const float ElementScale = 2f;
    private const float ScrollerXPosition = 18f;

    private static readonly Color32 HoverColor = new(45, 235, 198, 255);

    private static GameObject CreateOptionElement(Transform parent, string optionName, ref float lastY, string prefabName)
    {
        var optionPrefab = AssetManager.GetAsset<GameObject>(prefabName);
        var optionInstance = UnityEngine.Object.Instantiate(optionPrefab, parent);
        optionInstance.transform.localPosition = new Vector3(ElementXPosition, lastY, ElementZPosition);
        lastY -= DefaultRate;
        optionInstance.transform.localScale = Vector3.one * ElementScale;
        optionInstance.transform.Find("Text").GetComponent<TextMeshPro>().text = optionName;
        return optionInstance;
    }

    private static void ConfigurePassiveButton(PassiveButton button, Action onClick, SpriteRenderer spriteRenderer = null)
    {
        button.OnClick = new();
        button.OnClick.AddListener(onClick);
        ConfigureButtonHoverEffects(button, spriteRenderer);
    }

    private static void ConfigureButtonHoverEffects(PassiveButton button, SpriteRenderer spriteRenderer)
    {
        button.OnMouseOver = new();
        button.OnMouseOver.AddListener((UnityAction)(() =>
        {
            if (spriteRenderer != null)
                spriteRenderer.color = HoverColor;
        }));

        button.OnMouseOut = new();
        button.OnMouseOut.AddListener((UnityAction)(() =>
        {
            if (spriteRenderer != null)
                spriteRenderer.color = Color.white;
        }));
    }

    private static GameObject CreateCheckBox(Transform parent, CustomOption option, ref float lastY)
    {
        GameObject optionInstance = CreateOptionElement(parent, ModTranslation.GetString(option.Name), ref lastY, "Option_Check");
        var passiveButton = optionInstance.AddComponent<PassiveButton>();
        var checkMark = optionInstance.transform.Find("CheckMark");

        SetupCheckBoxButton(passiveButton, checkMark, option);
        passiveButton.Colliders = new[] { optionInstance.GetComponent<BoxCollider2D>() };

        // チェックボックスの場合はSelectedTextの代わりにCheckMarkを使用するため、
        // 特別な処理としてCheckMarkをTextとして扱う
        var checkMarkTMP = checkMark.gameObject.AddComponent<TextMeshPro>();
        RoleOptionMenu.RoleOptionMenuObjectData.CurrentOptionDisplays.Add((checkMarkTMP, option));

        return optionInstance;
    }

    /// <summary>
    /// オプションが表示されるべきかを判断します
    /// </summary>
    /// <param name="option">チェック対象のオプション</param>
    /// <returns>表示すべき場合はtrue、非表示にすべき場合はfalse</returns>
    private static bool ShouldOptionBeActive(CustomOption option) => option.ShouldDisplay();

    /// <summary>
    /// すべてのオプションの表示状態を更新します
    /// </summary>
    /// <param name="parentTransform">オプションの親Transform</param>
    /// <param name="roleOption">現在のロールオプション</param>
    private static void UpdateOptionsActive(Transform parentTransform, RoleOptionManager.RoleOption roleOption)
    {
        if (parentTransform == null || roleOption == null)
            return;

        // 子オプションの表示状態を更新
        foreach (var option in roleOption.Options)
        {
            UpdateOptionAndChildrenActive(parentTransform, option);
        }
    }

    /// <summary>
    /// オプションとその子オプションの表示状態を再帰的に更新します
    /// </summary>
    /// <param name="parentTransform">オプションの親Transform</param>
    /// <param name="option">更新対象のオプション</param>
    private static void UpdateOptionAndChildrenActive(Transform parentTransform, CustomOption option)
    {
        // このオプションのGameObjectを探す
        var optionObj = FindOptionGameObject(parentTransform, ModTranslation.GetString(option.Name));
        if (optionObj != null)
        {
            // 表示状態を更新
            bool shouldBeActive = ShouldOptionBeActive(option);
            optionObj.SetActive(shouldBeActive);
        }

        // 子オプションも更新
        if (option.ChildrenOption != null)
        {
            foreach (var childOption in option.ChildrenOption)
            {
                UpdateOptionAndChildrenActive(parentTransform, childOption);
            }
        }
    }

    /// <summary>
    /// 指定した名前のオプションGameObjectを探します
    /// </summary>
    /// <param name="parentTransform">検索対象の親Transform</param>
    /// <param name="optionName">オプション名</param>
    /// <returns>見つかったGameObject、または見つからない場合はnull</returns>
    private static GameObject FindOptionGameObject(Transform parentTransform, string optionName)
    {
        for (int i = 0; i < parentTransform.childCount; i++)
        {
            Transform child = parentTransform.GetChild(i);
            TextMeshPro textComp = child.Find("Text")?.GetComponent<TextMeshPro>();
            if (textComp != null && textComp.text == optionName)
            {
                return child.gameObject;
            }
        }
        return null;
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

            // 表示状態を更新
            UpdateDisplayAfterOptionChange(passiveButton.transform.parent, option);

            // ホストの場合、他のプレイヤーに同期
            if (AmongUsClient.Instance.AmHost)
            {
                CustomOptionManager.RpcSyncOption(option.Id, newValue ? (byte)1 : (byte)0);
            }
        }, spriteRenderer);
    }

    private static GameObject CreateSelect(Transform parent, CustomOption option, ref float lastY)
    {
        GameObject optionInstance = CreateOptionElement(parent, ModTranslation.GetString(option.Name), ref lastY, "Option_Select");
        var selectedText = optionInstance.transform.Find("SelectedText").GetComponent<TextMeshPro>();
        selectedText.text = UIHelper.FormatOptionValue(option.Selections[option.Selection], option);

        SetupSelectButtons(optionInstance, selectedText, option);

        // 選択肢の表示とオプションをリストに追加
        RoleOptionMenu.RoleOptionMenuObjectData.CurrentOptionDisplays.Add((selectedText, option));

        return optionInstance;
    }
    private static GameObject CreateNumberOfCrewsSelectAndPerSelect(Transform parent, RoleOptionManager.RoleOption roleOption, ref float lastY)
    {
        GameObject optionInstance = CreateOptionElement(parent, ModTranslation.GetString("NumberOfCrews"), ref lastY, "Option_Select");
        var selectedText = optionInstance.transform.Find("SelectedText").GetComponent<TextMeshPro>();
        selectedText.text = ModTranslation.GetString("NumberOfCrewsSelected", roleOption.NumberOfCrews);

        var minusButton = optionInstance.transform.Find("Button_Minus").gameObject;
        var minusPassiveButton = minusButton.AddComponent<PassiveButton>();
        var minusSpriteRenderer = minusPassiveButton.GetComponent<SpriteRenderer>();
        bool isExist = RoleOptionMenu.RoleOptionMenuObjectData.RoleDetailButtonDictionary.TryGetValue(roleOption.RoleId, out var roleDetailButton);

        ConfigurePassiveButton(minusPassiveButton, () =>
        {
            byte playerCount = (byte)PlayerControl.AllPlayerControls.Count;
            if (15 >= playerCount)
                playerCount = 15;
            roleOption.NumberOfCrews--;
            if (roleOption.NumberOfCrews < 0)
                roleOption.NumberOfCrews = playerCount;
            if (roleOption.NumberOfCrews > playerCount)
                roleOption.NumberOfCrews = playerCount;
            selectedText.text = ModTranslation.GetString("NumberOfCrewsSelected", roleOption.NumberOfCrews);
            if (isExist)
                RoleOptionMenu.UpdateRoleDetailButtonColor(roleDetailButton.GetComponent<SpriteRenderer>(), roleOption);
            RoleOptionManager.RpcSyncRoleOptionDelay(roleOption.RoleId, roleOption.NumberOfCrews, roleOption.Percentage);
        }, minusSpriteRenderer);

        var plusButton = optionInstance.transform.Find("Button_Plus").gameObject;
        var plusPassiveButton = plusButton.AddComponent<PassiveButton>();
        var plusSpriteRenderer = plusPassiveButton.GetComponent<SpriteRenderer>();

        // 確率設定のオプションを生成
        var perOption = CreateAssignPerSelect(parent, roleOption, ref lastY);
        var percentageText = perOption.transform.Find("SelectedText").GetComponent<TextMeshPro>();

        ConfigurePassiveButton(plusPassiveButton, () =>
        {
            byte playerCount = (byte)PlayerControl.AllPlayerControls.Count;
            if (15 >= playerCount)
                playerCount = 15;
            byte oldValue = roleOption.NumberOfCrews;
            roleOption.NumberOfCrews++;
            if (roleOption.NumberOfCrews > playerCount)
                roleOption.NumberOfCrews = 0;
            selectedText.text = ModTranslation.GetString("NumberOfCrewsSelected", roleOption.NumberOfCrews);
            if (isExist)
                RoleOptionMenu.UpdateRoleDetailButtonColor(roleDetailButton.GetComponent<SpriteRenderer>(), roleOption);

            if (oldValue == 0 && roleOption.NumberOfCrews > 0 && roleOption.Percentage == 0)
            {
                roleOption.Percentage = 100;
                percentageText.text = "100%";
            }
            RoleOptionManager.RpcSyncRoleOptionDelay(roleOption.RoleId, roleOption.NumberOfCrews, roleOption.Percentage);
        }, plusSpriteRenderer);

        RoleOptionMenu.RoleOptionMenuObjectData.CurrentRoleNumbersOfCrewsText = selectedText;
        RoleOptionMenu.RoleOptionMenuObjectData.CurrentRolePercentageText = percentageText;
        RoleOptionMenu.RoleOptionMenuObjectData.CurrentRoleId = roleOption.RoleId;

        return optionInstance;
    }

    private static GameObject CreateAssignPerSelect(Transform parent, RoleOptionManager.RoleOption roleOption, ref float lastY)
    {
        GameObject optionInstance = CreateOptionElement(parent, ModTranslation.GetString("AssignPer"), ref lastY, "Option_Select");
        var selectedText = optionInstance.transform.Find("SelectedText").GetComponent<TextMeshPro>();
        selectedText.text = roleOption.Percentage + "%";

        var minusButton = optionInstance.transform.Find("Button_Minus").gameObject;
        var minusPassiveButton = minusButton.AddComponent<PassiveButton>();
        var minusSpriteRenderer = minusPassiveButton.GetComponent<SpriteRenderer>();
        bool isExist = RoleOptionMenu.RoleOptionMenuObjectData.RoleDetailButtonDictionary.TryGetValue(roleOption.RoleId, out var roleDetailButton);
        ConfigurePassiveButton(minusPassiveButton, () =>
        {
            roleOption.Percentage -= 10;
            if (roleOption.Percentage < 0)
                roleOption.Percentage = 100;
            if (roleOption.Percentage > 100)
                roleOption.Percentage = 100;
            selectedText.text = roleOption.Percentage + "%";
            if (isExist)
                RoleOptionMenu.UpdateRoleDetailButtonColor(roleDetailButton.GetComponent<SpriteRenderer>(), roleOption);
            RoleOptionManager.RpcSyncRoleOptionDelay(roleOption.RoleId, roleOption.NumberOfCrews, roleOption.Percentage);
        }, minusSpriteRenderer);

        var plusButton = optionInstance.transform.Find("Button_Plus").gameObject;
        var plusPassiveButton = plusButton.AddComponent<PassiveButton>();
        var plusSpriteRenderer = plusPassiveButton.GetComponent<SpriteRenderer>();

        ConfigurePassiveButton(plusPassiveButton, () =>
        {
            roleOption.Percentage += 10;
            if (roleOption.Percentage > 100)
                roleOption.Percentage = 0;
            selectedText.text = roleOption.Percentage + "%";
            if (isExist)
                RoleOptionMenu.UpdateRoleDetailButtonColor(roleDetailButton.GetComponent<SpriteRenderer>(), roleOption);
            RoleOptionManager.RpcSyncRoleOptionDelay(roleOption.RoleId, roleOption.NumberOfCrews, roleOption.Percentage);
        }, plusSpriteRenderer);
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
            UpdateOptionSelection(option, newSelection, selectedText);
        }, spriteRenderer);
    }

    private static void SetupPlusButton(GameObject optionInstance, TextMeshPro selectedText, CustomOption option)
    {
        var plusButton = optionInstance.transform.Find("Button_Plus").gameObject;
        var passiveButton = plusButton.AddComponent<PassiveButton>();
        var spriteRenderer = passiveButton.GetComponent<SpriteRenderer>();

        ConfigurePassiveButton(passiveButton, () =>
        {
            byte newSelection = option.Selection < option.Selections.Length - 1 ? (byte)(option.Selection + 1) : (byte)0;
            UpdateOptionSelection(option, newSelection, selectedText);
        }, spriteRenderer);
    }

    private static void UpdateOptionSelection(CustomOption option, byte newSelection, TextMeshPro selectedText)
    {
        option.UpdateSelection(newSelection);
        selectedText.text = UIHelper.FormatOptionValue(option.Selections[option.Selection], option);

        // 表示状態を更新
        UpdateDisplayAfterOptionChange(selectedText.transform.parent.parent, option);

        // ホストの場合、他のプレイヤーに同期
        if (AmongUsClient.Instance.AmHost)
        {
            CustomOptionManager.RpcSyncOption(option.Id, newSelection);
        }
    }

    /// <summary>
    /// オプション変更後に表示状態を更新するヘルパーメソッド
    /// </summary>
    /// <param name="parentTransform">親Transform</param>
    /// <param name="changedOption">変更されたオプション</param>
    private static void UpdateDisplayAfterOptionChange(Transform parentTransform, CustomOption changedOption)
    {
        // 子オプションを持つ場合のみ表示状態を更新
        if ((changedOption.ChildrenOption != null && changedOption.ChildrenOption.Count > 0) ||
            changedOption.ParentOption != null) // 親を持つ場合も更新（同じカテゴリの他のオプションが影響を受ける可能性がある）
        {
            var roleId = RoleOptionMenu.RoleOptionMenuObjectData.CurrentRoleId;
            var roleOption = RoleOptionManager.RoleOptions.FirstOrDefault(x => x.RoleId == roleId);
            if (roleOption != null)
            {
                Transform topParent = FindTopParent(parentTransform);
                UpdateOptionsActive(topParent, roleOption);
                RecalculateOptionsPosition(topParent, RoleOptionMenu.RoleOptionMenuObjectData.SettingsScroller);
            }
        }
    }

    /// <summary>
    /// 最上位の親Transformを見つける
    /// </summary>
    /// <param name="transform">子Transform</param>
    /// <returns>最上位の親Transform</returns>
    private static Transform FindTopParent(Transform transform)
    {
        Transform current = transform;
        while (current.parent != null && current.parent.name != "InnerContent" && current.parent.name != "SettingsScroller")
        {
            current = current.parent;
        }
        return current;
    }

    public static void SetupScroll(Transform parent)
    {
        RoleOptionMenu.RoleOptionMenuObjectData.SettingsScroller = parent.transform.Find("SettingsScroller").GetComponent<Scroller>();
        RoleOptionMenu.RoleOptionMenuObjectData.SettingsInner = parent.transform.Find("SettingsScroller/InnerContent");
    }

    public static void ClickedRole(RoleOptionManager.RoleOption roleOption)
    {
        float lastY = DefaultLastY;
        // parentを破棄
        var parent = RoleOptionMenu.RoleOptionMenuObjectData.SettingsInner.Find("Parent");
        if (parent != null)
            GameObject.Destroy(parent.gameObject);

        // 表示リストをクリア
        RoleOptionMenu.RoleOptionMenuObjectData.CurrentOptionDisplays.Clear();

        // 現在のロールオプションを保存（RoleIdだけでなく、直接ロールオプションへの参照も保持）
        RoleOptionMenu.RoleOptionMenuObjectData.CurrentRoleId = roleOption.RoleId;

        int index = CreateRoleOptions(roleOption, ref lastY);

        // スクロール位置をリセット
        ResetScrollPosition();

        UpdateScrollerBounds(index);
    }

    // スクロール位置をリセットするメソッド
    private static void ResetScrollPosition()
    {
        var settingsInner = RoleOptionMenu.RoleOptionMenuObjectData.SettingsInner;
        var settingsScroller = RoleOptionMenu.RoleOptionMenuObjectData.SettingsScroller;

        if (settingsInner != null && settingsScroller != null)
        {
            // スクロール位置を一番上にリセット
            settingsInner.localPosition = new Vector3(settingsInner.localPosition.x, 0f, settingsInner.localPosition.z);
            settingsScroller.UpdateScrollBars();
        }
    }

    /// <summary>
    /// 表示されているオプションの位置を再計算します
    /// </summary>
    /// <param name="parentTransform">オプションの親Transform</param>
    /// <param name="scroller">スクロールコンポーネント</param>
    private static void RecalculateOptionsPosition(Transform parentTransform, Scroller scroller)
    {
        float lastY = DefaultLastY;
        int activeCount = 0;

        for (int i = 0; i < parentTransform.childCount; i++)
        {
            Transform child = parentTransform.GetChild(i);
            if (!child.gameObject.activeInHierarchy)
                continue;

            // オプションの位置を更新
            child.localPosition = new Vector3(ElementXPosition, lastY, ElementZPosition);
            lastY -= DefaultRate;
            activeCount++;
        }

        // スクロールバーの範囲を設定
        if (scroller != null)
        {
            scroller.ContentYBounds.max = activeCount <= 4 ? 0.1f : (activeCount - 4) * DefaultRate + 2f;
            scroller.UpdateScrollBars();
        }
    }

    private static int CreateRoleOptions(RoleOptionManager.RoleOption roleOption, ref float lastY)
    {
        var parent = new GameObject("Parent");
        parent.transform.SetParent(RoleOptionMenu.RoleOptionMenuObjectData.SettingsInner);
        parent.transform.localScale = Vector3.one;
        parent.transform.localPosition = Vector3.zero;
        parent.layer = 5;
        CreateNumberOfCrewsSelectAndPerSelect(parent.transform, roleOption, ref lastY);
        int index = 2;
        Logger.Info($"roleOption: {roleOption.RoleId}");
        foreach (var option in roleOption.Options)
        {
            Logger.Info($"option: {option.Name}");
            if (option.IsBooleanOption)
                CreateCheckBox(parent.transform, option, ref lastY);
            else
                CreateSelect(parent.transform, option, ref lastY);
            index++;
        }

        // オプションの表示状態を初期化
        UpdateOptionsActive(parent.transform, roleOption);

        // 表示状態更新後に位置を再計算
        RecalculateOptionsPosition(parent.transform, RoleOptionMenu.RoleOptionMenuObjectData.SettingsScroller);

        return index;
    }

    private static void UpdateScrollerBounds(int index)
    {
        // スクロールバーの範囲を設定
        RoleOptionMenu.RoleOptionMenuObjectData.SettingsScroller.ContentYBounds.max =
            index < 4 ? 0.1f : index == 4 ? 2.1f : (index - 4) * DefaultRate + 2f; // 最小値を0.1fに設定して常にスクロールバーが表示されるようにする

        // スクロールバーを更新
        RoleOptionMenu.RoleOptionMenuObjectData.SettingsScroller.UpdateScrollBars();

        Logger.Info($"Updated Max {index} {RoleOptionMenu.RoleOptionMenuObjectData.SettingsScroller.ContentYBounds.max}");
    }

    /// <summary>
    /// 設定画面の表示を更新する
    /// </summary>
    /// <param name="roleOption">更新対象のロールオプション</param>
    public static void UpdateSettingsDisplay(RoleOptionManager.RoleOption roleOption)
    {
        var data = RoleOptionMenu.RoleOptionMenuObjectData;
        if (data == null) return;

        // 人数と確率の表示を更新
        if (data.CurrentRoleId == roleOption.RoleId)
        {
            if (data.CurrentRoleNumbersOfCrewsText != null)
            {
                data.CurrentRoleNumbersOfCrewsText.text = ModTranslation.GetString("NumberOfCrewsSelected", roleOption.NumberOfCrews);
            }
            if (data.CurrentRolePercentageText != null)
            {
                data.CurrentRolePercentageText.text = roleOption.Percentage + "%";
            }
        }

        // その他のオプションの表示を更新
        if (data.CurrentOptionDisplays == null) return;
        foreach (var (text, option) in data.CurrentOptionDisplays)
        {
            if (text == null) continue;

            if (option.IsBooleanOption)
            {
                // チェックボックスの場合、TextMeshProの親オブジェクト（CheckMark）の表示を切り替え
                text.transform.gameObject.SetActive((bool)option.Value);
            }
            else
            {
                text.text = UIHelper.FormatOptionValue(option.Value, option);
            }
        }

        // 親オプションに基づいて子オプションの表示状態を更新
        var parent = data.SettingsInner?.Find("Parent");
        if (parent != null && roleOption != null)
        {
            UpdateOptionsActive(parent, roleOption);
            RecalculateOptionsPosition(parent, data.SettingsScroller);
        }
    }

    public static void HideRoleSettings()
    {
        var parent = RoleOptionMenu.RoleOptionMenuObjectData?.SettingsInner?.Find("Parent");
        if (parent != null)
            GameObject.Destroy(parent.gameObject);

        // 表示リストをクリア
        if (RoleOptionMenu.RoleOptionMenuObjectData?.CurrentOptionDisplays != null)
            RoleOptionMenu.RoleOptionMenuObjectData.CurrentOptionDisplays.Clear();
    }
}
