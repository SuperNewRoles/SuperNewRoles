using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using SuperNewRoles.Roles.Impostor;

namespace SuperNewRoles.MapCustoms;
public static class HideSporeMask
{
    [HarmonyPatch(typeof(Mushroom),nameof(Mushroom.FixedUpdate))]
    public static class MushroomFixedUpdatePatch
    {
        public static void Postfix(Mushroom __instance)
        {
            if (PlayerControl.LocalPlayer.IsRole(RoleId.Mushroomer) &&
                !Mushroomer.HasGasMask.GetBool())
                return;
            if (!MapCustomHandler.IsMapCustom(MapCustomHandler.MapCustomId.TheFungle)
                || !MapCustom.TheFungleHideSporeMask.GetBool())
                return;
            if (MapCustom.TheFungleHideSporeMaskOnlyImpostor.GetBool() &&
                !PlayerControl.LocalPlayer.IsImpostor())
            {
                //元インポスターがそのまま見えてしまわないように対策
                if (__instance.sporeMask.transform.localScale.x == 0)
                    __instance.sporeMask.transform.localScale = new(2.4f, 2.4f, 1.2f);
                return;
            }
            __instance.sporeMask.transform.localScale = new(0,0,0);
        }
    }
}