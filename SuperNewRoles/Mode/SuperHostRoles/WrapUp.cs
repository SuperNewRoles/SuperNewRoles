using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNewRoles.Mode.SuperHostRoles
{
    class WrapUpClass
    {
        public static void WrapUp(GameData.PlayerInfo exiled)
        {
            if (!AmongUsClient.Instance.AmHost) return;
            Roles.BestFalseCharge.WrapUp();
            if (exiled == null) return;
            Roles.Jester.WrapUp(exiled);
            Roles.Nekomata.WrapUp(exiled);
        }
    }
}
