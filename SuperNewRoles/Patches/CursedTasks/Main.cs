using System.Collections.Generic;
using System.Linq;
using SuperNewRoles.Mode;
using UnityEngine;

namespace SuperNewRoles.Patches.CursedTasks;

public static class Main
{
    private const int OptionId = 1271;
    public static CustomOption CursedTask;
    public static void SetupCustomOptions()
    {
        CursedTask = CustomOption.Create(OptionId, false, CustomOptionType.Generic, "CursedTaskOption", false, null, isHeader: true);
    }

    public static bool IsCursed;
    public static void ClearAndReload()
    {
        IsCursed = CursedTask.GetBool() && ModeHandler.IsMode(ModeId.Default);

        CursedSampleTask.Data = new();

        CursedDivertPowerTask.Data = new();
        CursedDivertPowerTask.Change = false;

        CursedShowerTask.Timer = new();

        CursedFixShowerTask.Data = new();
    }

    public static List<int> Range(int start, int count) => Enumerable.Range(start, count + 1).ToList();

    // 1024 : 月城さんが決めた数字 , 1183 : ポケモン全国図鑑のリージョンホーム含めたポケモンの数(2023年03月02日現在)
    public static int Num { get { return Random.RandomRange(0, 1 + 1) == 1 ? 1024 : 1183; } }
}
