using System;
using System.Collections.Generic;
using UnityEngine;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability.CustomButton;
using SuperNewRoles.Roles.Impostor;
using AmongUs.GameOptions;
using SuperNewRoles.Modules.Events.Bases;
using SuperNewRoles.Events.PCEvents;
using SuperNewRoles.Events;

namespace SuperNewRoles.Roles.Ability;

public class DoppelgangerAbility : CustomButtonBase, IButtonEffect
{
    public float DurationTime;
    public float CoolTime;
    public float SucCool;
    public float NotSucCool;

    public override Sprite Sprite => HudManager.Instance?.KillButton?.graphic?.sprite;
    public override string buttonText => ModTranslation.GetString("DoppelgangerButtonText");
    protected override KeyType keytype => KeyType.Ability1;
    public override float DefaultTimer => CoolTime;

    public override ShowTextType showTextType => ShowTextType.Show;
    public override string showText => string.Format(ModTranslation.GetString("DoppelgangerDurationTimerText"), (int)EffectTimer);

    public bool isEffectActive { get; set; }

    public Action OnEffectEnds => () =>
    {
        _shapeTarget = null;
        PlayerControl.LocalPlayer.RpcShapeshift(PlayerControl.LocalPlayer, PlayerControl.LocalPlayer.CanMove);
    };

    public float EffectDuration => DurationTime;

    public float EffectTimer { get; set; }
    public bool effectCancellable => true;

    private PlayerControl _shapeTarget;

    public DoppelgangerAbility(DoppelgangerAbilityOption option)
    {
        DurationTime = option.DurationTime;
        CoolTime = option.CoolTime;
        SucCool = option.SucCool;
        NotSucCool = option.NotSucCool;
    }

    public override void OnClick()
    {
        float nowKillCool = PlayerControl.LocalPlayer.killTimer;
        DoppelgangerShape();
        ExPlayerControl.LocalPlayer.SetKillTimerUnchecked(nowKillCool, CoolTime);
    }

    private void DoppelgangerShape()
    {
        FastDestroyableSingleton<RoleManager>.Instance.SetRole(PlayerControl.LocalPlayer, RoleTypes.Shapeshifter);
        ExPlayerControl.LocalPlayer.Data.Role.TryCast<ShapeshifterRole>()?.UseAbility();
        FastDestroyableSingleton<RoleManager>.Instance.SetRole(PlayerControl.LocalPlayer, RoleTypes.Impostor);
    }

    public override bool CheckIsAvailable()
    {
        if (!ExPlayerControl.LocalPlayer.IsAlive()) return false;
        if (!PlayerControl.LocalPlayer.CanMove) return false;
        return true;
    }

    public override bool CheckHasButton()
    {
        return ExPlayerControl.LocalPlayer.IsAlive();
    }

    public void OnMurderPlayer(MurderEventData data)
    {
        if (data.killer != Player) return;
        float timer = data.target == _shapeTarget ?
                         SucCool :
                         NotSucCool;
        ExPlayerControl.LocalPlayer.SetKillTimerUnchecked(timer, timer);
    }

    private void OnShapeshift(ShapeshiftEventData data)
    {
        _shapeTarget = data.target;
        EffectTimer = DurationTime;
    }

    private void OnFixedUpdate()
    {
        if (_shapeTarget == null)
            EffectTimer = 5f;
    }

    private EventListener<MurderEventData> _murderEvent;
    private EventListener<ShapeshiftEventData> _shapeshiftEvent;
    private EventListener _fixedUpdateEvent;

    public override void AttachToLocalPlayer()
    {
        base.AttachToLocalPlayer();
        _murderEvent = MurderEvent.Instance.AddListener(OnMurderPlayer);
        _shapeshiftEvent = ShapeshiftEvent.Instance.AddListener(OnShapeshift);
        _fixedUpdateEvent = FixedUpdateEvent.Instance.AddListener(OnFixedUpdate);
    }

    public override void DetachToLocalPlayer()
    {
        base.DetachToLocalPlayer();
        _murderEvent?.RemoveListener();
        _shapeshiftEvent?.RemoveListener();
        _fixedUpdateEvent?.RemoveListener();
    }

    public class DoppelgangerAbilityOption
    {
        public float DurationTime;
        public float CoolTime;
        public float SucCool;
        public float NotSucCool;

        public DoppelgangerAbilityOption(float durationTime, float coolTime, float sucCool, float notSucCool)
        {
            DurationTime = durationTime;
            CoolTime = coolTime;
            SucCool = sucCool;
            NotSucCool = notSucCool;
        }
    }
}