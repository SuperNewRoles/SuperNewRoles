using System;
using UnityEngine;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability.CustomButton;
using SuperNewRoles.Events;
using SuperNewRoles.Modules.Events.Bases;
using SuperNewRoles.Events.PCEvents;
using AmongUs.GameOptions;

namespace SuperNewRoles.Roles.Ability;

public class LighterAbility : CustomButtonBase, IButtonEffect
{
    public override Sprite Sprite => AssetManager.GetAsset<Sprite>("LighterLightOnButton.png");
    public override string buttonText => ModTranslation.GetString("LighterButtonText");
    protected override KeyType keytype => KeyType.Ability1;
    public override float DefaultTimer => coolTime;
    private float coolTime;
    private float durationTime;
    private EventListener<ShipStatusLightEventData> _onLightEvent;
    private static bool isLightOn = false;

    private float radius;
    public bool isEffectActive { get; set; }

    public Action OnEffectEnds => () =>
    {
    };

    public float EffectDuration => durationTime;
    public bool effectCancellable => true;

    public float EffectTimer { get; set; }

    public LighterAbility(float coolTime, float durationTime, float radius)
    {
        this.coolTime = coolTime;
        this.durationTime = durationTime;
        this.radius = radius;
    }

    public override void OnClick()
    {
    }

    public override bool CheckIsAvailable()
    {
        return ExPlayerControl.LocalPlayer.IsAlive();
    }

    public override void AttachToLocalPlayer()
    {
        base.AttachToLocalPlayer();
        _onLightEvent = ShipStatusLightEvent.Instance.AddListener(OnLightEvent);
    }

    public override void DetachToLocalPlayer()
    {
        base.DetachToLocalPlayer();
        _onLightEvent?.RemoveListener();
    }

    private void OnLightEvent(ShipStatusLightEventData data)
    {
        if (data.player.PlayerId == ExPlayerControl.LocalPlayer.PlayerId && isEffectActive)
        {
            // ライトが点灯している場合は明るくする
            data.lightRadius = Mathf.Lerp(ShipStatus.Instance.MinLightRadius, ShipStatus.Instance.MaxLightRadius, 1f) * radius;
        }
    }
}