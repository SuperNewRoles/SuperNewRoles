using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace SuperNewRoles.CustomObject;

public class ModStringOption : OptionBehaviour
{
    public TextMeshPro TitleText;

    public TextMeshPro ValueText;

    public CustomOption CurrentCustomOption;

    public List<PassiveButton> ControllerSelectable;

    public object[] Values;

    public int CurrentSelection;

    private int OldValue = -1;

    public void FixedUpdate()
    {
        if (OldValue != CurrentSelection)
        {
            OldValue = CurrentSelection;
            ValueText.text = Values[CurrentSelection].ToString();
        }
    }

    public void Increase()
    {
        CurrentSelection = (CurrentSelection + 1 + Values.Length) % Values.Length;
        UpdateValue();
    }

    public void Decrease()
    {
        CurrentSelection = (CurrentSelection - 1 + Values.Length) % Values.Length;
        UpdateValue();
    }

    public override int GetInt() => CurrentSelection;

    private void UpdateValue()
    {
        // TODO:ここに値を変えた際の処理を書く
        ValueText.text = Values[CurrentSelection].ToString();
        CurrentCustomOption.UpdateSelection(CurrentSelection);
    }
}
