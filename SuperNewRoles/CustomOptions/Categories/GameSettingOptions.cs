using SuperNewRoles.Modules;
using SuperNewRoles.CustomOptions;

namespace SuperNewRoles.CustomOptions.Categories;

public static class GameSettingOptions
{
    [CustomOptionBool("CannotTaskTrigger", false, parentFieldName: nameof(Categories.GameSettings))]
    public static bool CannotTaskTrigger;
    // |:========== ベントアニメーション有効化の設定 ==========:|
    [CustomOptionBool("VentAnimationPlaySetting", true, parentFieldName: nameof(Categories.GameSettings))]
    public static bool VentAnimationPlaySetting;
    [CustomOptionBool("SyncSpawn", false, parentFieldName: nameof(Categories.GameSettings))]
    public static bool SyncSpawn;
    [CustomOptionBool("DisableTaskWin", false, parentFieldName: nameof(Categories.GameSettings))]
    public static bool DisableTaskWin;
    [CustomOptionBool("DisableHijackTaskWin", false, parentFieldName: nameof(Categories.GameSettings))]
    public static bool DisableHijackTaskWin;
    [CustomOptionBool("HideGhostRoles", false, parentFieldName: nameof(Categories.GameSettings))]
    public static bool HideGhostRoles;
    [CustomOptionSelect("GhostVoteDisplay", typeof(GhostVoteDisplayType), "GhostVoteDisplayType.", parentFieldName: nameof(Categories.GameSettings), defaultValue: GhostVoteDisplayType.Vanilla)]
    public static GhostVoteDisplayType GhostVoteDisplay;
}

public enum GhostVoteDisplayType
{
    Vanilla,
    Hide,
    Show,
}
