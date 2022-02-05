using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNewRoles.Roles
{
    [HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.ShowNormalMap))]
    public static class ShowSabotageMapPatch
    {
        public static void Prefix(MapBehaviour __instance, ref bool __state)
        {
            
        }
    }
}
