using SuperNewRoles.CustomOption;

namespace SuperNewRoles.Mode.RandomColor
{
    public class RandomColorOptions
    {
        public static CustomOption.CustomOption RandomColorMode;
        public static CustomOption.CustomOption HideName;
        public static CustomOption.CustomOption RandomNameColor;
        public static CustomOption.CustomOption RandomColorMeeting;
        public static void Load()
        {
            RandomColorMode = CustomOption.CustomOption.Create(497, true, CustomOptionType.Generic, "SettingRandomColorMode", false, ModeHandler.ModeSetting);
            HideName = CustomOption.CustomOption.Create(498, true, CustomOptionType.Generic, "RandomColorHideNameSetting", false, RandomColorMode);
            RandomNameColor = CustomOption.CustomOption.Create(499, true, CustomOptionType.Generic, "RandomColorNameColorSetting", true, RandomColorMode);
            RandomColorMeeting = CustomOption.CustomOption.Create(500, true, CustomOptionType.Generic, "RandomColorMeetingSetting", true, RandomColorMode);
        }
    }
}