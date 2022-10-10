using SuperNewRoles.Patches;

namespace SuperNewRoles.Mode.Detective
{
    class DetectiveOptions
    {
        public static CustomOption DetectiveMode;
        public static CustomOption IsWinNotCheckDetective;
        public static CustomOption DetectiveIsNotTask;
        public static CustomOption IsNotDetectiveVote;
        public static CustomOption IsNotDetectiveMeetingButton;
        public static void Load()
        {
            DetectiveMode = CustomOption.Create(501, true, CustomOptionType.Generic, "SettingDetectiveMode", false, ModeHandler.ModeSetting);
            IsWinNotCheckDetective = CustomOption.Create(502, true, CustomOptionType.Generic, "DetectiveModeIsWinNotCheckSetting", false, DetectiveMode);
            DetectiveIsNotTask = CustomOption.Create(503, true, CustomOptionType.Generic, "DetectiveModeIsNotTaskSetting", false, DetectiveMode);
            IsNotDetectiveVote = CustomOption.Create(504, true, CustomOptionType.Generic, "DetectiveModeIsNotDetectiveVoteSetting", false, DetectiveMode);
            IsNotDetectiveMeetingButton = CustomOption.Create(505, true, CustomOptionType.Generic, "DetectiveModeIsNotDetectiveMeetingButtonSetting", false, DetectiveMode);
        }
    }
}