using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using SuperNewRoles.Patch;
using SuperNewRoles.CustomRPC;

namespace SuperNewRoles.Roles
{
    class Doctor
    {
        [Harmony]
        public class VitalsPatch
        {
            static float vitalsTimer = 0f;
            static TMPro.TextMeshPro TimeRemaining;
            private static List<TMPro.TextMeshPro> hackerTexts = new();

            public static void ResetData()
            {
                vitalsTimer = 0f;
                if (TimeRemaining != null)
                {
                    UnityEngine.Object.Destroy(TimeRemaining);
                    TimeRemaining = null;
                }
            }

            [HarmonyPatch(typeof(VitalsMinigame), nameof(VitalsMinigame.Update))]
            class VitalsMinigameUpdatePatch
            {
                static void Postfix(VitalsMinigame __instance)
                {
                    if (PlayerControl.LocalPlayer.isRole(RoleId.Doctor) && !RoleClass.Doctor.MyPanelFlag)
                    {
                        for (int k = 0; k < __instance.vitals.Length; k++)
                        {
                            VitalsPanel vitalsPanel = __instance.vitals[k];
                            GameData.PlayerInfo player = GameData.Instance.AllPlayers[k];

                            // Hacker update
                            if (vitalsPanel.IsDead)
                            {
                                DeadPlayer deadPlayer = DeadPlayer.deadPlayers?.Where(x => x.player?.PlayerId == player?.PlayerId)?.FirstOrDefault();
                                if (deadPlayer != null && deadPlayer.timeOfDeath != null && k < hackerTexts.Count && hackerTexts[k] != null)
                                {
                                    float timeSinceDeath = (float)(DateTime.UtcNow - deadPlayer.timeOfDeath).TotalMilliseconds;
                                    hackerTexts[k].gameObject.SetActive(true);
                                    hackerTexts[k].text = Math.Round(timeSinceDeath / 1000) + "s";
                                }
                            }
                        }
                    }
                    else
                    {
                        foreach (TMPro.TextMeshPro text in hackerTexts)
                            if (text != null && text.gameObject != null)
                                text.gameObject.SetActive(false);
                    }
                }
            }
        }
    }
}
