using HarmonyLib;
using SuperNewRoles.CustomOptions.Categories;

namespace SuperNewRoles.Patches;

[HarmonyPatch(typeof(HashRandom), nameof(HashRandom.FastNext))]
public static class HashRandomFastNextPatch
{
    public static bool Prefix(ref int __result, int maxInt)
    {
        if (GeneralSettingOptions.AdvancedRandom)
        {
            __result = ModHelpers.GetRandomInt(maxInt - 1);
            return false;
        }
        return true;
    }
}

[HarmonyPatch(typeof(HashRandom), nameof(HashRandom.Next), [typeof(int)])]
public static class HashRandomNextPatch
{
    public static bool Prefix(ref int __result, int maxInt)
    {
        if (GeneralSettingOptions.AdvancedRandom)
        {
            __result = ModHelpers.GetRandomInt(maxInt - 1);
            return false;
        }
        return true;
    }
}