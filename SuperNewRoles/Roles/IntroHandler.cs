using System;
using System.Collections.Generic;
using System.Text;
using static SuperNewRoles.Roles.EvilGambler;

namespace SuperNewRoles.Roles
{
    class IntroHandler
    {
        public static void Handler() {
            EvilGamblerMurder.temp = PlayerControl.GameOptions.KillCooldown;
        }
    }
}
