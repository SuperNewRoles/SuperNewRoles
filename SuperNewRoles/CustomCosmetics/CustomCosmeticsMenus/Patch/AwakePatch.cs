using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using Hazel;
using UnityEngine;

namespace SuperNewRoles.CustomCosmetics.CustomCosmeticsMenus.Patch
{
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CmdCheckColor))]
    class AwakePatcha
    {
        public static bool Prefix(PlayerControl __instance,byte bodyColor)
        {
            if (!__instance.AmOwner) return true;
            if (AmongUsClient.Instance.AmClient)
            {
                __instance.SetColor(bodyColor);
            }
            MessageWriter obj = AmongUsClient.Instance.StartRpcImmediately(__instance.NetId, 8, SendOption.None);
            obj.Write(bodyColor);
            AmongUsClient.Instance.FinishRpcImmediately(obj);
            return false;
        }
    }
    
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
            __instance.transform.FindChild("Background/RightPanel").localPosition = new Vector3(0, 0, -4.29f);
            UpdatePatch.area = __instance.transform.FindChild("Background/RightPanel/PlayerVoteArea").GetComponent<PlayerVoteArea>();
        }
    }
    [HarmonyPatch(typeof(PlayerCustomizationMenu), nameof(PlayerCustomizationMenu.Update))]
    class UpdatePatch
    {
        public static PlayerVoteArea area;
        public static void Postfix(PlayerCustomizationMenu __instance)
        {
            __instance.equipButton.SetActive(false);
            __instance.equippedText.SetActive(false);
            foreach (TabButton button in __instance.Tabs)
            {
                GameObject btn = button.Tab.gameObject;
                if (btn.active)
                {
                    btn.SetActive(false);
                }
            }
            var panel = __instance.transform.FindChild("Background/RightPanel");
            panel.localPosition = new Vector3(0, 0, -4.29f);
            panel.FindChild("Gradient").gameObject.SetActive(false);
            panel.FindChild("Item Name").gameObject.SetActive(false);
            area.PreviewNameplate(SaveManager.LastNamePlate);
            area.gameObject.SetActive(true);

        }
    }
    [HarmonyPatch(typeof(PlayerCustomizationMenu), nameof(PlayerCustomizationMenu.OpenTab))]
    class TabSelectPatch
    {
        public static bool IsFirst = false;
        public static bool Prefix(PlayerCustomizationMenu __instance, InventoryTab tab)
        {
            return true;
            return IsFirst;
        }
    }
}
