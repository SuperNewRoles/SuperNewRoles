using HarmonyLib;
using SuperNewRoles.CustomOptions.Categories;

namespace SuperNewRoles.Patches;

[HarmonyPatch(typeof(Vent), nameof(Vent.EnterVent))]
class EnterVentAnimPatch
{
    public static bool Prefix([HarmonyArgument(0)] PlayerControl pc) => GameSettingOptions.VentAnimationPlaySetting || pc.AmOwner;
}
[HarmonyPatch(typeof(Vent), nameof(Vent.ExitVent))]
class ExitVentAnimPatch
{
    public static bool Prefix([HarmonyArgument(0)] PlayerControl pc) => GameSettingOptions.VentAnimationPlaySetting || pc.AmOwner;
}