using System.Collections.Generic;

namespace SuperNewRoles.Mode.NotImpostorCheck
{
    class main
    {
        public static List<int> Impostors;
        public static void ClearAndReload()
        {
            if (AmongUsClient.Instance.AmHost)
            {
                Impostors = new();
            }
        }
    }
}