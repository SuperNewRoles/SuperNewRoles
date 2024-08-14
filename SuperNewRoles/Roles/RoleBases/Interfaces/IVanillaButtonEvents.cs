using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperNewRoles.Roles.RoleBases.Interfaces;

public interface IVanillaButtonEvents
{
    public bool KillButtonDoClick(KillButton button) => true;

    public bool KillButtonCheckClick(KillButton button, PlayerControl target) => true;

    public bool KillButtonSetTarget(KillButton button, PlayerControl target) => true;

    public bool UseButtonDoClick(UseButton button) => true;

    public bool UseButtonSetTarget(UseButton button, IUsable target) => true;

    public bool VentButtonDoClick(VentButton button) => true;

    public bool VentButtonSetTarget(VentButton button, Vent target) => true;
}

