using HarmonyLib;
using SuperNewRoles.Modules;

namespace SuperNewRoles.Patches;

[HarmonyPatch(typeof(ClipboardHelper), nameof(ClipboardHelper.GetClipboardString))]
public static class FixClipboardUnicodePatch
{
    public static bool Prepare()
    {
        // Androidプラットフォームの場合はパッチを適用しない
        return !ModHelpers.IsAndroid();
    }
}