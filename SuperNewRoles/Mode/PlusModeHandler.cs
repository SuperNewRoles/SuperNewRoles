using System.Collections.Generic;
using SuperNewRoles.Mode.PlusMode;

namespace SuperNewRoles.Mode
{
    enum PlusModeId
    {
        No,
        NotSabotage,
        NotTaskWin
    }
    class PlusModeHandler
    {
        public static List<PlusModeId> thisPlusModes;
        public static void ClearAndReload()
        {
            thisPlusModes = new List<PlusModeId>();
            foreach (PlusModeId mode in PlusModeIds)
            {
                if (IsMode(mode))
                {
                    thisPlusModes.Add(mode);
                }
            }
        }
        public static List<PlusModeId> PlusModeIds = new()
        {
            PlusModeId.NotSabotage,
            PlusModeId.NotTaskWin
        };
        public static bool IsMode(PlusModeId Modeid)
        {
            return Modeid switch
            {
                PlusModeId.NotSabotage => Options.PlusModeSetting.GetBool() && Options.NoSabotageModeSetting.GetBool(),
                PlusModeId.NotTaskWin => Options.PlusModeSetting.GetBool() && Options.NoTaskWinModeSetting.GetBool(),
                _ => false,
            };
        }
    }
}