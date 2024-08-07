using HarmonyLib;
using SuperNewRoles.Roles.RoleBases;
using SuperNewRoles.Roles.RoleBases.Interfaces;

namespace SuperNewRoles.Patches;

public static class VanillaButtonEventsPatch
{
    [HarmonyPatch(typeof(UseButton))]
    public class UseButtonPatch
    {
        [HarmonyPatch(nameof(UseButton.DoClick)), HarmonyPrefix]
        public static bool DoClickPrefix(UseButton __instance)
        {
            if (PlayerControl.LocalPlayer.GetRoleBase() is IVanillaButtonEvents @event && !@event.UseButtonDoClick(__instance)) return false;
            return true;
        }

        [HarmonyPatch(nameof(UseButton.SetTarget)), HarmonyPrefix]
        public static bool SetTargetPrefix(UseButton __instance, IUsable target)
        {
            if (PlayerControl.LocalPlayer.GetRoleBase() is IVanillaButtonEvents @event && !@event.UseButtonSetTarget(__instance, target)) return false;
            return true;
        }
    }


    [HarmonyPatch(typeof(VentButton))]
    public class VentButtonPatch
    {
        [HarmonyPatch(nameof(VentButton.DoClick)), HarmonyPrefix]
        public static bool DoClickPrefix(VentButton __instance)
        {
            if (PlayerControl.LocalPlayer.GetRoleBase() is IVanillaButtonEvents @event && !@event.VentButtonDoClick(__instance)) return false;
            return true;
        }

        [HarmonyPatch(nameof(VentButton.SetTarget)), HarmonyPrefix]
        public static bool SetTargetPrefix(VentButton __instance, Vent target)
        {
            if (PlayerControl.LocalPlayer.GetRoleBase() is IVanillaButtonEvents @event && !@event.VentButtonSetTarget(__instance, target)) return false;
            return true;
        }
    }
}
