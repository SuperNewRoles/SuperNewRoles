using SuperNewRoles.Modules;

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
}
