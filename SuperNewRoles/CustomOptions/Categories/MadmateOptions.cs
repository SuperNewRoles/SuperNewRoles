using SuperNewRoles.Modules;

namespace SuperNewRoles.CustomOptions.Categories;

public static class MadmateOptions
{
    [Modifier]
    public static CustomOptionCategory MadmateSettings;
    [CustomOptionBool("MadmateCannotFixComms", true, parentFieldName: nameof(MadmateSettings))]
    public static bool MadmateCannotFixComms;
    [CustomOptionBool("MadmateCannotFixElectrical", true, parentFieldName: nameof(MadmateSettings))]
    public static bool MadmateCannotFixElectrical;
    [CustomOptionBool("MadmateCannotFixReactor", true, parentFieldName: nameof(MadmateSettings))]
    public static bool MadmateCannotFixReactor;
}