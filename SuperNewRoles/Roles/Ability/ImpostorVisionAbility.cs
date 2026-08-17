using System;
using UnityEngine;
using SuperNewRoles.Modules;

namespace SuperNewRoles.Roles.Ability;

public class ImpostorVisionAbility : AbilityBase, IPrioritizedAbility
{
    public int Priority { get; }
    public Func<bool?> HasImpostorVision { get; }

    public ImpostorVisionAbility(Func<bool?> hasImpostorVision, int priority = AbilityPriority.Default)
    {
        HasImpostorVision = hasImpostorVision;
        Priority = priority;
    }

    public override void AttachToLocalPlayer()
    {
    }
}
