using HarmonyLib;
using SuperNewRoles.CustomOptions.Categories;
using SuperNewRoles.Modules;

namespace SuperNewRoles.Patches;

[HarmonyPatch(typeof(AprilFoolsMode), nameof(AprilFoolsMode.ShouldFlipSkeld))]
public static class AprilFoolsModeStartPatch
{
    public static void Postfix(ref bool __result)
    {
        if (GameSettingOptions.AprilFoolsEnableDleks)
            __result = true;
    }
}
// なんかエラー出ててわろた
/*

[HarmonyPatch(typeof(AprilFoolsMode), nameof(AprilFoolsMode.ShouldHorseAround))]
public static class AprilFoolsModeShouldHorseAroundPatch
{
    public static void Postfix(ref bool __result)
    {
        if (GameSettingOptions.AprilFoolsOutfitType == AprilFoolsOutfitType.Horse)
            __result = true;
    }
}

[HarmonyPatch(typeof(AprilFoolsMode), nameof(AprilFoolsMode.ShouldLongAround))]
public static class AprilFoolsModeShouldLongAroundPatch
{
    public static void Postfix(ref bool __result)
    {
        if (GameSettingOptions.AprilFoolsOutfitType == AprilFoolsOutfitType.AlongUs)
            __result = true;
    }
}*/