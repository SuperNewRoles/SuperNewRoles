using HarmonyLib;
using SuperNewRoles.HelpMenus;
using SuperNewRoles.Modules;
using UnityEngine;

namespace SuperNewRoles.Patches;

[HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
public static class Debugger
{
    public static void Postfix(HudManager __instance)
    {
        // Shift Ctrl + D
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.D))
        {
        }
        else if (!__instance.IsIntroDisplayed && !ModHelpers.GetManyKeyDown(ControllerManagerUpdatePatch.HaisonKeyCodes) && Input.GetKeyDown(KeyCode.H))
        {
            HelpMenuObjectManager.ShowOrHideHelpMenu();
        }
    }
}
[HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Update))]
public static class GameStartManagerUpdatePatch
{
    public static void Postfix(GameStartManager __instance)
    {
        if (!CustomOptionManager.SkipStartGameCountdown)
            return;
        if (__instance.startState == GameStartManager.StartingStates.Countdown) // カウントダウン中
        {
            __instance.countDownTimer = 0; //カウント0
        }
    }
}

[HarmonyPatch(typeof(NetworkedPlayerInfo), nameof(NetworkedPlayerInfo.RpcSetTasks))]
public static class NetworkedPlayerInfoRpcSetTasksPatch
{
    public static void Postfix(NetworkedPlayerInfo __instance, Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStructArray<byte> taskTypeIds)
    {
        Logger.Info($"SetTasks: {string.Join(",", taskTypeIds)}");
    }
}