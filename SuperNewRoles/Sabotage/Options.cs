using SuperNewRoles.CustomOption;

namespace SuperNewRoles.Sabotage
{
    public static class Options
    {
        public static CustomOption.CustomOption SabotageSetting;
        public static CustomOption.CustomOption CognitiveDeficitSetting;
        public static CustomOption.CustomOption CognitiveDeficitOutfitUpdateTimeSetting;
        public static CustomOption.CustomOption CognitiveDeficitReleaseTimeSetting;
        public static CustomOption.CustomOption CognitiveDeficitIsAllEndSabotageSetting;
        public static void Load()
        {
            SabotageSetting = CustomOption.CustomOption.Create(512, false, CustomOptionType.Generic, "SabotageSetting", false, null, isHeader: true);
            CognitiveDeficitSetting = CustomOption.CustomOption.Create(513, false, CustomOptionType.Generic, "SabotageCognitiveDeficitSetting", false, SabotageSetting);
            CognitiveDeficitOutfitUpdateTimeSetting = CustomOption.CustomOption.Create(514, false, CustomOptionType.Generic, "CognitiveDeficitSabotageOutfitUpdateTimeSetting", 3f, 0.5f, 10f, 0.5f, CognitiveDeficitSetting);
            CognitiveDeficitReleaseTimeSetting = CustomOption.CustomOption.Create(515, false, CustomOptionType.Generic, "CognitiveDeficitSabotageReleaseTimeSetting", 3f, 0.5f, 10f, 0.5f, CognitiveDeficitSetting);
            CognitiveDeficitIsAllEndSabotageSetting = CustomOption.CustomOption.Create(516, false, CustomOptionType.Generic, "CognitiveDeficitSabotageIsAllPlayerEndSabotageSetting", true, CognitiveDeficitSetting);
        }
    }
}