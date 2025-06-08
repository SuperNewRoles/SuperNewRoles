using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using SuperNewRoles.CustomOptions.Categories;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles;
using SuperNewRoles.Roles.Impostor;

namespace SuperNewRoles.MapCustoms;
public static class HideSporeMask
{
    [HarmonyPatch(typeof(Mushroom), nameof(Mushroom.FixedUpdate))]
    public static class MushroomFixedUpdatePatch
    {
        public static bool CanUseGasMask()
        {
            // マッシュルーマーのガスマスク設定をチェック
            if (ExPlayerControl.LocalPlayer.Role is RoleId.Mushroomer &&
                Mushroomer.MushroomerHasGasMask)
                return true;

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