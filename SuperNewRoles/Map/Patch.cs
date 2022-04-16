using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SuperNewRoles.Map
{
    public static class Patch
    {
        [HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.Awake))]
        class ChangeMapPatch
        {
            public static void Postfix(MapBehaviour __instance)
            {
                if (Data.IsMap(CustomMapNames.Agartha))
                {
                    SuperNewRolesPlugin.Logger.LogInfo("マップ変更処理");
                    Agartha.Patch.MiniMapPatch.MinimapChange(__instance);
                }
            }
        }
    }
}
