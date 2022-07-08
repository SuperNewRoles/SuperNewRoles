using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using UnityEngine;

namespace SuperNewRoles.CustomCosmetics.CustomCosmeticsMenus.Patch
{
    [HarmonyPatch(typeof(PlayerCustomizationMenu), nameof(PlayerCustomizationMenu.Update))]
    class UpdatePatch
    {
        public static PlayerVoteArea area;
        public static void Postfix(PlayerCustomizationMenu __instance)
        {
            __instance.equipButton.SetActive(false);
            __instance.equippedText.SetActive(false);
            foreach (ColorChip chip in GameObject.FindObjectOfType<PlayerTab>().ColorChips)
            {
                chip.gameObject.SetActive(false);
            }

            foreach (TabButton button in __instance.Tabs)
            {
                GameObject btn = button.Tab.gameObject;
                if (btn.active && (btn.name != "ColorGroup" && btn.name != "HatsGroup"))
                {
                    btn.SetActive(false);
                }
            }
            var panel = __instance.transform.FindChild("Background/RightPanel");

            panel.FindChild("Gradient").gameObject.SetActive(false);
            panel.FindChild("Item Name").gameObject.SetActive(false);
            area.gameObject.SetActive(true);

            panel.localPosition = new Vector3(0, 0, -4.29f);
            __instance.PreviewArea.transform.localPosition = new Vector3(0, -0.5f, -3);
            area.transform.localPosition = new Vector3(3.5f, 1.75f, -70.71f);
            __instance.transform.FindChild("Header/Tabs/ColorTab").localPosition = new Vector3(-0.5f, 0, -5);
            __instance.transform.FindChild("Header/Tabs/HatsTab").localPosition = new Vector3(0.5f, 0, -5);

            area.PreviewNameplate(SaveManager.LastNamePlate);

        }
    }
}
