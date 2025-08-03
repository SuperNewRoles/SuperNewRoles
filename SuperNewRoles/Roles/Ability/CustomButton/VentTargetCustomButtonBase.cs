using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SuperNewRoles.Roles.Ability.CustomButton;

public abstract class VentTargetCustomButtonBase : CustomButtonBase
{
    public Vent Target { get; protected set; }
    public virtual bool ShowOutline { get; protected set; } = true;
    public abstract Color32 OutlineColor { get; }
    private Vent _lastShowTarget;
    public virtual IEnumerable<Vent> UntargetableVents { get; } = null;
    public virtual PlayerControl TargetingPlayer => PlayerControl.LocalPlayer;
    public virtual Func<Vent, bool> IsTargetable { get; } = null;
    public bool TargetIsExist => Target != null;
    public virtual bool IgnoreWalls => false;

    public override void OnUpdate()
    {
        base.OnUpdate();
        Target = SetTarget(untargetableVents: UntargetableVents, targetingPlayer: TargetingPlayer, isTargetable: IsTargetable, ignoreWalls: IgnoreWalls);
        if (ShowOutline && _lastShowTarget != Target)
        {
            if (_lastShowTarget != null) SetOutline(_lastShowTarget, false, OutlineColor);
            if (Target != null) SetOutline(Target, true, OutlineColor);
            _lastShowTarget = Target;
        }
    }
    
    private static void SetOutline(Vent vent, bool show, Color32 color)
    {
        var rend = vent.myRend;
        if (rend == null) return;
        rend.material.SetFloat("_Outline", show ? 1f : 0f);
        if (show) rend.material.SetColor("_OutlineColor", color);
    }
    
    public static Vent SetTarget(IEnumerable<Vent> untargetableVents = null, PlayerControl targetingPlayer = null, Func<Vent, bool> isTargetable = null, bool ignoreWalls = false)
    {
        if (!ShipStatus.Instance) return null;
        if (targetingPlayer == null) targetingPlayer = PlayerControl.LocalPlayer;
        if (targetingPlayer.inVent) return null;
        Vector2 center = targetingPlayer.Collider.bounds.center;
        foreach (Vent vent in ShipStatus.Instance.AllVents)
        {
            if (untargetableVents != null && untargetableVents.Any(x => x == vent)) continue;
            Vector3 position = vent.transform.position;
            float num = Vector2.Distance(center, position);
            if (num > vent.UsableDistance) continue;
            if (isTargetable != null && !isTargetable.Invoke(vent)) continue;
            if (!PhysicsHelpers.AnythingBetween(PlayerControl.LocalPlayer.Collider, center, position, Constants.ShipOnlyMask, useTriggers: false))
                return vent;
        }
        return null;
    }
}
