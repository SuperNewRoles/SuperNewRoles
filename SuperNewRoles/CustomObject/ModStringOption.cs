using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace SuperNewRoles.CustomObject;

public class ModStringOption : OptionBehaviour
{
    public string OptionTitle;

    public TextMeshPro TitleText;

    public TextMeshPro ValueText;

    public CustomOption currentCustomOption;

    public object[] Values;

    public int CurrentSelection;

    private int oldValue = -1;

    public override void SetUpFromData(BaseGameSetting data, int maskLayer)
    {
        base.SetUpFromData(data, maskLayer);
        /*
        StringGameSetting stringGameSetting = data as StringGameSetting;
        if (!((Object)(object)stringGameSetting == (Object)null))
        {
            Title = stringGameSetting.Title;
            Value = stringGameSetting.Index;
            Values = stringGameSetting.Values;
            stringOptionName = stringGameSetting.OptionName;
        }*/
    }

    private void Start()
    {
        Initialize();
        // TODO: 生成時に使う
        // GameManager.Instance.GameSettingsList.AllCategories.FirstOrDefault().AllGameSettings.FirstOrDefault(x => x.Type == OptionTypes.String);
    }

    public override void Initialize()
    {
        CurrentSelection = (int)GameOptionsManager.Instance.CurrentGameOptions.GetValue(data);
        TitleText.text = OptionTitle;
        ValueText.text = Values[CurrentSelection].ToString();
    }

    public void InitializeByMod(CustomOption option)
    {
        currentCustomOption = option;
        OptionTitle = option.GetName();
        Values = option.selections;
        CurrentSelection = option.GetSelection();
        TitleText.text = OptionTitle;
        ValueText.text = Values[CurrentSelection].ToString();
    }

    private void FixedUpdate()
    {
        if (oldValue != CurrentSelection)
        {
            oldValue = CurrentSelection;
            ValueText.text = Values[CurrentSelection].ToString();
        }
    }

    public void Increase()
    {
        CurrentSelection = Mathf.Clamp(CurrentSelection + 1, 0, Values.Length - 1);
        UpdateValue();
        OnValueChanged?.Invoke(this);
    }

    public void Decrease()
    {
        CurrentSelection = Mathf.Clamp(CurrentSelection - 1, 0, Values.Length - 1);
        UpdateValue();
        OnValueChanged?.Invoke(this);
    }

    public override int GetInt()
    {
        return CurrentSelection;
    }

    private void UpdateValue()
    {
        // TODO:ここに値を変えた際の処理を書く
        ValueText.text = Values[CurrentSelection].ToString();
        currentCustomOption.UpdateSelection(CurrentSelection);
    }
}
