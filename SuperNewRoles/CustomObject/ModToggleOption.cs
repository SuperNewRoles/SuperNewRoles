using TMPro;
using UnityEngine;

namespace SuperNewRoles.CustomObject;

public class ModToggleOption : ModOptionBehaviour
{
    public TextMeshPro TitleText;

    public SpriteRenderer CheckMark;

    public void Toggle()
    {
        ParentCustomOption.Set(!ParentCustomOption.GetBool());
        SettingsMenu.OptionUpdate();
    }

    public override bool GetBool() => ParentCustomOption.GetBool();

    public override void UpdateValue() => CheckMark.enabled = ParentCustomOption.GetBool();
}
