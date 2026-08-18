using SuperNewRoles.Events.PCEvents;
using SuperNewRoles.Modules;

namespace SuperNewRoles.Roles.Ability;

public class StuntmanAbility : AbilityBase, IAbilityCount
{
    public StuntmanAbility(int count)
    {
        Count = count;
    }


    public override void AttachToAlls()
    {
        base.AttachToAlls();
        SubscribeWithAbility(TryKillEvent.Instance, OnTryKill);
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
