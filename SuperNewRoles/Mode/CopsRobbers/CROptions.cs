using SuperNewRoles.CustomOption;

namespace SuperNewRoles.Mode.CopsRobbers
{
    public class CopsRobbersOptions
    {
        public static CustomOption.CustomOption CopsRobbersMode;
        public static CustomOption.CustomOption CRHideName;
        public static void Load()
        {
            CopsRobbersMode = CustomOption.CustomOption.Create(968, true, CustomOptionType.Generic, "CopsRobbersModeName", false, ModeHandler.ModeSetting);
            CRHideName = CustomOption.CustomOption.Create(969, true, CustomOptionType.Generic, "CRHideNameSetting", true, CopsRobbersMode);
        }
    }
}