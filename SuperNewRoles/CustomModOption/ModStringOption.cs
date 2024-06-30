using TMPro;

namespace SuperNewRoles.CustomModOption;

public class ModStringOption : ModOptionBehaviour
{
    public TextMeshPro TitleText;

    public TextMeshPro ValueText;

    public void Increase()
    {
        ParentCustomOption.SelectionAddition(1);
        SettingsMenu.OptionUpdate();
    }

    public void Decrease()
    {
        ParentCustomOption.SelectionAddition(-1);
        SettingsMenu.OptionUpdate();
    }

    public override int GetInt() => ParentCustomOption.GetSelection();

    public override void UpdateValue() => ValueText.text = ParentCustomOption.GetString();
}
