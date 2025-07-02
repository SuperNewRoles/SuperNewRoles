using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SuperNewRoles.Modules;

namespace SuperNewRoles.Roles.Ability;

public abstract class AbilityBase
{
    public ulong AbilityId { get; protected set; }
    public ExPlayerControl Player => Parent.Player;
    public AbilityParentBase Parent { get; private set; }

    public int Count { get; set; }
    public bool HasCount => Count > 0;

    public virtual void Attach(PlayerControl player, ulong abilityId, AbilityParentBase parent)
    {
        Parent = parent;
        AbilityId = abilityId;
        if (player == PlayerControl.LocalPlayer)
            AttachToLocalPlayer();
        else
            AttachToOthers();
        AttachToAlls();
    }

    public virtual void AttachToLocalPlayer() { }

    public virtual void AttachToOthers() { }

    public virtual void AttachToAlls() { }

    public virtual void Detach()
    {
        if (Player == PlayerControl.LocalPlayer)
            DetachToLocalPlayer();
        else
            DetachToOthers();
        DetachToAlls();
    }
    public virtual void DetachToLocalPlayer() { }
    public virtual void DetachToOthers() { }
    public virtual void DetachToAlls() { }
}
