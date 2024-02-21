using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using SuperNewRoles.Roles.Impostor;

namespace SuperNewRoles.Patches;
public static class MushroomPatch
{
    [HarmonyPatch(typeof(Mushroom), nameof(Mushroom.ResetState))]
    public static class MushroomResetStatePatch
    {
        public static void Postfix(Mushroom __instance)
        {
            Mushroomer.MushroomResetStatePatch(__instance);
        }
    }
}
