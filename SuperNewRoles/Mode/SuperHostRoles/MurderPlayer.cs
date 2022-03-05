using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNewRoles.Mode.SuperHostRoles
{
    class MurderPlayer
    {
        public static void Postfix(PlayerControl __instance,PlayerControl target)
        {
            Roles.Bait.MurderPostfix(__instance,target);
        }
    }
}
