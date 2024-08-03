using HarmonyLib;
using SuperNewRoles.Roles.RoleBases;
using SuperNewRoles.Roles.RoleBases.Interfaces;

namespace SuperNewRoles.Patches;

public static class VanillaButtonEventsPatch
{
    [HarmonyPatch(typeof(KillButton))]
    public static class KillButtonPatch
    {
        [HarmonyPatch(nameof(KillButton.DoClick)), HarmonyPrefix]
        public static bool DoClickPrefix(KillButton __instance)
        {
            if (PlayerControl.LocalPlayer.GetRoleBase() is IVanillaButtonEvents @event && !@event.KillButtonDoClick(__instance)) return false;
            return true;
        }

        [HarmonyPatch(nameof(KillButton.CheckClick)), HarmonyPrefix]
        public static bool CheckClickPrefix(KillButton __instance, PlayerControl target)
        {
            if (PlayerControl.LocalPlayer.GetRoleBase() is IVanillaButtonEvents @event && !@event.KillButtonCheckClick(__instance, target)) return false;
            return true;
        }

        [HarmonyPatch(nameof(KillButton.SetTarget)), HarmonyPrefix]
        public static bool SetTargetPrefix(KillButton __instance, PlayerControl target)
        {
            if (PlayerControl.LocalPlayer.GetRoleBase() is IVanillaButtonEvents @event && !@event.KillButtonSetTarget(__instance, target)) return false;
            return true;
        }
    }

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
