using System;
using SuperNewRoles.CustomOptions.Categories;

namespace SuperNewRoles.Modules;
[Flags]
public enum DisplayModeId
{
    Default = 1 << 0,
    BattleRoyal = 1 << 1,
    All = Default | BattleRoyal
}
public enum ModeId
{
    Default,
    //BattleRoyal,
}

public static class DisplayMode
{
    public static bool HasMode(DisplayModeId current, DisplayModeId target)
    {
        return target.HasFlag(current);
    }

    public static DisplayModeId GetCurrentMode()
    {
        switch (Categories.ModeOption)
        {
            case ModeId.Default:
                return DisplayModeId.Default;
            //case ModeId.BattleRoyal:
            //    return DisplayModeId.BattleRoyal;
            default:
                return DisplayModeId.Default;
        }
    }
}