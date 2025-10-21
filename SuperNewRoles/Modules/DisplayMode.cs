using System;
using SuperNewRoles.CustomOptions.Categories;

namespace SuperNewRoles.Modules;
[Flags]
public enum DisplayModeId
{
    None = 0,
    Default = 1 << 0,
    SuperHostRolesOnly = 1 << 1, // 元々の SHR true とは異なる挙動(通常モード 非表示)を目的に使用している。 ( SHRモードに設定された時のみ、「SHR 未実装」と 表記するのに使用中。)
    WCBattleRoyal = 1 << 2,
    BattleRoyal = 1 << 3,
    All = Default | SuperHostRolesOnly | WCBattleRoyal | BattleRoyal
}
public enum ModeId
{
    Default,
    WCBattleRoyal,
    BattleRoyal,
    SuperHostRoles,
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
            case ModeId.SuperHostRoles: // FIXME SHR実装時 想定通りの挙動をしない記述になっている
                return DisplayModeId.SuperHostRolesOnly;
            case ModeId.WCBattleRoyal:
                return DisplayModeId.WCBattleRoyal;
            case ModeId.BattleRoyal:
                return DisplayModeId.BattleRoyal;
            default:
                return DisplayModeId.Default;
        }
    }
}

public static class DisplayCanNotUseSHR // TODO SHR実装時 削除
{
    [CustomOptionSelect("NoSuperHostRoles", typeof(DisplayText), "DisplayText.", parentFieldName: nameof(Categories.ModeOption), displayMode: DisplayModeId.SuperHostRolesOnly, parentActiveValue: ModeId.SuperHostRoles)]
    public static DisplayText CanNotUseSHR;

    public enum DisplayText { CanNotUseSHR }
}
