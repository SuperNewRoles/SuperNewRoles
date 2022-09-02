using SuperNewRoles.Patch;

namespace SuperNewRoles.Mode.HideAndSeek
{
    public class HideAndSeekOptions
    {
        public static CustomOption HideAndSeekMode;
        public static CustomOption HASDeathTask;
        public static CustomOption HASUseSabo;
        public static CustomOption HASUseVent;
        public static void Load()
        {
            HideAndSeekMode = CustomOption.Create(475, true, CustomOptionType.Generic, "SettingHideAndSeekMode", false, ModeHandler.ModeSetting);
            HASDeathTask = CustomOption.Create(476, true, CustomOptionType.Generic, "HASDeathTaskSetting", false, HideAndSeekMode);
            HASUseSabo = CustomOption.Create(477, true, CustomOptionType.Generic, "HASUseSaboSetting", false, HideAndSeekMode);
            HASUseVent = CustomOption.Create(478, true, CustomOptionType.Generic, "HASUseVentSetting", false, HideAndSeekMode);
        }
    }
}