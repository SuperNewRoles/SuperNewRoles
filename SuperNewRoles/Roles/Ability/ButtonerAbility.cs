using System;
using System.Collections.Generic;
using Hazel;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability.CustomButton;
using UnityEngine;

namespace SuperNewRoles.Roles.Ability;

public record ButtonerAbilityData(float cooldown, int limit);
public class ButtonerAbility : CustomButtonBase, IAbilityCount
{
    public override float DefaultTimer => Data.cooldown;
    public override string buttonText => ModTranslation.GetString("ButtonerButtonText");
    private Sprite _sprite;
    public override Sprite Sprite => _sprite;
    protected override KeyType keytype => KeyType.Ability1;
    public ButtonerAbilityData Data { get; }

    public ButtonerAbility(ButtonerAbilityData data, Sprite sprite)
    {
        Data = data;
        _sprite = sprite;
    }

    public override void AttachToAlls()
    {
        base.AttachToAlls();
        Count = Data.limit;
    }

    public override bool CheckIsAvailable()
    {
        return Timer <= 0f && HasCount && !ModHelpers.IsSabotageAvailable();
    }

    public override void OnClick()
    {
        this.UseAbilityCount();
        ButtonerStartMeeting(this);
        ResetTimer();
    }

    public override ShowTextType showTextType => HasCount ? ShowTextType.ShowWithCount : ShowTextType.Hidden;

    [CustomRPC]
    public static void ButtonerStartMeeting(ButtonerAbility ability)
    {
        // AmongUsクライアントのRPC処理
        ability.Player.Player.ReportDeadBody(null);
        ability.Player.Player.RemainingEmergencies++;
    }
}