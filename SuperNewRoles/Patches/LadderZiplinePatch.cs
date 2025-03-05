using HarmonyLib;
using SuperNewRoles.CustomOptions.Categories;
using SuperNewRoles.Modules;
using UnityEngine;

namespace SuperNewRoles.Patches;

[HarmonyPatch(typeof(Ladder))]
public static class LadderPatch
{
    [HarmonyPatch(nameof(Ladder.MaxCoolDown), MethodType.Getter), HarmonyPrefix]
    public static bool LadderMaxCoolDownGetterPrefix(ref float __result)
    {
        if (MapSettingOptions.LadderCoolChangeOption)
        {
            __result = MapSettingOptions.LadderCoolTimeOption;
            if (MapSettingOptions.LadderImpostorCoolChangeOption && ExPlayerControl.LocalPlayer.IsImpostor())
                __result = MapSettingOptions.LadderImpostorCoolTimeOption;
            return false;
        }
        return true;
    }

    [HarmonyPatch(nameof(Ladder.Use)), HarmonyPostfix]
    public static void LadderUsePostfix(Ladder __instance)
    {
        if (!MapSettingOptions.LadderCoolChangeOption) return;
        __instance.CoolDown = MapSettingOptions.LadderCoolTimeOption;
        if (MapSettingOptions.LadderImpostorCoolChangeOption && ExPlayerControl.LocalPlayer.IsImpostor())
            __instance.CoolDown = MapSettingOptions.LadderImpostorCoolTimeOption;
    }

    [HarmonyPatch(nameof(Ladder.SetDestinationCooldown)), HarmonyPostfix]
    public static void LadderSetDestinationCooldownPostfix(Ladder __instance)
    {
        if (!MapSettingOptions.LadderCoolChangeOption) return;
        __instance.Destination.CoolDown = MapSettingOptions.LadderCoolTimeOption;
        if (MapSettingOptions.LadderImpostorCoolChangeOption && ExPlayerControl.LocalPlayer.IsImpostor())
            __instance.Destination.CoolDown = MapSettingOptions.LadderImpostorCoolTimeOption;
    }
}

[HarmonyPatch(typeof(ZiplineConsole))]
public static class ZiplineConsolePatch
{
    [HarmonyPatch(nameof(ZiplineConsole.MaxCoolDown), MethodType.Getter), HarmonyPrefix]
    public static bool ZiplineConsoleMaxCoolDownGetterPrefix(ref float __result)
    {
        if (MapSettingOptions.ZiplineCoolChangeOption)
        {
            __result = MapSettingOptions.ZiplineCoolTimeOption;
            if (MapSettingOptions.ZiplineImpostorCoolChangeOption && ExPlayerControl.LocalPlayer.IsImpostor())
                __result = MapSettingOptions.ZiplineImpostorCoolTimeOption;
            return false;
        }
        return true;
    }

    [HarmonyPatch(nameof(ZiplineConsole.Use)), HarmonyPostfix]
    public static void ZiplineConsoleUsePostfix(ZiplineConsole __instance)
    {
        if (!MapSettingOptions.ZiplineCoolChangeOption) return;
        __instance.CoolDown = MapSettingOptions.ZiplineCoolTimeOption;
        if (MapSettingOptions.ZiplineImpostorCoolChangeOption && ExPlayerControl.LocalPlayer.IsImpostor())
            __instance.CoolDown = MapSettingOptions.ZiplineImpostorCoolTimeOption;
    }

    [HarmonyPatch(nameof(ZiplineConsole.SetDestinationCooldown)), HarmonyPostfix]
    public static void ZiplineConsoleSetDestinationCooldownPostfix(ZiplineConsole __instance)
    {
        if (!MapSettingOptions.ZiplineCoolChangeOption) return;
        __instance.destination.CoolDown = MapSettingOptions.ZiplineCoolTimeOption;
        if (MapSettingOptions.ZiplineImpostorCoolChangeOption && ExPlayerControl.LocalPlayer.IsImpostor())
            __instance.destination.CoolDown = MapSettingOptions.ZiplineImpostorCoolTimeOption;
    }
}