using HarmonyLib;
using SuperNewRoles.Roles;
using SuperNewRoles.CustomRPC;

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
                if (PlayerControl.LocalPlayer.isRole(RoleId.Fox))
                {
                    __instance.ReportButton.ToggleVisible(visible: RoleClass.Fox.UseReport);
                }
            }
        }
    }
}
