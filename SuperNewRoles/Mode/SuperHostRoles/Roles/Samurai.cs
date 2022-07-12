using HarmonyLib;
using SuperNewRoles.Roles;
using SuperNewRoles.CustomRPC;

namespace SuperNewRoles.Mode.SuperHostRoles.Roles
{
    class Samurai
    {
        [HarmonyPatch(typeof(HudManager), nameof(HudManager.SetHudActive))]
        class SetHudActivePatch
        {
            public static void Postfix(HudManager __instance, [HarmonyArgument(0)] bool isActive)
            {
                if (!AmongUsClient.Instance.AmHost) return;
                if (PlayerControl.LocalPlayer.isRole(RoleId.Samurai))
                {
                    __instance.SabotageButton.ToggleVisible(visible: RoleClass.Samurai.UseSabo);
                    __instance.ImpostorVentButton.ToggleVisible(visible: RoleClass.Samurai.UseVent);
                }
            }
        }
        public static void IsSword()
        {
            RoleClass.Samurai.Sword = true;
        }
    }
}
