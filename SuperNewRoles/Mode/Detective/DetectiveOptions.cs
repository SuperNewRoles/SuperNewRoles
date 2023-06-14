namespace SuperNewRoles.Mode.Detective;

class DetectiveOptions
{
    public static CustomOption DetectiveMode;
    public static CustomOption IsWinNotCheckDetective;
    public static CustomOption DetectiveIsNotTask;
    public static CustomOption IsNotDetectiveVote;
    public static CustomOption IsNotDetectiveMeetingButton;
    public static void Load()
    {
        DetectiveMode = CustomOption.Create(101700, true, CustomOptionType.Generic, "SettingDetectiveMode", false, ModeHandler.ModeSetting);
        IsWinNotCheckDetective = CustomOption.Create(101701, true, CustomOptionType.Generic, "DetectiveModeIsWinNotCheckSetting", false, DetectiveMode);
        DetectiveIsNotTask = CustomOption.Create(101702, true, CustomOptionType.Generic, "DetectiveModeIsNotTaskSetting", false, DetectiveMode);
        IsNotDetectiveVote = CustomOption.Create(101703, true, CustomOptionType.Generic, "DetectiveModeIsNotDetectiveVoteSetting", false, DetectiveMode);
        IsNotDetectiveMeetingButton = CustomOption.Create(101704, true, CustomOptionType.Generic, "DetectiveModeIsNotDetectiveMeetingButtonSetting", false, DetectiveMode);
    }
}