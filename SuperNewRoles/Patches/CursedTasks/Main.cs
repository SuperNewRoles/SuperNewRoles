using SuperNewRoles.Mode;
using UnityEngine;

namespace SuperNewRoles.Patches.CursedTasks;

public static class Main
{
    private const int OptionId = 104500;
    public static CustomOption CursedTask;
    public static void SetupCustomOptions()
    {
        CursedTask = CustomOption.Create(OptionId, false, CustomOptionType.Generic, "CursedTaskOption", false, null, isHeader: true);
    }

    public static bool IsCursed;
    public static void ClearAndReload()
    {
        IsCursed = CursedTask.GetBool() && ModeHandler.IsMode(ModeId.Default);

        CursedDivertPowerTask.Data = new();

        CursedDressUpTask.IsDisabledPlatform = false;

        CursedFixShowerTask.Data = new();

        CursedStartFansTask.Data = new();

        CursedSampleTask.Data = new();

        CursedShowerTask.Timer = new();

        CursedToiletTask.Count = new();
    }

    // 1024 : 月城さんが決めた数字 , 1183 : ポケモン全国図鑑のリージョンホーム含めたポケモンの数(2023年03月02日現在)
    public static int Num { get { return Random.RandomRange(0, 1 + 1) == 1 ? 1024 : 1183; } }
}