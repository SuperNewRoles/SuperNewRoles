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
        public static CustomOption.CustomOption BlizzardSetting;
        public static CustomOption.CustomOption BlizzardSlowSpeedmagnificationSetting;
        public static CustomOption.CustomOption BlizzardskeldDurationSetting;
        public static CustomOption.CustomOption BlizzardmiraDurationSetting;
        public static CustomOption.CustomOption BlizzardpolusDurationSetting;
        public static CustomOption.CustomOption BlizzardairshipDurationSetting;
        public static CustomOption.CustomOption skeldReactorDuration;
        public static CustomOption.CustomOption miraReactorDuration;
        public static CustomOption.CustomOption polusReactorDuration;
        public static CustomOption.CustomOption airshipReactorDuration;
        public static void Load()
        {
            SabotageSetting = CustomOption.CustomOption.Create(296, "SabotageSetting", false, null, isHeader: true);
            //
            CognitiveDeficitSetting = CustomOption.CustomOption.Create(297, "SabotageCognitiveDeficitSetting", false, SabotageSetting);
            CognitiveDeficitOutfitUpdateTimeSetting = CustomOption.CustomOption.Create(298, "CognitiveDeficitSabotageOutfitUpdateTimeSetting", 3f,0.5f,10f,0.5f, CognitiveDeficitSetting);
            CognitiveDeficitReleaseTimeSetting = CustomOption.CustomOption.Create(299, "CognitiveDeficitSabotageReleaseTimeSetting", 3f, 0.5f, 10f, 0.5f, CognitiveDeficitSetting);
            CognitiveDeficitIsAllEndSabotageSetting = CustomOption.CustomOption.Create(300, "CognitiveDeficitSabotageIsAllPlayerEndSabotageSetting", true, CognitiveDeficitSetting);            //
            BlizzardSetting = CustomOption.CustomOption.Create(297, "SabotageBlizzardSetting", false, SabotageSetting);
            BlizzardSlowSpeedmagnificationSetting = CustomOption.CustomOption.Create(315, "BlizzardSlowSpeedmagnificationSetting", 3f, 0.5f, 10f, 0.5f, BlizzardSetting);
            BlizzardskeldDurationSetting = CustomOption.CustomOption.Create(316, "BlizzardskeldDurationSetting", 60f, 0f, 600f, 1f, BlizzardSetting);
            BlizzardmiraDurationSetting = CustomOption.CustomOption.Create(317, "BlizzardmiraDurationSetting", 60f, 0f, 600f, 1f, BlizzardSetting);
            BlizzardpolusDurationSetting = CustomOption.CustomOption.Create(316, "BlizzardpolusDurationSetting", 60f, 0f, 600f, 1f, BlizzardSetting);
            BlizzardairshipDurationSetting = CustomOption.CustomOption.Create(317, "BlizzardairshipDurationSetting", 60f, 0f, 600f, 1f, BlizzardSetting);
            skeldReactorDuration = CustomOption.CustomOption.Create(318, "skeldReactorDuration", 60f, 0f, 600f, 1f, SabotageSetting, format: "unitSeconds");
            miraReactorDuration = CustomOption.CustomOption.Create(319, "miraReactorDuration", 60f, 0f, 600f, 1f, SabotageSetting, format: "unitSeconds");
            polusReactorDuration = CustomOption.CustomOption.Create(320, "polusReactorDuration", 60f, 0f, 600f, 1f, SabotageSetting, format: "unitSeconds");
            airshipReactorDuration = CustomOption.CustomOption.Create(321, "airshipReactorDuration", 60f, 0f, 600f, 1f, SabotageSetting, format: "unitSeconds");
        }
    }
}
