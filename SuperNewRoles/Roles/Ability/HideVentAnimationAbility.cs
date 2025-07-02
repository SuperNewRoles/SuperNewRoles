using System;

namespace SuperNewRoles.Roles.Ability;

public class HideVentAnimationAbility : AbilityBase
{
    private Func<bool> _canHideVentAnimation;
    public HideVentAnimationAbility(Func<bool> canHideVentAnimation)
    {
        _canHideVentAnimation = canHideVentAnimation;
    }

    public bool CanHideVentAnimation()
    {
        return _canHideVentAnimation?.Invoke() ?? false;
    }
}