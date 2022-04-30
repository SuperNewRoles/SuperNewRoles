using System;
using System.Collections.Generic;
using System.Text;
using SuperNewRoles.CustomOption;
using UnityEngine;

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
            RandomColorMode = CustomOption.CustomOption.Create(197, true, CustomOptionType.Generic, "SettingRandomColorMode", false, ModeHandler.ModeSetting);
            HideName = CustomOption.CustomOption.Create(198, true, CustomOptionType.Generic, "RandomColorHideNameSetting", false, RandomColorMode);
            RandomNameColor = CustomOption.CustomOption.Create(199, true, CustomOptionType.Generic, "RandomColorNameColorSetting", true, RandomColorMode);
            RandomColorMeeting = CustomOption.CustomOption.Create(200, true, CustomOptionType.Generic, "RandomColorMeetingSetting", true, RandomColorMode);
        }
    }
}
