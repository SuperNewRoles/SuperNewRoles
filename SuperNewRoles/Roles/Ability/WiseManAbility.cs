using System;
using System.Collections.Generic;
using UnityEngine;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability.CustomButton;
using SuperNewRoles.Events;
using SuperNewRoles.Modules.Events.Bases;
using SuperNewRoles.Events.PCEvents;
using SuperNewRoles.Modules.Events;

namespace SuperNewRoles.Roles.Ability;

public class WiseManAbility : CustomButtonBase, IButtonEffect
{
    private readonly float _coolDown;
    private readonly float _duration;
    private readonly bool _enableWalk;

    public override Sprite Sprite => AssetManager.GetAsset<Sprite>("WiseManButton.png");
    public override string buttonText => ModTranslation.GetString("WiseManButtonText");
    protected override KeyType keytype => KeyType.Ability1;
    public override float DefaultTimer => _coolDown;

    // IButtonEffect
    public float EffectDuration => _duration;
    public Action OnEffectEnds => () =>
    {
        // エフェクト終了時の処理
        RpcSetWiseManStatus(false, Player.transform.position);
        Camera.main.GetComponent<FollowerCamera>().Locked = false;
    };
    public bool isEffectActive { get; set; }
    public float EffectTimer { get; set; }

    private EventListener<WrapUpEventData> _wrapUpEventListener;
    private EventListener<PlayerPhysicsFixedUpdateEventData> _playerPhysicsFixedUpdateEventListener;
    private EventListener<TryKillEventData> _tryKillEventListener;
    private bool active;
    private Vector3 position;

    public WiseManAbility(float coolDown, float duration, bool enableWalk)
    {
        _coolDown = coolDown;
        _duration = duration;
        _enableWalk = enableWalk;
    }

    public override void AttachToAlls()
    {
        base.AttachToAlls();
        _tryKillEventListener = TryKillEvent.Instance.AddListener(OnTryKill);
        _playerPhysicsFixedUpdateEventListener = PlayerPhysicsFixedUpdateEvent.Instance.AddListener(PhysicsUpdate);
    }
    public override void DetachToAlls()
    {
        base.DetachToAlls();
        _tryKillEventListener?.RemoveListener();
        _playerPhysicsFixedUpdateEventListener?.RemoveListener();
    }

    public override void AttachToLocalPlayer()
    {
        base.AttachToLocalPlayer();
        _wrapUpEventListener = WrapUpEvent.Instance.AddListener(x => OnMeetingStart());
    }

    public override void DetachToLocalPlayer()
    {
        base.DetachToLocalPlayer();
        _wrapUpEventListener?.RemoveListener();
    }

    public override void OnClick()
    {
        float randomAngle = GetRandomAngle();
        RpcSetWiseManStatus(true, Player.transform.position);
        Camera.main.GetComponent<FollowerCamera>().Locked = true;
    }

    private void OnTryKill(TryKillEventData data)
    {
        if (data.RefTarget != Player) return;
        if (data.Killer == data.RefTarget) return;
        data.RefSuccess = false;
        data.Killer.CustomDeath(CustomDeathType.Suicide);
    }

    public override bool CheckIsAvailable()
    {
        return PlayerControl.LocalPlayer.CanMove && !PlayerControl.LocalPlayer.Data.IsDead;
    }

    private static float GetRandomAngle()
    {
        var angles = new List<float> { 135, 90, 270, 225 };
        return angles[UnityEngine.Random.Range(0, angles.Count)];
    }

    [CustomRPC]
    public void RpcSetWiseManStatus(bool isActive, Vector3 position)
    {
        active = isActive;
        this.position = position;
    }

    private void PhysicsUpdate(PlayerPhysicsFixedUpdateEventData data)
    {
        if (!active) return;
        if (data.Instance.myPlayer != Player) return;
        if (_enableWalk)
        {
            // 賢者ステップを意図的にする
            Player.MyPhysics.body.velocity = new(1, 0);
            Player.transform.position = position;
        }
        // 自視点のみじゃないと有効にしたタイミングでむっちゃ不自然になる
        else if (Player.AmOwner)
            Player.MyPhysics.body.velocity = Vector2.zero;
    }

    private void OnMeetingStart()
    {
        RpcSetWiseManStatus(false, Player.transform.position);
        Camera.main.GetComponent<FollowerCamera>().Locked = false;
    }

    public override void OnMeetingEnds()
    {
        base.OnMeetingEnds();
        // ミーティング後は状態をリセット
        OnMeetingStart();
    }
}