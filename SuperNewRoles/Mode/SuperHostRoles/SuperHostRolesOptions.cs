using static SuperNewRoles.Modules.CustomOption;

namespace SuperNewRoles.Mode.SuperHostRoles;

class SuperHostRolesOptions
{
    public static CustomOption SettingSuperHostRolesMode;

    public static CustomOption SendRoleDescriptionOption;
    public static CustomOption SendYourRoleFirstTurnSetting;
    public static CustomOption SendYourRoleAllTurnSetting;

    public static void Load()
    {
        SettingSuperHostRolesMode = Create(105000, true, CustomOptionType.Generic, "SettingSuperHostRolesMode", false, ModeHandler.ModeSetting);
        SendRoleDescriptionOption = Create(105001, true, CustomOptionType.Generic, "SendRoleDescriptionOption", false, SettingSuperHostRolesMode);
        SendYourRoleFirstTurnSetting = Create(105002, true, CustomOptionType.Generic, "SendYourRoleFirstTurn", true, SendRoleDescriptionOption);
        SendYourRoleAllTurnSetting = Create(105003, true, CustomOptionType.Generic, "SendYourRoleAllTurn", false, SendRoleDescriptionOption);
    }

    public static class SettingClass
    {
        public static bool IsSendYourRoleFirstTurn => !(SettingSuperHostRolesMode.GetBool() && SendRoleDescriptionOption.GetBool() && (!(SendYourRoleFirstTurnSetting.GetBool() || SendYourRoleAllTurnSetting.GetBool())));
        public static bool IsSendYourRoleAllTurn => SettingSuperHostRolesMode.GetBool() && SendRoleDescriptionOption.GetBool() && SendYourRoleAllTurnSetting.GetBool();

        public static void ClearAndReload()
        { }
    }
}