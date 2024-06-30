using SuperNewRoles.Modules;
using TMPro;
using UnityEngine;

namespace SuperNewRoles.CustomModOption;

public class ModRoleOptionSetting : ModOptionBehaviour
{
    public TextMeshPro TitleText;

    public TextMeshPro CountText;

    public TextMeshPro ChanceText;

    public SpriteRenderer LabelSprite;

    public int OldCountSelection;
    private CustomOption _PlayerCountOption;
    public CustomOption PlayerCountOption
    {
        get
        {
            if (_PlayerCountOption == null) _PlayerCountOption = AllRoleSetClass.GetPlayerCountOption(ParentCustomOption.RoleId, false);
            return _PlayerCountOption;
        }
    }

    public void IncreaseCount()
    {
        PlayerCountOption.SelectionAddition(1);
        if (!(PlayerCountOption?.GetBool() ?? false)) ParentCustomOption.SetSelection(0);
        if (OldCountSelection == 0 && (PlayerCountOption?.GetBool() ?? false)) ParentCustomOption.SetSelection(10);
        SettingsMenu.OptionUpdate();
    }

    public void DecreaseCount()
    {
        PlayerCountOption.SelectionAddition(-1);
        if (!(PlayerCountOption?.GetBool() ?? false)) ParentCustomOption.SetSelection(0);
        if (OldCountSelection == 0 && (PlayerCountOption?.GetBool() ?? false)) ParentCustomOption.SetSelection(10);
        SettingsMenu.OptionUpdate();
    }

    public void IncreaseChance()
    {
        ParentCustomOption.SelectionAddition(1);
        if (!ParentCustomOption.GetBool()) PlayerCountOption?.SetSelection(0);
        if (OldCountSelection == 0 && ParentCustomOption.GetBool()) PlayerCountOption?.SetSelection(1);
        SettingsMenu.OptionUpdate();
    }

    public void DecreaseChance()
    {
        ParentCustomOption.SelectionAddition(-1);
        if (!ParentCustomOption.GetBool()) PlayerCountOption?.SetSelection(0);
        if (OldCountSelection == 0 && ParentCustomOption.GetBool()) PlayerCountOption?.SetSelection(1);
        SettingsMenu.OptionUpdate();
    }

    public void CreateRoleDetailsOption()
    {
        SettingsMenu.OldTabId = SettingsMenu.NowTabId;
        SettingsMenu.CategoryHeader.Title.text = ModTranslation.GetString("SettingOf", ParentCustomOption.GetName());
        foreach (CustomOption option in CustomOption.options.FindAll(x => x.RoleId == ParentCustomOption.RoleId && x != ParentCustomOption && x != PlayerCountOption))
        {
            if (option is CustomOptionBlank) continue;
            ModOptionBehaviour mod = option.IsToggle ? SettingsMenu.CreateModToggleOption(SettingsMenu.RoleDetailsSettings.transform, option) : SettingsMenu.CreateModStringOption(SettingsMenu.RoleDetailsSettings.transform, option);
            if (option.WithHeader) mod.HeaderMasked = SettingsMenu.CreateCategoryHeaderMasked(SettingsMenu.RoleDetailsSettings.transform, option.HeaderText == null ? option.GetName() : ModTranslation.GetString(option.HeaderText));
            SettingsMenu.RoleDetailsOptions.Add(mod);
            SettingsMenu.RoleDetailsTabSelectables.AddRange(mod.ControllerSelectable);
        }
    }

    public override void UpdateValue()
    {
        CountText.text = PlayerCountOption?.GetString() ?? "0";
        ChanceText.text = (int.TryParse(ParentCustomOption.GetString().Replace("%", ""), out int percent) ? percent : 0).ToString();
        OldCountSelection = PlayerCountOption?.GetSelection() ?? 0;
    }
}
