using System.Linq;
using SuperNewRoles.CustomOptions.Data;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace SuperNewRoles.CustomOptions;

public static class ExclusivityOptionMenu
{
    public static void ShowExclusivityOptionMenu()
    {
        while (RoleOptionManager.ExclusivitySettings.Count <= 15)
        {
            RoleOptionManager.ExclusivitySettings.Add(new ExclusivityData(0, new string[] { }));
        }
        if (ExclusivityOptionMenuObjectData.Instance?.ExclusivityOptionMenu == null)
            Initialize();
        ExclusivityOptionMenuObjectData.Instance.ExclusivityOptionMenu.SetActive(true);
        if (ExclusivityOptionMenuObjectData.Instance.ExclusivityEditMenu != null)
            ExclusivityOptionMenuObjectData.Instance.ExclusivityEditMenu.SetActive(false);
        ReGenerateMenu();
        RecalculateScrollerMax();
    }

    private static void Initialize()
    {
        var menu = UIHelper.InstantiateUIElement(
            "ExclusivityOptionMenu",
            RoleOptionMenu.GetGameSettingMenu().transform,
            new(0, 0, -2f),
            Vector3.one * 0.2f);
        menu.transform.Find("TitleText").GetComponent<TextMeshPro>().text = $"{ModTranslation.GetString("ExclusivityOptionMenuTitle")}";

        var header = menu.transform.Find("Header");
        header.Find("MaxText").GetComponent<TextMeshPro>().text = ModTranslation.GetString("ExclusivityOptionMenuMaxText");
        header.Find("AssignedRoleText").GetComponent<TextMeshPro>().text = ModTranslation.GetString("ExclusivityOptionMenuAssignedRoleText");
        new ExclusivityOptionMenuObjectData(menu);
    }

    private static GameObject CreateExclusivityOptionButton(Transform parent, int index)
    {
        var button = UIHelper.InstantiateUIElement(
            "ExclusivityOptionButton",
            parent,
            new(-17.255f, 4.2f - (index * 1.8f), -2f),
            Vector3.one);
        button.transform.Find("GroupText").GetComponent<TextMeshPro>().text = $"{ModTranslation.GetString("ExclusivityOptionMenuGroupText", index + 1)}";

        var exclusivitySetting = index < RoleOptionManager.ExclusivitySettings.Count
            ? RoleOptionManager.ExclusivitySettings[index].Roles
            : [];
        var maxAssignSelect = button.transform.Find("MaxAssign_Select").gameObject;
        var selectedText = maxAssignSelect.transform.Find("SelectedText").GetComponent<TextMeshPro>();

        var maxAssign = index < RoleOptionManager.ExclusivitySettings.Count
            ? RoleOptionManager.ExclusivitySettings[index].MaxAssign
            : 0;
        selectedText.text = maxAssign.ToString();

        button.transform.Find("AssignedText").GetComponent<TextMeshPro>().text = string.Join(", ", exclusivitySetting.Select(x => ModTranslation.GetString(x.ToString())));

        ConfigureMaxAssignSelectButtons(maxAssignSelect, selectedText, index);

        return button;
    }

    private static void ConfigureMaxAssignSelectButtons(GameObject selectObject, TextMeshPro selectedText, int index)
    {
        ConfigureMaxAssignSelectButton(selectObject, "Button_Minus", selectedText, false, index);
        ConfigureMaxAssignSelectButton(selectObject, "Button_Plus", selectedText, true, index);
    }

    private static void ConfigureMaxAssignSelectButton(
        GameObject selectObject,
        string buttonName,
        TextMeshPro selectedText,
        bool isIncrement,
        int index)
    {
        var button = selectObject.transform.Find(buttonName).gameObject;
        var passiveButton = button.AddComponent<PassiveButton>();
        passiveButton.Colliders = new Collider2D[] { button.GetComponent<BoxCollider2D>() };
        var spriteRenderer = passiveButton.GetComponent<SpriteRenderer>();

        UIHelper.ConfigurePassiveButton(passiveButton, (UnityAction)(() =>
        {
            HandleMaxAssignSelection(selectedText, isIncrement, index);
        }), spriteRenderer);
    }

    private static void HandleMaxAssignSelection(TextMeshPro selectedText, bool isIncrement, int index)
    {
        int currentValue = int.Parse(selectedText.text);
        int newValue;

        if (isIncrement)
        {
            newValue = currentValue < 15 ? currentValue + 1 : 0;
        }
        else
        {
            newValue = currentValue > 0 ? currentValue - 1 : 15;
        }

        selectedText.text = newValue.ToString();

        RoleOptionManager.ExclusivitySettings[index].MaxAssign = newValue;
    }

    private static void ConfigureEditButton(GameObject button, int index)
    {
        var editButton = button.transform.Find("EditButton").gameObject;
        var passiveButton = editButton.AddComponent<PassiveButton>();
        passiveButton.Colliders = new Collider2D[] { editButton.GetComponent<BoxCollider2D>() };
        var spriteRenderer = editButton.transform.Find("Background").GetComponent<SpriteRenderer>();

        UIHelper.ConfigurePassiveButton(passiveButton, (UnityAction)(() =>
        {
            ShowExclusivityEditMenu(index);
        }), spriteRenderer);
    }

    private static void ShowExclusivityEditMenu(int index)
    {
        var instance = ExclusivityOptionMenuObjectData.Instance;
        instance.CurrentEditingIndex = index;

        if (instance.ExclusivityEditMenu == null)
        {
            InitializeEditMenu();
        }

        instance.ExclusivityOptionMenu.SetActive(false);
        instance.ExclusivityEditMenu.SetActive(true);
        if (instance.RoleDetailButtonContainer != null)
            GameObject.Destroy(instance.RoleDetailButtonContainer);
        UpdateEditMenuContent(index);
    }

    private static void InitializeEditMenu()
    {
        var instance = ExclusivityOptionMenuObjectData.Instance;
        var menu = UIHelper.InstantiateUIElement(
            "ExclusivityEditMenu",
            RoleOptionMenu.GetGameSettingMenu().transform,
            new(0, 0, -3f),
            Vector3.one * 0.2f);

        instance.ExclusivityEditMenu = menu;
        instance.ExclusivityEditRightAreaScroller = menu.transform.Find("RightArea").GetComponent<Scroller>();
        instance.ExclusivityEditRightAreaInner = instance.ExclusivityEditRightAreaScroller.transform.Find("Inner").gameObject;
        var closeButton = menu.transform.Find("CloseButton").gameObject;
        UIHelper.SetText(closeButton, ModTranslation.GetString("Close"));
        var closePassiveButton = closeButton.AddComponent<PassiveButton>();
        closePassiveButton.Colliders = new Collider2D[] { closeButton.GetComponent<BoxCollider2D>() };
        var selectedObject = closeButton.transform.Find("Selected").gameObject;
        UIHelper.ConfigurePassiveButton(closePassiveButton, (UnityAction)(() =>
        {
            instance.ExclusivityEditMenu.SetActive(false);
            instance.ExclusivityOptionMenu.SetActive(true);
            instance.CurrentEditingIndex = -1;
            ReGenerateMenu();
        }), closeButton.GetComponent<SpriteRenderer>());

        InitializeEditMenuLeftButtons(menu);
    }

    private static void InitializeEditMenuLeftButtons(GameObject menu)
    {
        string[] buttons = ["Impostor", "Neutral", "Crewmate"];
        var leftButtons = menu.transform.Find("LeftButtons").gameObject;
        for (int i = 0; i < buttons.Length; i++)
        {
            var button = leftButtons.transform.Find($"{buttons[i]}Button").gameObject;
            var passiveButton = button.AddComponent<PassiveButton>();
            passiveButton.Colliders = new Collider2D[] { button.GetComponent<BoxCollider2D>() };
            var spriteRenderer = button.transform.GetComponent<SpriteRenderer>();
            UIHelper.SetText(button, ModTranslation.GetString(buttons[i]));
            var buttonType = buttons[i];
            UIHelper.ConfigurePassiveButton(passiveButton, (UnityAction)(() =>
            {
                Logger.Info($"buttonType: {buttonType}");
                GenerateRoleDetailButtons(buttonType);
            }), spriteRenderer);
        }
        GenerateRoleDetailButtons(buttons.First());
    }

    private static void GenerateRoleDetailButtons(string roleType)
    {
        var instance = ExclusivityOptionMenuObjectData.Instance;
        if (instance.ExclusivityEditRightAreaInner == null) return;

        // 既存のコンテナを削除
        if (instance.RoleDetailButtonContainer != null)
            GameObject.Destroy(instance.RoleDetailButtonContainer);

        // 新しいコンテナを作成
        var container = new GameObject("RoleDetailButtonContainer");
        container.transform.SetParent(instance.ExclusivityEditRightAreaInner.transform);
        container.transform.localPosition = Vector3.zero;
        container.transform.localScale = Vector3.one;
        instance.RoleDetailButtonContainer = container;

        // 陣営に応じたロールのリストを取得
        var roles = RoleOptionManager.RoleOptions.Where(x =>
        {
            var roleInfo = CustomRoleManager.AllRoles.FirstOrDefault(r => r.Role == x.RoleId);
            if (roleInfo == null) return false;
            return roleType switch
            {
                "Impostor" => roleInfo.AssignedTeam == AssignedTeamType.Impostor,
                "Neutral" => roleInfo.AssignedTeam == AssignedTeamType.Neutral,
                "Crewmate" => roleInfo.AssignedTeam == AssignedTeamType.Crewmate,
                _ => false
            };
        }).ToList();

        // ロールボタンを生成
        int index = 0;
        // デバッグ用に20回同じロールを生成
        /*for (int i = 0; i < 20; i++)
        {
            var debugRole = RoleOptionManager.RoleOptions[0];
            var button = GenerateRoleDetailButton(ModTranslation.GetString($"{debugRole.RoleId}"), container.transform, index, debugRole);
            ConfigureRoleDetailButton(button, debugRole, instance.CurrentEditingIndex);
            index++;
        }*/
        foreach (var roleOption in roles)
        {
            var roleInfo = CustomRoleManager.AllRoles.FirstOrDefault(r => r.Role == roleOption.RoleId);
            if (roleInfo == null) continue;

            string roleName = ModTranslation.GetString($"{roleInfo.Role}");
            var button = GenerateRoleDetailButton(roleName, container.transform, index, roleOption);
            ConfigureRoleDetailButton(button, roleOption, instance.CurrentEditingIndex);
            index++;
        }

        // スクロール範囲の調整
        instance.ExclusivityEditRightAreaScroller.ContentYBounds.max = index < 25 ? 0f : (0.38f * ((index - 24) / 4 + 1)) - 0.5f;
    }

    private static GameObject GenerateRoleDetailButton(string roleName, Transform parent, int index, RoleOptionManager.RoleOption roleOption)
    {
        var obj = GameObject.Instantiate(AssetManager.GetAsset<GameObject>(RoleOptionMenu.ROLE_DETAIL_BUTTON_ASSET_NAME));
        obj.transform.SetParent(parent);
        obj.transform.localScale = Vector3.one * 1.45f;

        // ボタンの位置を計算
        int col = index % 5;
        int row = index / 5;
        float posX = -10.65f + col * 7.725f;
        float posY = 5.85f - row * 1.85f;
        obj.transform.localPosition = new Vector3(posX, posY, -0.21f);

        // ロール名を設定
        obj.transform.Find("Text").GetComponent<TextMeshPro>().text = $"<b><color=#{ColorUtility.ToHtmlStringRGB(roleOption.RoleColor)}>{roleName}</color></b>";

        return obj;
    }

    private static void ConfigureRoleDetailButton(GameObject button, RoleOptionManager.RoleOption roleOption, int editingIndex)
    {
        var passiveButton = button.AddComponent<PassiveButton>();
        passiveButton.Colliders = new Collider2D[] { button.GetComponent<BoxCollider2D>() };
        var spriteRenderer = button.GetComponent<SpriteRenderer>();
        GameObject selectedObject = button.transform.Find("Selected").gameObject;

        passiveButton.OnClick = new();
        passiveButton.OnClick.AddListener((UnityAction)(() =>
        {
            var roleName = roleOption.RoleId;
            var settings = RoleOptionManager.ExclusivitySettings[editingIndex];
            var roles = settings.Roles.ToList();

            if (roles.Contains(roleName))
            {
                roles.Remove(roleName);
                spriteRenderer.color = new Color(1f, 1f, 1f, 0.6f);
            }
            else
            {
                roles.Add(roleName);
                spriteRenderer.color = Color.white;
            }

            RoleOptionManager.ExclusivitySettings[editingIndex].Roles = roles;
            ReGenerateMenu();
        }));

        passiveButton.OnMouseOver = new();
        passiveButton.OnMouseOver.AddListener((UnityAction)(() =>
        {
            selectedObject.SetActive(true);
        }));

        passiveButton.OnMouseOut = new();
        passiveButton.OnMouseOut.AddListener((UnityAction)(() =>
        {
            selectedObject.SetActive(false);
        }));

        // 初期の選択状態を設定
        var isSelected = RoleOptionManager.ExclusivitySettings[editingIndex].Roles.Contains(roleOption.RoleId);
        spriteRenderer.color = isSelected ? Color.white : new Color(1f, 1f, 1f, 0.6f);
    }

    private static void UpdateEditMenuContent(int index)
    {
        var instance = ExclusivityOptionMenuObjectData.Instance;
        var roles = index < RoleOptionManager.ExclusivitySettings.Count
            ? RoleOptionManager.ExclusivitySettings[index].Roles
            : new();
        instance.ExclusivityEditMenu.transform.Find("TitleText").GetComponent<TextMeshPro>().text =
            $"{ModTranslation.GetString("ExclusivityEditMenuGroupTitle", index + 1)}";
    }

    private static void ReGenerateMenu()
    {
        var instance = ExclusivityOptionMenuObjectData.Instance;
        if (instance.ExclusivityOptionButtonContainer != null)
            GameObject.Destroy(instance.ExclusivityOptionButtonContainer);
        var container = new GameObject("ExclusivityOptionButtonContainer");
        var containerTransform = container.transform;

        containerTransform.SetParent(instance.MainAreaInner.transform);
        containerTransform.localPosition = new(0, 0, -2f);
        containerTransform.localScale = Vector3.one;

        instance.ExclusivityOptionButtonContainer = container;

        const int buttonCount = 15;
        for (int i = 0; i < buttonCount; i++)
        {
            var button = CreateExclusivityOptionButton(containerTransform, i);
            ConfigureEditButton(button, i);
        }
    }

    private static void RecalculateScrollerMax()
    {
        var scroller = ExclusivityOptionMenuObjectData.Instance.MainAreaScroller;
        var childCount = ExclusivityOptionMenuObjectData.Instance.ExclusivityOptionButtonContainer.transform.childCount;
        scroller.ContentYBounds.max = childCount <= 9 ? 0 : (childCount - 9) * 1.64f;
    }
}