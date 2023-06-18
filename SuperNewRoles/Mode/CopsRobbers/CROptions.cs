namespace SuperNewRoles.Mode.CopsRobbers;

public class CopsRobbersOptions
{
    public static CustomOption CopsRobbersMode;
    public static CustomOption CRHideName;
    public static void Load()
    {
        CopsRobbersMode = CustomOption.Create(101800, true, CustomOptionType.Generic, "CopsRobbersModeName", false, ModeHandler.ModeSetting);
        CRHideName = CustomOption.Create(101801, true, CustomOptionType.Generic, "CRHideNameSetting", true, CopsRobbersMode);
    }
}