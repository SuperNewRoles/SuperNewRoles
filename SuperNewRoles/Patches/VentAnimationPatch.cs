using HarmonyLib;
using PowerTools;
using SuperNewRoles.CustomOptions.Categories;
using SuperNewRoles.Modules;
using SuperNewRoles.Modules.Compatibility;
using SuperNewRoles.Roles.Ability;

namespace SuperNewRoles.Patches;

public class HideVentAnimationPatch
{
    [HarmonyPatch(typeof(Vent), nameof(Vent.EnterVent))]
    [HarmonyAfter(LevelImposterSupport.PluginGuid)]
    class EnterVentAnimPatch
    {
        public static bool Prefix([HarmonyArgument(0)] PlayerControl pc)
        {
            if (LevelImposterSupport.IsCustomMap)
                return true;
            return !HideVentAnimation(pc);
        }

        public static void Postfix([HarmonyArgument(0)] PlayerControl pc, Vent __instance)
        {
            if (!LevelImposterSupport.IsCustomMap)
                return;
            if (!HideVentAnimation(pc))
                return;
            StopVentAnimation(__instance);
        }
    }

    [HarmonyPatch(typeof(Vent), nameof(Vent.ExitVent))]
    [HarmonyAfter(LevelImposterSupport.PluginGuid)]
    class ExitVentAnimPatch
    {
        public static bool Prefix([HarmonyArgument(0)] PlayerControl pc)
        {
            if (LevelImposterSupport.IsCustomMap)
                return true;
            return !HideVentAnimation(pc);
        }

        public static void Postfix([HarmonyArgument(0)] PlayerControl pc, Vent __instance)
        {
            if (!LevelImposterSupport.IsCustomMap)
                return;
            if (!HideVentAnimation(pc))
                return;
            StopVentAnimation(__instance);
        }
    }

    private static void StopVentAnimation(Vent vent)
    {
        if (vent == null)
            return;
        SpriteAnim spriteAnim = vent.GetComponent<SpriteAnim>();
        spriteAnim?.Stop();
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
