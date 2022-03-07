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
            RandomColorMode = CustomOption.CustomOption.Create(197, CustomOptions.cs(Color.white, "SettingRandomColorMode"), false, ModeHandler.ModeSetting);
            HideName = CustomOption.CustomOption.Create(198, CustomOptions.cs(Color.white, "RandomColorHideNameSetting"), false, RandomColorMode);
            RandomNameColor = CustomOption.CustomOption.Create(199, CustomOptions.cs(Color.white, "RandomColorNameColorSetting"), true, RandomColorMode);
            RandomColorMeeting = CustomOption.CustomOption.Create(200, CustomOptions.cs(Color.white, "RandomColorMeetingSetting"), true, RandomColorMode);
        }
    }
}
