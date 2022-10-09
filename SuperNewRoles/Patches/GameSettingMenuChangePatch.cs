using System;
using System.Linq;
using HarmonyLib;
using Il2CppSystem.Collections.Generic;
using UnityEngine;

namespace SuperNewRoles.Patches
{
    class GameSettingMenuChangePatch
    {
        [HarmonyPatch(typeof(GameSettingMenu), nameof(GameSettingMenu.Start))]
        class GameSettingMenuStartPatch
        {
            public static void Prefix(GameSettingMenu __instance)
            {
                __instance.HideForOnline = new Transform[] { };
            }

            public static void Postfix(GameSettingMenu __instance)
            {
                // Setup mapNameTransform
                Transform mapNameTransform = __instance.AllItems.FirstOrDefault(x => x.gameObject.activeSelf && x.name.Equals("MapName", StringComparison.OrdinalIgnoreCase));
                if (mapNameTransform == null) return;

                List<KeyValuePair<string, int>> options = new();
                for (int i = 0; i < Constants.MapNames.Length; i++)
                {
                    KeyValuePair<string, int> kvp = new()
                    {
                        key = Constants.MapNames[i],
                        value = i
                    };
                    options.Add(kvp);
                }
                mapNameTransform.GetComponent<KeyValueOption>().Values = options;
            }
        }
    }
}