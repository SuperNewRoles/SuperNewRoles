using System;
using SuperNewRoles.Events;
using SuperNewRoles.Events.PCEvents;
using SuperNewRoles.Modules;
using SuperNewRoles.Modules.Events.Bases;
using UnityEngine;
using AmongUs.GameOptions;

namespace SuperNewRoles.Roles.Ability;

public record EvilGamblerData(
    float successKillCooldown,
    float failureKillCooldown,
    int successChance
);

public class EvilGamblerAbility : AbilityBase
{
    private readonly EvilGamblerData _data;
    private EventListener<MurderEventData> _murderEventListener;
    private EventListener<WrapUpEventData> _wrapUpEventListener;

    public EvilGamblerAbility(EvilGamblerData data)
    {
        _data = data;
    }

    public override void AttachToLocalPlayer()
    {
        _murderEventListener = MurderEvent.Instance.AddListener(OnMurder);
        _wrapUpEventListener = WrapUpEvent.Instance.AddListener(OnWrapUp);
    }

    public override void DetachToLocalPlayer()
    {
        base.DetachToLocalPlayer();
        MurderEvent.Instance.RemoveListener(_murderEventListener);
        WrapUpEvent.Instance.RemoveListener(_wrapUpEventListener);
    }

    private void OnMurder(MurderEventData data)
    {
        if (Player == null) return;
        if (data.killer == null) return;
        if (Player.PlayerId == data.killer.PlayerId)
        {
            // キルが発生した時に確率判定を行い、キルクールを設定
            bool isSuccess = UnityEngine.Random.Range(0, 100) < _data.successChance;
            float killTime = isSuccess ? _data.successKillCooldown : _data.failureKillCooldown;

            new LateTask(() =>
            {
                ((ExPlayerControl)Player).SetKillTimerUnchecked(killTime, killTime);
            }, 0.017f);
        }
    }

    private void OnWrapUp(WrapUpEventData data)
    {
        if (Player == null) return;
        // 会議終了時にキルクールをリセット（インポスター共通設定に準じる）
        new LateTask(() =>
        {
            float killTime = GameOptionsManager.Instance.CurrentGameOptions.GetFloat(FloatOptionNames.KillCooldown);
            ((ExPlayerControl)Player).SetKillTimerUnchecked(killTime, killTime);
        }, 0.5f);
    }
}