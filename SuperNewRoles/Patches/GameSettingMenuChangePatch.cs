using System;
using System.Linq;
using HarmonyLib;
using Il2CppSystem.Collections.Generic;
using UnityEngine;

namespace SuperNewRoles.Patches;

class GameSettingMenuChangePatch
{
    [HarmonyPatch(typeof(GameSettingMenu), nameof(GameSettingMenu.Start))]
    class GameSettingMenuStartPatch
    {
        public static void Prefix(GameSettingMenu __instance)
        {
            __instance.GameSettingsTab.HideForOnline = new Transform[] { };
        }

        public static void Postfix(GameSettingMenu __instance)
        {
            // とりあえず封印
            /*
            // Setup mapNameTransform
            var mapNameTransform = __instance.GameSettingsTab.Children.FirstOrDefault(x => x.gameObject.activeSelf && x.name.Equals("MapName", StringComparison.OrdinalIgnoreCase));
            if (mapNameTransform == null) return;

            var options = new List<KeyValuePair<string, int>>();
            for (int i = 0; i < Constants.MapNames.Length; i++)
            {
                var kvp = new KeyValuePair<string, int>
                {
                    key = Constants.MapNames[i],
                    value = i
                };
                options.Add(kvp);
            }
            mapNameTransform.GetComponent<StringOption>().Values = options;*/
        }
    }
}