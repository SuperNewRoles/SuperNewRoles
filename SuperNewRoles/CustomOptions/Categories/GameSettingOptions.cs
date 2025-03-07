using SuperNewRoles.Modules;

namespace SuperNewRoles.CustomOptions.Categories;

public static class GameSettingOptions
{
    [CustomOptionBool("CannotTaskTrigger", false, parentFieldName: nameof(Categories.GameSettings))]
    public static bool CannotTaskTrigger;
    // |:========== ベントアニメーション有効化の設定 ==========:|
    [CustomOptionBool("VentAnimationPlaySetting", true, parentFieldName: nameof(Categories.GameSettings))]
    public static bool VentAnimationPlaySetting;
}
