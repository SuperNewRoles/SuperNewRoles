using HarmonyLib;
using SuperNewRoles.Modules;

namespace SuperNewRoles.HelpMenus;

[HarmonyPatch(typeof(HudManager), nameof(HudManager.Start))]
public static class HelpMenusHUuManagerStartPatch
{
    public static void Postfix(HudManager __instance)
    {
        AssetManager.Instantiate("HelpButton", __instance.transform);
    }
}
