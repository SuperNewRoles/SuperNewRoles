using System;
using SuperNewRoles.Events;
using SuperNewRoles.Events.PCEvents;
using SuperNewRoles.Modules;

namespace SuperNewRoles.Roles.Ability;

public class ChangeKillTimerAbility : AbilityBase
{
    public Func<float> KillTimerGetter { get; }

    public ChangeKillTimerAbility(Func<float> killTimerGetter)
    {
        KillTimerGetter = killTimerGetter;
    }

    public override void AttachToLocalPlayer()
    {
        SubscribeWithAbility(MurderEvent.Instance, OnMurder);
        SubscribeWithAbility(WrapUpEvent.Instance, OnWrapUp);
    }

    private void OnMurder(MurderEventData data)
    {
        if (Player == null) return;
        if (data.killer == null) return;
        if (Player.PlayerId == data.killer.PlayerId)
        {
            // キルが発生した時にKillTimerを設定
            float killTime = KillTimerGetter();
            new LateTask(() =>
            {
                ((ExPlayerControl)Player).SetKillTimerUnchecked(killTime, killTime);
            }, 0.05f);
        }
    }

    private void OnWrapUp(WrapUpEventData data)
    {
        if (Player == null) return;
        new LateTask(() =>
        {
            float killTime = KillTimerGetter();
            ((ExPlayerControl)Player).SetKillTimerUnchecked(killTime, killTime);
        }, 0.5f);
    }
}
