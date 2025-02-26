using System;
using UnityEngine;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability.CustomButton;
using SuperNewRoles.WaveCannonObj;

namespace SuperNewRoles.Roles.Ability;

public class WaveCannonAbility : CustomButtonBase, IButtonEffect
{
    private float coolDown;
    // IButtonEffect
    private float effectDuration;
    public float EffectDuration => effectDuration;
    public Action OnEffectEnds => () => { WaveCannonObject?.OnShoot(); };
    public bool isEffectActive { get; set; }
    public float EffectTimer { get; set; }

    public override Sprite Sprite => AssetManager.GetAsset<Sprite>("WaveCannonButton.png");
    public override string buttonText => "波動砲";
    protected override KeyCode? hotkey => KeyCode.F;
    public override float DefaultTimer => coolDown;
    public WaveCannonObjectBase WaveCannonObject { get; private set; }

    public WaveCannonAbility(float coolDown, float effectDuration)
    {
        this.coolDown = coolDown;
        this.effectDuration = effectDuration;
    }
    public override void OnClick()
    {
        //TODO:波動砲発射
        Logger.Info("波動砲発射！");
        WaveCannonObjectBase.RpcSpawnFromType(PlayerControl.LocalPlayer, WaveCannonType.Cannon, this.AbilityId);
        ResetTimer();
    }

    public override bool CheckIsAvailable()
    {
        //TODO:発射可能条件
        return PlayerControl.LocalPlayer.CanMove;
    }
    public override void Detach()
    {
        base.Detach();
        new LateTask(() => WaveCannonObject?.Detach(), 0f);
    }
    public void SpawnedWaveCannonObject(WaveCannonObjectBase waveCannonObject)
    {
        WaveCannonObject = waveCannonObject;
    }
}

public enum WaveCannonTypeForOption
{
    Tank = WaveCannonType.Tank,
    Cannon = WaveCannonType.Cannon,
    Santa = WaveCannonType.Santa
}
public enum WaveCannonType
{
    Tank,
    Cannon,
    Santa,
    Bullet,
}