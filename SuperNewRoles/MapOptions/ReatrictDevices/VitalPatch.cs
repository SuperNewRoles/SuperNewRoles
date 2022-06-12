using HarmonyLib;
using Hazel;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
//using static TheOtherRoles.TheOtherRoles;
//using static SuperNewRoles.GameHistory;
using System.Reflection;

namespace SuperNewRoles.Patch
{
    [Harmony]
    public class VitalsPatch
    {
        static float vitalsTimer = MapOptions.MapOption.CanUseVitalTime.getFloat();
        public static float RestrictVitalsTime = MapOptions.MapOption.CanUseVitalTime.getFloat();
        public static float RestrictVitalsTimeMax = MapOptions.MapOption.CanUseVitalTime.getFloat();
        static TMPro.TextMeshPro TimeRemaining;

        public static void ClearAndReload()
        {
            //vitalsTimer = 0f;
            ResetData();
            RestrictVitalsTime = MapOptions.MapOption.CanUseVitalTime.getFloat();
            RestrictVitalsTimeMax = MapOptions.MapOption.CanUseVitalTime.getFloat();
        }

        public static void ResetData()
        {
            vitalsTimer = MapOptions.MapOption.CanUseVitalTime.getFloat();
            if (TimeRemaining != null)
            {
                UnityEngine.Object.Destroy(TimeRemaining);
                TimeRemaining = null;
            }
        }

        static void UseVitalsTime()
        {
            // Don't waste network traffic if we're out of time.
            if (MapOptions.MapOption.RestrictVital.getBool() && RestrictVitalsTime > 0f && PlayerControl.LocalPlayer.isAlive())
            {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.UseVitalsTime, Hazel.SendOption.Reliable, -1);
                writer.Write(vitalsTimer);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                CustomRPC.RPCProcedure.UseVitalTime(vitalsTimer);
            }
            vitalsTimer = 0f;
        }

        [HarmonyPatch(typeof(VitalsMinigame), nameof(VitalsMinigame.Begin))]
        class VitalsMinigameStartPatch
        {
            static void Postfix(VitalsMinigame __instance)
            {
                vitalsTimer = 0f;
            }
        }

        [HarmonyPatch(typeof(VitalsMinigame), nameof(VitalsMinigame.Update))]
        class VitalsMinigameUpdatePatch
        {
            static bool Prefix(VitalsMinigame __instance)
            {
                if (!Mode.ModeHandler.isMode(Mode.ModeId.SuperHostRoles) && MapOptions.MapOption.MapOptionSetting.getBool())
                {
                    vitalsTimer += Time.deltaTime;
                    if (vitalsTimer > 0.1f)
                        UseVitalsTime();

                    if (MapOptions.MapOption.RestrictVital.getBool())
                    {
                        if (TimeRemaining == null)
                        {
                            TimeRemaining = UnityEngine.Object.Instantiate(FastDestroyableSingleton<HudManager>.Instance.TaskText, __instance.transform);
                            TimeRemaining.alignment = TMPro.TextAlignmentOptions.BottomRight;
                            TimeRemaining.transform.position = Vector3.zero;
                            TimeRemaining.transform.localPosition = new Vector3(1.7f, 4.45f);
                            TimeRemaining.transform.localScale *= 1.8f;
                            TimeRemaining.color = Palette.White;
                        }

                        if (RestrictVitalsTime <= 0f)
                        {
                            __instance.Close();
                            return false;
                        }

                        string timeString = TimeSpan.FromSeconds(RestrictVitalsTime).ToString(@"mm\:ss\.ff");
                        TimeRemaining.text = String.Format(ModTranslation.getString("timeRemaining"), timeString);
                        TimeRemaining.gameObject.SetActive(true);
                    }

                    return true;
                }
                return true;
            }
        }

        [HarmonyPatch]
        class VitalsMinigameClosePatch
        {
            private static IEnumerable<MethodBase> TargetMethods()
            {
                return typeof(Minigame).GetMethods().Where(x => x.Name == "Close");
            }

            static void Prefix(Minigame __instance)
            {
                if (__instance is VitalsMinigame)
                    UseVitalsTime();
            }
        }
    }
}