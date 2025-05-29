using SuperNewRoles.Modules;

namespace SuperNewRoles.CustomOptions.Categories;

public static class Categories
{
    public static CustomOptionCategory PresetSettings;
    public static CustomOptionCategory GeneralSettings;
    public static CustomOptionCategory ModeSettings;
    public static CustomOptionCategory GameSettings;
    public static CustomOptionCategory MapSettings;
    public static CustomOptionCategory MapEditSettings;

    [CustomOptionSelect("ModeOption", typeof(ModeId), "ModeId.", parentFieldName: nameof(ModeSettings))]
    public static ModeId ModeOption;
}
