using SuperNewRoles.Mode;

namespace SuperNewRoles.Patches.CursedTasks;


public class Main
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
    }
}
