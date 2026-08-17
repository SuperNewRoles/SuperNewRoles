using System;
using SuperNewRoles.Roles.Ability;

namespace SuperNewRoles.Ability;

public class KillableAbility : AbilityBase, IPrioritizedAbility
{
    public int Priority { get; }
    private readonly Func<bool?> canKill;
    public bool? CanKill => canKill?.Invoke();
    public KillableAbility(Func<bool?> canKill, int priority = AbilityPriority.Default)
    {
        this.canKill = canKill;
        Priority = priority;
    }
}
