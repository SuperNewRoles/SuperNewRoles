using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using SuperNewRoles.CustomOptions.Categories;
using SuperNewRoles.Modules;

namespace SuperNewRoles.MapCustoms;
public static class HideSporeMask
{
    [HarmonyPatch(typeof(Mushroom), nameof(Mushroom.FixedUpdate))]
    public static class MushroomFixedUpdatePatch
    {
        public static bool CanUseGasMask()
        {
            if (MapCustomHandler.IsMapCustom(MapCustomHandler.MapCustomId.TheFungle)
                && MapEditSettingsOptions.TheFungleHideSporeMask)
            {
                if (!MapEditSettingsOptions.TheFungleHideSporeMaskOnlyImpostor ||
                ExPlayerControl.LocalPlayer.IsImpostor())
                    return true;
            }
            return false;
        }
        public static void Postfix(Mushroom __instance)
        {
            if (CanUseGasMask())
                __instance.sporeMask.transform.localScale = new(0, 0, 0);
            else if (__instance.sporeMask.transform.localScale.x == 0)
                __instance.sporeMask.transform.localScale = new(2.4f, 2.4f, 1.2f);
        }
    }
}