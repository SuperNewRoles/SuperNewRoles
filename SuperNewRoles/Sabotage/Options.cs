using UnityEngine;
using static SuperNewRoles.Modules.CustomOption;
using static SuperNewRoles.Modules.CustomOptionHolder;

namespace SuperNewRoles.Sabotage;
public static class Options
{
    public static CustomOption SabotageSetting;
    public static CustomOption CognitiveDeficitSetting;
    public static CustomOption CognitiveDeficitOutfitUpdateTimeSetting;
    public static CustomOption CognitiveDeficitReleaseTimeSetting;
    public static CustomOption CognitiveDeficitIsAllEndSabotageSetting;
    public static void Load()
    {
        SabotageSetting = Create(103400, false, CustomOptionType.Generic, Cs(new Color(204f / 187f, 51f / 255f, 0, 1f), "SabotageSetting"), false, null, isHeader: true, withHeader: true);
        CognitiveDeficitSetting = Create(103401, false, CustomOptionType.Generic, "SabotageCognitiveDeficitSetting", false, SabotageSetting);
        CognitiveDeficitOutfitUpdateTimeSetting = Create(103402, false, CustomOptionType.Generic, "CognitiveDeficitSabotageOutfitUpdateTimeSetting", 3f, 0.5f, 10f, 0.5f, CognitiveDeficitSetting);
        CognitiveDeficitReleaseTimeSetting = Create(103403, false, CustomOptionType.Generic, "CognitiveDeficitSabotageReleaseTimeSetting", 3f, 0.5f, 10f, 0.5f, CognitiveDeficitSetting);
        CognitiveDeficitIsAllEndSabotageSetting = Create(103404, false, CustomOptionType.Generic, "CognitiveDeficitSabotageIsAllPlayerEndSabotageSetting", true, CognitiveDeficitSetting);
    }
}