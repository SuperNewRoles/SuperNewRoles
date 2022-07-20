using HarmonyLib;
using SuperNewRoles.CustomRPC;
using SuperNewRoles.Roles;

namespace SuperNewRoles.Mode.SuperHostRoles.Roles
{
    class Samurai
    {
        [HarmonyPatch(typeof(HudManager), nameof(HudManager.SetHudActive))]
        class SetHudActivePatch
        {
            public static void Postfix(HudManager __instance)
            {
                if (!AmongUsClient.Instance.AmHost) return;
                if (PlayerControl.LocalPlayer.IsRole(RoleId.Samurai))
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