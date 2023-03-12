using HarmonyLib;

namespace SuperNewRoles.Mode.SuperHostRoles.Roles;

class Fox
{
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.SetHudActive), new System.Type[] { typeof(PlayerControl), typeof(RoleBehaviour), typeof(bool) })]
    class SetHudActivePatch
    {
        public static void Postfix(HudManager __instance)
        {
            if (!AmongUsClient.Instance.AmHost) return;
            if (PlayerControl.LocalPlayer.IsRole(RoleId.Fox))
            {
                __instance.ReportButton.ToggleVisible(visible: SuperNewRoles.Roles.Neutral.Fox.FoxReport.GetBool());
            }
        }
    }
}