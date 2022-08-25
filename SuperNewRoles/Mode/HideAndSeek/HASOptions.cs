using SuperNewRoles.CustomOption;

namespace SuperNewRoles.Mode.HideAndSeek
{
    public class HideAndSeekOptions
    {
        public static CustomOption.CustomOption HideAndSeekMode;
        public static CustomOption.CustomOption HASDeathTask;
        public static CustomOption.CustomOption HASUseSabo;
        public static CustomOption.CustomOption HASUseVent;
        public static void Load()
        {
            HideAndSeekMode = CustomOption.CustomOption.Create(475, true, CustomOptionType.Generic, "SettingHideAndSeekMode", false, ModeHandler.ModeSetting);
            HASDeathTask = CustomOption.CustomOption.Create(476, true, CustomOptionType.Generic, "HASDeathTaskSetting", false, HideAndSeekMode);
            HASUseSabo = CustomOption.CustomOption.Create(477, true, CustomOptionType.Generic, "HASUseSaboSetting", false, HideAndSeekMode);
            HASUseVent = CustomOption.CustomOption.Create(478, true, CustomOptionType.Generic, "HASUseVentSetting", false, HideAndSeekMode);
        }
    }
}