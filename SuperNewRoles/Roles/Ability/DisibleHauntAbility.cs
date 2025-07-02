using System;
using SuperNewRoles.Events;
using SuperNewRoles.Modules;
using SuperNewRoles.Modules.Events.Bases;

namespace SuperNewRoles.Roles.Ability;

public class DisibleHauntAbility : AbilityBase
{
    private Func<bool> _isDisible;
    public DisibleHauntAbility(Func<bool> isDisible)
    {
        _isDisible = isDisible;
    }
    private EventListener _hudUpdateEvent;
    public override void AttachToLocalPlayer()
    {
        base.AttachToLocalPlayer();
        _hudUpdateEvent = HudUpdateEvent.Instance.AddListener(OnHudUpdate);
    }
    public override void DetachToLocalPlayer()
    {
        base.DetachToLocalPlayer();
        _hudUpdateEvent?.RemoveListener();
    }

    private void OnHudUpdate()
    {
        if (!_isDisible()) return;
        if (!ExPlayerControl.LocalPlayer.Data.Role.IsDead) return;
        FastDestroyableSingleton<HudManager>.Instance.AbilityButton.gameObject.SetActive(false);
    }

}
