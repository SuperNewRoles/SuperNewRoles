using HarmonyLib;
using System;
using UnityEngine;

namespace SuperNewRoles.Patch
{
    [HarmonyPatch(typeof(EmergencyMinigame), nameof(EmergencyMinigame.Update))]
    class EmergencyMinigameUpdatePatch
    {
        static void Postfix(EmergencyMinigame __instance)
        {
            // Handle max number of meetings
            if (__instance.state == 1)
            {
                int meetingsCount =0;
                int localRemaining = CachedPlayer.LocalPlayer.PlayerControl.RemainingEmergencies;
                int teamRemaining = Mathf.Max(0, CustomOptions.MaxNumberOfMeetings.GetInt() - meetingsCount);
                int remaining = Mathf.Min(localRemaining, teamRemaining);

                __instance.StatusText.text = "<size=100%>" + string.Format(ModTranslation.GetString("船員 {0} が残り\n\n\n緊急招集できる"), CachedPlayer.LocalPlayer.PlayerControl.name) + "</size>";
                __instance.NumberText.text = string.Format(ModTranslation.GetString("{0}回　とクルー全体が　{1}回"), localRemaining.ToString(), teamRemaining.ToString());
                __instance.ButtonActive = remaining > 0;
                __instance.ClosedLid.gameObject.SetActive(!__instance.ButtonActive);
                __instance.OpenLid.gameObject.SetActive(__instance.ButtonActive);
            }
        }
    }
}