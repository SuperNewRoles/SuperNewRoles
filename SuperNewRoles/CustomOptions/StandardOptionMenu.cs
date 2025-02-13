using SuperNewRoles.Modules;
using UnityEngine;
using UnityEngine.Events;

namespace SuperNewRoles.CustomOptions;
public static class StandardOptionMenu
{
    public static void ShowStandardOptionMenu()
    {
        if (StandardOptionMenuObjectData.Instance?.StandardOptionMenu == null)
            Initialize();
        StandardOptionMenuObjectData.Instance.StandardOptionMenu.SetActive(true);
    }
    public static void Initialize()
    {
        var obj = GameObject.Instantiate(AssetManager.GetAsset<GameObject>("StandardOptionMenu"), RoleOptionMenu.GetGameSettingMenu().transform);
        obj.transform.localScale = Vector3.one;
        obj.transform.localPosition = new(0, 0, -2f);

        new StandardOptionMenuObjectData(obj);
        int index = 0;
        foreach (var category in CustomOptionManager.OptionCategories)
        {
            GenerateRoleDetailButton(ModTranslation.GetString(category.Name), index++);
        }
    }
    /// <summary>
    /// ロール詳細ボタンを生成する
    /// </summary>
    /// <param name="roleName">ロールの名前</param>
    /// <param name="index">ボタンのインデックス（位置計算用）</param>
    /// <param name="roleOption">ロールのオプション情報</param>
    /// <returns>生成されたボタンのGameObject</returns>
    public static GameObject GenerateRoleDetailButton(string text, int index)
    {
        if (StandardOptionMenuObjectData.Instance == null)
            return null;

        var obj = GameObject.Instantiate(AssetManager.GetAsset<GameObject>(RoleOptionMenu.ROLE_DETAIL_BUTTON_ASSET_NAME));
        obj.transform.SetParent(StandardOptionMenuObjectData.Instance.LeftAreaInner.transform);
        obj.transform.localScale = Vector3.one * 0.48f;

        obj.transform.localPosition = new Vector3(-3.614f, 1.4f - (index * 0.6125f), -0.21f);
        obj.transform.Find("Text").GetComponent<TMPro.TextMeshPro>().text = text;

        var passiveButton = obj.AddComponent<PassiveButton>();
        passiveButton.Colliders = new Collider2D[1] { obj.GetComponent<BoxCollider2D>() };
        passiveButton.OnClick = new();

        GameObject selectedObject = null;
        var spriteRenderer = obj.GetComponent<SpriteRenderer>();

        passiveButton.OnClick.AddListener((UnityAction)(() =>
        {
            // TODO: ロールの設定画面を表示する処理を追加
            Logger.Info("ロールの設定画面を表示する処理を追加");
            if (CustomOptionManager.CategoryByFieldName.TryGetValue(text, out var category) && category == CustomOptionManager.PresetSettings)
            {
                ShowPresetOptionMenu();
            }
        }));

        passiveButton.OnMouseOut = new();
        passiveButton.OnMouseOut.AddListener((UnityAction)(() =>
        {
            if (selectedObject == null)
                selectedObject = obj.transform.Find("Selected").gameObject;
            selectedObject.SetActive(false);
        }));

        passiveButton.OnMouseOver = new();
        passiveButton.OnMouseOver.AddListener((UnityAction)(() =>
        {
            if (selectedObject == null)
                selectedObject = obj.transform.Find("Selected").gameObject;
            selectedObject.SetActive(true);
        }));

        return obj;
    }
    public static void ShowPresetOptionMenu()
    {

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