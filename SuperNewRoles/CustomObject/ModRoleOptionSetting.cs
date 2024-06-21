using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AmongUs.GameOptions;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

namespace SuperNewRoles.CustomObject;

public class ModRoleOptionSetting : OptionBehaviour
{
    public TextMeshPro TitleText;

    public TextMeshPro CountText;

    public TextMeshPro ChanceText;

    public SpriteRenderer LabelSprite;

    public List<PassiveButton> ControllerSelectable;

    public int RoleMaxCount;

    public int RoleChance;

    public CustomRoleOption RoleOption;

    private CustomOption _PlayerCountOption;
    public CustomOption PlayerCountOption
    {
        get
        {
            if (_PlayerCountOption == null) _PlayerCountOption = AllRoleSetClass.GetPlayerCountOption(RoleOption.RoleId, false);
            return _PlayerCountOption;
        }
    }

    public int RoleMax => PlayerCountOption.selections.Length;
    public int ChanceMax => RoleOption.selections.Length;


    public void UpdateValuesAndText()
    {
        RoleMaxCount = PlayerCountOption.GetInt();
        RoleChance = int.TryParse(RoleOption.GetString().Replace("%", ""), out int percent) ? percent : 0;
        CountText.text = RoleMaxCount.ToString();
        ChanceText.text = RoleChance.ToString();
    }

    public void IncreaseCount()
    {
        PlayerCountOption.selection = (PlayerCountOption.selection + 1 + RoleMax) % RoleMax;
        UpdateValuesAndText();
    }

    public void DecreaseCount()
    {
        PlayerCountOption.selection = (PlayerCountOption.selection - 1 + RoleMax) % RoleMax;
        UpdateValuesAndText();
    }

    public void IncreaseChance()
    {
        RoleOption.selection = (RoleOption.selection + 1 + ChanceMax) % ChanceMax;
        UpdateValuesAndText();
    }

    public void DecreaseChance()
    {
        RoleOption.selection = (RoleOption.selection - 1 + ChanceMax) % ChanceMax;
        UpdateValuesAndText();
    }
}
