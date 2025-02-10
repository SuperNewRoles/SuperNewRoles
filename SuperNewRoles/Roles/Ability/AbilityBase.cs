using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperNewRoles.Roles.Ability;

public abstract class AbilityBase
{
    //Discordの通り
    public PlayerControl Player { get; private set; }

    public void Attach(PlayerControl player)
    {
        Player = player;
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
