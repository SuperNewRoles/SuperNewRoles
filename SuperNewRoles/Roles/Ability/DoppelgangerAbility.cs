using System;
using System.Collections.Generic;
using UnityEngine;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Impostor;
using AmongUs.GameOptions;
using SuperNewRoles.Modules.Events.Bases;
using SuperNewRoles.Events.PCEvents;
using SuperNewRoles.Events;
using SuperNewRoles.Roles.Ability.CustomButton;

namespace SuperNewRoles.Roles.Ability;

public class DoppelgangerAbility : AbilityBase
{
    public float DurationTime;
    public float CoolTime;
    public float SucCool;
    public float NotSucCool;

    // ShapeshiftButtonAbilityのインスタンスを保持
    private ShapeshiftButtonAbility _shapeshiftButtonAbility;

    public DoppelgangerAbility(DoppelgangerAbilityOption option)
    {
        DurationTime = option.DurationTime;
        CoolTime = option.CoolTime;
        SucCool = option.SucCool;
        NotSucCool = option.NotSucCool;
    }

    public void OnMurderPlayer(MurderEventData data)
    {
        if (data.killer != Player) return;
        if (_shapeshiftButtonAbility == null) return;

        PlayerControl currentTarget = _shapeshiftButtonAbility.ShapeTarget;

        float timer = data.target == currentTarget ?
                         SucCool :
                         NotSucCool;
        new LateTask(() => ExPlayerControl.LocalPlayer.SetKillTimerUnchecked(timer, timer), 0.02f, "DoppelgangerAbility");
    }

    private EventListener<MurderEventData> _murderEvent;

    public override void AttachToLocalPlayer()
    {
        _murderEvent = MurderEvent.Instance.AddListener(OnMurderPlayer);
    }

    public override void DetachToLocalPlayer()
    {
        _murderEvent?.RemoveListener();
        _murderEvent = null;
    }

    public override void AttachToAlls()
    {
        base.AttachToAlls();

        _shapeshiftButtonAbility = new ShapeshiftButtonAbility(CoolTime, DurationTime, "DoppelgangerButton.png");
        Player.AttachAbility(_shapeshiftButtonAbility, new AbilityParentAbility(this));
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