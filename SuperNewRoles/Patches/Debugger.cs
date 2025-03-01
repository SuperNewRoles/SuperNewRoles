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

namespace SuperNewRoles.Patches;

[HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
public static class Debugger
{
    public static void Postfix()
    {
        // Shift Ctrl + D
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.D))
        {
        }
        else if (Input.GetKeyDown(KeyCode.H))
        {
            HelpMenuObjectManager.ShowOrHideHelpMenu();
        }
    }
}
[HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Update))]
public static class GameStartManagerUpdatePatch
{
    public static void Postfix()
    {
        if (!CustomOptionManager.SkipStartGameCountdown)
            return;
        if (GameStartManager.InstanceExists && FastDestroyableSingleton<GameStartManager>.Instance.startState == GameStartManager.StartingStates.Countdown) // カウントダウン中
        {
            FastDestroyableSingleton<GameStartManager>.Instance.countDownTimer = 0; //カウント0
        }
    }
}