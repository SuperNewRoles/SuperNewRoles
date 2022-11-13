using HarmonyLib;

using SuperNewRoles.Roles;

namespace SuperNewRoles.Mode.SuperHostRoles.Roles
{
    class Fox
    {
        [HarmonyPatch(typeof(HudManager), nameof(HudManager.SetHudActive))]
        class SetHudActivePatch
        {
            public static void Postfix(HudManager __instance)
            {
                if (!AmongUsClient.Instance.AmHost) return;
                if (CachedPlayer.LocalPlayer.PlayerControl.IsRole(RoleId.Fox))
                {
                    __instance.ReportButton.ToggleVisible(visible: RoleClass.Fox.UseReport);
                }
            }
        }
    }
}