using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AmongUs.GameOptions;

namespace SuperNewRoles.Roles.Role;
public class IntroInfo
{
    public RoleTypes IntroSound;
    public RoleId Role;
    public IntroInfo(RoleId role, RoleTypes introSound = RoleTypes.Crewmate)
    {
        this.Role = role;
        this.IntroSound = introSound;
    }
}