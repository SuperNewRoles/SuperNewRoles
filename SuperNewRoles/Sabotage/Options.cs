using SuperNewRoles.Patches;

namespace SuperNewRoles.Sabotage
{
    public static class Options
    {
        public static CustomOption SabotageSetting;
        public static CustomOption CognitiveDeficitSetting;
        public static CustomOption CognitiveDeficitOutfitUpdateTimeSetting;
        public static CustomOption CognitiveDeficitReleaseTimeSetting;
        public static CustomOption CognitiveDeficitIsAllEndSabotageSetting;
        public static void Load()
        {
            SabotageSetting = CustomOption.Create(512, false, CustomOptionType.Generic, "SabotageSetting", false, null, isHeader: true);
            CognitiveDeficitSetting = CustomOption.Create(513, false, CustomOptionType.Generic, "SabotageCognitiveDeficitSetting", false, SabotageSetting);
            CognitiveDeficitOutfitUpdateTimeSetting = CustomOption.Create(514, false, CustomOptionType.Generic, "CognitiveDeficitSabotageOutfitUpdateTimeSetting", 3f, 0.5f, 10f, 0.5f, CognitiveDeficitSetting);
            CognitiveDeficitReleaseTimeSetting = CustomOption.Create(515, false, CustomOptionType.Generic, "CognitiveDeficitSabotageReleaseTimeSetting", 3f, 0.5f, 10f, 0.5f, CognitiveDeficitSetting);
            CognitiveDeficitIsAllEndSabotageSetting = CustomOption.Create(516, false, CustomOptionType.Generic, "CognitiveDeficitSabotageIsAllPlayerEndSabotageSetting", true, CognitiveDeficitSetting);
        }
    }
}