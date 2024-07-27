using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace SuperNewRoles.Roles;

class Doctor
{
    public static void FixedUpdate()
    {
        if (RoleClass.Doctor.IsChargingNow && Vector2.Distance(GameObject.Find("panel_vitals").transform.position, CachedPlayer.LocalPlayer.transform.position) <= 1.2f)
        {
            RoleClass.Doctor.BatteryZeroTime -= Time.fixedDeltaTime;
            RoleClass.Doctor.Battery = (int)(RoleClass.Doctor.BatteryZeroTime * (100f / RoleClass.Doctor.ChargeTime));
            if (RoleClass.Doctor.BatteryZeroTime <= 0)
            {
                RoleClass.Doctor.Battery = 100;
                RoleClass.Doctor.IsChargingNow = false;
                RoleClass.Doctor.BatteryZeroTime = RoleClass.Doctor.UseTime;
            }
        }
    }
    [Harmony]
    public class VitalsPatch
    {
        //static float vitalsTimer = 0f;
        static TMPro.TextMeshPro TimeRemaining;
        private static List<TMPro.TextMeshPro> hackerTexts = new();

        public static void ResetData()
        {
            //vitalsTimer = 0f;
            if (TimeRemaining != null)
            {
                UnityEngine.Object.Destroy(TimeRemaining);
                TimeRemaining = null;
            }
        }

        [HarmonyPatch(typeof(VitalsMinigame), nameof(VitalsMinigame.Begin))]
        class VitalsMinigameStartPatch
        {
            static void Postfix(VitalsMinigame __instance)
            {
                if (PlayerControl.LocalPlayer.IsRole(RoleId.Doctor))
                {
                    hackerTexts = new();
                    foreach (VitalsPanel panel in __instance.vitals)
                    {
                        TMPro.TextMeshPro text = UnityEngine.Object.Instantiate(__instance.SabText, panel.transform);
                        hackerTexts.Add(text);
                        UnityEngine.Object.DestroyImmediate(text.GetComponent<AlphaBlink>());
                        text.gameObject.SetActive(false);
                        text.transform.localScale = Vector3.one * 0.75f;
                        text.transform.localPosition = new(-0.75f, -0.23f, 0f);

                    }
                }
            }
        }
        [HarmonyPatch(typeof(Minigame), nameof(Minigame.Close), new Type[] { })]
        class VitalsMinigameClosePatch
        {
            public static void Prefix(Minigame __instance)
            {
                if (GameObject.FindObjectOfType<VitalsMinigame>() && PlayerControl.LocalPlayer.IsRole(RoleId.Doctor))
                {
                    new LateTask(() => RoleClass.Doctor.MyPanelFlag = false, 0.5f, "Doctor flag");
                }
            }
        }
        [HarmonyPatch(typeof(VitalsMinigame), nameof(VitalsMinigame.Update))]
        class VitalsMinigameUpdatePatch
        {
            static void Postfix(VitalsMinigame __instance)
            {
                if (PlayerControl.LocalPlayer.IsRole(RoleId.Doctor) && !RoleClass.Doctor.MyPanelFlag)
                {
                    for (int k = 0; k < __instance.vitals.Length; k++)
                    {
                        VitalsPanel vitalsPanel = __instance.vitals[k];
                        NetworkedPlayerInfo player = GameData.Instance.AllPlayers[k];
                        if (vitalsPanel.IsDead)
                        {
                            DeadPlayer deadPlayer = DeadPlayer.deadPlayers?.FirstOrDefault(x => x.playerId == player?.PlayerId);
                            if (deadPlayer != null && deadPlayer.timeOfDeath != null && k < hackerTexts.Count && hackerTexts[k] != null)
                            {
                                float timeSinceDeath = (float)(DateTime.UtcNow - deadPlayer.timeOfDeath).TotalMilliseconds;
                                hackerTexts[k].gameObject.SetActive(true);
                                hackerTexts[k].text = Math.Round(timeSinceDeath / 1000) + "s";
                            }
                        }
                    }
                }
                else if (PlayerControl.LocalPlayer.IsRole(RoleId.Doctor) && RoleClass.Doctor.MyPanelFlag)
                {
                    if (!RoleClass.Doctor.IsChargingNow)
                    {
                        RoleClass.Doctor.BatteryZeroTime -= Time.deltaTime;
                        if (RoleClass.Doctor.BatteryZeroTime <= 0)
                        {
                            RoleClass.Doctor.Battery = 0;
                            RoleClass.Doctor.IsChargingNow = true;
                            RoleClass.Doctor.BatteryZeroTime = RoleClass.Doctor.ChargeTime;
                            __instance.Close();
                        }
                    }
                }
                else
                {
                    foreach (TMPro.TextMeshPro text in hackerTexts.AsSpan())
                        if (text != null && text.gameObject != null)
                            text.gameObject.SetActive(false);
                }
            }
        }
    }
}