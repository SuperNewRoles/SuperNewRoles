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
            if (category == CustomOptionManager.PresetSettings)
            {
                ShowPresetOptionMenu();
            }
        }));

        passiveButton.OnMouseOut = new UnityEvent();
        passiveButton.OnMouseOut.AddListener((UnityAction)(() =>
        {
            selectedObject?.SetActive(false);
        }));

        passiveButton.OnMouseOver = new UnityEvent();
        passiveButton.OnMouseOver.AddListener((UnityAction)(() =>
        {
            selectedObject?.SetActive(true);
        }));

        return obj;
    }

    public static void ShowPresetOptionMenu()
    {
        float lastY = 1.6f;
        GenerateStandardOptionCheck(CustomOptionManager.CustomOptions.FirstOrDefault(x => x.IsBooleanOption), ref lastY);
        GenerateStandardOptionSelect(CustomOptionManager.CustomOptions.FirstOrDefault(x => !x.IsBooleanOption), ref lastY);
    }

    public static void GenerateStandardOptionCheck(CustomOption option, ref float lastY)
    {
        // ヘルパーを利用してStandardOption_Checkを生成
        var check = InstantiateUIElement("StandardOption_Check", StandardOptionMenuObjectData.Instance.RightAreaInner.transform,
            new Vector3(3.9f, lastY, -0.21f), Vector3.one * 0.4f);
        lastY -= 0.7f;

        // オプション名を設定
        check.transform.Find("Text").GetComponent<TMPro.TextMeshPro>().text = ModTranslation.GetString(option.Name);

        var passiveButton = check.AddComponent<PassiveButton>();
        passiveButton.Colliders = new Collider2D[] { check.GetComponent<BoxCollider2D>() };
        var spriteRenderer = check.GetComponent<SpriteRenderer>();
        var checkMark = check.transform.Find("CheckMark");

        // 初期状態を設定
        if (checkMark != null)
        {
            checkMark.gameObject.SetActive((bool)option.Value);
        }

        ConfigurePassiveButton(passiveButton, () =>
        {
            if (checkMark != null)
            {
                bool newValue = !checkMark.gameObject.activeSelf;
                checkMark.gameObject.SetActive(newValue);
                option.UpdateSelection(newValue ? (byte)1 : (byte)0);
            }
        }, spriteRenderer);
    }

    public static void GenerateStandardOptionSelect(CustomOption option, ref float lastY)
    {
        // ヘルパーを利用してStandardOption_Selectを生成
        var selected = InstantiateUIElement("StandardOption_Select", StandardOptionMenuObjectData.Instance.RightAreaInner.transform,
            new Vector3(3.9f, lastY, -0.21f), Vector3.one * 0.4f);
        lastY -= 0.7f;

        // オプション名を設定
        selected.transform.Find("Text").GetComponent<TMPro.TextMeshPro>().text = ModTranslation.GetString(option.Name);

        var selectedText = selected.transform.Find("SelectedText").GetComponent<TMPro.TextMeshPro>();
        selectedText.text = FormatOptionValue(option.Selections[option.Selection], option);

        // マイナスボタンの設定
        var minusButton = selected.transform.Find("Button_Minus").gameObject;
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
        var plusButton = selected.transform.Find("Button_Plus").gameObject;
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
}

public class StandardOptionMenuObjectData : OptionMenuBase
{
    public static StandardOptionMenuObjectData Instance { get; private set; }
    public GameObject StandardOptionMenu { get; }
    public GameObject LeftAreaInner { get; }
    public GameObject RightAreaInner { get; }
    public StandardOptionMenuObjectData(GameObject standardOptionMenu) : base()
    {
        Instance = this;
        StandardOptionMenu = standardOptionMenu;
        LeftAreaInner = StandardOptionMenu.transform.Find("LeftArea/Scroller/Inner").gameObject;
        RightAreaInner = StandardOptionMenu.transform.Find("RightArea/Scroller/Inner").gameObject;
    }

    public override void Hide()
    {
        if (StandardOptionMenu != null)
            StandardOptionMenu.SetActive(false);
    }
}