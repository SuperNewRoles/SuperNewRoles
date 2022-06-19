using System;
using System.Collections.Generic;
using System.Text;
using static SuperNewRoles.EndGame.CheckGameEndPatch;

namespace SuperNewRoles.Mode.NotImpostorCheck
{
    class main
    {
        public static List<int> Impostors;
        public static void ClearAndReload()
        {
            if (AmongUsClient.Instance.AmHost)
            {
                Impostors = new List<int>();
            }
        }
    }
}
