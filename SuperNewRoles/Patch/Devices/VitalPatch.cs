using System;
using HarmonyLib;
using Hazel;
using UnityEngine;

namespace SuperNewRoles.Patch
{
    [Harmony]
    public class VitalsPatch
    {
        static float vitalsTimer = MapOptions.MapOption.CanUseVitalTime.GetFloat();
        public static float RestrictVitalsTime = MapOptions.MapOption.CanUseVitalTime.GetFloat();
        public static float RestrictVitalsTimeMax = MapOptions.MapOption.CanUseVitalTime.GetFloat();
        static TMPro.TextMeshPro TimeRemaining;
        public static void ClearAndReload()
        {
            vitalsTimer = MapOptions.MapOption.CanUseVitalTime.GetFloat();
            if (TimeRemaining != null)
            {
                UnityEngine.Object.Destroy(TimeRemaining);
                TimeRemaining = null;
            }
            RestrictVitalsTime = MapOptions.MapOption.CanUseVitalTime.GetFloat();
            RestrictVitalsTimeMax = MapOptions.MapOption.CanUseVitalTime.GetFloat();
        }

        static void UseVitalsTime()
        {
            // Don't waste network traffic if we're out of time.
            if (MapOptions.MapOption.RestrictVital.GetBool() && RestrictVitalsTime > 0f && PlayerControl.LocalPlayer.IsAlive() && MapOptions.MapOption.RestrictDevicesOption.GetBool() && MapOptions.MapOption.MapOptionSetting.GetBool())
            {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.UseVitalsTime, SendOption.Reliable, -1);
                writer.Write(vitalsTimer);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.UseVitalTime(vitalsTimer);
            }
            vitalsTimer = 0f;
        }
        [HarmonyPatch(typeof(VitalsMinigame), nameof(VitalsMinigame.Begin))]
        class VitalsMinigameStartPatch
        {
            static void Postfix() => vitalsTimer = 0f;
        }
        [HarmonyPatch(typeof(VitalsMinigame), nameof(VitalsMinigame.Update))]
        class VitalsMinigameUpdatePatch
        {
            static bool Prefix(VitalsMinigame __instance)
            {
                if (Mode.ModeHandler.IsMode(Mode.ModeId.Default) && MapOptions.MapOption.MapOptionSetting.GetBool() && MapOptions.MapOption.RestrictDevicesOption.GetBool() && MapOptions.MapOption.RestrictVital.GetBool())
                {
                    vitalsTimer += Time.deltaTime;
                    if (vitalsTimer > 0.1f)
                        UseVitalsTime();
                    if (MapOptions.MapOption.RestrictVital.GetBool())
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
                        TimeRemaining.text = String.Format(ModTranslation.GetString("timeRemaining"), timeString);
                        TimeRemaining.gameObject.SetActive(true);
                    }
                    return true;
                }
                return true;
            }
        }
    }
}