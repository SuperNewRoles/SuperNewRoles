using System;
using System.Collections.Generic;
using System.Text;
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
        public static CustomOption.CustomOption BlizzardSetting;
        public static CustomOption.CustomOption BlizzardSlowSpeedmagnificationSetting;
        public static CustomOption.CustomOption BlizzardskeldDurationSetting;
        public static CustomOption.CustomOption BlizzardmiraDurationSetting;
        public static CustomOption.CustomOption BlizzardpolusDurationSetting;
        public static CustomOption.CustomOption BlizzardairshipDurationSetting;
        public static CustomOption.CustomOption BlizzardagarthaDurationSetting;
        public static CustomOption.CustomOption ReactorDurationSetting;
        public static CustomOption.CustomOption skeldReactorDuration;
        public static CustomOption.CustomOption miraReactorDuration;
        public static CustomOption.CustomOption polusReactorDuration;
        public static CustomOption.CustomOption airshipReactorDuration;
        public static void Load()
        {
            SabotageSetting = CustomOption.CustomOption.Create(296, false, CustomOptionType.Generic, "SabotageSetting", false, null, isHeader: true);
            //
            CognitiveDeficitSetting = CustomOption.CustomOption.Create(297, false, CustomOptionType.Generic, "SabotageCognitiveDeficitSetting", false, SabotageSetting);
            CognitiveDeficitOutfitUpdateTimeSetting = CustomOption.CustomOption.Create(298, false, CustomOptionType.Generic, "CognitiveDeficitSabotageOutfitUpdateTimeSetting", 3f, 0.5f, 10f, 0.5f, CognitiveDeficitSetting);
            CognitiveDeficitReleaseTimeSetting = CustomOption.CustomOption.Create(299, false, CustomOptionType.Generic, "CognitiveDeficitSabotageReleaseTimeSetting", 3f, 0.5f, 10f, 0.5f, CognitiveDeficitSetting);
            CognitiveDeficitIsAllEndSabotageSetting = CustomOption.CustomOption.Create(300, false, CustomOptionType.Generic, "CognitiveDeficitSabotageIsAllPlayerEndSabotageSetting", true, CognitiveDeficitSetting);            //

            BlizzardSetting = CustomOption.CustomOption.Create(315, false, CustomOptionType.Generic, "SabotageBlizzardSetting", false, SabotageSetting);
            BlizzardSlowSpeedmagnificationSetting = CustomOption.CustomOption.Create(316, false, CustomOptionType.Generic, "BlizzardSlowSpeedmagnificationSetting", 0.5f, 0.0f, 5f, 0.25f, BlizzardSetting);
            BlizzardskeldDurationSetting = CustomOption.CustomOption.Create(317, false, CustomOptionType.Generic, "BlizzardskeldDurationSetting", 60f, 0f, 600f, 1f, BlizzardSetting);
            BlizzardmiraDurationSetting = CustomOption.CustomOption.Create(318, false, CustomOptionType.Generic, "BlizzardmiraDurationSetting", 60f, 0f, 600f, 1f, BlizzardSetting);
            BlizzardpolusDurationSetting = CustomOption.CustomOption.Create(319, false, CustomOptionType.Generic, "BlizzardpolusDurationSetting", 60f, 0f, 600f, 1f, BlizzardSetting);
            BlizzardairshipDurationSetting = CustomOption.CustomOption.Create(320, false, CustomOptionType.Generic, "BlizzardairshipDurationSetting", 60f, 0f, 600f, 1f, BlizzardSetting);
            BlizzardagarthaDurationSetting = CustomOption.CustomOption.Create(321, false, CustomOptionType.Generic, "BlizzardagarthaDurationSetting", 60f, 0f, 600f, 1f, BlizzardSetting);
            ReactorDurationSetting = CustomOption.CustomOption.Create(322, false, CustomOptionType.Generic, "ReactorDurationSetting", false, SabotageSetting);
            skeldReactorDuration = CustomOption.CustomOption.Create(318, false, CustomOptionType.Generic, "skeldReactorDuration", 60f, 0f, 600f, 1f, ReactorDurationSetting, format: "unitSeconds");
            miraReactorDuration = CustomOption.CustomOption.Create(319, false, CustomOptionType.Generic, "miraReactorDuration", 60f, 0f, 600f, 1f, ReactorDurationSetting, format: "unitSeconds");
            polusReactorDuration = CustomOption.CustomOption.Create(320, false, CustomOptionType.Generic, "polusReactorDuration", 60f, 0f, 600f, 1f, ReactorDurationSetting, format: "unitSeconds");
            airshipReactorDuration = CustomOption.CustomOption.Create(321, false, CustomOptionType.Generic, "airshipReactorDuration", 60f, 0f, 600f, 1f, ReactorDurationSetting, format: "unitSeconds");
        }
    }
}           
