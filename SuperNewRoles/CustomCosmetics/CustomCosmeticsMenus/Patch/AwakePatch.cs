using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using Hazel;
using TMPro;
using UnityEngine;

namespace SuperNewRoles.CustomCosmetics.CustomCosmeticsMenus.Patch
{   
    [HarmonyPatch(typeof(PlayerCustomizationMenu),nameof(PlayerCustomizationMenu.Start))]
    class AwakePatch
    {
        public static void Prefix()
        {
            TabSelectPatch.IsFirst = true;
        }
        public static void Postfix(PlayerCustomizationMenu __instance)
        {
            TabSelectPatch.IsFirst = false;
            //var ClosetTab = PlayerCustomizationMenu.Instantiate(__instance.Tabs[0].Button.transform.parent.parent, __instance.Tabs[0].Button.transform.parent.parent.parent);
            //var StarTab = PlayerCustomizationMenu.Instantiate(__instance.Tabs[0].Button.transform.parent.parent, __instance.Tabs[0].Button.transform.parent.parent.parent);
            int i = 0;
            foreach (TabButton button in __instance.Tabs)
            {
                SuperNewRolesPlugin.Logger.LogInfo(button.Button.transform.parent.parent.name);
                if (i > 1)
                {
                    button.Button.transform.parent.parent.gameObject.SetActive(false);
                }
                i++;
            }

            UpdatePatch.area = __instance.transform.FindChild("Background/RightPanel/PlayerVoteArea").GetComponent<PlayerVoteArea>();

            ObjectData.HatText = GameObject.Instantiate(__instance.transform.FindChild("ColorGroup/Text").GetComponent<TextMeshPro>(), __instance.Tabs[0].Tab.transform);
            GameObject.Destroy(ObjectData.HatText.GetComponent<TextTranslatorTMP>());
            ObjectData.VisorText = GameObject.Instantiate(ObjectData.HatText, __instance.Tabs[0].Tab.transform);
            ObjectData.SkinText = GameObject.Instantiate(ObjectData.HatText, __instance.Tabs[0].Tab.transform);
            ObjectData.ColorText = GameObject.Instantiate(ObjectData.HatText, __instance.Tabs[0].Tab.transform);

            ObjectData.HatText.text = FastDestroyableSingleton<TranslationController>.Instance.GetString(StringNames.HatLabel);
            ObjectData.VisorText.text = FastDestroyableSingleton<TranslationController>.Instance.GetString(StringNames.Visor);
            ObjectData.SkinText.text = FastDestroyableSingleton<TranslationController>.Instance.GetString(StringNames.SkinLabel);
            ObjectData.ColorText.text = FastDestroyableSingleton<TranslationController>.Instance.GetString(StringNames.Colors);

        }
    }
    [HarmonyPatch(typeof(PlayerCustomizationMenu), nameof(PlayerCustomizationMenu.OpenTab))]
    class TabSelectPatch
    {
        public static bool IsFirst = false;
        public static bool Prefix(PlayerCustomizationMenu __instance, InventoryTab tab)
        {
            return IsFirst;
        }
    }
}
