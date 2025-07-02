using System;
using SuperNewRoles.Roles.Ability;

namespace SuperNewRoles.Ability;

public class KillableAbility : AbilityBase
{
    private Func<bool> canKill;
    public bool CanKill => canKill?.Invoke() ?? false;
    public KillableAbility(Func<bool> canKill)
    {
        this.canKill = canKill;
    }
}