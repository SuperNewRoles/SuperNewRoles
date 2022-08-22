using SuperNewRoles.CustomOption;

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
            DetectiveMode = CustomOption.CustomOption.Create(501, true, CustomOptionType.Generic, "SettingDetectiveMode", false, ModeHandler.ModeSetting);
            IsWinNotCheckDetective = CustomOption.CustomOption.Create(502, true, CustomOptionType.Generic, "DetectiveModeIsWinNotCheckSetting", false, DetectiveMode);
            DetectiveIsNotTask = CustomOption.CustomOption.Create(503, true, CustomOptionType.Generic, "DetectiveModeIsNotTaskSetting", false, DetectiveMode);
            IsNotDetectiveVote = CustomOption.CustomOption.Create(504, true, CustomOptionType.Generic, "DetectiveModeIsNotDetectiveVoteSetting", false, DetectiveMode);
            IsNotDetectiveMeetingButton = CustomOption.CustomOption.Create(505, true, CustomOptionType.Generic, "DetectiveModeIsNotDetectiveMeetingButtonSetting", false, DetectiveMode);
        }
    }
}