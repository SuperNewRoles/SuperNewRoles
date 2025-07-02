using System.Collections.Generic;
using System.Linq;
using SuperNewRoles.Modules;
using UnityEngine;
using UnityEngine.Events;
using SuperNewRoles.CustomOptions.Data;
using TMPro;

namespace SuperNewRoles.CustomOptions;

public static class StandardOptionMenu
{
    private static class Constants
    {
        public const float ButtonSpacing = 0.6125f;
        public const float ButtonScale = 0.48f;
        public const float InitialYPosition = 1.4f;
        public const float InitialXPosition = -3.614f;
    }

    public static void ShowStandardOptionMenu()
    {
        if (StandardOptionMenuObjectData.Instance?.StandardOptionMenu == null)
            Initialize();
        StandardOptionMenuObjectData.Instance.StandardOptionMenu.SetActive(true);
    }

    public static void Initialize()
    {
        var menu = UIHelper.InstantiateUIElement("StandardOptionMenu",
            RoleOptionMenu.GetGameSettingMenu().transform,
            new Vector3(0, 0, -2f),
            Vector3.one);
        new StandardOptionMenuObjectData(menu);

        GenerateCategoryButtons();
    }

    private static void GenerateCategoryButtons()
    {
        int index = 0;
        foreach (var category in CustomOptionManager.OptionCategories)
        {
            if (category.IsModifier)
                continue;
            GenerateRoleDetailButton(category, index++);
        }
    }

    public static GameObject GenerateRoleDetailButton(CustomOptionCategory category, int index)
    {
        if (StandardOptionMenuObjectData.Instance == null)
            return null;

        var buttonPosition = new Vector3(
            Constants.InitialXPosition,
            Constants.InitialYPosition - (index * Constants.ButtonSpacing),
            UIHelper.Constants.DefaultZPosition);

        var obj = UIHelper.InstantiateUIElement(
            RoleOptionMenu.ROLE_DETAIL_BUTTON_ASSET_NAME,
            StandardOptionMenuObjectData.Instance.LeftAreaInner.transform,
            buttonPosition,
            Vector3.one * Constants.ButtonScale);

        UIHelper.SetText(obj, ModTranslation.GetString(category.Name));
        ConfigureButton(obj, category);

        return obj;
    }

    private static void ConfigureButton(GameObject buttonObj, CustomOptionCategory category)
    {
        var passiveButton = buttonObj.AddComponent<PassiveButton>();
        passiveButton.Colliders = new Collider2D[1] { buttonObj.GetComponent<BoxCollider2D>() };
        GameObject selectedObject = buttonObj.transform.Find("Selected")?.gameObject;

        UIHelper.ConfigurePassiveButton(passiveButton, (UnityAction)(() =>
        {
            HandleButtonClick(category, selectedObject);
        }));

        ConfigureButtonHoverEffects(passiveButton, selectedObject, category);
    }

    private static void HandleButtonClick(CustomOptionCategory category, GameObject selectedObject)
    {
        Logger.Info($"{category.Name}");
        var menuData = StandardOptionMenuObjectData.Instance;

        // 現在のメニューを非表示に
        if (menuData.CurrentOptionMenu != null)
            menuData.CurrentOptionMenu.SetActive(false);
        menuData.CurrentOptionMenu = null;

        // プリセットボタンのコンテナを非表示に
        if (menuData.PresetButtonsContainer != null)
            menuData.PresetButtonsContainer.SetActive(false);

        // 現在の選択ボタンを更新
        if (menuData.CurrentSelectedButton != null)
            menuData.CurrentSelectedButton.SetActive(false);

        // ModeMenuを非表示に
        if (menuData.ModeMenu != null)
            menuData.ModeMenu.SetActive(false);
        menuData.CurrentSelectedButton = selectedObject;
        menuData.CurrentCategory = category;
        selectedObject?.SetActive(true);

        // 適切なメニューを表示
        if (category == Categories.Categories.PresetSettings)
        {
            ShowPresetOptionMenu();
        }
        else if (category == Categories.Categories.ModeSettings)
        {
            ShowModeOptionMenu();
        }
        else
        {
            ShowDefaultOptionMenu(category, menuData.RightAreaInner.transform);
        }
    }
    private static void ShowModeOptionMenu()
    {
        if (StandardOptionMenuObjectData.Instance.ModeMenu == null)
        {
            // ModeMenu from asset
            var modeMenu = UIHelper.InstantiateUIElement("ModeMenu",
                StandardOptionMenuObjectData.Instance.RightArea.transform,
            new Vector3(0, 0, -7f),
                Vector3.one);
            StandardOptionMenuObjectData.Instance.ModeMenu = modeMenu;
        }
        else
            StandardOptionMenuObjectData.Instance.ModeMenu.SetActive(true);
        var modeOptionText = StandardOptionMenuObjectData.Instance.ModeMenu.transform.Find("ModeOption/Text").GetComponent<TextMeshPro>();
        modeOptionText.text = $"{ModTranslation.GetString("ModeOption")}";
        ConfigureModeOption(StandardOptionMenuObjectData.Instance.ModeMenu);
        ShowDefaultOptionMenu(Categories.Categories.ModeSettings, StandardOptionMenuObjectData.Instance.RightAreaInner.transform);
    }
    private static void ConfigureModeOption(GameObject modeMenu)
    {
        var modeOption = modeMenu.transform.Find("ModeOption").gameObject;
        ConfigureSelectOptionButtons(modeOption, modeOption.transform.Find("SelectedText").GetComponent<TextMeshPro>(), CustomOptionManager.GetCustomOptionByFieldName(nameof(Categories.Categories.ModeOption)));
    }
    private static void ConfigureButtonHoverEffects(PassiveButton button, GameObject selectedObject, CustomOptionCategory category)
    {
        button.OnMouseOut = new UnityEvent();
        button.OnMouseOut.AddListener((UnityAction)(() =>
        {
            if (StandardOptionMenuObjectData.Instance.CurrentCategory != category)
                selectedObject?.SetActive(false);
        }));

        button.OnMouseOver = new UnityEvent();
        button.OnMouseOver.AddListener((UnityAction)(() =>
        {
            if (StandardOptionMenuObjectData.Instance.CurrentCategory != category)
                selectedObject?.SetActive(true);
        }));
    }

    public static void ShowPresetOptionMenu()
    {
        var menuData = StandardOptionMenuObjectData.Instance;
        // プリセットボタンを生成
        var rightAreaInner = StandardOptionMenuObjectData.Instance.RightAreaInner;
        GeneratePresetButtons(rightAreaInner);
        if (menuData.StandardOptionMenus.TryGetValue(Categories.Categories.PresetSettings.Name, out var menu))
        {
            menuData.CurrentOptionMenu = menu;
            menu.SetActive(true);
            return;
        }

        var presetMenu = CreatePresetMenu();
        ConfigurePresetWriteBox(presetMenu);

        ConfigurePresetTitle(presetMenu);
        ConfigureNowPresetText(presetMenu);

        menuData.StandardOptionMenus[Categories.Categories.PresetSettings.Name] = presetMenu;
        menuData.CurrentOptionMenu = presetMenu;
    }
    private static void ConfigureNowPresetText(GameObject presetMenu)
    {
        var nowPresetText = presetMenu?.transform?.Find("NowPreset")?.gameObject;
        if (nowPresetText == null)
            return;
        nowPresetText.transform.Find("StaticText").GetComponent<TextMeshPro>().text = ModTranslation.GetString("PresetSettings.NowPreset");
        UpdateNowPresetText(presetMenu);
    }
    private static void UpdateNowPresetText(GameObject presetMenu)
    {
        var nowPresetText = presetMenu?.transform?.Find("NowPreset")?.gameObject;
        if (nowPresetText == null)
            return;
        nowPresetText.transform.Find("NowPresetText").GetComponent<TextMeshPro>().text = CustomOptionSaver.GetPresetName(CustomOptionSaver.CurrentPreset);
    }
    private static void ConfigurePresetTitle(GameObject presetMenu)
    {
        var presetTitle = presetMenu?.transform?.Find("PresetTitle")?.gameObject;
        if (presetTitle == null)
            return;
        UIHelper.SetText(presetTitle, ModTranslation.GetString("PresetSettings"));
    }
    private static GameObject CreatePresetMenu()
    {
        var menu = UIHelper.InstantiateUIElement(
            "PresetMenu",
            StandardOptionMenuObjectData.Instance.RightArea.transform,
            new(0, 0, -5f),
            Vector3.one);

        // PresetTitleのテキストを翻訳
        var presetTitle = menu.transform.Find("PresetTitle/Text").gameObject;
        UIHelper.SetText(presetTitle, ModTranslation.GetString("PresetMenuTitle"));

        var submitPreset = menu.transform.Find("SubmitPreset").gameObject;
        var addPresetButton = submitPreset.transform.Find("AddPresetButton").gameObject;

        // テキストの設定
        UIHelper.SetText(addPresetButton, ModTranslation.GetString("AddPreset"));

        // PassiveButtonの設定
        var passiveButton = addPresetButton.AddComponent<PassiveButton>();
        passiveButton.Colliders = new Collider2D[1] { addPresetButton.GetComponent<BoxCollider2D>() };
        var spriteRenderer = addPresetButton.GetComponent<SpriteRenderer>();

        UIHelper.ConfigurePassiveButton(passiveButton, (UnityAction)(() =>
        {
            HandleAddPreset();
        }), spriteRenderer);

        return menu;
    }

    private static void HandleAddPreset()
    {
        var writeBox = StandardOptionMenuObjectData.Instance.CurrentOptionMenu.transform.Find("SubmitPreset/WriteBox");
        var writeBoxTextBoxTMP = writeBox.GetComponent<TextBoxTMP>();
        var writeBoxTMP = writeBox.transform.Find("Text").GetComponent<TextMeshPro>();
        string text = writeBoxTextBoxTMP.text;
        Logger.Info(text);
        if (string.IsNullOrEmpty(text))
            return;
        int maxPreset = CustomOptionSaver.PresetNames.Any() ? CustomOptionSaver.PresetNames.Keys.Max() : -1;
        int newPreset = maxPreset + 1;

        CustomOptionSaver.SetPresetName(newPreset, text);
        CustomOptionSaver.CurrentPreset = newPreset;
        CustomOptionSaver.Save();
        writeBoxTextBoxTMP.Clear();
        writeBoxTMP.text = ModTranslation.GetString("PresetPleaseInput");
        GeneratePresetButtons(StandardOptionMenuObjectData.Instance.RightAreaInner);
        UpdateNowPresetText(StandardOptionMenuObjectData.Instance.CurrentOptionMenu);
    }
    private static void GeneratePresetButtons(GameObject container)
    {
        var menuData = StandardOptionMenuObjectData.Instance;

        // 既存のコンテナを削除
        if (menuData.PresetButtonsContainer != null)
        {
            GameObject.Destroy(menuData.PresetButtonsContainer);
        }

        // 新しいコンテナを作成
        var buttonsContainer = new GameObject("PresetButtonsContainer");
        buttonsContainer.transform.SetParent(container.transform);
        buttonsContainer.transform.localScale = Vector3.one;
        buttonsContainer.transform.localPosition = new(0, 0, 4.7f);
        menuData.PresetButtonsContainer = buttonsContainer;
        buttonsContainer.SetActive(true);

        float xPos = 1.25f;
        float yPos = 1f;
        const float ySpacing = -0.55f;

        int index = 0;
        foreach (var preset in CustomOptionSaver.PresetNames)
        {
            var buttonPosition = new Vector3(
                xPos,
                yPos + (index * ySpacing),
                -0.21f
            );

            // プリセットボタンの生成
            var presetButton = UIHelper.InstantiateUIElement(
                "PresetButton",
                buttonsContainer.transform,
                buttonPosition,
                Vector3.one * 0.4f
            );
            presetButton.name = $"PresetButton_{preset.Key}";

            UIHelper.SetText(presetButton, preset.Value);
            ConfigurePresetButton(presetButton, preset.Key);

            // ゴミ箱ボタンの生成
            var trashButton = presetButton.transform.Find("TrashButton").gameObject;
            ConfigureTrashButton(trashButton, preset.Key);

            index++;
        }

        // Scrollerの更新
        if (menuData.RightAreaScroller != null)
        {
            float maxBound = index <= 4 ? 0f : ((index - 4) * 0.7f);
            menuData.RightAreaScroller.ContentYBounds.max = maxBound;
        }
    }

    private static void ConfigurePresetButton(GameObject buttonObj, int presetId)
    {
        var passiveButton = buttonObj.AddComponent<PassiveButton>();
        passiveButton.Colliders = new Collider2D[] { buttonObj.GetComponent<BoxCollider2D>() };
        var spriteRenderer = buttonObj.transform.Find("Background").GetComponent<SpriteRenderer>();

        UIHelper.ConfigurePassiveButton(passiveButton, (UnityAction)(() =>
        {
            Logger.Info($"Preset {presetId} selected");
            CustomOptionSaver.Save();
            CustomOptionSaver.LoadPreset(presetId);
            UpdateNowPresetText(StandardOptionMenuObjectData.Instance.CurrentOptionMenu);
            OptionMenuBase.UpdateOptionDisplayAll();
        }), spriteRenderer, selectedObject: buttonObj.transform.Find("Selected")?.gameObject);
    }

    private static void ConfigureTrashButton(GameObject buttonObj, int presetId)
    {
        var passiveButton = buttonObj.AddComponent<PassiveButton>();
        passiveButton.Colliders = new Collider2D[] { buttonObj.GetComponent<BoxCollider2D>() };
        var selectedObject = buttonObj.transform.Find("Selected")?.gameObject;
        if (selectedObject != null)
            selectedObject.SetActive(false);

        UIHelper.ConfigurePassiveButton(passiveButton, (UnityAction)(() =>
        {
            HandleDeletePreset(presetId);
        }));
        passiveButton.OnMouseOver = new();
        passiveButton.OnMouseOver.AddListener((UnityAction)(() =>
        {
            selectedObject?.SetActive(true);
        }));

        passiveButton.OnMouseOut = new();
        passiveButton.OnMouseOut.AddListener((UnityAction)(() =>
        {
            selectedObject?.SetActive(false);
        }));
    }

    private static void HandleDeletePreset(int presetId)
    {
        if (!CustomOptionSaver.PresetNames.ContainsKey(presetId))
            return;

        CustomOptionSaver.RemovePreset(presetId);

        // プリセットボタンを再生成
        var rightAreaInner = StandardOptionMenuObjectData.Instance.RightAreaInner;
        GeneratePresetButtons(rightAreaInner);
        UpdateNowPresetText(StandardOptionMenuObjectData.Instance.CurrentOptionMenu);
    }

    private static void ConfigurePresetButton(
        GameObject selectPresets,
        string buttonName,
        TMPro.TextMeshPro selectedText,
        bool isIncrement)
    {
        var button = selectPresets.transform.Find(buttonName).gameObject;
        var passiveButton = button.AddComponent<PassiveButton>();
        passiveButton.Colliders = new Collider2D[] { button.GetComponent<BoxCollider2D>() };
        var spriteRenderer = passiveButton.GetComponent<SpriteRenderer>();

        UIHelper.ConfigurePassiveButton(passiveButton, (UnityAction)(() =>
        {
            HandlePresetNavigation(selectedText, isIncrement);
        }), spriteRenderer);
    }

    private static void HandlePresetNavigation(TMPro.TextMeshPro selectedText, bool isIncrement)
    {
        CustomOptionSaver.Save(); // 現在のプリセットを保存

        int newPreset;
        if (isIncrement)
        {
            newPreset = CustomOptionSaver.CurrentPreset < CustomOptionSaver.PresetNames.Keys.Max() ?
                CustomOptionSaver.CurrentPreset + 1 :
                0;
        }
        else
        {
            newPreset = CustomOptionSaver.CurrentPreset > 0 ?
                CustomOptionSaver.CurrentPreset - 1 :
                CustomOptionSaver.PresetNames.Keys.Max();
        }

        CustomOptionSaver.LoadPreset(newPreset);
        UpdateNowPresetText(StandardOptionMenuObjectData.Instance.CurrentOptionMenu);
        OptionMenuBase.UpdateOptionDisplayAll();
        selectedText.text = CustomOptionSaver.GetPresetName(newPreset);
    }

    private static void ConfigurePresetWriteBox(GameObject presetMenu)
    {
        var writeBox = presetMenu.transform.Find("SubmitPreset/WriteBox").gameObject;
        var writeBoxTextBoxTMP = writeBox.GetComponent<TextBoxTMP>();
        var writeBoxTMP = writeBox.transform.Find("Text").GetComponent<TextMeshPro>();
        var writeBoxPassiveButton = writeBox.AddComponent<PassiveButton>();
        var writeBoxSpriteRenderer = writeBox.transform.Find("Background").GetComponent<SpriteRenderer>();

        UIHelper.SetText(writeBox, ModTranslation.GetString("PresetPleaseInput"));

        ConfigureWriteBoxButton(writeBox, writeBoxPassiveButton, writeBoxSpriteRenderer, writeBoxTextBoxTMP, writeBoxTMP);
        writeBoxTextBoxTMP.OnFocusLost = new();
        writeBoxTextBoxTMP.OnFocusLost.AddListener((UnityAction)(() =>
        {
            if (string.IsNullOrEmpty(writeBoxTextBoxTMP.text))
                UIHelper.SetText(writeBox, ModTranslation.GetString("PresetPleaseInput"));
        }));
        // ConfigureWriteBoxEnterEvent(writeBoxTextBoxTMP);
    }

    private static void ConfigureWriteBoxButton(
        GameObject writeBox,
        PassiveButton writeBoxPassiveButton,
        SpriteRenderer writeBoxSpriteRenderer,
        TextBoxTMP writeBoxTextBoxTMP,
        TextMeshPro writeBoxTMP)
    {
        writeBoxPassiveButton.Colliders = new Collider2D[] { writeBox.GetComponent<BoxCollider2D>() };

        UIHelper.ConfigurePassiveButton(writeBoxPassiveButton, (UnityAction)(() =>
        {
            writeBoxTextBoxTMP.GiveFocus();
            if (string.IsNullOrEmpty(writeBoxTextBoxTMP.text))
                writeBoxTMP.text = "";
        }), writeBoxSpriteRenderer, Color.green);
    }


    public static void ShowDefaultOptionMenu(CustomOptionCategory category, Transform parent)
    {
        var menuData = StandardOptionMenuObjectData.Instance;
        if (menuData.StandardOptionMenus.TryGetValue(category.Name, out var existingMenu))
        {
            ShowExistingMenu(existingMenu, menuData);
            return;
        }

        var defaultMenu = CreateDefaultMenu(category.Name, parent);
        GenerateOptionsForCategory(category, defaultMenu.transform);
        UpdateOptionsActive();
        RecalculateOptionsPosition(defaultMenu.transform, menuData.RightAreaScroller);
        menuData.RightAreaInner.transform.localPosition = new(menuData.RightAreaInner.transform.localPosition.x, 0, menuData.RightAreaInner.transform.localPosition.z);

        menuData.StandardOptionMenus.Add(category.Name, defaultMenu);
        menuData.CurrentOptionMenu = defaultMenu;
    }
    private static void UpdateOptionsActive()
    {
        var menuData = StandardOptionMenuObjectData.Instance;
        if (!menuData.CategoryOptionObjects.TryGetValue(menuData.CurrentCategory.Name, out var optionObjects))
            return;

        foreach (var (option, gameObject) in optionObjects)
        {
            bool shouldBeActive = ShouldOptionBeActive(option);
            gameObject.SetActive(shouldBeActive);
        }
    }
    private static bool ShouldOptionBeActive(CustomOption option) => option.ShouldDisplay();
    private static void RecalculateOptionsPosition(Transform menuTransform, Scroller scroller)
    {
        float lastY = 1.6f;
        int activeCount = 0;
        for (int i = 0; i < menuTransform.childCount; i++)
        {
            Transform child = menuTransform.GetChild(i);
            if (!child.gameObject.activeSelf)
                continue;
            child.localPosition = new Vector3(3.42f, lastY, -0.21f);
            lastY -= 0.7f;
            activeCount++;
        }

        if (scroller != null)
        {
            scroller.ContentYBounds.max = activeCount <= 6 ? 0f : (activeCount - 6) * 0.7f;
        }
    }

    private static void GenerateOptionsForCategory(CustomOptionCategory category, Transform menuTransform)
    {
        var optionObjects = new List<(CustomOption, GameObject)>();
        foreach (var option in category.Options)
        {
            // トップレベルのオプションはisChildフラグをfalseにして生成
            var obj = GenerateStandardOption(option, menuTransform, false);
            optionObjects.Add((option, obj));
            // 子オプションがあれば再帰的に生成
            GenerateChildOptions(option, menuTransform, optionObjects);
        }
        StandardOptionMenuObjectData.Instance.CategoryOptionObjects[category.Name] = optionObjects;
    }

    private static void GenerateChildOptions(CustomOption parentOption, Transform menuTransform, List<(CustomOption, GameObject)> optionObjects)
    {
        if (parentOption.ChildrenOption == null) return;

        foreach (var childOption in parentOption.ChildrenOption)
        {
            var obj = GenerateStandardOption(childOption, menuTransform, true);
            optionObjects.Add((childOption, obj));
            GenerateChildOptions(childOption, menuTransform, optionObjects);
        }
    }

    private static GameObject GenerateStandardOption(CustomOption option, Transform parent, bool isChild)
    {
        GameObject obj;
        if (option.IsBooleanOption)
        {
            obj = GenerateStandardOptionCheck(option, parent, isChild);
        }
        else
        {
            obj = GenerateStandardOptionSelect(option, parent, isChild);
        }
        return obj;
    }

    private static GameObject GenerateStandardOptionCheck(CustomOption option, Transform parent, bool isChild)
    {
        var check = CreateOptionCheckObject(option, parent, isChild);
        var checkMark = check.transform.Find("CheckMark").gameObject;

        // 初期状態を設定
        checkMark.SetActive((bool)option.Value);

        // UIデータを保存
        StandardOptionMenuObjectData.Instance.AddOptionUIData(
            StandardOptionMenuObjectData.Instance.CurrentCategory.Name,
            option,
            check,
            true
        );

        ConfigureCheckOptionButton(check, checkMark, option);
        return check;
    }

    private static GameObject CreateOptionCheckObject(CustomOption option, Transform parent, bool isChild)
    {
        var assetName = isChild ? "StandardChildOption_Check" : "StandardOption_Check";
        var check = UIHelper.InstantiateUIElement(
            assetName,
            parent,
            Vector3.zero,
            Vector3.one * 0.4f);

        UIHelper.SetText(check, ModTranslation.GetString(option.Name));
        return check;
    }

    private static void ConfigureCheckOptionButton(GameObject check, GameObject checkMark, CustomOption option)
    {
        var passiveButton = check.AddComponent<PassiveButton>();
        passiveButton.Colliders = new Collider2D[] { check.GetComponent<BoxCollider2D>() };
        var spriteRenderer = check.GetComponent<SpriteRenderer>();

        UIHelper.ConfigurePassiveButton(passiveButton, (UnityAction)(() =>
        {
            bool newValue = !checkMark.activeSelf;
            checkMark.SetActive(newValue);
            option.UpdateSelection(newValue ? (byte)1 : (byte)0);
            UpdateOptionsActive();
            RecalculateOptionsPosition(check.transform.parent, StandardOptionMenuObjectData.Instance.RightAreaScroller);
        }), spriteRenderer);
    }

    private static GameObject GenerateStandardOptionSelect(CustomOption option, Transform parent, bool isChild)
    {
        var selectObject = CreateOptionSelectObject(option, parent, isChild);
        var selectedText = selectObject.transform.Find("SelectedText").GetComponent<TMPro.TextMeshPro>();

        // UIデータを保存
        StandardOptionMenuObjectData.Instance.AddOptionUIData(
            StandardOptionMenuObjectData.Instance.CurrentCategory.Name,
            option,
            selectObject,
            false
        );

        ConfigureSelectOptionButtons(selectObject, selectedText, option);
        return selectObject;
    }

    private static GameObject CreateOptionSelectObject(CustomOption option, Transform parent, bool isChild)
    {
        var assetName = isChild ? "StandardChildOption_Select" : "StandardOption_Select";
        var selectObject = UIHelper.InstantiateUIElement(
            assetName,
            parent,
            Vector3.zero,
            Vector3.one * 0.4f);

        UIHelper.SetText(selectObject, ModTranslation.GetString(option.Name));
        return selectObject;
    }

    private static void ConfigureSelectOptionButtons(GameObject selectObject, TMPro.TextMeshPro selectedText, CustomOption option)
    {
        ConfigureSelectButton(selectObject, "Button_Minus", selectedText, option, isIncrement: false);
        ConfigureSelectButton(selectObject, "Button_Plus", selectedText, option, isIncrement: true);
    }

    private static void ConfigureSelectButton(
        GameObject selectObject,
        string buttonName,
        TMPro.TextMeshPro selectedText,
        CustomOption option,
        bool isIncrement)
    {
        var button = selectObject.transform.Find(buttonName).gameObject;
        var passiveButton = button.AddComponent<PassiveButton>();
        passiveButton.Colliders = new Collider2D[] { button.GetComponent<BoxCollider2D>() };
        var spriteRenderer = passiveButton.GetComponent<SpriteRenderer>();

        selectedText.text = option.GetCurrentSelectionString();

        UIHelper.ConfigurePassiveButton(passiveButton, (UnityAction)(() =>
        {
            HandleOptionSelection(option, selectedText, isIncrement);
        }), spriteRenderer);
    }

    private static void HandleOptionSelection(CustomOption option, TMPro.TextMeshPro selectedText, bool isIncrement)
    {
        byte newSelection;
        int additional = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) ? RoleOptionSettings.ShiftSelection : 1;
        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
        {
            newSelection = isIncrement ? (byte)(option.Selections.Length - 1) : (byte)0;
        }
        else
        {
            if (isIncrement)
            {
                newSelection = option.Selection + additional < option.Selections.Length ? (byte)(option.Selection + additional) : (byte)0;
            }
            else
            {
                newSelection = option.Selection - additional >= 0 ? (byte)(option.Selection - additional) : (byte)(option.Selections.Length - additional);
            }
        }

        UpdateOptionSelection(option, newSelection, selectedText);
        UpdateOptionsActive();
        RecalculateOptionsPosition(StandardOptionMenuObjectData.Instance.CurrentOptionMenu.transform, StandardOptionMenuObjectData.Instance.RightAreaScroller);

        // ホストの場合、他のプレイヤーに同期
        if (AmongUsClient.Instance.AmHost)
        {
            CustomOptionManager.RpcSyncOption(option.Id, newSelection);
        }
    }

    private static void UpdateOptionSelection(CustomOption option, byte newSelection, TMPro.TextMeshPro selectedText)
    {
        option.UpdateSelection(newSelection);
        selectedText.text = option.GetCurrentSelectionString();
        StandardOptionMenuObjectData.Instance.UpdateOptionDisplay();
    }

    private static void UpdatePresetText(TMPro.TextMeshPro textComponent, int preset)
    {
        string presetName = CustomOptionSaver.GetPresetName(preset);
        textComponent.text = presetName;
    }

    private static void UpdateOptionUIValues(CustomOption option, Transform menuTransform)
    {
        var menuData = StandardOptionMenuObjectData.Instance;
        if (!menuData.CategoryOptionUIData.TryGetValue(menuData.CurrentCategory.Name, out var optionUIDataList))
            return;

        foreach (var optionUIData in optionUIDataList)
        {
            if (optionUIData.Option == option)
            {
                if (option.IsBooleanOption && optionUIData is StandardOptionMenuObjectData.CheckOptionUIData checkData)
                {
                    checkData.CheckMark.SetActive((bool)option.Value);
                }
                else if (!option.IsBooleanOption && optionUIData is StandardOptionMenuObjectData.SelectOptionUIData selectData)
                {
                    selectData.SelectedText.text = UIHelper.FormatOptionValue(option.Selections[option.Selection], option);
                }
                break;
            }
        }

        // 子オプションも再帰的に更新
        if (option.ChildrenOption != null)
        {
            foreach (var childOption in option.ChildrenOption)
            {
                UpdateOptionUIValues(childOption, menuTransform);
            }
        }
    }

    private static void ShowExistingMenu(GameObject existingMenu, StandardOptionMenuObjectData menuData)
    {
        menuData.CurrentOptionMenu = existingMenu;
        existingMenu.SetActive(true);
        menuData.UpdateOptionDisplay();
        RecalculateOptionsPosition(existingMenu.transform, menuData.RightAreaScroller);
        menuData.RightAreaInner.transform.localPosition = new Vector3(menuData.RightAreaInner.transform.localPosition.x, 0, menuData.RightAreaInner.transform.localPosition.z);
    }

    private static GameObject CreateDefaultMenu(string categoryName, Transform parent)
    {
        var menu = new GameObject($"{categoryName}OptionMenu");
        menu.transform.SetParent(parent);
        menu.transform.localScale = Vector3.one;
        menu.transform.localPosition = Vector3.zero;
        return menu;
    }
}