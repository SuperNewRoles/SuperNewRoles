using HarmonyLib;
using SuperNewRoles.Roles.RoleBases.Interfaces;
using SuperNewRoles.Roles.RoleBases;

namespace SuperNewRoles.Patches;

[HarmonyPatch(typeof(UseButton))]
public class UseButtonPatch
{
    [HarmonyPatch(nameof(UseButton.DoClick)), HarmonyPrefix]
    public static bool DoClickPrefix(UseButton __instance)
    {
        if (PlayerControl.LocalPlayer.GetRoleBase() is IUseButtonEvent @event && !@event.UseButtonDoClick(__instance)) return false;
        return true;
    }

    [HarmonyPatch(nameof(UseButton.SetTarget)), HarmonyPrefix]
    public static bool SetTargetPrefix(UseButton __instance, IUsable target)
    {
        if (PlayerControl.LocalPlayer.GetRoleBase() is IUseButtonEvent @event && !@event.UseButtonSetTarget(__instance, target)) return false;
        return true;
    }
}
