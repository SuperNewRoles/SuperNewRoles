using System;
using SuperNewRoles.Events;
using SuperNewRoles.Modules;

namespace SuperNewRoles.Roles.Ability;

public class DisibleHauntAbility : AbilityBase
{
    private Func<bool> _isDisible;
    public DisibleHauntAbility(Func<bool> isDisible)
    {
        _isDisible = isDisible;
    }
    public override void AttachToLocalPlayer()
    {
        base.AttachToLocalPlayer();
        SubscribeWithAbility(HudUpdateEvent.Instance, OnHudUpdate);
    }
    private void OnHudUpdate()
    {
        if (!_isDisible()) return;
        if (!ExPlayerControl.LocalPlayer.Data.Role.IsDead) return;
        FastDestroyableSingleton<HudManager>.Instance.AbilityButton.gameObject.SetActive(false);
    }

}
