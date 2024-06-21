using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace SuperNewRoles.CustomObject;

public class ModToggleOption : OptionBehaviour
{
    public TextMeshPro TitleText;

    public SpriteRenderer CheckMark;

    public CustomOption CurrentCustomOption;

    public List<PassiveButton> ControllerSelectable;

    public bool OldValue;

    public void FixedUpdate()
    {
        bool @bool = GetBool();
        if (OldValue != @bool)
        {
            OldValue = @bool;
            CheckMark.enabled = @bool;
        }
    }

    public void Toggle()
    {
        CheckMark.enabled = !CheckMark.enabled;
        UpdateValue();
    }

    public override bool GetBool() => CheckMark.enabled;
    public override int GetInt() => CheckMark.enabled ? 1 : 0;

    public void UpdateValue() => CurrentCustomOption.UpdateSelection(GetInt());
}
