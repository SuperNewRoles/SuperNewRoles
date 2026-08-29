using System;
using System.Collections.Generic;
using UnityEngine;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability.CustomButton;
using SuperNewRoles.Events;
using SuperNewRoles.Events.PCEvents;
using SuperNewRoles.Modules.Events;
using Il2CppSystem.Collections.Generic;

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

    public bool Active { get; private set; }
    private Vector3 position;
    public bool Guarded { get; set; }

    public WiseManAbility(float coolDown, float duration, bool enableWalk)
    {
        _coolDown = coolDown;
        _duration = duration;
        _enableWalk = enableWalk;
    }

    public override void AttachToAlls()
    {
        base.AttachToAlls();
        SubscribeWithAbility(TryKillEvent.Instance, OnTryKill);
        SubscribeWithAbility(PlayerPhysicsFixedUpdateEvent.Instance, PhysicsUpdate);
        SubscribeWithAbility(DieEvent.Instance, OnDie);
    }
    public override void DetachToAlls()
    {
        base.DetachToAlls();
        Active = false;
    }

    private void OnDie(DieEventData data)
    {
        if (data.player != Player) return;
        if (!Active) return;
        if (data.player.AmOwner)
            data.player.moveable = true;
        Active = false;
        Camera.main.GetComponent<FollowerCamera>().Locked = false;
    }

    public override void AttachToLocalPlayer()
    {
        base.AttachToLocalPlayer();
        SubscribeWithAbility(WrapUpEvent.Instance, x => OnMeetingStart());
    }

    public override void DetachToLocalPlayer()
    {
        base.DetachToLocalPlayer();
        if (Active)
        {
            Player.Player.moveable = true;
            Camera.main.GetComponent<FollowerCamera>().Locked = false;
        }
    }

    public override void OnClick()
    {
        RpcSetWiseManStatus(true, Player.transform.position);
        Camera.main.GetComponent<FollowerCamera>().Locked = true;
    }

    private void OnTryKill(TryKillEventData data)
    {
        if (Guarded) return;
        if (data.RefTarget != Player) return;
        if (data.Killer == data.RefTarget) return;
        if (!Active) return;
        data.RefSuccess = false;
        
        // 波動砲による攻撃の場合、攻撃者をカウンターキルしない
        if (!data.Killer.TryGetAbility<WaveCannonAbility>(out _))
        {
            // 反撃キルはホストのみがRPC送信し、クライアント間の死亡表示ズレを防ぐ
            if (AmongUsClient.Instance.AmHost && data.Killer != null && data.Killer.IsAlive())
            {
                data.Killer.RpcCustomDeath(CustomDeathType.Suicide);
            }
        }
        
        Guarded = true;
    }

    public override bool CheckIsAvailable()
    {
        return PlayerControl.LocalPlayer.CanMove && !PlayerControl.LocalPlayer.Data.IsDead;
    }

    [CustomRPC]
    public void RpcSetWiseManStatus(bool isActive, Vector3 position)
    {
        Active = isActive;
        this.position = position;
        Guarded = false;
    }

    private void PhysicsUpdate(PlayerPhysicsFixedUpdateEventData data)
    {
        if (!Active) return;
        if (data.Instance.myPlayer != Player) return;
        if (Player.IsDead()) return;
        if (_enableWalk)
        {
            // 賢者ステップを意図的にする
            Player.MyPhysics.body.velocity = new(1, 0);
            Player.transform.position = position;
        }
        // 自視点のみじゃないと有効にしたタイミングでむっちゃ不自然になる
        else if (Player.AmOwner)
        {
            Player.MyPhysics.body.velocity = Vector2.zero;
            Player.transform.position = position;
        }
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
