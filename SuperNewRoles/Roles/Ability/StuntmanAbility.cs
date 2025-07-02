using SuperNewRoles.Events.PCEvents;
using SuperNewRoles.Modules;
using SuperNewRoles.Modules.Events.Bases;

namespace SuperNewRoles.Roles.Ability;

public class StuntmanAbility : AbilityBase, IAbilityCount
{
    public StuntmanAbility(int count)
    {
        Count = count;
    }

    private EventListener<TryKillEventData> _tryKillEvent;

    public override void AttachToAlls()
    {
        base.AttachToAlls();
        _tryKillEvent = TryKillEvent.Instance.AddListener(OnTryKill);
    }
    public override void DetachToAlls()
    {
        base.DetachToAlls();
        _tryKillEvent?.RemoveListener();
    }

    private void OnTryKill(TryKillEventData data)
    {
        if (data.RefTarget != Player) return;
        if (!HasCount) return;
        Count--;
        data.RefSuccess = false;
        if (data.Killer.AmOwner || ExPlayerControl.LocalPlayer.IsDead())
            data.RefTarget.Player.ShowFailedMurder();
        if (data.Killer.AmOwner)
            data.Killer.ResetKillCooldown();
    }
}