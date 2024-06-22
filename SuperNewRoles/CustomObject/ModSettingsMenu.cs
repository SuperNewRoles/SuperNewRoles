using System.Collections.Generic;
using System.Linq;
using SuperNewRoles.Mode;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SuperNewRoles.CustomObject;

public class ModSettingsMenu : MonoBehaviour
{
    public GameObject GenericSettings;
    public GameObject ImpostorSettings;
    public GameObject NeutralSettings;
    public GameObject CrewmateSettings;
    public GameObject ModifierSettings;
    public GameObject MatchTagSettings;

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
    public List<UiElement> GenericTabSelectables;
    public List<UiElement> ImpostorTabSelectables;
    public List<UiElement> NeutralTabSelectables;
    public List<UiElement> CrewmateTabSelectables;
    public List<UiElement> ModifierTabSelectables;
    public List<UiElement> MatchTagTabSelectables;

    public List<ModOptionBehaviour> GenericOptions;
    public List<ModOptionBehaviour> ImpostorOptions;
    public List<ModOptionBehaviour> NeutralOptions;
    public List<ModOptionBehaviour> CrewmateOptions;
    public List<ModOptionBehaviour> ModifierOptions;
    public List<ModOptionBehaviour> MatchTagOptions;

    public static readonly float FirstYPosition = 1.312f;
    public static readonly float CategoryHeaderMaskedSpan = 0.45f;
    public static readonly float CategoryHeaderEditRoleSpan = 0.65f;
    public static readonly float RoleOptionSettingSpan = 0.43f;
    public static readonly float OptionSpan = 0.44f;
    public float YPosition;
    public CategoryHeaderMasked CategoryHeader;
    public CategoryHeaderEditRole CategoryHeaderEditRoleOrigin;
    public RoleOptionSetting RoleOptionSettingOrigin;
    public ToggleOption CheckboxOrigin;
    public StringOption StringOptionOrigin;

    public void Start()
    {
        ControllerSelectable = new();
        GenericTabSelectables = new();
        ImpostorTabSelectables = new();
        NeutralTabSelectables = new();
        CrewmateTabSelectables = new();
        ModifierTabSelectables = new();
        MatchTagTabSelectables = new();
        GenericOptions = new();
        ImpostorOptions = new();
        NeutralOptions = new();
        CrewmateOptions = new();
        ModifierOptions = new();
        MatchTagOptions = new();

        GameSettingMenu menu = GameSettingMenu.Instance;
        RolesSettingsMenu roles = menu.RoleSettingsTab;
        CategoryHeaderEditRoleOrigin = roles.categoryHeaderEditRoleOrigin;
        RoleOptionSettingOrigin = roles.roleOptionSettingOrigin;
        CheckboxOrigin = roles.checkboxOrigin;
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

        GameObject generic = Object.Instantiate(instance, header.transform);
        generic.name = "GenericButton";
        SpriteRenderer generic_renderer = generic.GetComponent<SpriteRenderer>();
        generic_renderer.sprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.Setting_Custom.png", 100f);
        generic_renderer.size = Vector2.one * 0.75f;
        GenericButton = generic.GetComponent<PassiveButton>();
        GenericButton.OnClick.AddListener(() => OpenTab(0));
        GenericButton.OnMouseOut.AddListener(() => generic_renderer.color = Color.gray);
        GenericButton.OnMouseOver.AddListener(() => generic_renderer.color = Color.white);

        GameObject impostor = Object.Instantiate(instance, header.transform);
        impostor.name = "ImpostorButton";
        impostor.transform.localPosition += new Vector3(0.75f, 0f);
        SpriteRenderer impostor_renderer = impostor.GetComponent<SpriteRenderer>();
        impostor_renderer.sprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.Setting_Impostor.png", 100f);
        impostor_renderer.size = Vector2.one * 0.75f;
        ImpostorButton = impostor.GetComponent<PassiveButton>();
        ImpostorButton.OnClick.AddListener(() => OpenTab(1));
        ImpostorButton.OnMouseOut.AddListener(() => impostor_renderer.color = Color.gray);
        ImpostorButton.OnMouseOver.AddListener(() => impostor_renderer.color = Color.white);

        GameObject neutral = Object.Instantiate(instance, header.transform);
        neutral.name = "NeutralButton";
        neutral.transform.localPosition += new Vector3(0.75f, 0f) * 2;
        SpriteRenderer neutral_renderer = neutral.GetComponent<SpriteRenderer>();
        neutral_renderer.sprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.Setting_Neutral.png", 100f);
        neutral_renderer.size = Vector2.one * 0.75f;
        NeutralButton = neutral.GetComponent<PassiveButton>();
        NeutralButton.OnClick.AddListener(() => OpenTab(2));
        NeutralButton.OnMouseOut.AddListener(() => neutral_renderer.color = Color.gray);
        NeutralButton.OnMouseOver.AddListener(() => neutral_renderer.color = Color.white);

        GameObject crewmate = Object.Instantiate(instance, header.transform);
        crewmate.name = "CrewmateButton";
        crewmate.transform.localPosition += new Vector3(0.75f, 0f) * 3;
        SpriteRenderer crewmate_renderer = crewmate.GetComponent<SpriteRenderer>();
        crewmate_renderer.sprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.Setting_Crewmate.png", 100f);
        crewmate_renderer.size = Vector2.one * 0.75f;
        CrewmateButton = crewmate.GetComponent<PassiveButton>();
        CrewmateButton.OnClick.AddListener(() => OpenTab(3));
        CrewmateButton.OnMouseOut.AddListener(() => crewmate_renderer.color = Color.gray);
        CrewmateButton.OnMouseOver.AddListener(() => crewmate_renderer.color = Color.white);

        GameObject modifier = Object.Instantiate(instance, header.transform);
        modifier.name = "ModifierButton";
        modifier.transform.localPosition += new Vector3(0.75f, 0f) * 4;
        SpriteRenderer modifier_renderer = modifier.GetComponent<SpriteRenderer>();
        modifier_renderer.sprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.Setting_Modifier.png", 100f);
        modifier_renderer.size = Vector2.one * 0.75f;
        ModifierButton = modifier.GetComponent<PassiveButton>();
        ModifierButton.OnClick.AddListener(() => OpenTab(4));
        ModifierButton.OnMouseOut.AddListener(() => modifier_renderer.color = Color.gray);
        ModifierButton.OnMouseOver.AddListener(() => modifier_renderer.color = Color.white);

        GameObject match_tag = Object.Instantiate(instance, header.transform);
        match_tag.name = "MatchTagButton";
        match_tag.transform.localPosition += new Vector3(0.75f, 0f) * 5;
        SpriteRenderer match_tag_renderer = match_tag.GetComponent<SpriteRenderer>();
        match_tag_renderer.sprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.TabIcon.png", 100f);
        match_tag_renderer.size = Vector2.one * 0.75f;
        MatchTagButton = match_tag.GetComponent<PassiveButton>();
        MatchTagButton.OnClick.AddListener(() => OpenTab(5));
        MatchTagButton.OnMouseOut.AddListener(() => match_tag_renderer.color = Color.gray);
        MatchTagButton.OnMouseOver.AddListener(() => match_tag_renderer.color = Color.white);

        Object.Destroy(instance);
        #endregion

        #region タブ生成
        CategoryHeader = Object.Instantiate(roles.advHeader, ScrollBar.Inner);
        CategoryHeader.gameObject.name = "CategoryHeaderMasked";
        CategoryHeader.Title.text = "";

        List<CustomRoleOption> role_options = CustomRoleOption.RoleOptions.Values.ToList();
        List<CustomOption> options = CustomOption.options.ToList();

        GenericSettings = new("Generic Tab");
        GenericSettings.transform.SetParent(ScrollBar.Inner);
        GenericSettings.transform.localPosition = new(0f, 0f, -5f);
        CteateNotRoleOptions(GenericSettings.transform, CustomOptionType.Generic);

        ImpostorSettings = new("Impostor Tab");
        ImpostorSettings.transform.SetParent(ScrollBar.Inner);
        ImpostorSettings.transform.localPosition = new(0f, 0f, -5f);
        CteateNotRoleOptions(ImpostorSettings.transform, CustomOptionType.Impostor);
        ImpostorOptions.Add(CreateCategoryHeaderEditRole(
            ImpostorSettings.transform,
            FastDestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ImpostorRolesHeader),
            (Palette.ImpostorRoleHeaderTextRed, Palette.ImpostorRoleHeaderRed, Palette.ImpostorRoleHeaderVeryDarkRed, Palette.ImpostorRoleHeaderDarkRed)
        ));
        CreateRoleOnlyOptions(ImpostorSettings.transform, CustomOptionType.Impostor, Palette.ImpostorRoleHeaderRed);

        NeutralSettings = new("Neutral Tab");
        NeutralSettings.transform.SetParent(ScrollBar.Inner);
        NeutralSettings.transform.localPosition = new(0f, 0f, -5f);
        CteateNotRoleOptions(NeutralSettings.transform, CustomOptionType.Neutral);
        NeutralOptions.Add(CreateCategoryHeaderEditRole(
            NeutralSettings.transform,
            ModTranslation.GetString("NeutralRolesHeader"),
            (new(78, 78, 78, byte.MaxValue), new(128, 128, 128, byte.MaxValue), new(51, 51, 51, 127), new(51, 51, 51, byte.MaxValue))
        ));
        CreateRoleOnlyOptions(NeutralSettings.transform, CustomOptionType.Neutral, new(128, 128, 128, byte.MaxValue));

        CrewmateSettings = new("Crewmate Tab");
        CrewmateSettings.transform.SetParent(ScrollBar.Inner);
        CrewmateSettings.transform.localPosition = new(0f, 0f, -5f);
        CteateNotRoleOptions(CrewmateSettings.transform, CustomOptionType.Crewmate);
        CrewmateOptions.Add(CreateCategoryHeaderEditRole(
            CrewmateSettings.transform,
            FastDestroyableSingleton<TranslationController>.Instance.GetString(StringNames.CrewmateRolesHeader),
            (Palette.CrewmateRoleHeaderTextBlue, Palette.CrewmateRoleHeaderBlue, Palette.CrewmateRoleHeaderVeryDarkBlue, Palette.CrewmateRoleHeaderDarkBlue)
        ));
        CreateRoleOnlyOptions(CrewmateSettings.transform, CustomOptionType.Crewmate, Palette.CrewmateRoleHeaderBlue);

        ModifierSettings = new("Modifier Tab");
        ModifierSettings.transform.SetParent(ScrollBar.Inner);
        ModifierSettings.transform.localPosition = new(0f, 0f, -5f);
        CteateNotRoleOptions(ModifierSettings.transform, CustomOptionType.Modifier);
        ModifierOptions.Add(CreateCategoryHeaderEditRole(
            ModifierSettings.transform,
            ModTranslation.GetString("ModifierHeader"),
            (new(212, 78, 144, byte.MaxValue), new(255, 140, 197, byte.MaxValue), new(204, 112, 158, 127), new(204, 112, 158, byte.MaxValue))
        ));
        CreateRoleOnlyOptions(ModifierSettings.transform, CustomOptionType.Modifier, new(255, 140, 197, byte.MaxValue));

        MatchTagSettings = new("Match Tag Tab");
        MatchTagSettings.transform.SetParent(ScrollBar.Inner);
        MatchTagSettings.transform.localPosition = new(0f, 0f, -5f);
        CteateNotRoleOptions(MatchTagSettings.transform, CustomOptionType.MatchTag);
        #endregion
    }

    public void OptionUpdate()
    {
        if (!AmongUsClient.Instance.AmHost && CustomOptionHolder.hideSettings.GetBool())
            return;

        ModeId mode = ModeHandler.GetMode(false);
        foreach (List<ModOptionBehaviour> options in new List<ModOptionBehaviour>[] { GenericOptions, ImpostorOptions, NeutralOptions, CrewmateOptions, ModifierOptions, MatchTagOptions })
        {
            YPosition = FirstYPosition;
            foreach (ModOptionBehaviour option in options)
            {
                if (option is ModCategoryHeaderEditRole header)
                {
                    Vector3 pos = option.transform.localPosition;
                    pos.y = YPosition -= CategoryHeaderEditRoleSpan;
                    option.transform.localPosition = pos;
                    YPosition -= 0.092f;
                }
                else
                {
                    CustomOption parent = option.ParentCustomOption.parent;
                    bool enabled = true;

                    if (option.ParentCustomOption.openSelection != -1 && option.ParentCustomOption.openSelection != parent?.selection)
                        enabled = false;
                    else if (option.ParentCustomOption.HasCanShowAction && !option.ParentCustomOption.CanShowByFunc)
                        enabled = false;
                    else if (option.ParentCustomOption.IsHidden(mode))
                        enabled = false;

                    while (parent != null && enabled)
                    {
                        enabled = parent.Enabled;
                        parent = parent.parent;
                        if (parent is CustomRoleOption)
                            enabled = false;
                    }

                    option.gameObject.SetActive(enabled);
                    option.HeaderMasked?.gameObject.SetActive(enabled);
                    if (enabled)
                    {
                        bool isRoleOption = option is ModRoleOptionSetting;

                        if (!isRoleOption && option.ParentCustomOption.isHeader) YPosition -= OptionSpan;
                        if (option.HeaderMasked != null)
                        {
                            Vector3 pos1 = option.HeaderMasked.transform.localPosition;
                            pos1.y = YPosition -= CategoryHeaderMaskedSpan;
                            option.HeaderMasked.transform.localPosition = pos1;
                        }
                        float span = OptionSpan;
                        if (isRoleOption) span = RoleOptionSettingSpan;
                        Vector3 pos2 = option.transform.localPosition;
                        pos2.y = YPosition -= OptionSpan;
                        option.transform.localPosition = pos2;
                    }
                    option.UpdateValue();
                }
            }
        }
    }

    public void CteateNotRoleOptions(Transform transform, CustomOptionType type)
    {
        foreach (CustomOption option in CustomOption.options.FindAll(x => x.type == type && x.RoleId == RoleId.DefaultRole))
        {
            if (option is CustomOptionBlank) continue;
            ModOptionBehaviour mod = option.IsToggle ? CreateModToggleOption(transform, option) : CreateModStringOption(transform, option);
            if (option.WithHeader) mod.HeaderMasked = CreateCategoryHeaderMasked(transform, option.HeaderText ?? option.GetName());
            (type switch
            {
                CustomOptionType.Generic => GenericOptions,
                CustomOptionType.Impostor => ImpostorOptions,
                CustomOptionType.Neutral => NeutralOptions,
                CustomOptionType.Crewmate => CrewmateOptions,
                CustomOptionType.Modifier => ModifierOptions,
                CustomOptionType.MatchTag => MatchTagOptions,
                _ => GenericOptions
            }).Add(mod);
            (type switch
            {
                CustomOptionType.Generic => GenericTabSelectables,
                CustomOptionType.Impostor => ImpostorTabSelectables,
                CustomOptionType.Neutral => NeutralTabSelectables,
                CustomOptionType.Crewmate => CrewmateTabSelectables,
                CustomOptionType.Modifier => ModifierTabSelectables,
                CustomOptionType.MatchTag => MatchTagTabSelectables,
                _ => GenericTabSelectables
            }).AddRange(mod.ControllerSelectable);
        }
    }

    public void CreateRoleOnlyOptions(Transform transform, CustomOptionType type, Color32 color)
    {
        foreach (CustomRoleOption option in CustomRoleOption.RoleOptions.Values.ToList().FindAll(x => x.type == type))
        {
            ModOptionBehaviour mod = CreateRoleOptionSetting(transform, option, color);
            (option.type switch
            {
                CustomOptionType.Generic => GenericOptions,
                CustomOptionType.Impostor => ImpostorOptions,
                CustomOptionType.Neutral => NeutralOptions,
                CustomOptionType.Crewmate => CrewmateOptions,
                CustomOptionType.Modifier => ModifierOptions,
                CustomOptionType.MatchTag => MatchTagOptions,
                _ => GenericOptions
            }).Add(mod);
            (option.type switch
            {
                CustomOptionType.Generic => GenericTabSelectables,
                CustomOptionType.Impostor => ImpostorTabSelectables,
                CustomOptionType.Neutral => NeutralTabSelectables,
                CustomOptionType.Crewmate => CrewmateTabSelectables,
                CustomOptionType.Modifier => ModifierTabSelectables,
                CustomOptionType.MatchTag => MatchTagTabSelectables,
                _ => GenericTabSelectables
            }).AddRange(mod.ControllerSelectable);
        }
    }

    public CategoryHeaderMasked CreateCategoryHeaderMasked(Transform transform, string text)
    {
        CategoryHeaderMasked masked = Object.Instantiate(CategoryHeader, transform);
        masked.transform.localPosition = new(-0.44f, 0f, 5f);
        masked.Title.text = text;
        return masked;
    }

    public ModCategoryHeaderEditRole CreateCategoryHeaderEditRole(Transform transform, string text, (Color32 Text, Color32 Label, Color32 Blank, Color32 Header) colors)
    {
        CategoryHeaderEditRole obj = Object.Instantiate(CategoryHeaderEditRoleOrigin, transform);
        ModCategoryHeaderEditRole mod = obj.gameObject.AddComponent<ModCategoryHeaderEditRole>();
        mod.transform.localPosition = new(4.986f, 0, -2f);
        mod.Title = obj.Title;
        mod.Background = obj.Background;
        mod.BlankLabel = obj.blankLabel;
        mod.CountLabel = obj.countLabel;
        mod.ChanceLabel = obj.chanceLabel;
        mod.SettingsMenu = this;
        mod.Title.text = text;
        mod.Title.color = colors.Text;
        mod.Background.color = colors.Label;
        mod.BlankLabel.color = colors.Blank;
        mod.CountLabel.color = colors.Header;
        mod.ChanceLabel.color = colors.Header;
        return mod;
    }

    public static float RoleTextOutlineWidth = 0.07f;
    public static Color RoleTextOutlineColor = Color.black;
    public ModRoleOptionSetting CreateRoleOptionSetting(Transform transform, CustomRoleOption role, Color color)
    {
        RoleOptionSetting obj = Object.Instantiate(RoleOptionSettingOrigin, transform);
        ModRoleOptionSetting mod = obj.gameObject.AddComponent<ModRoleOptionSetting>();
        mod.transform.localPosition = new(-0.15f, 0f, -2f);
        mod.TitleText = obj.titleText;
        mod.CountText = obj.countText;
        mod.ChanceText = obj.chanceText;
        mod.LabelSprite = obj.labelSprite;
        mod.ControllerSelectable = obj.ControllerSelectable.ToList();
        mod.ParentCustomOption = role;
        mod.SettingsMenu = this;
        mod.TitleText.text = role.GetName();
        mod.TitleText.SetOutlineThickness(RoleTextOutlineWidth);
        mod.TitleText.materialForRendering.SetFloat("_FaceDilate", RoleTextOutlineWidth);
        mod.TitleText.UpdateFontAsset();
        mod.TitleText.SetOutlineColor(RoleTextOutlineColor);
        mod.UpdateValue();
        mod.LabelSprite.color = color;
        mod.ControllerSelectable[0].OnClick.AddListener(mod.DecreaseCount);
        mod.ControllerSelectable[1].OnClick.AddListener(mod.IncreaseCount);
        mod.ControllerSelectable[2].OnClick.AddListener(mod.IncreaseChance);
        mod.ControllerSelectable[3].OnClick.AddListener(mod.DecreaseChance);
        GameObject.Destroy(obj);
        return mod;
    }

    public ModStringOption CreateModStringOption(Transform transform, CustomOption option)
    {
        StringOption obj = GameObject.Instantiate(StringOptionOrigin, transform);
        ModStringOption mod = obj.gameObject.AddComponent<ModStringOption>();
        mod.transform.localPosition = new(1f, 0f, -2f);
        mod.TitleText = obj.TitleText;
        mod.ValueText = obj.ValueText;
        mod.ParentCustomOption = option;
        mod.SettingsMenu = this;
        mod.TitleText.text = option.GetName();
        mod.TitleText.alignment = TextAlignmentOptions.Center;
        mod.UpdateValue();
        mod.ControllerSelectable = mod.transform.GetComponentsInChildren<PassiveButton>(true).ToList();
        mod.ControllerSelectable[0].OnClick.AddListener(mod.Decrease);
        mod.ControllerSelectable[1].OnClick.AddListener(mod.Increase);
        GameObject.Destroy(obj);
        return mod;
    }

    public ModToggleOption CreateModToggleOption(Transform transform, CustomOption option)
    {
        ToggleOption obj = GameObject.Instantiate(CheckboxOrigin, transform);
        ModToggleOption mod = obj.gameObject.AddComponent<ModToggleOption>();
        mod.transform.localPosition = new(1f, 0f, -2f);
        mod.TitleText= obj.TitleText;
        mod.CheckMark = obj.CheckMark;
        mod.ParentCustomOption = option;
        mod.SettingsMenu = this;
        mod.TitleText.text = option.GetName();
        mod.TitleText.fontSizeMax = 2.5f;
        mod.TitleText.fontSizeMin = 1.5f;
        mod.TitleText.fontSize = 2f;
        mod.TitleText.alignment = TextAlignmentOptions.Center;
        mod.UpdateValue();
        mod.ControllerSelectable = mod.transform.GetComponentsInChildren<PassiveButton>(true).ToList();
        mod.ControllerSelectable[0].OnClick.AddListener(mod.Toggle);
        GameObject.Destroy(obj);
        return mod;
    }

    private void OnDisable() => CloseMenu();

    public void OpenMenu()
    {
        new LateTask(() =>
        {
            ControllerManager.Instance.OpenOverlayMenu(name, BackButton, DefaultButtonSelected, ControllerSelectable.ToIl2CppList());
            OpenTab(0);
        }, 0f, "ModSettingsMenu");
    }
    public void CloseMenu() => ControllerManager.Instance.CloseOverlayMenu(name);

    public void OpenTab(int id)
    {
        CloseAllTab();
        switch (id)
        {
            case 0:
                GenericSettings.SetActive(true);
                CategoryHeader.Title.text = ModTranslation.GetString("SettingSuperNewRoles");
                ControllerSelectable.AddRange(GenericTabSelectables);
                ScrollBar.CalculateAndSetYBounds(GenericOptions.Count + CustomOption.options.Count(x => x.type == CustomOptionType.Generic && x.isHeader) + 3.5f, 1f, 6f, 0.43f);
                break;
            case 1:
                ImpostorSettings.SetActive(true);
                CategoryHeader.Title.text = ModTranslation.GetString("SettingImpostor");
                ControllerSelectable.AddRange(ImpostorTabSelectables);
                ScrollBar.CalculateAndSetYBounds(ImpostorOptions.Count + CustomOption.options.Count(x => x.type == CustomOptionType.Impostor && x.isHeader) + 3.5f, 1f, 6f, 0.43f);
                break;
            case 2:
                NeutralSettings.SetActive(true);
                CategoryHeader.Title.text = ModTranslation.GetString("SettingNeutral");
                ControllerSelectable.AddRange(NeutralTabSelectables);
                ScrollBar.CalculateAndSetYBounds(NeutralOptions.Count + CustomOption.options.Count(x => x.type == CustomOptionType.Neutral && x.isHeader) + 3.5f, 1f, 6f, 0.43f);
                break;
            case 3:
                CrewmateSettings.SetActive(true);
                CategoryHeader.Title.text = ModTranslation.GetString("SettingCrewmate");
                ControllerSelectable.AddRange(CrewmateTabSelectables);
                ScrollBar.CalculateAndSetYBounds(CrewmateOptions.Count + CustomOption.options.Count(x => x.type == CustomOptionType.Crewmate && x.isHeader) + 3.5f, 1f, 6f, 0.43f);
                break;
            case 4:
                ModifierSettings.SetActive(true);
                CategoryHeader.Title.text = ModTranslation.GetString("modifierSettings");
                ControllerSelectable.AddRange(ModifierTabSelectables);
                ScrollBar.CalculateAndSetYBounds(ModifierOptions.Count + CustomOption.options.Count(x => x.type == CustomOptionType.Modifier && x.isHeader) + 3.5f, 1f, 6f, 0.43f);
                break;
            case 5:
                MatchTagSettings.SetActive(true);
                CategoryHeader.Title.text = ModTranslation.GetString("SettingRegulation");
                ControllerSelectable.AddRange(MatchTagTabSelectables);
                ScrollBar.CalculateAndSetYBounds(MatchTagOptions.Count + CustomOption.options.Count(x => x.type == CustomOptionType.MatchTag && x.isHeader) + 3.5f, 1f, 6f, 0.43f);
                break;
        }
        ScrollBar.ScrollToTop();
        OptionUpdate();
        ControllerManager.Instance.CurrentUiState.SelectableUiElements = ControllerSelectable.ToIl2CppList();
    }

    public void CloseAllTab()
    {
        GenericSettings.SetActive(false);
        ImpostorSettings.SetActive(false);
        NeutralSettings.SetActive(false);
        CrewmateSettings.SetActive(false);
        ModifierSettings.SetActive(false);
        MatchTagSettings.SetActive(false);

        GenericButton.ReceiveMouseOut();
        ImpostorButton.ReceiveMouseOut();
        NeutralButton.ReceiveMouseOut();
        CrewmateButton.ReceiveMouseOut();
        ModifierButton.ReceiveMouseOut();
        MatchTagButton.ReceiveMouseOut();

        ControllerSelectable.Clear();
        
    }
}