using SuperNewRoles.Modules;
using SuperNewRoles.Roles;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;
using System.Collections.Generic;

namespace SuperNewRoles.CustomOptions;
public static class BulkRoleSettings
{
    private struct BulkRoleColor
    {
        public Color32 DarkHeaderColor;
        public Color32 LightHeaderColor;
        public Color32 LeftColor;
        public BulkRoleColor(Color32 darkHeaderColor, Color32 lightHeaderColor, Color32 leftColor)
        {
            DarkHeaderColor = darkHeaderColor;
            LightHeaderColor = lightHeaderColor;
            LeftColor = leftColor;
        }
    }
    private static Dictionary<RoleOptionMenuType, BulkRoleColor> BulkRoleColors = new()
    {
        { RoleOptionMenuType.Crewmate, new(
            new Color32(22, 121, 121, 255),
            new Color32(42, 159, 159, 255),
            new Color32(98, 175, 176, 255)
        ) },
        { RoleOptionMenuType.Impostor, new(
            new Color32(78, 26, 26, 255),
            new Color32(140, 35, 35, 255),
            new Color32(217, 56, 56, 255)
        ) },
        { RoleOptionMenuType.Neutral, new(
            new Color32(32, 32, 32, 255),
            new Color32(68, 68, 68, 255),
            new Color32(104, 104, 104, 255)
        ) },
    };
    private static BulkRoleColor GetCurrentBulkRoleColor()
    {
        var instance = RoleOptionMenu.RoleOptionMenuObjectData;
        if (instance == null || !BulkRoleColors.TryGetValue(instance.CurrentRoleType, out var color))
        {
            return new(
                new Color32(255, 255, 255, 255),
                new Color32(255, 255, 255, 255),
                new Color32(255, 255, 255, 255)
            );
        }
        return color;
    }
    public static GameObject InitializeBulkRoleSettingsMenu()
    {
        // ロールの設定を初期化
        var obj = GameObject.Instantiate(AssetManager.GetAsset<GameObject>("BulkRoleSettingsMenu"));
        obj.transform.SetParent(RoleOptionMenu.RoleOptionMenuObjectData.MenuObject.transform.parent);
        obj.transform.localPosition = new Vector3(0, 0, -1f);
        obj.transform.localScale = Vector3.one;

        var ReturnButton = obj.transform.Find("ReturnButton").gameObject;
        var passiveButton = ReturnButton.AddComponent<PassiveButton>();
        passiveButton.OnClick = new();
        passiveButton.OnClick.AddListener((UnityAction)(() =>
        {
            // 非表示も兼ねている
            RoleOptionMenu.ShowRoleOptionMenu(RoleOptionMenu.RoleOptionMenuObjectData.CurrentRoleType);
        }));
        passiveButton.OnMouseOver = new();
        passiveButton.OnMouseOver.AddListener((UnityAction)(() =>
        {
            ReturnButton.transform.Find("Selected").gameObject.SetActive(true);
        }));
        passiveButton.OnMouseOut = new();
        passiveButton.OnMouseOut.AddListener((UnityAction)(() =>
        {
            ReturnButton.transform.Find("Selected").gameObject.SetActive(false);
        }));

        obj.transform.Find("BulkRoleTitleText").GetComponent<TextMeshPro>().text = ModTranslation.GetString("BulkRoleButton");

        SetupBulkHeaderBar("Left", obj);
        SetupBulkHeaderBar("Right", obj);

        SetupScroller(obj);
        return obj;
    }
    public static void SetupScroller(GameObject obj)
    {
        var scrollerBase = obj.transform.Find("Scroller");
        // RoleOptionMenuObjectDataに一括設定メニュー用のスクローラーと内部コンテンツを保存
        RoleOptionMenu.RoleOptionMenuObjectData.BulkSettingsScroller = scrollerBase.GetComponent<Scroller>();
        RoleOptionMenu.RoleOptionMenuObjectData.BulkSettingsInner = scrollerBase.transform.Find("InnerContent");
    }
    public static void HideBulkRoleSettings()
    {
        if (RoleOptionMenu.RoleOptionMenuObjectData?.BulkRoleSettingsMenu != null)
            RoleOptionMenu.RoleOptionMenuObjectData.BulkRoleSettingsMenu.SetActive(false);
    }
    private static void GenerateBulkRoleSetting(Transform innerTransform, int index, RoleOptionManager.RoleOption roleOption)
    {
        var bulkRoleSetting = CreateBulkRoleSettingObject(innerTransform, index);
        SetupColor(bulkRoleSetting, roleOption);
        SetUpText(bulkRoleSetting, roleOption);
        SetupBeansSelector(bulkRoleSetting, roleOption);
        SetupPercentageSelector(bulkRoleSetting, roleOption);
    }
    private static void SetupColor(GameObject bulkRoleSetting, RoleOptionManager.RoleOption roleOption)
    {
        var color = GetCurrentBulkRoleColor();
        bulkRoleSetting.transform.Find("RoleName_Bar").GetComponent<SpriteRenderer>().color = color.LeftColor;
    }
    private static void SetUpText(GameObject bulkRoleSetting, RoleOptionManager.RoleOption roleOption)
    {
        bulkRoleSetting.transform.Find("RoleName_Text").GetComponent<TextMeshPro>().text = ModTranslation.GetString(roleOption.RoleId.ToString());
    }
    private static GameObject CreateBulkRoleSettingObject(Transform innerTransform, int index)
    {
        var bulkRoleSetting = GameObject.Instantiate(AssetManager.GetAsset<GameObject>("BulkRoleSetting"));
        bulkRoleSetting.transform.SetParent(innerTransform);
        bulkRoleSetting.transform.localScale = Vector3.one;

        float xPos = index % 2 == 0 ? 0.83f : 5.485f;
        float yPos = -0.05f - (0.32f * (index / 2));
        bulkRoleSetting.transform.localPosition = new(xPos, yPos, 0);

        return bulkRoleSetting;
    }
    private static void SetupBeansSelector(GameObject bulkRoleSetting, RoleOptionManager.RoleOption roleOption)
    {
        var beansSelect = bulkRoleSetting.transform.Find("Select_Beans").gameObject;
        var selectedText = beansSelect.transform.Find("SelectedText").GetComponent<TextMeshPro>();
        selectedText.text = roleOption.NumberOfCrews.ToString();

        SetupSelectorButton(
            beansSelect,
            "Button_Minus",
            () => UpdateBeansValue(selectedText, -1, roleOption),
            isBeansSelector: true
        );

        SetupSelectorButton(
            beansSelect,
            "Button_Plus",
            () => UpdateBeansValue(selectedText, 1, roleOption),
            isBeansSelector: true
        );
    }
    private static void SetupPercentageSelector(GameObject bulkRoleSetting, RoleOptionManager.RoleOption roleOption)
    {
        var perSelect = bulkRoleSetting.transform.Find("Select_Per").gameObject;
        var selectedText = perSelect.transform.Find("SelectedText").GetComponent<TextMeshPro>();
        selectedText.text = $"{roleOption.Percentage}%";

        SetupSelectorButton(
            perSelect,
            "Button_Minus",
            () => UpdatePercentageValue(selectedText, -10, roleOption)
        );

        SetupSelectorButton(
            perSelect,
            "Button_Plus",
            () => UpdatePercentageValue(selectedText, 10, roleOption)
        );
    }
    private static void SetupSelectorButton(GameObject parent, string buttonName, System.Action onClick, bool isBeansSelector = false)
    {
        var button = parent.transform.Find(buttonName).gameObject;
        var passiveButton = button.AddComponent<PassiveButton>();
        var spriteRenderer = passiveButton.GetComponent<SpriteRenderer>();
        ConfigurePassiveButton(passiveButton, onClick, spriteRenderer);
    }
    private static void UpdateBeansValue(TextMeshPro text, int delta, RoleOptionManager.RoleOption roleOption)
    {
        int currentValue = int.Parse(text.text);
        int maxPlayers = PlayerControl.AllPlayerControls != null ?
            Mathf.Max(15, PlayerControl.AllPlayerControls.Count) : 15;

        int newValue = currentValue + delta;
        if (newValue > maxPlayers)
            newValue = 0;
        else if (newValue < 0)
            newValue = maxPlayers;

        text.text = newValue.ToString();
        roleOption.NumberOfCrews = (byte)newValue;

        // 人数が0から1以上に変更され、かつ確率が0%の場合、100%に設定
        if (currentValue == 0 && newValue > 0 && roleOption.Percentage == 0)
        {
            roleOption.Percentage = 100;
            var perSelect = text.transform.parent.parent.Find("Select_Per/SelectedText")?.GetComponent<TextMeshPro>();
            if (perSelect != null)
            {
                perSelect.text = "100%";
            }
        }
        RoleOptionMenu.UpdateNumOfCrewsSelect(roleOption);
        if (RoleOptionMenu.RoleOptionMenuObjectData.RoleDetailButtonDictionary.TryGetValue(roleOption.RoleId, out var roleDetailButton))
        {
            RoleOptionMenu.UpdateRoleDetailButtonColor(roleDetailButton.GetComponent<SpriteRenderer>(), roleOption);
        }
        if (newValue == 0)
        {
            RoleOptionMenu.RoleOptionMenuObjectData.CurrentBulkSettingsIndex--;
            UpdateSettingsScroller(RoleOptionMenu.RoleOptionMenuObjectData.CurrentBulkSettingsIndex);
        }
    }
    private static void UpdatePercentageValue(TextMeshPro text, int delta, RoleOptionManager.RoleOption roleOption)
    {
        string currentText = text.text.Replace("%", "");
        int currentValue = int.Parse(currentText);

        int newValue = currentValue + delta;
        if (newValue > 100)
            newValue = 0;
        else if (newValue < 0)
            newValue = 100;

        text.text = $"{newValue}%";
        roleOption.Percentage = newValue;
    }
    private static void ConfigurePassiveButton(PassiveButton button, System.Action onClick, SpriteRenderer spriteRenderer)
    {
        button.OnClick = new();
        button.OnClick.AddListener((UnityAction)onClick);
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
    public static void InitializeBulkRoleButton()
    {
        var obj = GameObject.Instantiate(AssetManager.GetAsset<GameObject>("BulkRoleButton"));
        obj.transform.SetParent(RoleOptionMenu.RoleOptionMenuObjectData.MenuObject.transform);
        obj.transform.localPosition = new Vector3(4.7f, 8.55f, -4f);
        obj.transform.localScale = Vector3.one * 2.15f;

        obj.transform.Find("Text").GetComponent<TextMeshPro>().text = ModTranslation.GetString("BulkRoleButton");
        var passiveButton = obj.AddComponent<PassiveButton>();
        passiveButton.Colliders = new Collider2D[1];
        passiveButton.Colliders[0] = obj.GetComponent<BoxCollider2D>();
        passiveButton.OnClick = new();
        GameObject SelectedObject = null;
        passiveButton.OnClick.AddListener((UnityAction)(() =>
        {
            RoleOptionMenu.RoleOptionMenuObjectData.MenuObject.SetActive(false);
            if (RoleOptionMenu.RoleOptionMenuObjectData.BulkRoleSettingsMenu == null)
                RoleOptionMenu.RoleOptionMenuObjectData.BulkRoleSettingsMenu = InitializeBulkRoleSettingsMenu();
            else
                RoleOptionMenu.RoleOptionMenuObjectData.BulkRoleSettingsMenu.SetActive(true);
            if (RoleOptionMenu.RoleOptionMenuObjectData.CurrentBulkSettingsParent != null)
                GameObject.Destroy(RoleOptionMenu.RoleOptionMenuObjectData.CurrentBulkSettingsParent);
            UpdateSettingsMenuColor();
            RoleOptionMenu.RoleOptionMenuObjectData.CurrentBulkSettingsParent = new GameObject("BulkSettingsParent");
            RoleOptionMenu.RoleOptionMenuObjectData.CurrentBulkSettingsParent.transform.SetParent(RoleOptionMenu.RoleOptionMenuObjectData.BulkSettingsInner);
            RoleOptionMenu.RoleOptionMenuObjectData.CurrentBulkSettingsParent.transform.localScale = Vector3.one;
            RoleOptionMenu.RoleOptionMenuObjectData.CurrentBulkSettingsParent.transform.localPosition = new Vector3(0, 0, 0);
            int index = 0;
            foreach (var roleOption in RoleOptionManager.RoleOptions)
            {
                var roleBase = CustomRoleManager.AllRoles.FirstOrDefault(r => r.Role == roleOption.RoleId);
                if (roleBase != null && roleBase.OptionTeam == RoleOptionMenu.RoleOptionMenuObjectData.CurrentRoleType && roleOption.NumberOfCrews > 0)
                {
                    GenerateBulkRoleSetting(RoleOptionMenu.RoleOptionMenuObjectData.CurrentBulkSettingsParent.transform, index++, roleOption);
                }
            }
            RoleOptionMenu.RoleOptionMenuObjectData.CurrentBulkSettingsIndex = index;
            UpdateSettingsScroller(index);
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
    }
    public static void UpdateSettingsMenuColor()
    {
        var bulkRoleColor = GetCurrentBulkRoleColor();
        string[] leftright = ["Left", "Right"];
        foreach (var side in leftright)
        {
            var headerBar = RoleOptionMenu.RoleOptionMenuObjectData.BulkRoleSettingsMenu.transform.Find($"HeaderBar_{side}");
            headerBar.Find("Bar_Dark").GetComponent<SpriteRenderer>().color = bulkRoleColor.DarkHeaderColor;
            headerBar.Find("MaxPeople_Back").GetComponent<SpriteRenderer>().color = bulkRoleColor.LightHeaderColor;
            headerBar.Find("AssignPer_Back").GetComponent<SpriteRenderer>().color = bulkRoleColor.LightHeaderColor;
        }
    }
    public static void UpdateSettingsScroller(int index)
    {
        RoleOptionMenu.RoleOptionMenuObjectData.BulkSettingsScroller.ContentYBounds.max = index <= 20 ? 0f : (0.145f * (index - 20));
    }
    public static void SetupBulkHeaderBar(string side, GameObject obj)
    {
        var headerBar = obj.transform.Find($"HeaderBar_{side}");
        headerBar.Find("MaxPeople_Text").GetComponent<TextMeshPro>().text = $"{ModTranslation.GetString("MaxPeople")}";
        headerBar.Find("AssignPer_Text").GetComponent<TextMeshPro>().text = $"{ModTranslation.GetString("AssignPer")}";
    }
}