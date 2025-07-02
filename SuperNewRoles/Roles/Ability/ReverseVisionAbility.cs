using System;

namespace SuperNewRoles.Roles.Ability;

public class ReverseVisionAbility : AbilityBase
{
    public Func<bool> HasReverseVision { get; }
    public Func<bool> IsImpostorVision { get; }

    public ReverseVisionAbility(Func<bool> hasReverseVision, Func<bool> IsImpostorVision)
    {
        this.HasReverseVision = hasReverseVision;
        this.IsImpostorVision = IsImpostorVision;
    }
}