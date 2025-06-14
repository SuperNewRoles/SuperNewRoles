using HarmonyLib;
using SuperNewRoles.CustomOptions.Categories;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability;

namespace SuperNewRoles.Patches;

public class HideVentAnimationPatch
{
    [HarmonyPatch(typeof(Vent), nameof(Vent.EnterVent))]
    class EnterVentAnimPatch
    {
        public static bool Prefix([HarmonyArgument(0)] PlayerControl pc) => !HideVentAnimation(pc);
    }
    [HarmonyPatch(typeof(Vent), nameof(Vent.ExitVent))]
    class ExitVentAnimPatch
    {
        public static bool Prefix([HarmonyArgument(0)] PlayerControl pc) => !HideVentAnimation(pc);
    }
    private static bool HideVentAnimation(ExPlayerControl pc)
    {
        if (pc.TryGetAbility<HideVentAnimationAbility>(out var ability) && ability.CanHideVentAnimation())
            return true;
        if (pc.AmOwner) return false;
        if (!GameSettingOptions.VentAnimationPlaySetting) return true;
        return false;
    }

}