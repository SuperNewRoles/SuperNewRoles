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

        // 現在の選択ボタンを更新
        if (menuData.CurrentSelectedButton != null)
            menuData.CurrentSelectedButton.SetActive(false);
        menuData.CurrentSelectedButton = selectedObject;
        menuData.CurrentCategory = category;
        selectedObject?.SetActive(true);

        // 適切なメニューを表示
        if (category == CustomOptionManager.PresetSettings)
        {
            ShowPresetOptionMenu();
        }
        else
        {
            ShowDefaultOptionMenu(category, menuData.RightAreaInner.transform);
        }
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
        if (menuData.StandardOptionMenus.TryGetValue(CustomOptionManager.PresetSettings.Name, out var menu))
        {
            menuData.CurrentOptionMenu = menu;
            menu.SetActive(true);
            return;
        }

        var presetMenu = CreatePresetMenu();
        ConfigurePresetWriteBox(presetMenu);

        // プリセットボタンを生成
        var rightAreaInner = StandardOptionMenuObjectData.Instance.RightAreaInner;
        GeneratePresetButtons(rightAreaInner);

        // ConfigurePresetMenu(presetMenu);

        menuData.StandardOptionMenus[CustomOptionManager.PresetSettings.Name] = presetMenu;
        menuData.CurrentOptionMenu = presetMenu;
    }

    private static GameObject CreatePresetMenu()
    {
        var menu = UIHelper.InstantiateUIElement(
            "PresetMenu",
            StandardOptionMenuObjectData.Instance.RightArea.transform,
            new(0, 0, -2f),
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
        CustomOptionSaver.Save();
        writeBoxTextBoxTMP.Clear();
        GeneratePresetButtons(StandardOptionMenuObjectData.Instance.RightAreaInner);
    }
    private static void GeneratePresetButtons(GameObject container)
    {
        // 既存のボタンをクリア
        for (int i = 0; i < container.transform.childCount; i++)
        {
            Transform child = container.transform.GetChild(i);
            if (child.name.StartsWith("PresetButton"))
            {
                GameObject.Destroy(child.gameObject);
            }
        }

        float xPos = 1.42f;
        float yPos = 1.6f;
        const float ySpacing = -0.7f;

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
                container.transform,
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
        if (StandardOptionMenuObjectData.Instance.RightAreaScroller != null)
        {
            float minY = yPos + ((index - 1) * ySpacing);
            StandardOptionMenuObjectData.Instance.RightAreaScroller.ContentYBounds.max =
                Mathf.Max(0f, -minY + 4.5f);
        }
    }

    private static void ConfigurePresetButton(GameObject buttonObj, int presetId)
    {
        var passiveButton = buttonObj.AddComponent<PassiveButton>();
        passiveButton.Colliders = new Collider2D[] { buttonObj.GetComponent<BoxCollider2D>() };
        var spriteRenderer = buttonObj.transform.Find("Background").GetComponent<SpriteRenderer>();

        UIHelper.ConfigurePassiveButton(passiveButton, (UnityAction)(() =>
        {
            CustomOptionSaver.Save();
            CustomOptionSaver.LoadPreset(presetId);
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
    }

    private static void ConfigurePresetNavigationButtons(GameObject selectPresets, TMPro.TextMeshPro selectedText)
    {
        ConfigurePresetButton(selectPresets, "Button_Minus", selectedText, isIncrement: false);
        ConfigurePresetButton(selectPresets, "Button_Plus", selectedText, isIncrement: true);
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
        UpdatePresetText(selectedText, newPreset);
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
        RecalculateOptionsPosition(defaultMenu.transform, menuData.RightAreaScroller);

        menuData.StandardOptionMenus.Add(category.Name, defaultMenu);
        menuData.CurrentOptionMenu = defaultMenu;
    }

    private static void RecalculateOptionsPosition(Transform menuTransform, Scroller scroller)
    {
        float lastY = 1.6f;
        float minY = lastY;
        for (int i = 0; i < menuTransform.childCount; i++)
        {
            Transform child = menuTransform.GetChild(i);
            child.localPosition = new Vector3(3.42f, lastY, -0.21f);
            lastY -= 0.7f;
            minY = lastY;
        }

        if (scroller != null)
        {
            scroller.ContentYBounds.max = Mathf.Max(0f, -minY + 4.5f);
        }
    }

    private static void GenerateOptionsForCategory(CustomOptionCategory category, Transform menuTransform)
    {
        foreach (var option in category.Options)
        {
            // トップレベルのオプションはisChildフラグをfalseにして生成
            GenerateStandardOption(option, menuTransform, false);
            // 子オプションがあれば再帰的に生成
            GenerateChildOptions(option, menuTransform);
        }
    }

    private static void GenerateChildOptions(CustomOption parentOption, Transform menuTransform)
    {
        if (parentOption.ChildrenOption == null) return;

        foreach (var childOption in parentOption.ChildrenOption)
        {
            GenerateStandardOption(childOption, menuTransform, true);
            GenerateChildOptions(childOption, menuTransform);
        }
    }

    private static void GenerateStandardOption(CustomOption option, Transform parent, bool isChild)
    {
        if (option.IsBooleanOption)
        {
            GenerateStandardOptionCheck(option, parent, isChild);
        }
        else
        {
            GenerateStandardOptionSelect(option, parent, isChild);
        }
    }

    private static void GenerateStandardOptionCheck(CustomOption option, Transform parent, bool isChild)
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
        }), spriteRenderer);
    }

    private static void GenerateStandardOptionSelect(CustomOption option, Transform parent, bool isChild)
    {
        var selectObject = CreateOptionSelectObject(option, parent, isChild);
        var selectedText = selectObject.transform.Find("SelectedText").GetComponent<TMPro.TextMeshPro>();
        selectedText.text = UIHelper.FormatOptionValue(option.Selections[option.Selection], option);

        // UIデータを保存
        StandardOptionMenuObjectData.Instance.AddOptionUIData(
            StandardOptionMenuObjectData.Instance.CurrentCategory.Name,
            option,
            selectObject,
            false
        );

        ConfigureSelectOptionButtons(selectObject, selectedText, option);
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

        UIHelper.ConfigurePassiveButton(passiveButton, (UnityAction)(() =>
        {
            HandleOptionSelection(option, selectedText, isIncrement);
        }), spriteRenderer);
    }

    private static void HandleOptionSelection(CustomOption option, TMPro.TextMeshPro selectedText, bool isIncrement)
    {
        byte newSelection;
        if (isIncrement)
        {
            newSelection = option.Selection < option.Selections.Length - 1 ?
                (byte)(option.Selection + 1) :
                (byte)0;
        }
        else
        {
            newSelection = option.Selection > 0 ?
                (byte)(option.Selection - 1) :
                (byte)(option.Selections.Length - 1);
        }

        UpdateOptionSelection(option, newSelection, selectedText);
    }

    private static void UpdateOptionSelection(CustomOption option, byte newSelection, TMPro.TextMeshPro selectedText)
    {
        option.UpdateSelection(newSelection);
        StandardOptionMenuObjectData.Instance.UpdateOptionDisplay();
    }

    private static string FormatOptionValue(object value, CustomOption option)
    {
        if (value is float floatValue)
        {
            var attribute = option.Attribute as CustomOptionFloatAttribute;
            if (attribute != null)
            {
                float step = attribute.Step;
                if (step >= 1f) return string.Format("{0:F0}", floatValue);
                else if (step >= 0.1f) return string.Format("{0:F1}", floatValue);
                else return string.Format("{0:F2}", floatValue);
            }
            return floatValue.ToString();
        }
        return value.ToString();
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
                    selectData.SelectedText.text = FormatOptionValue(option.Selections[option.Selection], option);
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