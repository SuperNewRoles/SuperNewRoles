using System.Collections.Generic;
using SuperNewRoles;
using SuperNewRoles.CustomOptions.Categories;
using SuperNewRoles.Mode;
using SuperNewRoles.Modules;

namespace SuperNewRoles.Patches.CursedTasks;

public static class Main
{
    public static bool IsCursed => GameSettingOptions.CursedTaskOption && ModeManager.IsMode(ModeId.Default);

    public static void ClearAndReload()
    {
        CursedDivertPowerTask.Data = new Dictionary<uint, CursedDivertPowerTask.CursedDivertPower>();
        CursedDivertPowerTask.SliderOrder = null;

        CursedDressUpTask.IsDisabledPlatform = false;

        CursedFixShowerTask.Data = new Dictionary<uint, CursedFixShowerTask.CursedFixShower>();
        CursedStartFansTask.Data = new Dictionary<uint, byte[]>();
        CursedSampleTask.Data = new Dictionary<uint, CursedSampleTask.CursedSample>();
        CursedShowerTask.Timer = new Dictionary<uint, float>();
        CursedToiletTask.Count = new Dictionary<uint, int>();
    }

    public static int Num => ModHelpers.GetRandomInt(1) == 1 ? 1024 : 1183;
}
