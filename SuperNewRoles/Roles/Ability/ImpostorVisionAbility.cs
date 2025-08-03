using System;
using UnityEngine;
using SuperNewRoles.Modules;

namespace SuperNewRoles.Roles.Ability;

public class ImpostorVisionAbility : AbilityBase
{
    public Func<bool> HasImpostorVision { get; }

    public ImpostorVisionAbility(Func<bool> hasImpostorVision)
    {
        HasImpostorVision = hasImpostorVision;
    }

    public override void AttachToLocalPlayer()
    {
    }
}