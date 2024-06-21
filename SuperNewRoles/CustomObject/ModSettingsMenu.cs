using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SuperNewRoles.CustomObject;

public class ModSettingsMenu : MonoBehaviour
{
    public GameObject RoleChancesSettings;
    public GameObject GenericSettings;
    public GameObject ImpostorSettings;
    public GameObject NeutralSettings;
    public GameObject CrewmateSettings;
    public GameObject ModifierSettings;
    public GameObject MatchTagSettings;

    public PassiveButton RoleChancesButton;
    public PassiveButton GenericButton;
    public PassiveButton ImpostorButton;
    public PassiveButton NeutralButton;
    public PassiveButton CrewmateButton;
    public PassiveButton ModifierButton;
    public PassiveButton MatchTagButton;

    public Scroller ScrollBar;
    public UiElement BackButton;
    public UiElement DefaultButtonSelected = null;
    public List<UiElement> ControllerSelectable;
    public List<UiElement> RoleChancesTabSelectables;
    public List<UiElement> GenericTabSelectables;
    public List<UiElement> ImpostorTabSelectables;
    public List<UiElement> NeutralTabSelectables;
    public List<UiElement> CrewmateTabSelectables;
    public List<UiElement> ModifierTabSelectables;
    public List<UiElement> MatchTagSelectables;

    public List<RoleOptionSetting> RoleChances;

    public readonly float FirstY = 1.312f;
    public float SetY;
    public CategoryHeaderMasked CategoryHeader;
    public CategoryHeaderEditRole CategoryHeaderEditRoleOrigin;
    public RoleOptionSetting RoleOptionSettingOrigin;
    public ToggleOption CheckboxOrigin;
    public NumberOption NumberOptionOrigin;
    public StringOption StringOptionOrigin;

    public void Start()
    {
        ControllerSelectable = new();
        RoleChancesTabSelectables = new();
        GenericTabSelectables = new();
        ImpostorTabSelectables = new();
        NeutralTabSelectables = new();
        CrewmateTabSelectables = new();
        ModifierTabSelectables = new();
        MatchTagSelectables = new();
        RoleChances = new();

        GameSettingMenu menu = GameSettingMenu.Instance;
        RolesSettingsMenu roles = menu.RoleSettingsTab;
        CategoryHeaderEditRoleOrigin = roles.categoryHeaderEditRoleOrigin;
        RoleOptionSettingOrigin = roles.roleOptionSettingOrigin;
        CheckboxOrigin = roles.checkboxOrigin;
        NumberOptionOrigin = roles.numberOptionOrigin;
        StringOptionOrigin = roles.stringOptionOrigin;

        GameObject roles_menu_object = Object.Instantiate(roles.gameObject, transform.parent);
        roles_menu_object.transform.Find("Gradient").SetParent(transform);
        Transform scroller_transform = roles_menu_object.transform.Find("Scroller");
        scroller_transform.SetParent(transform);
        ScrollBar = scroller_transform.GetComponent<Scroller>();
        ScrollBar.Inner.DestroyChildren();
        ScrollBar.ContentYBounds = new(0f, 0f);
        Transform close_button_transform = roles_menu_object.transform.Find("CloseButton");
        close_button_transform.SetParent(transform);
        BackButton = close_button_transform.GetComponent<PassiveButton>();
        roles_menu_object.transform.Find("UI_ScrollbarTrack").SetParent(transform);
        roles_menu_object.transform.Find("UI_Scrollbar").SetParent(transform);
        Object.Destroy(roles_menu_object);

        #region タブ変更ボタン
        GameObject header = new("HeaderButtons");
        header.transform.SetParent(transform);
        new LateTask(() => header.transform.localPosition = Vector3.zero, 0f, "ModSettingsMenu");
        Object.Instantiate(roles.transform.Find("HeaderButtons/DividerImage").gameObject, header.transform).name = "DividerImage";

        GameObject instance = new("Instance");
        instance.transform.SetParent(header.transform);
        instance.transform.localPosition = new(-2.8f, 2.3f, -2f);
        instance.transform.localScale = Vector3.one;
        instance.layer = 5;
        SpriteRenderer instance_renderer = instance.AddComponent<SpriteRenderer>();
        instance_renderer.drawMode = SpriteDrawMode.Sliced;
        instance_renderer.size = Vector2.one * 0.75f;
        instance_renderer.color = Color.gray;
        BoxCollider2D instance_collider = instance.AddComponent<BoxCollider2D>();
        instance_collider.offset = Vector2.zero;
        instance_collider.size = Vector2.one * 0.75f;
        PassiveButton instance_button = instance.AddComponent<PassiveButton>();
        instance_button.Colliders = new Collider2D[] { instance_collider };
        instance_button.OnMouseOut = roles.AllButton.OnMouseOut;
        instance_button.OnMouseOver = roles.AllButton.OnMouseOver;
        instance_button.ClickSound = roles.AllButton.ClickSound;
        instance_button.HoverSound = roles.AllButton.HoverSound;

        GameObject role_chances = Object.Instantiate(instance, header.transform);
        role_chances.name = "RoleChancesButton";
        SpriteRenderer role_chances_renderer = role_chances.GetComponent<SpriteRenderer>();
        role_chances_renderer.sprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.TabIcon.png", 100f);
        role_chances_renderer.size = Vector2.one * 0.75f;
        RoleChancesButton = role_chances.GetComponent<PassiveButton>();
        RoleChancesButton.OnClick.AddListener(() => OpenTab(0));
        RoleChancesButton.OnMouseOut.AddListener(() => role_chances_renderer.color = Color.gray);
        RoleChancesButton.OnMouseOver.AddListener(() => role_chances_renderer.color = Color.white);

        GameObject generic = Object.Instantiate(instance, header.transform);
        generic.name = "GenericButton";
        generic.transform.localPosition += new Vector3(0.75f, 0f);
        SpriteRenderer generic_renderer = generic.GetComponent<SpriteRenderer>();
        generic_renderer.sprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.TabIcon.png", 100f);
        generic_renderer.size = Vector2.one * 0.75f;
        GenericButton = generic.GetComponent<PassiveButton>();
        GenericButton.OnClick.AddListener(() => OpenTab(1));
        GenericButton.OnMouseOut.AddListener(() => generic_renderer.color = Color.gray);
        GenericButton.OnMouseOver.AddListener(() => generic_renderer.color = Color.white);

        GameObject impostor = Object.Instantiate(instance, header.transform);
        impostor.name = "ImpostorButton";
        impostor.transform.localPosition += new Vector3(0.75f, 0f) * 2;
        SpriteRenderer impostor_renderer = impostor.GetComponent<SpriteRenderer>();
        impostor_renderer.sprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.Setting_Impostor.png", 100f);
        impostor_renderer.size = Vector2.one * 0.75f;
        ImpostorButton = impostor.GetComponent<PassiveButton>();
        ImpostorButton.OnClick.AddListener(() => OpenTab(2));
        ImpostorButton.OnMouseOut.AddListener(() => impostor_renderer.color = Color.gray);
        ImpostorButton.OnMouseOver.AddListener(() => impostor_renderer.color = Color.white);

        GameObject neutral = Object.Instantiate(instance, header.transform);
        neutral.name = "NeutralButton";
        neutral.transform.localPosition += new Vector3(0.75f, 0f) * 3;
        SpriteRenderer neutral_renderer = neutral.GetComponent<SpriteRenderer>();
        neutral_renderer.sprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.Setting_Neutral.png", 100f);
        neutral_renderer.size = Vector2.one * 0.75f;
        NeutralButton = neutral.GetComponent<PassiveButton>();
        NeutralButton.OnClick.AddListener(() => OpenTab(3));
        NeutralButton.OnMouseOut.AddListener(() => neutral_renderer.color = Color.gray);
        NeutralButton.OnMouseOver.AddListener(() => neutral_renderer.color = Color.white);

        GameObject crewmate = Object.Instantiate(instance, header.transform);
        crewmate.name = "CrewmateButton";
        crewmate.transform.localPosition += new Vector3(0.75f, 0f) * 4;
        SpriteRenderer crewmate_renderer = crewmate.GetComponent<SpriteRenderer>();
        crewmate_renderer.sprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.Setting_Crewmate.png", 100f);
        crewmate_renderer.size = Vector2.one * 0.75f;
        CrewmateButton = crewmate.GetComponent<PassiveButton>();
        CrewmateButton.OnClick.AddListener(() => OpenTab(4));
        CrewmateButton.OnMouseOut.AddListener(() => crewmate_renderer.color = Color.gray);
        CrewmateButton.OnMouseOver.AddListener(() => crewmate_renderer.color = Color.white);

        GameObject modifier = Object.Instantiate(instance, header.transform);
        modifier.name = "ModifierButton";
        modifier.transform.localPosition += new Vector3(0.75f, 0f) * 5;
        SpriteRenderer modifier_renderer = modifier.GetComponent<SpriteRenderer>();
        modifier_renderer.sprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.Setting_Modifier.png", 100f);
        modifier_renderer.size = Vector2.one * 0.75f;
        ModifierButton = modifier.GetComponent<PassiveButton>();
        ModifierButton.OnClick.AddListener(() => OpenTab(5));
        ModifierButton.OnMouseOut.AddListener(() => modifier_renderer.color = Color.gray);
        ModifierButton.OnMouseOver.AddListener(() => modifier_renderer.color = Color.white);

        GameObject match_tag = Object.Instantiate(instance, header.transform);
        match_tag.name = "MatchTagButton";
        match_tag.transform.localPosition += new Vector3(0.75f, 0f) * 6;
        SpriteRenderer match_tag_renderer = match_tag.GetComponent<SpriteRenderer>();
        match_tag_renderer.sprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.TabIcon.png", 100f);
        match_tag_renderer.size = Vector2.one * 0.75f;
        MatchTagButton = match_tag.GetComponent<PassiveButton>();
        MatchTagButton.OnClick.AddListener(() => OpenTab(6));
        MatchTagButton.OnMouseOut.AddListener(() => match_tag_renderer.color = Color.gray);
        MatchTagButton.OnMouseOver.AddListener(() => match_tag_renderer.color = Color.white);

        Object.Destroy(instance);
        #endregion

        #region タブ生成
        CategoryHeader = Object.Instantiate(roles.advHeader, ScrollBar.Inner);
        CategoryHeader.gameObject.name = "CategoryHeaderMasked";
        CategoryHeader.Title.text = "";

        RoleChancesSettings = new("Role Chances Tab");
        RoleChancesSettings.transform.SetParent(ScrollBar.Inner);
        RoleChancesSettings.transform.localPosition = new(0f, 0f, -5f);
        SetY = FirstY;
        List<CustomRoleOption> role_options = CustomRoleOption.RoleOptions.Values.ToList();
        CreateCategoryHeaderEditRole(
            RoleChancesSettings.transform,
            FastDestroyableSingleton<TranslationController>.Instance.GetString(StringNames.CrewmateRolesHeader),
            (Palette.CrewmateRoleHeaderTextBlue, Palette.CrewmateRoleHeaderBlue, Palette.CrewmateRoleHeaderVeryDarkBlue, Palette.CrewmateRoleHeaderDarkBlue)
        );
        SetY -= 0.092f;
        foreach (CustomRoleOption role in role_options.FindAll(x => x.type == CustomOptionType.Crewmate))
        {
            if (role.IsToggle) continue;
            RoleOptionSetting option = CreateRoleOptionSetting(RoleChancesSettings.transform, role, Palette.CrewmateRoleHeaderBlue);
            RoleChances.Add(option);
            foreach (UiElement element in option.ControllerSelectable)
                RoleChancesTabSelectables.Add(element);
        }
        CreateCategoryHeaderEditRole(
            RoleChancesSettings.transform,
            FastDestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ImpostorRolesHeader),
            (Palette.ImpostorRoleHeaderTextRed, Palette.ImpostorRoleHeaderRed, Palette.ImpostorRoleHeaderVeryDarkRed, Palette.ImpostorRoleHeaderDarkRed)
        );
        SetY -= 0.092f;
        foreach (CustomRoleOption role in role_options.FindAll(x => x.type == CustomOptionType.Impostor))
        {
            if (role.IsToggle) continue;
            RoleOptionSetting option = CreateRoleOptionSetting(RoleChancesSettings.transform, role, Palette.ImpostorRoleHeaderRed);
            RoleChances.Add(option);
            foreach (UiElement element in option.ControllerSelectable)
                RoleChancesTabSelectables.Add(element);
        }
        CreateCategoryHeaderEditRole(
            RoleChancesSettings.transform,
            ModTranslation.GetString("NeutralRolesHeader"),
            (new(0.306f, 0.306f, 0.306f), new(0.502f, 0.502f, 0.502f), new(0.2f, 0.2f, 0.2f, 0.498f), new(0.2f, 0.2f, 0.2f))
        );
        SetY -= 0.092f;
        foreach (CustomRoleOption role in role_options.FindAll(x => x.type == CustomOptionType.Neutral))
        {
            if (role.IsToggle) continue;
            RoleOptionSetting option = CreateRoleOptionSetting(RoleChancesSettings.transform, role, new(0.502f, 0.502f, 0.502f)); // new(0.4f, 0.4f, 0.4f)
            RoleChances.Add(option);
            foreach (UiElement element in option.ControllerSelectable)
                RoleChancesTabSelectables.Add(element);
        }

        GenericSettings = new("Generic Tab");
        GenericSettings.transform.SetParent(ScrollBar.Inner);
        GenericSettings.transform.localPosition = new(0f, 0f, -5f);

        ImpostorSettings = new("Impostor Tab");
        ImpostorSettings.transform.SetParent(ScrollBar.Inner);
        ImpostorSettings.transform.localPosition = new(0f, 0f, -5f);

        NeutralSettings = new("Neutral Tab");
        NeutralSettings.transform.SetParent(ScrollBar.Inner);
        NeutralSettings.transform.localPosition = new(0f, 0f, -5f);

        CrewmateSettings = new("Crewmate Tab");
        CrewmateSettings.transform.SetParent(ScrollBar.Inner);
        CrewmateSettings.transform.localPosition = new(0f, 0f, -5f);

        ModifierSettings = new("Modifier Tab");
        ModifierSettings.transform.SetParent(ScrollBar.Inner);
        ModifierSettings.transform.localPosition = new(0f, 0f, -5f);

        MatchTagSettings = new("Match Tag Tab");
        MatchTagSettings.transform.SetParent(ScrollBar.Inner);
        MatchTagSettings.transform.localPosition = new(0f, 0f, -5f);
        #endregion
    }

    public CategoryHeaderEditRole CreateCategoryHeaderEditRole(Transform transform, string text, (Color Text, Color Label, Color Blank, Color Header) colors)
    {
        CategoryHeaderEditRole category = Object.Instantiate(CategoryHeaderEditRoleOrigin, transform);
        category.transform.localPosition = new(4.986f, SetY -= 0.65f, -2f);
        category.Title.text = text;
        category.Title.color = colors.Text;
        category.Background.color = colors.Label;
        category.blankLabel.color = colors.Blank;
        category.countLabel.color = colors.Header;
        category.chanceLabel.color = colors.Header;
        return category;
    }

    public RoleOptionSetting CreateRoleOptionSetting(Transform transform, CustomRoleOption role, Color color)
    {
        RoleOptionSetting option = Object.Instantiate(RoleOptionSettingOrigin, transform);
        CustomRoleOptionSetting custom = option.gameObject.AddComponent<CustomRoleOptionSetting>();
        option.transform.localPosition = new(-0.15f, SetY -= 0.43f, -2f);
        custom.RoleOption = role;
        custom.Parent = option;
        option.titleText.text = role.GetName();
        option.titleText.SetOutlineThickness(0.05f);
        option.titleText.materialForRendering.SetFloat("_FaceDilate", 0.05f);
        option.titleText.UpdateFontAsset();
        option.titleText.SetOutlineColor(new(0, 0, 0, 127));
        option.labelSprite.color = color;
        custom.UpdateValuesAndText();
        (option.ControllerSelectable[0].OnClick = new()).AddListener(() =>
        {
            int length = custom.PlayerCountOption.selections.Length;
            custom.PlayerCountOption.selection = Mathf.Clamp((custom.PlayerCountOption.selection - 1 + length) % length, 0, length - 1);
            custom.UpdateValuesAndText();
        });
        (option.ControllerSelectable[1].OnClick = new()).AddListener(() =>
        {
            int length = custom.PlayerCountOption.selections.Length;
            custom.PlayerCountOption.selection = Mathf.Clamp((custom.PlayerCountOption.selection + 1 + length) % length, 0, length - 1);
            custom.UpdateValuesAndText();
        });
        (option.ControllerSelectable[2].OnClick = new()).AddListener(() =>
        {
            int length = custom.RoleOption.selections.Length;
            custom.RoleOption.selection = Mathf.Clamp((custom.RoleOption.selection + 1 + length) % length, 0, length - 1);
            custom.UpdateValuesAndText();
        });
        (option.ControllerSelectable[3].OnClick = new()).AddListener(() =>
        {
            int length = custom.RoleOption.selections.Length;
            custom.RoleOption.selection = Mathf.Clamp((custom.RoleOption.selection - 1 + length) % length, 0, length - 1);
            custom.UpdateValuesAndText();
        });
        return option;
    }

    private void OnDisable() => CloseMenu();

    public void OpenMenu()
    {
        ControllerManager.Instance.OpenOverlayMenu(name, BackButton, DefaultButtonSelected, ControllerSelectable.ToIl2CppList());
        new LateTask(() => OpenTab(0), 0f, "ModSettingsMenu");
    }
    public void CloseMenu() => ControllerManager.Instance.CloseOverlayMenu(name);

    public void OpenTab(int id)
    {
        CloseAllTab();
        switch (id)
        {
            case 0:
                RoleChancesSettings.SetActive(true);
                CategoryHeader.Title.text = FastDestroyableSingleton<TranslationController>.Instance.GetString(StringNames.RoleQuotaLabel);
                ControllerSelectable.AddRange(RoleChancesTabSelectables);
                ScrollBar.CalculateAndSetYBounds(CustomRoleOption.RoleOptions.Count + 3, 1f, 6f, 0.43f);
                ControllerManager.Instance.SetDefaultSelection(RoleChances[0].ControllerSelectable[0]);
                break;
            case 1:
                GenericSettings.SetActive(true);
                // ControllerSelectable.AddRange(GenericTabSelectables);
                ScrollBar.CalculateAndSetYBounds(0, 1f, 6f, 0.43f);
                break;
            case 2:
                ImpostorSettings.SetActive(true);
                // ControllerSelectable.AddRange(ImpostorTabSelectables);
                ScrollBar.CalculateAndSetYBounds(0, 1f, 6f, 0.43f);
                break;
            case 3:
                NeutralSettings.SetActive(true);
                // ControllerSelectable.AddRange(NeutralTabSelectables);
                ScrollBar.CalculateAndSetYBounds(0, 1f, 6f, 0.43f);
                break;
            case 4:
                CrewmateSettings.SetActive(true);
                // ControllerSelectable.AddRange(CrewmateTabSelectables);
                ScrollBar.CalculateAndSetYBounds(0, 1f, 6f, 0.43f);
                break;
            case 5:
                ModifierSettings.SetActive(true);
                // ControllerSelectable.AddRange(ModifierTabSelectables);
                ScrollBar.CalculateAndSetYBounds(0, 1f, 6f, 0.43f);
                break;
            case 6:
                MatchTagSettings.SetActive(true);
                // ControllerSelectable.AddRange(MatchTagSelectables);
                ScrollBar.CalculateAndSetYBounds(0, 1f, 6f, 0.43f);
                break;
        }
        ScrollBar.ScrollToTop();
        ControllerManager.Instance.CurrentUiState.SelectableUiElements = ControllerSelectable.ToIl2CppList();
    }

    public void CloseAllTab()
    {
        RoleChancesSettings.SetActive(false);
        GenericSettings.SetActive(false);
        ImpostorSettings.SetActive(false);
        NeutralSettings.SetActive(false);
        CrewmateSettings.SetActive(false);
        ModifierSettings.SetActive(false);
        MatchTagSettings.SetActive(false);

        RoleChancesButton.ReceiveMouseOut();
        GenericButton.ReceiveMouseOut();
        ImpostorButton.ReceiveMouseOut();
        NeutralButton.ReceiveMouseOut();
        CrewmateButton.ReceiveMouseOut();
        ModifierButton.ReceiveMouseOut();
        MatchTagButton.ReceiveMouseOut();

        ControllerSelectable.Clear();
        
    }
}

public class CustomRoleOptionSetting : MonoBehaviour
{
    public RoleOptionSetting Parent;
    public CustomRoleOption RoleOption;
    private CustomOption _PlayerCountOption;
    public CustomOption PlayerCountOption
    {
        get
        {
            if (_PlayerCountOption == null) _PlayerCountOption = AllRoleSetClass.GetPlayerCountOption(RoleOption.RoleId);
            return _PlayerCountOption;
        }
    }

    public void UpdateValuesAndText()
    {
        Parent.roleMaxCount = AllRoleSetClass.GetPlayerCount(RoleOption.RoleId);
        Parent.roleChance = int.TryParse(RoleOption.GetString().Replace("%", ""), out int percent) ? percent : 0;
        Parent.countText.text = Parent.roleMaxCount.ToString();
        Parent.chanceText.text = Parent.roleChance.ToString();
    }
}