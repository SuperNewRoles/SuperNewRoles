using SuperNewRoles.Mode;

namespace SuperNewRoles.Patches.CursedTasks;

public static class Main
{
    private const int OptionId = 1247;
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
    }

    public static int Random(int min = 1, int max = 10) => UnityEngine.Random.RandomRange(min, max + 1);

    // 1024 : 月城さんが決めた数字 , 1183 : ポケモン全国図鑑のリージョンホーム含めたポケモンの数(2023年03月02日現在)
    public static int Num { get { return Random(1, 2) == 1 ? 1024 : 1183; } }
}
