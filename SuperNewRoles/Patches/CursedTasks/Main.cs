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
        CursedDivertPowerTask.Data = new();
        CursedDivertPowerTask.SliderOrder = null;

        CursedDressUpTask.IsDisabledPlatform = false;

        CursedFixShowerTask.Data = new();
        CursedStartFansTask.Data = new();
        CursedSampleTask.Data = new();
        CursedShowerTask.Timer = new();
        CursedToiletTask.Count = new();
    }

    public static void IntroFinished()
    {
        if (!IsCursed) return;
        CursedMushroom.SpawnCustomMushroomFungle();
    }

    public static int Num => ModHelpers.GetRandomInt(1) == 1 ? 1024 : 1183;
}
