using UnityEngine;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability.CustomButton;
using SuperNewRoles.Events;
using System;
using SuperNewRoles.Modules.Events.Bases;
using SuperNewRoles.Events.PCEvents;

namespace SuperNewRoles.Roles.Ability;

public class HawkAbility : CustomButtonBase, IButtonEffect
{
    private readonly float _coolDown;
    private readonly float _duration;
    private readonly float _zoomMagnification;
    private const float DEFAULT_ZOOM_SIZE = 3f;

    public override Sprite Sprite => AssetManager.GetAsset<Sprite>("HawkHawkEye.png");
    public override string buttonText => ModTranslation.GetString("HawkButtonText");
    protected override KeyType keytype => KeyType.Ability1;
    public override float DefaultTimer => _coolDown;

    // IButtonEffect
    public float EffectDuration => _duration;
    public Action OnEffectEnds => () =>
    {
        if (ExPlayerControl.LocalPlayer.IsAlive())
            FastDestroyableSingleton<HudManager>.Instance.ShadowQuad.gameObject.SetActive(true);
        if (_cannotWalkInEffect)
            PlayerControl.LocalPlayer.moveable = true;
    };
    public bool isEffectActive { get; set; }
    public float EffectTimer { get; set; }
    private readonly bool _cannotWalkInEffect;

    private EventListener<HawkEventData> hawkEventListener;
    private EventListener<MeetingStartEventData> meetingStartEventListener;
    private EventListener<DieEventData> dieEventListener;

    public HawkAbility(float coolDown, float duration, float zoomMagnification, bool cannotWalkInEffect)
    {
        _coolDown = coolDown;
        _duration = duration;
        _zoomMagnification = zoomMagnification;
        _cannotWalkInEffect = cannotWalkInEffect;
    }

    public override void AttachToLocalPlayer()
    {
        base.AttachToLocalPlayer();
        hawkEventListener = HawkEvent.Instance.AddListener((data) =>
        {
            OnHawk(data);
        });
        meetingStartEventListener = MeetingStartEvent.Instance.AddListener((data) =>
        {
            OnStartMeeting();
        });
        dieEventListener = DieEvent.Instance.AddListener((data) =>
        {
            OnDie();
        });
    }

    public override void DetachToLocalPlayer()
    {
        base.DetachToLocalPlayer();
        hawkEventListener?.RemoveListener();
        meetingStartEventListener?.RemoveListener();
        dieEventListener?.RemoveListener();
    }

    public override void OnClick()
    {
        FastDestroyableSingleton<HudManager>.Instance.ShadowQuad.gameObject.SetActive(false);
        if (_cannotWalkInEffect)
        {
            PlayerControl.LocalPlayer.moveable = false;
            PlayerControl.LocalPlayer.MyPhysics.body.velocity = Vector2.zero;
        }
    }

    private void OnHawk(HawkEventData data)
    {
        if (ExPlayerControl.LocalPlayer.IsDead()) return;
        if (MeetingHud.Instance != null) return;
        if (!isEffectActive) return;
        data.RefCancelZoom = true;
        data.RefZoomSize = (int)(DEFAULT_ZOOM_SIZE * _zoomMagnification);
        data.RefAcceleration = true;
    }

    private void OnStartMeeting()
    {
        isEffectActive = false;
        FastDestroyableSingleton<HudManager>.Instance.ShadowQuad.gameObject.SetActive(true);
        ResetTimer();
    }

    private void OnDie()
    {
        isEffectActive = false;
        ResetTimer();
    }

    public override bool CheckIsAvailable()
    {
        return PlayerControl.LocalPlayer.CanMove && !PlayerControl.LocalPlayer.Data.IsDead;
    }
}