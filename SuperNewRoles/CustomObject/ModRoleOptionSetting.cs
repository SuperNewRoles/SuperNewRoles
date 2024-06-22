using TMPro;
using UnityEngine;

namespace SuperNewRoles.CustomObject;

public class ModRoleOptionSetting : ModOptionBehaviour
{
    public TextMeshPro TitleText;

    public TextMeshPro CountText;

    public TextMeshPro ChanceText;

    public SpriteRenderer LabelSprite;

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
        PlayerCountOption?.Addition(1);
        SettingsMenu.OptionUpdate();
    }

    public void DecreaseCount()
    {
        PlayerCountOption?.Addition(-1);
        SettingsMenu.OptionUpdate();
    }

    public void IncreaseChance()
    {
        ParentCustomOption.Addition(1);
        SettingsMenu.OptionUpdate();
    }

    public void DecreaseChance()
    {
        ParentCustomOption.Addition(-1);
        SettingsMenu.OptionUpdate();
    }

    public override void UpdateValue()
    {
        CountText.text = PlayerCountOption?.GetString() ?? "0";
        ChanceText.text = (int.TryParse(ParentCustomOption.GetString().Replace("%", ""), out int percent) ? percent : 0).ToString();
    }
}
