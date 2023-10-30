using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

namespace SuperNewRoles.MapCustoms;
public static class HideSporeMask
{
    [HarmonyPatch(typeof(Mushroom),nameof(Mushroom.FixedUpdate))]
    public static class MushroomFixedUpdatePatch
    {
        public static void Postfix(Mushroom __instance)
        {
            if (!MapCustomHandler.IsMapCustom(MapCustomHandler.MapCustomId.TheFungle) ||
                !MapCustom.TheFungleHideSporeMask.GetBool())
                return;
            if (MapCustom.TheFungleHideSporeMaskOnlyImpostor.GetBool() &&
                !PlayerControl.LocalPlayer.IsImpostor())
            {
                //元インポスターがそのまま見えてしまわないように対策
                if (!__instance.sporeMask.activeSelf && __instance.spores.enabled)
                    __instance.sporeMask.SetActive(true);
                return;
            }
            __instance.sporeMask.SetActive(false);
        }
    }
}