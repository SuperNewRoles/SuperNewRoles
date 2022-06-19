using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using SuperNewRoles.Roles;

namespace SuperNewRoles.Mode.SuperHostRoles.Roles
{
    class Fox
    {
        [HarmonyPatch(typeof(HudManager), nameof(HudManager.SetHudActive))]
        class SetHudActivePatch
        {
            public static void Postfix(HudManager __instance, [HarmonyArgument(0)] bool isActive)
            {
                if (!AmongUsClient.Instance.AmHost) return;
                if (RoleHelpers.isRole(CustomRPC.RoleId.Fox))
                {
                    __instance.ReportButton.ToggleVisible(visible: RoleClass.Fox.UseReport);
                }
            }
        }
    }
}
