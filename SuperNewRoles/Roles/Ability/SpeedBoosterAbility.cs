using System;
using SuperNewRoles.Modules;
using SuperNewRoles.Modules.Events;
using SuperNewRoles.Modules.Events.Bases;
using SuperNewRoles.Roles.Ability.CustomButton;
using UnityEngine;

namespace SuperNewRoles.Roles.Ability;

public class SpeedBoosterAbility : CustomButtonBase, IButtonEffect
{
    private EventListener<PlayerPhysicsFixedUpdateEventData> _physicsUpdateListener;

    private bool SpeedBoosterActive;

    public bool isEffectActive { get; set; }
    public float EffectTimer { get; set; }
    Action IButtonEffect.OnEffectEnds => () => { RpcSetSpeedBooster(this, false); };
    public float EffectDuration => _effectDuration;
    public bool effectCancellable => true;

    public override float DefaultTimer => _coolTime;
    public override string buttonText => ModTranslation.GetString("SpeedBoosterButton");
    public override Sprite Sprite => AssetManager.GetAsset<Sprite>("SpeedUpButton.png");
    protected override KeyType keytype => KeyType.Ability1;

    private float _coolTime;
    private float _effectDuration;
    private float _multiplier;

    public SpeedBoosterAbility(float coolTime, float effectDuration, float multiplier)
    {
        _coolTime = coolTime;
        _effectDuration = effectDuration;
        _multiplier = multiplier;
    }

    public override bool CheckIsAvailable() => true;
    public override void OnClick() { RpcSetSpeedBooster(this, true); }

    [CustomRPC]
    public static void RpcSetSpeedBooster(SpeedBoosterAbility ability, bool isSpeedBoosterActive)
    {
        ability.SpeedBoosterActive = isSpeedBoosterActive;
    }

    public override void AttachToLocalPlayer()
    {
        base.AttachToLocalPlayer();
        _physicsUpdateListener = PlayerPhysicsFixedUpdateEvent.Instance.AddListener(OnPhysicsFixedUpdate);
    }

    public override void DetachToLocalPlayer()
    {
        base.DetachToLocalPlayer();
        _physicsUpdateListener?.RemoveListener();
    }

    private void OnPhysicsFixedUpdate(PlayerPhysicsFixedUpdateEventData data)
    {
        if (!data.Instance.AmOwner) return;
        if (MeetingHud.Instance != null)
        {
            SpeedBoosterActive = false;
            return;
        }
        if (SpeedBoosterActive)
        {
            var vel = data.Instance.body.velocity;
            data.Instance.body.velocity = vel * _multiplier;
        }
    }
}