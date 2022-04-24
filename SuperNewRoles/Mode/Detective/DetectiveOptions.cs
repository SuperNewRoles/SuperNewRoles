using SuperNewRoles.CustomOption;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SuperNewRoles.Mode.Detective
{
    class DetectiveOptions
    {
        public static CustomOption.CustomOption DetectiveMode;
        public static CustomOption.CustomOption IsWinNotCheckDetective;
        public static CustomOption.CustomOption DetectiveIsNotTask;
        public static CustomOption.CustomOption IsNotDetectiveVote;
        public static CustomOption.CustomOption IsNotDetectiveMeetingButton;
        public static void Load()
        {
            DetectiveMode = CustomOption.CustomOption.Create(205, CustomOptions.cs(Color.white, "SettingDetectiveMode"), false, ModeHandler.ModeSetting);
            IsWinNotCheckDetective = CustomOption.CustomOption.Create(206, CustomOptions.cs(Color.white, "DetectiveModeIsWinNotCheckSetting"), false, DetectiveMode);
            DetectiveIsNotTask = CustomOption.CustomOption.Create(207, CustomOptions.cs(Color.white, "DetectiveModeIsNotTaskSetting"), false, DetectiveMode);
            IsNotDetectiveVote = CustomOption.CustomOption.Create(208, CustomOptions.cs(Color.white, "DetectiveModeIsNotDetectiveVoteSetting"), false, DetectiveMode);
            IsNotDetectiveMeetingButton = CustomOption.CustomOption.Create(209, CustomOptions.cs(Color.white, "DetectiveModeIsNotDetectiveMeetingButtonSetting"), false, DetectiveMode);
        }
    }
}
