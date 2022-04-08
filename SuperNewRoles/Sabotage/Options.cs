using System;
using System.Collections.Generic;
using System.Text;

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
            SabotageSetting = CustomOption.CustomOption.Create(296, "SabotageSetting", false, null, isHeader: true);
            //
            CognitiveDeficitSetting = CustomOption.CustomOption.Create(297, "SabotageCognitiveDeficitSetting", false, SabotageSetting);
            CognitiveDeficitOutfitUpdateTimeSetting = CustomOption.CustomOption.Create(298, "CognitiveDeficitSabotageOutfitUpdateTimeSetting", 3f,0.5f,10f,0.5f, CognitiveDeficitSetting);
            CognitiveDeficitReleaseTimeSetting = CustomOption.CustomOption.Create(299, "CognitiveDeficitSabotageReleaseTimeSetting", 3f, 0.5f, 10f, 0.5f, CognitiveDeficitSetting);
            CognitiveDeficitIsAllEndSabotageSetting = CustomOption.CustomOption.Create(300, "CognitiveDeficitSabotageIsAllPlayerEndSabotageSetting", true, CognitiveDeficitSetting);            //
        }
    }
}
