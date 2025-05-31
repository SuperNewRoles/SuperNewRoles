using System.Collections;
using System.Linq;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using HarmonyLib;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Modules;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using SuperNewRoles.HelpMenus;
using System.Collections.Generic;

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