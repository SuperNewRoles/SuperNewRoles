using System;
using System.Collections.Generic;
using System.Linq;
using SuperNewRoles;
using SuperNewRoles.Mode;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SuperNewRoles.CustomModOption;

public class ModSettingsMenu : MonoBehaviour
{
    public GameObject GenericSettings;
    public GameObject ImpostorSettings;
    public GameObject NeutralSettings;
    public GameObject CrewmateSettings;
    public GameObject ModifierSettings;
    public GameObject MatchTagSettings;
    public GameObject RoleDetailsSettings;

    public PassiveButton GenericButton;
    public PassiveButton ImpostorButton;
    public PassiveButton NeutralButton;
    public PassiveButton CrewmateButton;
    public PassiveButton ModifierButton;
    public PassiveButton MatchTagButton;

    public Scroller ScrollBar;
    public UiElement BackButton;
    public UiElement DefaultButtonSelected = null;

    public OptionTabId NowTabId;
    public OptionTabId OldTabId;

    public List<UiElement> ControllerSelectable;
    public List<UiElement> GenericTabSelectables;
    public List<UiElement> ImpostorTabSelectables;
    public List<UiElement> NeutralTabSelectables;
    public List<UiElement> CrewmateTabSelectables;
    public List<UiElement> ModifierTabSelectables;
    public List<UiElement> MatchTagTabSelectables;
    public List<UiElement> RoleDetailsTabSelectables;

    public List<ModOptionBehaviour> GenericOptions;
    public List<ModOptionBehaviour> ImpostorOptions;
    public List<ModOptionBehaviour> NeutralOptions;
    public List<ModOptionBehaviour> CrewmateOptions;
    public List<ModOptionBehaviour> ModifierOptions;
    public List<ModOptionBehaviour> MatchTagOptions;
    public List<ModOptionBehaviour> RoleDetailsOptions;

    public const int TabLength = 7;

    public static Dictionary<OptionTabId, Action> OnTabOpen;
    public static List<OptionTabId> OptionGeneratedTabs;

    public const float FirstYPosition = 1.312f;
    public const float CategoryHeaderMaskedSpan = 0.45f;
    public const float CategoryHeaderEditRoleSpan = 0.65f;
    public const float RoleOptionSettingSpan = 0.43f;
    public const float OptionSpan = 0.44f;
    public float YPosition;
    public CategoryHeaderMasked CategoryHeader;
    public CategoryHeaderEditRole CategoryHeaderEditRoleOrigin;
    public RoleOptionSetting RoleOptionSettingOrigin;
    public ToggleOption CheckboxOrigin;
    public StringOption StringOptionOrigin;
    private bool IsActivateRoleHeader = false;
    private bool isOnlySelectedRole = false;
    private ToggleOption OnlySelectedRoleToggle;

    public void Start()
    {
        ControllerSelectable = new();
        GenericTabSelectables = new();
        ImpostorTabSelectables = new();
        NeutralTabSelectables = new();
        CrewmateTabSelectables = new();
        ModifierTabSelectables = new();
        MatchTagTabSelectables = new();
        RoleDetailsTabSelectables = new();
        GenericOptions = new();
        ImpostorOptions = new();
        NeutralOptions = new();
        CrewmateOptions = new();
        ModifierOptions = new();
        MatchTagOptions = new();
        RoleDetailsOptions = new();
        OnTabOpen = new();
        OptionGeneratedTabs = new();
        NowTabId = 0;
        OldTabId = 0;
        IsActivateRoleHeader = false;
        isOnlySelectedRole = false;


        RolesSettingsMenu roles = GameSettingMenu.Instance.RoleSettingsTab;
        CategoryHeaderEditRoleOrigin = roles.categoryHeaderEditRoleOrigin;
        RoleOptionSettingOrigin = roles.roleOptionSettingOrigin;
        CheckboxOrigin = roles.checkboxOrigin;
        StringOptionOrigin = roles.stringOptionOrigin;

        GameObject roles_menu_object = Instantiate(roles.gameObject, transform.parent);
        roles_menu_object.transform.Find("Gradient").gameObject.SetActive(false);//.SetParent(transform);
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

        Destroy(roles_menu_object);

        #region タブ変更ボタン
        GameObject header = new("HeaderButtons");
        header.transform.SetParent(transform);
        // new LateTask(() =>
        header.transform.localPosition = Vector3.zero;
        //, 0f, "ModSettingsMenu");
        Instantiate(roles.transform.Find("HeaderButtons/DividerImage").gameObject, header.transform).name = "DividerImage";

        GameObject instance = new("Instance");
        instance.transform.SetParent(header.transform);
        instance.transform.localPosition = new(-2.8f, 2.3f, -2f);
        instance.layer = 5;
        instance.gameObject.SetActive(true);
        SpriteRenderer instance_renderer = instance.AddComponent<SpriteRenderer>();
        instance_renderer.drawMode = SpriteDrawMode.Sliced;
        instance_renderer.size = Vector2.one * 0.75f;
        instance_renderer.color = Color.gray;
        BoxCollider2D instance_collider = instance.AddComponent<BoxCollider2D>();
        instance_collider.offset = Vector2.zero;
        instance_collider.size = Vector2.one * 0.75f;
        PassiveButton instance_button = instance.AddComponent<PassiveButton>();
        instance_button.Colliders = new Collider2D[] { instance_collider };
        instance_button.OnMouseOut = new();
        instance_button.OnMouseOver = new();
        instance_button.ClickSound = roles.AllButton.ClickSound;
        instance_button.HoverSound = roles.AllButton.HoverSound;

        GameObject generic = Instantiate(instance, header.transform);
        generic.name = "GenericButton";
        SpriteRenderer generic_renderer = generic.GetComponent<SpriteRenderer>();
        generic_renderer.sprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.Setting_Custom.png", 100f);
        generic_renderer.size = Vector2.one * 0.75f;
        GenericButton = generic.GetComponent<PassiveButton>();
        GenericButton.OnClick.AddListener(() => OpenTab(0));
        GenericButton.OnMouseOut.AddListener(() => { if (NowTabId != OptionTabId.Generic) generic_renderer.color = Color.gray; });
        GenericButton.OnMouseOver.AddListener(() => generic_renderer.color = Color.white);
        generic_renderer.color = Color.white;

        GameObject impostor = Instantiate(instance, header.transform);
        impostor.name = "ImpostorButton";
        impostor.transform.localPosition += new Vector3(0.75f, 0f);
        SpriteRenderer impostor_renderer = impostor.GetComponent<SpriteRenderer>();
        impostor_renderer.sprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.Setting_Impostor.png", 100f);
        impostor_renderer.size = Vector2.one * 0.75f;
        ImpostorButton = impostor.GetComponent<PassiveButton>();
        ImpostorButton.OnClick.AddListener(() => OpenTab(OptionTabId.Impostor));
        ImpostorButton.OnMouseOut.AddListener(() => { if (NowTabId != OptionTabId.Impostor) impostor_renderer.color = Color.gray; });
        ImpostorButton.OnMouseOver.AddListener(() => impostor_renderer.color = Color.white);
        
        GameObject neutral = Instantiate(instance, header.transform);
        neutral.name = "NeutralButton";
        neutral.transform.localPosition += new Vector3(0.75f, 0f) * 2;
        SpriteRenderer neutral_renderer = neutral.GetComponent<SpriteRenderer>();
        neutral_renderer.sprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.Setting_Neutral.png", 100f);
        neutral_renderer.size = Vector2.one * 0.75f;
        NeutralButton = neutral.GetComponent<PassiveButton>();
        NeutralButton.OnClick.AddListener(() => OpenTab(OptionTabId.Neutral));
        NeutralButton.OnMouseOut.AddListener(() => { if (NowTabId != OptionTabId.Neutral) neutral_renderer.color = Color.gray; });
        NeutralButton.OnMouseOver.AddListener(() => neutral_renderer.color = Color.white);

        GameObject crewmate = Instantiate(instance, header.transform);
        crewmate.name = "CrewmateButton";
        crewmate.transform.localPosition += new Vector3(0.75f, 0f) * 3;
        SpriteRenderer crewmate_renderer = crewmate.GetComponent<SpriteRenderer>();
        crewmate_renderer.sprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.Setting_Crewmate.png", 100f);
        crewmate_renderer.size = Vector2.one * 0.75f;
        CrewmateButton = crewmate.GetComponent<PassiveButton>();
        CrewmateButton.OnClick.AddListener(() => OpenTab(OptionTabId.Crewmate));
        CrewmateButton.OnMouseOut.AddListener(() => { if (NowTabId != OptionTabId.Crewmate) crewmate_renderer.color = Color.gray; });
        CrewmateButton.OnMouseOver.AddListener(() => crewmate_renderer.color = Color.white);

        GameObject modifier = Instantiate(instance, header.transform);
        modifier.name = "ModifierButton";
        modifier.transform.localPosition += new Vector3(0.75f, 0f) * 4;
        SpriteRenderer modifier_renderer = modifier.GetComponent<SpriteRenderer>();
        modifier_renderer.sprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.Setting_Modifier.png", 100f);
        modifier_renderer.size = Vector2.one * 0.75f;
        ModifierButton = modifier.GetComponent<PassiveButton>();
        ModifierButton.OnClick.AddListener(() => OpenTab(OptionTabId.Modifier));
        ModifierButton.OnMouseOut.AddListener(() => { if (NowTabId != OptionTabId.Modifier) modifier_renderer.color = Color.gray; });
        ModifierButton.OnMouseOver.AddListener(() => modifier_renderer.color = Color.white);

        GameObject match_tag = Instantiate(instance, header.transform);
        match_tag.name = "MatchTagButton";
        match_tag.transform.localPosition += new Vector3(0.75f, 0f) * 5;
        SpriteRenderer match_tag_renderer = match_tag.GetComponent<SpriteRenderer>();
        match_tag_renderer.sprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.TabIcon.png", 100f);
        match_tag_renderer.size = Vector2.one * 0.75f;
        MatchTagButton = match_tag.GetComponent<PassiveButton>();
        MatchTagButton.OnClick.AddListener(() => OpenTab(OptionTabId.MatchTag));
        MatchTagButton.OnMouseOut.AddListener(() => { if (NowTabId != OptionTabId.MatchTag) match_tag_renderer.color = Color.gray; });
        MatchTagButton.OnMouseOver.AddListener(() => match_tag_renderer.color = Color.white);

        Destroy(instance);
        #endregion

        #region 有効な役職のみボタン生成
        OnlySelectedRoleToggle = Instantiate(CheckboxOrigin, ScrollBar.Inner);
        OnlySelectedRoleToggle.transform.localPosition = new(1.9f, 0.82f, 0);
        OnlySelectedRoleToggle.LabelBackground.enabled = false;
        OnlySelectedRoleToggle.TitleText.transform.localPosition = new(2.7f, - 0.02f, 0f);
        OnlySelectedRoleToggle.boolOptionName = AmongUs.GameOptions.BoolOptionNames.Invalid;
        OnlySelectedRoleToggle.OnValueChanged = (Il2CppSystem.Action<OptionBehaviour>)((x) =>
        {
            isOnlySelectedRole = x.GetBool();
            OptionUpdate();
        });
        OnlySelectedRoleToggle.CheckMark.enabled = false;
        OnlySelectedRoleToggle.gameObject.SetActive(false);
        #endregion

        #region タブ生成
        CategoryHeader = Instantiate(roles.advHeader, ScrollBar.Inner);
        CategoryHeader.gameObject.name = "CategoryHeaderMasked";
        Destroy(CategoryHeader.Title.GetComponent<TextTranslatorTMP>());
        CategoryHeader.Title.text = ModTranslation.GetString("SettingSuperNewRoles"); ;

        var role_options = CustomRoleOption.RoleOptions.Values;
        List<CustomOption> options = CustomOption.options;

        GenericSettings = new("Generic Tab");
        GenericSettings.transform.SetParent(ScrollBar.Inner);
        GenericSettings.transform.localPosition = new(0f, 0f, -5f);
        OnTabOpen[0] = () => {
            if (OptionGeneratedTabs.Contains(OptionTabId.Generic))
                return;
            CreateNotRoleOptions(GenericSettings.transform, CustomOptionType.Generic);
            OptionGeneratedTabs.Add(OptionTabId.Generic);
        };

        ImpostorSettings = new("Impostor Tab");
        ImpostorSettings.transform.SetParent(ScrollBar.Inner);
        ImpostorSettings.transform.localPosition = new(0f, 0f, -5f);
        ImpostorOptions.Add(CreateCategoryHeaderEditRole(
            ImpostorSettings.transform,
            FastDestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ImpostorRolesHeader),
            (Palette.ImpostorRoleHeaderTextRed, Palette.ImpostorRoleHeaderRed, Palette.ImpostorRoleHeaderVeryDarkRed, Palette.ImpostorRoleHeaderDarkRed)
        ));
        OnTabOpen[OptionTabId.Impostor] = () =>
        {
            if (OptionGeneratedTabs.Contains(OptionTabId.Impostor))
                return;
            CreateNotRoleOptions(ImpostorSettings.transform, CustomOptionType.Impostor);
            CreateRoleOnlyOptions(ImpostorSettings.transform, CustomOptionType.Impostor, Palette.ImpostorRoleHeaderRed);
            OptionGeneratedTabs.Add(OptionTabId.Impostor);
        };

        NeutralSettings = new("Neutral Tab");
        NeutralSettings.transform.SetParent(ScrollBar.Inner);
        NeutralSettings.transform.localPosition = new(0f, 0f, -5f);
        NeutralOptions.Add(CreateCategoryHeaderEditRole(
            NeutralSettings.transform,
            ModTranslation.GetString("NeutralRolesHeader"),
            (new(78, 78, 78, byte.MaxValue), new(128, 128, 128, byte.MaxValue), new(51, 51, 51, 127), new(51, 51, 51, byte.MaxValue))
        ));
        OnTabOpen[OptionTabId.Neutral] = () =>
        {
            if (OptionGeneratedTabs.Contains(OptionTabId.Neutral))
                return;
            CreateNotRoleOptions(NeutralSettings.transform, CustomOptionType.Neutral);
            CreateRoleOnlyOptions(NeutralSettings.transform, CustomOptionType.Neutral, new(128, 128, 128, byte.MaxValue));
            OptionGeneratedTabs.Add(OptionTabId.Neutral);
        };

        CrewmateSettings = new("Crewmate Tab");
        CrewmateSettings.transform.SetParent(ScrollBar.Inner);
        CrewmateSettings.transform.localPosition = new(0f, 0f, -5f);
        CrewmateOptions.Add(CreateCategoryHeaderEditRole(
            CrewmateSettings.transform,
            FastDestroyableSingleton<TranslationController>.Instance.GetString(StringNames.CrewmateRolesHeader),
            (Palette.CrewmateRoleHeaderTextBlue, Palette.CrewmateRoleHeaderBlue, Palette.CrewmateRoleHeaderVeryDarkBlue, Palette.CrewmateRoleHeaderDarkBlue)
        ));

        OnTabOpen[OptionTabId.Crewmate] = () =>
        {
            if (OptionGeneratedTabs.Contains(OptionTabId.Crewmate))
                return;
            CreateNotRoleOptions(CrewmateSettings.transform, CustomOptionType.Crewmate);
            CreateRoleOnlyOptions(CrewmateSettings.transform, CustomOptionType.Crewmate, Palette.CrewmateRoleHeaderBlue);
            OptionGeneratedTabs.Add(OptionTabId.Crewmate);
        };

        ModifierSettings = new("Modifier Tab");
        ModifierSettings.transform.SetParent(ScrollBar.Inner);
        ModifierSettings.transform.localPosition = new(0f, 0f, -5f);
        OnTabOpen[OptionTabId.Modifier] = () =>
        {
            if (OptionGeneratedTabs.Contains(OptionTabId.Modifier))
                return;
            CreateNotRoleOptions(ModifierSettings.transform, CustomOptionType.Modifier);
            ModifierOptions.Add(CreateCategoryHeaderEditRole(
                ModifierSettings.transform,
                ModTranslation.GetString("ModifierHeader"),
                (new(212, 78, 144, byte.MaxValue), new(255, 140, 197, byte.MaxValue), new(204, 112, 158, 127), new(204, 112, 158, byte.MaxValue))
            ));
            CreateRoleOnlyOptions(ModifierSettings.transform, CustomOptionType.Modifier, new(255, 140, 197, byte.MaxValue));
            OptionGeneratedTabs.Add(OptionTabId.Modifier);
        };
        MatchTagSettings = new("Match Tag Tab");
        MatchTagSettings.transform.SetParent(ScrollBar.Inner);
        MatchTagSettings.transform.localPosition = new(0f, 0f, -5f);
        OnTabOpen[OptionTabId.MatchTag] = () =>
        {
            if (OptionGeneratedTabs.Contains(OptionTabId.MatchTag))
                return;
            CreateNotRoleOptions(MatchTagSettings.transform, CustomOptionType.MatchTag);
            OptionGeneratedTabs.Add(OptionTabId.MatchTag);
        };

        RoleDetailsSettings = new("Role Details Tab");
        RoleDetailsSettings.transform.SetParent(ScrollBar.Inner);
        RoleDetailsSettings.transform.localPosition = new(0f, 0f, -5f);
        PassiveButton close_button = Instantiate(GameSettingMenu.Instance.transform.Find("CloseButton").GetComponent<PassiveButton>(), RoleDetailsSettings.transform);
        close_button.gameObject.name = "Close Button";
        close_button.gameObject.layer = 5;
        close_button.transform.localPosition = new(4.18f, 0.4f, -2f);
        close_button.ClickMask = ScrollBar.Hitbox;
        close_button.OnClick = new();
        close_button.OnClick.AddListener(() => OpenTab(OldTabId));
        foreach (SpriteRenderer sprite in close_button.GetComponentsInChildren<SpriteRenderer>(true))
            sprite.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
        RoleDetailsSettings.gameObject.SetActive(false);
        #endregion

        foreach (PassiveButton button in ScrollBar.Inner.GetComponentsInChildren<PassiveButton>(true))
            button.ClickMask = ScrollBar.Hitbox;
    }

    public void OptionUpdate()
    {
        if (!AmongUsClient.Instance.AmHost && CustomOptionHolder.hideSettings.GetBool())
            return;

        ModeId mode = ModeHandler.GetMode(false);
        for (int i = 0; i < TabLength; i++)
        {
            List<ModOptionBehaviour> options = i switch
            {
                0 => GenericOptions,
                1 => ImpostorOptions,
                2 => NeutralOptions,
                3 => CrewmateOptions,
                4 => ModifierOptions,
                5 => MatchTagOptions,
                6 => RoleDetailsOptions,
                _ => GenericOptions
            };

            YPosition = FirstYPosition;
            if (i == 6)
                YPosition -= OptionSpan;
            foreach (ModOptionBehaviour option in options)
            {
                if (option is ModCategoryHeaderEditRole header)
                {
                    Vector3 pos = option.transform.localPosition;
                    pos.y = YPosition -= CategoryHeaderEditRoleSpan;
                    option.transform.localPosition = pos;
                    YPosition -= 0.092f;
                    continue;
                }
                CustomOption parent = option.ParentCustomOption.parent;
                bool enabled = true;

                if (isOnlySelectedRole && option is ModRoleOptionSetting && !option.ParentCustomOption.Enabled)
                    enabled = false;
                else if (option.ParentCustomOption.openSelection != -1 && option.ParentCustomOption.openSelection != parent?.selection)
                    enabled = false;
                else if (option.ParentCustomOption.HasCanShowAction && !option.ParentCustomOption.CanShowByFunc)
                    enabled = false;
                else if (option.ParentCustomOption.IsHidden(mode))
                    enabled = false;

                while (parent != null && enabled)
                {
                    if (parent is CustomRoleOption) break;
                    enabled = parent.Enabled;
                    parent = parent.parent;
                }

                option.gameObject.SetActive(enabled);
                option.HeaderMasked?.gameObject.SetActive(enabled);
                option.UpdateValue();
                if (!enabled)
                    continue;
                bool isRoleOption = option is ModRoleOptionSetting;

                if (!isRoleOption && option.ParentCustomOption.isHeader)
                    YPosition -= OptionSpan;
                if (option.HeaderMasked != null)
                {
                    Vector3 pos1 = option.HeaderMasked.transform.localPosition;
                    pos1.y = YPosition -= CategoryHeaderMaskedSpan;
                    option.HeaderMasked.transform.localPosition = pos1;
                    YPosition -= CategoryHeaderMaskedSpan;
                }
                Vector3 pos2 = option.transform.localPosition;
                pos2.y = YPosition -= OptionSpan;
                option.transform.localPosition = pos2;
            }
            if ((OptionTabId)i == NowTabId)
            {
                ScrollBar.ContentYBounds.max = Mathf.Abs(YPosition - FirstYPosition) - 2.98f;
                ScrollBar.UpdateScrollBars();
            }
        }
    }

    public void CreateNotRoleOptions(Transform transform, CustomOptionType type)
    {
        #region SetUpList
        List<ModOptionBehaviour> typeList = type switch
        {
            CustomOptionType.Generic => GenericOptions,
            CustomOptionType.Impostor => ImpostorOptions,
            CustomOptionType.Neutral => NeutralOptions,
            CustomOptionType.Crewmate => CrewmateOptions,
            CustomOptionType.Modifier => ModifierOptions,
            CustomOptionType.MatchTag => MatchTagOptions,
            _ => GenericOptions
        };
        List<UiElement> AddedSelectables = new();
        List<List<PassiveButton>> AddedSelectablesLists = new();
        # endregion

        foreach (CustomOption option in CustomOption.options)
        {
            if (option.type != type || option.RoleId != RoleId.DefaultRole) continue;
            if (option is CustomOptionBlank) continue;
            ModOptionBehaviour mod = option.IsToggle ? CreateModToggleOption(transform, option) : CreateModStringOption(transform, option);
            if (option.WithHeader) mod.HeaderMasked = CreateCategoryHeaderMasked(transform, option.HeaderText == null ? option.GetName() : ModTranslation.GetString(option.HeaderText));
            typeList.Add(mod);
            if (mod.ControllerSelectable.Count == 0)
                continue;
            else if (mod.ControllerSelectable.Count == 1)
                AddedSelectables.Add(mod.ControllerSelectable[0]);
            else
                AddedSelectablesLists.Add(mod.ControllerSelectable);
        }

        List<UiElement> selectables = type switch
        {
            CustomOptionType.Generic => GenericTabSelectables,
            CustomOptionType.Impostor => ImpostorTabSelectables,
            CustomOptionType.Neutral => NeutralTabSelectables,
            CustomOptionType.Crewmate => CrewmateTabSelectables,
            CustomOptionType.Modifier => ModifierTabSelectables,
            CustomOptionType.MatchTag => MatchTagTabSelectables,
            _ => GenericTabSelectables
        };
        selectables.AddRange(AddedSelectables);
        selectables.AddRange(AddedSelectablesLists.SelectMany(x => x));
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
        CategoryHeaderMasked masked = Instantiate(CategoryHeader, transform);
        masked.transform.localPosition = new(-0.44f, 0f, 5f);
        new LateTask(() => masked.Title.text = text, 0f, "ModSettingsMenu");
        return masked;
    }

    public ModCategoryHeaderEditRole CreateCategoryHeaderEditRole(Transform transform, string text, (Color32 Text, Color32 Label, Color32 Blank, Color32 Header) colors)
    {
        CategoryHeaderEditRole obj = Instantiate(CategoryHeaderEditRoleOrigin, transform);
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
        mod.gameObject.SetActive(false);
        new LateTask(() => mod.gameObject.SetActive(true), 0f, "ActiveCategoryHeaderEditRole");
        return mod;
    }

    public static float RoleTextOutlineWidth = 0.07f;
    public static Color RoleTextOutlineColor = Color.black;

    public ModRoleOptionSetting CreateRoleOptionSetting(Transform transform, CustomRoleOption role, Color color)
    {
        RoleOptionSetting obj = Instantiate(RoleOptionSettingOrigin, transform);
        ModRoleOptionSetting mod = obj.gameObject.AddComponent<ModRoleOptionSetting>();
        mod.transform.localPosition = new(-0.15f, 0f, -2f);
        mod.TitleText = obj.titleText;
        mod.CountText = obj.countText;
        mod.ChanceText = obj.chanceText;
        mod.LabelSprite = obj.labelSprite;
        BoxCollider2D collider = mod.LabelSprite.gameObject.AddComponent<BoxCollider2D>();
        collider.offset = Vector2.zero;
        collider.size = mod.LabelSprite.size;
        PassiveButton button = mod.LabelSprite.gameObject.AddComponent<PassiveButton>();
        button.Colliders = new Collider2D[] { collider };
        (button.OnMouseOut = new()).AddListener(() => mod.LabelSprite.color = color);
        (button.OnMouseOver = new()).AddListener(() =>
        {
            Color.RGBToHSV(color, out float h, out float s, out float v);
            mod.LabelSprite.color = Color.HSVToRGB(h, s, v / 2f);
        });
        RolesSettingsMenu roles = GameSettingMenu.Instance.RoleSettingsTab;
        button.ClickSound = roles.AllButton.ClickSound;
        button.HoverSound = roles.AllButton.HoverSound;
        mod.ParentCustomOption = role;
        mod.SettingsMenu = this;
        mod.TitleText.text = role.GetName();
        mod.TitleText.SetOutlineThickness(RoleTextOutlineWidth);
        mod.TitleText.materialForRendering.SetFloat("_FaceDilate", RoleTextOutlineWidth);
        mod.TitleText.UpdateFontAsset();
        mod.TitleText.SetOutlineColor(RoleTextOutlineColor);
        mod.UpdateValue();
        mod.LabelSprite.color = color;
        mod.ControllerSelectable = mod.GetComponentsInChildren<PassiveButton>(true).ToList();
        mod.ControllerSelectable[0].OnClick.AddListener(mod.DecreaseCount);
        mod.ControllerSelectable[1].OnClick.AddListener(mod.IncreaseCount);
        mod.ControllerSelectable[2].OnClick.AddListener(mod.DecreaseChance);
        mod.ControllerSelectable[3].OnClick.AddListener(mod.IncreaseChance);
        mod.ControllerSelectable[4].OnClick.AddListener(() => OpenTab(OptionTabId.RoleDetails, mod.CreateRoleDetailsOption));
        foreach (var item in mod.ControllerSelectable)
            item.ClickMask = ScrollBar.Hitbox;

        Destroy(obj);
        return mod;
    }

    private static Vector3 childOffset = new(1.37f, 0);
    private static Vector2 additionalSizeDelta = new(0.6f, 0);
    private static Vector3 TitleTextMinusPositionAdditional = new(0.01f, 0);
    private static Vector3 OptionLocalPosition = new(0.85f, 0f, -2f);

    public ModStringOption CreateModStringOption(Transform transform, CustomOption option)
    {
        StringOption obj = Instantiate(StringOptionOrigin, transform);
        ModStringOption mod = obj.gameObject.AddComponent<ModStringOption>();
        mod.transform.localPosition = OptionLocalPosition;
        mod.TitleText = obj.TitleText;
        mod.ValueText = obj.ValueText;
        mod.ParentCustomOption = option;
        mod.SettingsMenu = this;
        mod.TitleText.text = option.GetName();
        mod.TitleText.alignment = TextAlignmentOptions.Center;
        mod.TitleText.fontSizeMax = 2.7f;
        mod.TitleText.fontSizeMin = 1.5f;
        mod.TitleText.fontSize = 2.25f;
        mod.TitleText.transform.localScale *= 1.5f;
        mod.TitleText.transform.localPosition -= TitleTextMinusPositionAdditional;
        mod.UpdateValue();
        mod.ControllerSelectable = mod.GetComponentsInChildren<PassiveButton>(true).ToList();
        mod.ControllerSelectable[0].OnClick.AddListener(mod.Decrease);
        mod.ControllerSelectable[1].OnClick.AddListener(mod.Increase);
        foreach (var item in mod.ControllerSelectable)
            item.ClickMask = ScrollBar.Hitbox;

        mod.TitleText.rectTransform.sizeDelta += additionalSizeDelta;
        mod.LabelBackground = obj.LabelBackground;
        mod.LabelBackground.transform.localScale = new(1.9f, 1f, 0.9984f);
        foreach (GameObject child in mod.gameObject.GetChildren())
        {
            if (child == mod.LabelBackground.gameObject ||
                child == mod.TitleText.gameObject)
                continue;
            child.transform.localPosition += childOffset;
        }

        Destroy(obj);
        return mod;
    }

    public ModToggleOption CreateModToggleOption(Transform transform, CustomOption option)
    {
        ToggleOption obj = Instantiate(CheckboxOrigin, transform);
        ModToggleOption mod = obj.gameObject.AddComponent<ModToggleOption>();
        mod.transform.localPosition = OptionLocalPosition;
        mod.TitleText = obj.TitleText;
        mod.CheckMark = obj.CheckMark;
        mod.ParentCustomOption = option;
        mod.SettingsMenu = this;
        mod.TitleText.text = option.GetName();
        mod.TitleText.fontSizeMax = 2.7f;
        mod.TitleText.fontSizeMin = 1.5f;
        mod.TitleText.fontSize = 2.25f;
        mod.TitleText.alignment = TextAlignmentOptions.Center;
        mod.TitleText.transform.localScale *= 1.5f;
        mod.TitleText.transform.localPosition -= TitleTextMinusPositionAdditional;
        mod.UpdateValue();
        mod.CheckMark.transform.parent.transform.localPosition = new(1.17f, -0.042f);
        mod.ControllerSelectable = mod.GetComponentsInChildren<PassiveButton>(true).ToList();
        mod.ControllerSelectable[0].OnClick.AddListener(mod.Toggle);
        foreach (var item in mod.ControllerSelectable)
            item.ClickMask = ScrollBar.Hitbox;

        mod.TitleText.rectTransform.sizeDelta += additionalSizeDelta;
        mod.LabelBackground = obj.LabelBackground;
        mod.LabelBackground.transform.localScale = new(1.9f, 1f, 0.9984f);
        foreach (GameObject child in mod.gameObject.GetChildren())
        {
            if (child == mod.LabelBackground.gameObject ||
                child == mod.TitleText.gameObject)
                continue;
            child.transform.localPosition += childOffset;
        }

        Destroy(obj);
        return mod;
    }

    private void OnDisable() => CloseMenu();

    public void OpenMenu()
    {
        new LateTask(() =>
        {
            ControllerManager.Instance.OpenOverlayMenu(name, BackButton, DefaultButtonSelected, ControllerSelectable.ToIl2CppList());
            OpenTab(0);
            GenericButton.ReceiveMouseOver();
        }, 0f, "ModSettingsMenu");
    }
    public void CloseMenu() => ControllerManager.Instance.CloseOverlayMenu(name);

    public void OpenTab(OptionTabId id, Action details = null)
    {
        CloseAllTab();
        switch (id)
        {
            case OptionTabId.Generic:
                GenericSettings.SetActive(true);
                CategoryHeader.Title.text = ModTranslation.GetString("SettingSuperNewRoles");
                ControllerSelectable.AddRange(GenericTabSelectables);
                break;
            case OptionTabId.Impostor:
                ImpostorSettings.SetActive(true);
                CategoryHeader.Title.text = ModTranslation.GetString("SettingImpostor");
                ControllerSelectable.AddRange(ImpostorTabSelectables);
                OnlySelectedRoleToggle.gameObject.SetActive(true);
                break;
            case OptionTabId.Neutral:
                NeutralSettings.SetActive(true);
                CategoryHeader.Title.text = ModTranslation.GetString("SettingNeutral");
                ControllerSelectable.AddRange(NeutralTabSelectables);
                OnlySelectedRoleToggle.gameObject.SetActive(true);
                break;
            case OptionTabId.Crewmate:
                CrewmateSettings.SetActive(true);
                CategoryHeader.Title.text = ModTranslation.GetString("SettingCrewmate");
                ControllerSelectable.AddRange(CrewmateTabSelectables);
                OnlySelectedRoleToggle.gameObject.SetActive(true);
                break;
            case OptionTabId.Modifier:
                ModifierSettings.SetActive(true);
                CategoryHeader.Title.text = ModTranslation.GetString("ModifierSettings");
                ControllerSelectable.AddRange(ModifierTabSelectables);
                break;
            case OptionTabId.MatchTag:
                MatchTagSettings.SetActive(true);
                CategoryHeader.Title.text = ModTranslation.GetString("SettingRegulation");
                ControllerSelectable.AddRange(MatchTagTabSelectables);
                break;
            case OptionTabId.RoleDetails:
                RoleDetailsSettings.SetActive(true);
                details?.Invoke();
                ControllerSelectable.AddRange(RoleDetailsTabSelectables);
                break;
        }
        if (OnlySelectedRoleToggle.gameObject.active)
            new LateTask(() => OnlySelectedRoleToggle.TitleText.text = ModTranslation.GetString("SettingOnlySelectedRole"), 0f, "OnlySelectedRoleToggle");
        if (OnTabOpen.TryGetValue(id, out Action action))
            action();
        ScrollBar.CalculateAndSetYBounds(3.5f, 1f, 6f, 0.43f);
        NowTabId = id;
        TabButtonAllMouseOut();
        OptionUpdate();
        ScrollBar.ScrollToTop();
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
        RoleDetailsSettings.SetActive(false);
        OnlySelectedRoleToggle.gameObject.SetActive(false);
        foreach (GameObject obj in RoleDetailsSettings.GetChildren())
        {
            if (obj.name == "Close Button") continue;
            Destroy(obj);
        }

        RoleDetailsOptions.Clear();
        RoleDetailsTabSelectables.Clear();

        ControllerSelectable.Clear();
    }

    public void TabButtonAllMouseOut()
    {
        GenericButton.ReceiveMouseOut();
        ImpostorButton.ReceiveMouseOut();
        NeutralButton.ReceiveMouseOut();
        CrewmateButton.ReceiveMouseOut();
        ModifierButton.ReceiveMouseOut();
        MatchTagButton.ReceiveMouseOut();
    }
}