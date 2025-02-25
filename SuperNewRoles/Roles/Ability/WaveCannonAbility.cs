using System;
using UnityEngine;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability.CustomButton;

namespace SuperNewRoles.Roles.Ability;

public class WaveCannonAbility : CustomButtonBase
{
    private float coolDown;
    public override Sprite Sprite => AssetManager.GetAsset<Sprite>("WaveCannonButton.png");//TODO:スプライト
    public override string buttonText => "波動砲";//TODO:翻訳
    protected override KeyCode? hotkey => KeyCode.F;
    public override float DefaultTimer => coolDown;
    public WaveCannonAbility(float coolDown)
    {
        this.coolDown = coolDown;
    }
    public override void OnClick()
    {
        //TODO:波動砲発射
        Logger.Info("波動砲発射！");
        ResetTimer();
    }

    public override bool CheckIsAvailable()
    {
        //TODO:発射可能条件
        return PlayerControl.LocalPlayer.CanMove;
    }
}
