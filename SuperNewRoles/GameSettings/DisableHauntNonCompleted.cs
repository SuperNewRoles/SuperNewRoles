using AmongUs.GameOptions;
using HarmonyLib;
using SuperNewRoles.CustomOptions.Categories;
using SuperNewRoles.Modules;

namespace SuperNewRoles.GameSettings;

[HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
public static class HudManagerUpdatePatch
{
    public static void Postfix(HudManager __instance)
    {
        if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started) return;
        if (!GameSettingOptions.DisableHauntNonCompleted) return;
        if (ExPlayerControl.LocalPlayer.IsAlive()) return;
        if (!ExPlayerControl.LocalPlayer.IsTaskTriggerRole()) return;
        if (ExPlayerControl.LocalPlayer.IsTaskComplete()) return;
        if (ExPlayerControl.LocalPlayer.Data.Role.Role == RoleTypes.GuardianAngel) return;
        __instance.AbilityButton.gameObject.SetActive(false);
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CompleteTask))]
public static class PlayerControlCompleteTask
{
    public static void Postfix(PlayerControl __instance)
    {
        if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started) return;
        if (!GameSettingOptions.DisableHauntNonCompleted) return;
        if (!__instance.AmOwner) return;
        if (ExPlayerControl.LocalPlayer.IsAlive()) return;
        if (!ExPlayerControl.LocalPlayer.IsTaskTriggerRole()) return;
        if (!ExPlayerControl.LocalPlayer.IsTaskComplete()) return;
        HudManager.Instance.AbilityButton.gameObject.SetActive(true);
    }
}