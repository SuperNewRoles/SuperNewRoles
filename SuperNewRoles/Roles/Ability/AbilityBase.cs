using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperNewRoles.Roles.Ability;

public abstract class AbilityBase
{
    public ulong AbilityId { get; protected set; }
    public PlayerControl Player { get; private set; }

    public int Count { get; set; }
    public bool HasCount => Count > 0;

    public void Attach(PlayerControl player, ulong abilityId)
    {
        Player = player;
        AbilityId = abilityId;
        if (player == PlayerControl.LocalPlayer)
            AttachToLocalPlayer();
        else
            AttachToOthers();
    }

    public abstract void AttachToLocalPlayer();

    public virtual void AttachToOthers() { }

    public virtual void Detach()
    {
        Player = null;
    }
}
