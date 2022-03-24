using SuperNewRoles.Mode.PlusMode;
using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNewRoles.Mode
{
    enum PlusModeId
    {
        No,
        NotSabotage
    }
    class PlusModeHandler
    {
        public static List<PlusModeId> thisPlusModes;
        public static void ClearAndReload()
        {
            thisPlusModes = new List<PlusModeId>();
            foreach (PlusModeId mode in PlusModeIds)
            {
                if (isMode(mode))
                {
                    thisPlusModes.Add(mode);
                }
            }
        }
        public static List<PlusModeId> PlusModeIds = new List<PlusModeId>()
        {
            PlusModeId.NotSabotage
        };
        public static bool isMode(PlusModeId Modeid)
        {
            switch (Modeid)
            {
                case PlusModeId.NotSabotage:
                    return Options.PlusModeSetting.getBool() && Options.NoSabotageModeSetting.getBool();
            }
            return false;
        }
    }
}
