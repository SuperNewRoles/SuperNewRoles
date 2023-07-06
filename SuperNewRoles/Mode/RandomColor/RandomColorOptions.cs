namespace SuperNewRoles.Mode.RandomColor;

public class RandomColorOptions
{
    public static CustomOption RandomColorMode;
    public static CustomOption HideName;
    public static CustomOption RandomNameColor;
    public static CustomOption RandomColorMeeting;
    public static void Load()
    {
        RandomColorMode = CustomOption.Create(101600, true, CustomOptionType.Generic, "SettingRandomColorMode", false, ModeHandler.ModeSetting);
        HideName = CustomOption.Create(101601, true, CustomOptionType.Generic, "RandomColorHideNameSetting", false, RandomColorMode);
        RandomNameColor = CustomOption.Create(101602, true, CustomOptionType.Generic, "RandomColorNameColorSetting", true, RandomColorMode);
        RandomColorMeeting = CustomOption.Create(101603, true, CustomOptionType.Generic, "RandomColorMeetingSetting", true, RandomColorMode);
    }
}