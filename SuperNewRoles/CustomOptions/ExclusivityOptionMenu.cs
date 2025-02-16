using System.Linq;
using SuperNewRoles.CustomOptions.Data;
using SuperNewRoles.Modules;
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
        menu.transform.Find("TitleText").GetComponent<TextMeshPro>().text = $"<b>{ModTranslation.GetString("ExclusivityOptionMenuTitle")}</b>";

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
        button.transform.Find("GroupText").GetComponent<TextMeshPro>().text = $"<b>{ModTranslation.GetString("ExclusivityOptionMenuGroupText", index + 1)}</b>";

        var exclusivitySetting = index < RoleOptionManager.ExclusivitySettings.Count
            ? RoleOptionManager.ExclusivitySettings[index].Roles
            : [];
        var maxAssignSelect = button.transform.Find("MaxAssign_Select").gameObject;
        var selectedText = maxAssignSelect.transform.Find("SelectedText").GetComponent<TextMeshPro>();

        var maxAssign = index < RoleOptionManager.ExclusivitySettings.Count
            ? RoleOptionManager.ExclusivitySettings[index].MaxAssign
            : 0;
        selectedText.text = maxAssign.ToString();

        button.transform.Find("AssignedText").GetComponent<TextMeshPro>().text = string.Join(", ", exclusivitySetting.Select(x => ModTranslation.GetString(x)));

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
            newValue = currentValue < 15 ? currentValue + 1 : 1;
        }
        else
        {
            newValue = currentValue > 1 ? currentValue - 1 : 15;
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
            UIHelper.ConfigurePassiveButton(passiveButton, (UnityAction)(() =>
            {
            }), spriteRenderer);
        }
    }

    private static void UpdateEditMenuContent(int index)
    {
        var instance = ExclusivityOptionMenuObjectData.Instance;
        var roles = index < RoleOptionManager.ExclusivitySettings.Count
            ? RoleOptionManager.ExclusivitySettings[index].Roles
            : new string[] { };
        instance.ExclusivityEditMenu.transform.Find("TitleText").GetComponent<TextMeshPro>().text =
            $"<b>{ModTranslation.GetString("ExclusivityEditMenuGroupTitle", index + 1)}</b>";
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