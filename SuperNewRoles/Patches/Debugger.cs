using System.Linq;
using HarmonyLib;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Modules;
using UnityEngine;
using UnityEngine.Events;

namespace SuperNewRoles.Patches;

[HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
public static class Debugger
{
    public static void Postfix()
    {
        // Shift Ctrl + D
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.D))
        {
            Logger.Info("Debugger Clicked");
            CustomOptionsMenu.ShowOptionsMenu();
            RoleOptionMenu.ShowRoleOptionMenu(RoleOptionMenuType.Crewmate);

            // Scroll生成部分
            RoleOptionSettings.GenerateScroll(RoleOptionMenu.RoleOptionMenuObjectData.MenuObject.transform);
        }
    }
}

