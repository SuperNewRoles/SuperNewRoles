using System.Collections.Generic;
using System.Linq;
using SuperNewRoles.Modules;
using UnityEngine;
using UnityEngine.Events;

namespace SuperNewRoles.CustomOptions;
public static class StandardOptionMenu
{
    // UI要素生成のヘルパーメソッド
    private static GameObject InstantiateUIElement(string assetName, Transform parent, Vector3 localPosition, Vector3 localScale)
    {
        GameObject obj = GameObject.Instantiate(AssetManager.GetAsset<GameObject>(assetName), parent);
        obj.transform.localScale = localScale;
        obj.transform.localPosition = localPosition;
        return obj;
    }

    public static void ShowStandardOptionMenu()
    {
        if (StandardOptionMenuObjectData.Instance?.StandardOptionMenu == null)
            Initialize();
        StandardOptionMenuObjectData.Instance.StandardOptionMenu.SetActive(true);
    }

    public static void Initialize()
    {
        // ヘルパーを利用してStandardOptionMenuを生成
        var menu = InstantiateUIElement("StandardOptionMenu", RoleOptionMenu.GetGameSettingMenu().transform, new Vector3(0, 0, -2f), Vector3.one);
        new StandardOptionMenuObjectData(menu);
        int index = 0;
        foreach (var category in CustomOptionManager.OptionCategories)
        {
            GenerateRoleDetailButton(category, index++);
        }
    }

    /// <summary>
    /// ロール詳細ボタンを生成する
    /// </summary>
    /// <param name="category">ロールのカテゴリー情報</param>
    /// <param name="index">ボタンのインデックス（位置計算用）</param>
    /// <returns>生成されたボタンのGameObject</returns>
    public static GameObject GenerateRoleDetailButton(CustomOptionCategory category, int index)
    {
        if (StandardOptionMenuObjectData.Instance == null)
            return null;

        // ヘルパーを利用してロール詳細ボタンを生成
        var obj = InstantiateUIElement(RoleOptionMenu.ROLE_DETAIL_BUTTON_ASSET_NAME, StandardOptionMenuObjectData.Instance.LeftAreaInner.transform,
            new Vector3(-3.614f, 1.4f - (index * 0.6125f), -0.21f), Vector3.one * 0.48f);

        // ボタンのテキストを設定
        obj.transform.Find("Text").GetComponent<TMPro.TextMeshPro>().text = ModTranslation.GetString(category.Name);

        var passiveButton = obj.AddComponent<PassiveButton>();
        passiveButton.Colliders = new Collider2D[1] { obj.GetComponent<BoxCollider2D>() };
        passiveButton.OnClick = new();

        // "Selected"オブジェクトを事前に取得
        GameObject selectedObject = obj.transform.Find("Selected")?.gameObject;

        passiveButton.OnClick.AddListener((UnityAction)(() =>
        {
            Logger.Info($"{category.Name}");
            if (StandardOptionMenuObjectData.Instance.CurrentOptionMenu != null)
                StandardOptionMenuObjectData.Instance.CurrentOptionMenu.SetActive(false);
            StandardOptionMenuObjectData.Instance.CurrentOptionMenu = null;
            if (StandardOptionMenuObjectData.Instance.CurrentSelectedButton != null)
                StandardOptionMenuObjectData.Instance.CurrentSelectedButton.SetActive(false);
            StandardOptionMenuObjectData.Instance.CurrentSelectedButton = selectedObject;
            StandardOptionMenuObjectData.Instance.CurrentCategory = category;
            selectedObject?.SetActive(true);
            if (category == CustomOptionManager.PresetSettings)
            {
                ShowPresetOptionMenu();
            }
            else
            {
                ShowDefaultOptionMenu(category, StandardOptionMenuObjectData.Instance.RightAreaInner.transform);
            }
        }));

        passiveButton.OnMouseOut = new UnityEvent();
        passiveButton.OnMouseOut.AddListener((UnityAction)(() =>
        {
            if (StandardOptionMenuObjectData.Instance.CurrentCategory != category)
                selectedObject?.SetActive(false);
        }));

        passiveButton.OnMouseOver = new UnityEvent();
        passiveButton.OnMouseOver.AddListener((UnityAction)(() =>
        {
            if (StandardOptionMenuObjectData.Instance.CurrentCategory != category)
                selectedObject?.SetActive(true);
        }));

        return obj;
    }

    public static void ShowPresetOptionMenu()
    {
        if (StandardOptionMenuObjectData.Instance.StandardOptionMenus.TryGetValue(CustomOptionManager.PresetSettings.Name, out var menu))
        {
            StandardOptionMenuObjectData.Instance.CurrentOptionMenu = menu;
            menu.SetActive(true);
            return;
        }
        var presetMenu = InstantiateUIElement("PresetMenu", StandardOptionMenuObjectData.Instance.RightAreaInner.transform, new Vector3(0, 0, 0f), Vector3.one);

        // プリセット選択の設定
        var selectPresets = presetMenu.transform.Find("SelectPresets").gameObject;
        var selectedText = selectPresets.transform.Find("SelectedText").GetComponent<TMPro.TextMeshPro>();
        UpdatePresetText(selectedText, CustomOptionSaver.CurrentPreset);

        // プリセット名入力の設定
        var writeBox = presetMenu.transform.Find("WriteBox").gameObject;
        var writeBoxTextBoxTMP = writeBox.GetComponent<TextBoxTMP>();
        var writeBoxPassiveButton = writeBox.AddComponent<PassiveButton>();
        var writeBoxSpriteRenderer = writeBox.transform.Find("Background").GetComponent<SpriteRenderer>();
        writeBoxPassiveButton.Colliders = new Collider2D[] { writeBox.GetComponent<BoxCollider2D>() };
        writeBoxPassiveButton.OnClick = new();
        writeBoxPassiveButton.OnClick.AddListener((UnityAction)(() =>
        {
            writeBoxTextBoxTMP.GiveFocus();
        }));

        writeBoxPassiveButton.OnMouseOver = new();
        writeBoxPassiveButton.OnMouseOver.AddListener((UnityAction)(() =>
        {
            if (!writeBoxTextBoxTMP.hasFocus)
                writeBoxSpriteRenderer.color = Color.green;
        }));
        writeBoxPassiveButton.OnMouseOut = new();
        writeBoxPassiveButton.OnMouseOut.AddListener((UnityAction)(() =>
        {
            if (!writeBoxTextBoxTMP.hasFocus)
                writeBoxSpriteRenderer.color = Color.white;
        }));
        writeBoxTextBoxTMP.OnEnter = new();
        writeBoxTextBoxTMP.OnEnter.AddListener((UnityAction)delegate
        {
            string text = writeBoxTextBoxTMP.text;
            // 現在の最大プリセット番号を取得
            int maxPreset = CustomOptionSaver.PresetNames.Any() ? CustomOptionSaver.PresetNames.Keys.Max() : -1;
            // 新しいプリセット番号（最大値+1、ただし9を超えない）
            int newPreset = Mathf.Min(maxPreset + 1, 9);
            if (newPreset <= 9)
            {
                CustomOptionSaver.Save(); // 現在のプリセットを保存
                CustomOptionSaver.SetPresetName(newPreset, text);
                CustomOptionSaver.LoadPreset(newPreset);
                UpdatePresetText(selectedText, newPreset);
            }
            writeBoxTextBoxTMP.LoseFocus();
        });

        // マイナスボタンの設定
        var minusButton = selectPresets.transform.Find("Button_Minus").gameObject;
        var minusPassiveButton = minusButton.AddComponent<PassiveButton>();
        minusPassiveButton.Colliders = new Collider2D[] { minusButton.GetComponent<BoxCollider2D>() };
        var minusSpriteRenderer = minusPassiveButton.GetComponent<SpriteRenderer>();
        ConfigurePassiveButton(minusPassiveButton, () =>
        {
            CustomOptionSaver.Save(); // 現在のプリセットを保存
            int newPreset = CustomOptionSaver.CurrentPreset > 0 ? CustomOptionSaver.CurrentPreset - 1 : CustomOptionSaver.PresetNames.Keys.Max();
            CustomOptionSaver.LoadPreset(newPreset);
            UpdatePresetText(selectedText, newPreset);
            writeBoxTextBoxTMP.text = CustomOptionSaver.GetPresetName(newPreset);
        }, minusSpriteRenderer);

        // プラスボタンの設定
        var plusButton = selectPresets.transform.Find("Button_Plus").gameObject;
        var plusPassiveButton = plusButton.AddComponent<PassiveButton>();
        plusPassiveButton.Colliders = new Collider2D[] { plusButton.GetComponent<BoxCollider2D>() };
        var plusSpriteRenderer = plusPassiveButton.GetComponent<SpriteRenderer>();
        ConfigurePassiveButton(plusPassiveButton, () =>
        {
            CustomOptionSaver.Save(); // 現在のプリセットを保存
            int newPreset = CustomOptionSaver.CurrentPreset < CustomOptionSaver.PresetNames.Keys.Max() ? CustomOptionSaver.CurrentPreset + 1 : 0;
            CustomOptionSaver.LoadPreset(newPreset);
            UpdatePresetText(selectedText, newPreset);
            writeBoxTextBoxTMP.text = CustomOptionSaver.GetPresetName(newPreset);
        }, plusSpriteRenderer);

        StandardOptionMenuObjectData.Instance.StandardOptionMenus.Add(CustomOptionManager.PresetSettings.Name, presetMenu);
        StandardOptionMenuObjectData.Instance.CurrentOptionMenu = presetMenu;
    }
    public static void ShowDefaultOptionMenu(CustomOptionCategory category, Transform parent)
    {
        // StandardOptionMenuObjectData.Instanceへの参照をローカル変数に保持して読みやすくする
        var menuData = StandardOptionMenuObjectData.Instance;
        if (menuData.StandardOptionMenus.TryGetValue(category.Name, out var existingMenu))
        {
            menuData.CurrentOptionMenu = existingMenu;
            existingMenu.SetActive(true);

            // 既存メニューの設定値を更新
            foreach (var option in category.Options)
            {
                UpdateOptionUIValues(option, existingMenu.transform);
            }
            return;
        }

        var defaultMenu = new GameObject($"{category.Name}OptionMenu");
        var menuTransform = defaultMenu.transform;
        menuTransform.SetParent(parent);
        menuTransform.localScale = Vector3.one;
        menuTransform.localPosition = Vector3.zero; // (0,0,0f)の代わりにVector3.zeroを使用

        menuData.StandardOptionMenus.Add(category.Name, defaultMenu);
        menuData.CurrentOptionMenu = defaultMenu;

        float lastY = 1.6f;

        // 再帰的に子オプションを生成するローカル関数
        void GenerateChildOptions(CustomOption parentOption)
        {
            if (parentOption.ChildrenOption == null) return;
            foreach (var childOption in parentOption.ChildrenOption)
            {
                GenerateStandardOption(childOption, menuTransform, true, ref lastY);
                GenerateChildOptions(childOption);
            }
        }

        foreach (var option in category.Options)
        {
            // トップレベルのオプションはisChildフラグをfalseにして生成
            GenerateStandardOption(option, menuTransform, false, ref lastY);
            // 子オプションがあれば再帰的に生成
            GenerateChildOptions(option);
        }
    }

    private static void GenerateStandardOption(CustomOption option, Transform parent, bool isChild, ref float lastY)
    {
        if (option.IsBooleanOption)
        {
            GenerateStandardOptionCheck(option, parent, isChild, ref lastY);
        }
        else
        {
            GenerateStandardOptionSelect(option, parent, isChild, ref lastY);
        }
    }
    public static void GenerateStandardOptionCheck(CustomOption option, Transform parent, bool isChild, ref float lastY)
    {
        // ヘルパーを利用してStandardOption_Checkを生成
        var check = InstantiateUIElement(isChild ? "StandardChildOption_Check" : "StandardOption_Check", parent,
            new Vector3(3.42f, lastY, -0.21f), Vector3.one * 0.4f);
        lastY -= 0.7f;

        // オプション名を設定
        check.transform.Find("Text").GetComponent<TMPro.TextMeshPro>().text = ModTranslation.GetString(option.Name);

        var passiveButton = check.AddComponent<PassiveButton>();
        passiveButton.Colliders = new Collider2D[] { check.GetComponent<BoxCollider2D>() };
        var spriteRenderer = check.GetComponent<SpriteRenderer>();
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

        ConfigurePassiveButton(passiveButton, () =>
        {
            bool newValue = !checkMark.activeSelf;
            checkMark.SetActive(newValue);
            option.UpdateSelection(newValue ? (byte)1 : (byte)0);
        }, spriteRenderer);
    }

    public static void GenerateStandardOptionSelect(CustomOption option, Transform parent, bool isChild, ref float lastY)
    {
        // ヘルパーを利用してStandardOption_Selectを生成
        var selecte = InstantiateUIElement(isChild ? "StandardChildOption_Select" : "StandardOption_Select", parent,
            new Vector3(3.42f, lastY, -0.21f), Vector3.one * 0.4f);
        lastY -= 0.7f;

        // オプション名を設定
        selecte.transform.Find("Text").GetComponent<TMPro.TextMeshPro>().text = ModTranslation.GetString(option.Name);

        var selectedText = selecte.transform.Find("SelectedText").GetComponent<TMPro.TextMeshPro>();
        selectedText.text = FormatOptionValue(option.Selections[option.Selection], option);

        // UIデータを保存
        StandardOptionMenuObjectData.Instance.AddOptionUIData(
            StandardOptionMenuObjectData.Instance.CurrentCategory.Name,
            option,
            selecte,
            false
        );

        // マイナスボタンの設定
        var minusButton = selecte.transform.Find("Button_Minus").gameObject;
        var minusPassiveButton = minusButton.AddComponent<PassiveButton>();
        minusPassiveButton.Colliders = new Collider2D[] { minusButton.GetComponent<BoxCollider2D>() };
        var minusSpriteRenderer = minusPassiveButton.GetComponent<SpriteRenderer>();
        ConfigurePassiveButton(minusPassiveButton, () =>
        {
            byte newSelection = option.Selection > 0 ?
                (byte)(option.Selection - 1) :
                (byte)(option.Selections.Length - 1);
            UpdateOptionSelection(option, newSelection, selectedText);
        }, minusSpriteRenderer);

        // プラスボタンの設定
        var plusButton = selecte.transform.Find("Button_Plus").gameObject;
        var plusPassiveButton = plusButton.AddComponent<PassiveButton>();
        plusPassiveButton.Colliders = new Collider2D[] { plusButton.GetComponent<BoxCollider2D>() };
        var plusSpriteRenderer = plusPassiveButton.GetComponent<SpriteRenderer>();
        ConfigurePassiveButton(plusPassiveButton, () =>
        {
            byte newSelection = option.Selection < option.Selections.Length - 1 ?
                (byte)(option.Selection + 1) :
                (byte)0;
            UpdateOptionSelection(option, newSelection, selectedText);
        }, plusSpriteRenderer);
    }

    private static void UpdateOptionSelection(CustomOption option, byte newSelection, TMPro.TextMeshPro selectedText)
    {
        option.UpdateSelection(newSelection);
        selectedText.text = FormatOptionValue(option.Selections[option.Selection], option);
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

    private static void ConfigurePassiveButton(PassiveButton button, System.Action onClick, SpriteRenderer spriteRenderer)
    {
        button.OnClick = new();
        button.OnClick.AddListener((UnityAction)onClick);
        button.OnMouseOver = new UnityEvent();
        button.OnMouseOver.AddListener((UnityAction)(() =>
        {
            if (spriteRenderer != null)
                spriteRenderer.color = new Color32(45, 235, 198, 255);
        }));
        button.OnMouseOut = new UnityEvent();
        button.OnMouseOut.AddListener((UnityAction)(() =>
        {
            if (spriteRenderer != null)
                spriteRenderer.color = Color.white;
        }));
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
}

public class StandardOptionMenuObjectData : OptionMenuBase
{
    public abstract class OptionUIDataBase
    {
        public CustomOption Option { get; set; }
        public GameObject UIObject { get; set; }
    }

    public class CheckOptionUIData : OptionUIDataBase
    {
        public GameObject CheckMark { get; set; }
    }

    public class SelectOptionUIData : OptionUIDataBase
    {
        public TMPro.TextMeshPro SelectedText { get; set; }
    }

    public static StandardOptionMenuObjectData Instance { get; private set; }
    public GameObject StandardOptionMenu { get; }
    public GameObject LeftAreaInner { get; }
    public GameObject RightAreaInner { get; }
    public GameObject CurrentOptionMenu { get; set; }
    public CustomOptionCategory CurrentCategory { get; set; }
    public GameObject CurrentSelectedButton { get; set; }
    public Dictionary<string, GameObject> StandardOptionMenus { get; } = new();
    public Dictionary<string, List<OptionUIDataBase>> CategoryOptionUIData { get; } = new();

    public StandardOptionMenuObjectData(GameObject standardOptionMenu) : base()
    {
        Instance = this;
        StandardOptionMenu = standardOptionMenu;
        LeftAreaInner = StandardOptionMenu.transform.Find("LeftArea/Scroller/Inner").gameObject;
        RightAreaInner = StandardOptionMenu.transform.Find("RightArea/Scroller/Inner").gameObject;
    }

    public void AddOptionUIData(string categoryName, CustomOption option, GameObject uiObject, bool isBooleanOption)
    {
        if (!CategoryOptionUIData.ContainsKey(categoryName))
        {
            CategoryOptionUIData[categoryName] = new List<OptionUIDataBase>();
        }

        if (isBooleanOption)
        {
            CategoryOptionUIData[categoryName].Add(new CheckOptionUIData
            {
                Option = option,
                UIObject = uiObject,
                CheckMark = uiObject.transform.Find("CheckMark").gameObject
            });
        }
        else
        {
            CategoryOptionUIData[categoryName].Add(new SelectOptionUIData
            {
                Option = option,
                UIObject = uiObject,
                SelectedText = uiObject.transform.Find("SelectedText").GetComponent<TMPro.TextMeshPro>()
            });
        }
    }

    public override void Hide()
    {
        if (StandardOptionMenu != null)
            StandardOptionMenu.SetActive(false);
    }
}