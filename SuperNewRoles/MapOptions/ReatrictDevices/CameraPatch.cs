using HarmonyLib;
using Hazel;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;

namespace SuperNewRoles.Patch
{
    [Harmony]
    public class CameraPatch
    {
        static float cameraTimer;
        public static float RestrictCameraTime;
        public static float RestrictCameraTimeMax;
        public static void ClearAndReload()
        {
            //cameraTimer = 0f;
            ResetData();
            RestrictCameraTime = MapOptions.MapOption.CanUseCameraTime.getFloat();
            RestrictCameraTimeMax = MapOptions.MapOption.CanUseCameraTime.getFloat();
        }

        public static void ResetData()
        {
            cameraTimer = MapOptions.MapOption.CanUseCameraTime.getFloat();
            SurveillanceMinigamePatch.ResetData();
            PlanetSurveillanceMinigamePatch.ResetData();
        }

        static void UseCameraTime()
        {
            // Don't waste network traffic if we're out of time.
            if (MapOptions.MapOption.RestrictDevicesOption.getBool() && MapOptions.MapOption.RestrictCamera.getBool() && PlayerControl.LocalPlayer.isAlive())
            {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.UseCameraTime, Hazel.SendOption.Reliable, -1);
                writer.Write(cameraTimer);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                CustomRPC.RPCProcedure.UseCameraTime(cameraTimer);
            }
            cameraTimer = 0f;
        }

        [HarmonyPatch]
        class SurveillanceMinigamePatch
        {
            private static int page = 0;
            private static float timer = 0f;
            static TMPro.TextMeshPro TimeRemaining;

            public static void ResetData()
            {
                if (TimeRemaining != null)
                {
                    UnityEngine.Object.Destroy(TimeRemaining);
                    TimeRemaining = null;
                }
            }

            [HarmonyPatch(typeof(SurveillanceMinigame), nameof(SurveillanceMinigame.Begin))]
            class SurveillanceMinigameBeginPatch
            {
                public static void Prefix(SurveillanceMinigame __instance)
                {
                    cameraTimer = 0f;
                }

                public static void Postfix(SurveillanceMinigame __instance)
                {
                    // Add securityGuard cameras
                    page = 0;
                    timer = 0;
                    if (MapUtilities.CachedShipStatus.AllCameras.Length > 4 && __instance.FilteredRooms.Length > 0)
                    {
                        __instance.textures = __instance.textures.ToList().Concat(new RenderTexture[MapUtilities.CachedShipStatus.AllCameras.Length - 4]).ToArray();
                        for (int i = 4; i < MapUtilities.CachedShipStatus.AllCameras.Length; i++)
                        {
                            SurvCamera surv = MapUtilities.CachedShipStatus.AllCameras[i];
                            Camera camera = UnityEngine.Object.Instantiate<Camera>(__instance.CameraPrefab);
                            camera.transform.SetParent(__instance.transform);
                            camera.transform.position = new Vector3(surv.transform.position.x, surv.transform.position.y, 8f);
                            camera.orthographicSize = 2.35f;
                            RenderTexture temporary = RenderTexture.GetTemporary(256, 256, 16, (RenderTextureFormat)0);
                            __instance.textures[i] = temporary;
                            camera.targetTexture = temporary;
                        }
                    }
                }
            }

            [HarmonyPatch(typeof(SurveillanceMinigame), nameof(SurveillanceMinigame.Update))]
            class SurveillanceMinigameUpdatePatch
            {
                public static bool Prefix(SurveillanceMinigame __instance)
                {
                    if (!Mode.ModeHandler.isMode(Mode.ModeId.SuperHostRoles) && MapOptions.MapOption.MapOptionSetting.getBool())
                    {
                        cameraTimer += Time.deltaTime;
                        if (cameraTimer > 0.1f)
                            UseCameraTime();

                        if (MapOptions.MapOption.RestrictCamera.getBool())
                        {
                            if (TimeRemaining == null)
                            {
                                TimeRemaining = UnityEngine.Object.Instantiate(FastDestroyableSingleton<HudManager>.Instance.TaskText, __instance.transform);
                                TimeRemaining.alignment = TMPro.TextAlignmentOptions.Center;
                                TimeRemaining.transform.position = Vector3.zero;
                                TimeRemaining.transform.localPosition = new Vector3(0.0f, -1.7f);
                                TimeRemaining.transform.localScale *= 1.8f;
                                TimeRemaining.color = Palette.White;
                            }

                            if (RestrictCameraTime <= 0f)
                            {
                                __instance.Close();
                                return false;
                            }

                            string timeString = TimeSpan.FromSeconds(RestrictCameraTime).ToString(@"mm\:ss\.ff");
                            TimeRemaining.text = String.Format(ModTranslation.getString("timeRemaining"), timeString);
                            TimeRemaining.gameObject.SetActive(true);

                        }

                        // Update normal and securityGuard cameras
                        timer += Time.deltaTime;
                        int numberOfPages = Mathf.CeilToInt(MapUtilities.CachedShipStatus.AllCameras.Length / 4f);

                        bool update = false;

                        if (timer > 3f || Input.GetKeyDown(KeyCode.RightArrow))
                        {
                            update = true;
                            timer = 0f;
                            page = (page + 1) % numberOfPages;
                        }
                        else if (Input.GetKeyDown(KeyCode.LeftArrow))
                        {
                            page = (page + numberOfPages - 1) % numberOfPages;
                            update = true;
                            timer = 0f;
                        }

                        if ((__instance.isStatic || update) && !PlayerTask.PlayerHasTaskOfType<IHudOverrideTask>(PlayerControl.LocalPlayer))
                        {
                            __instance.isStatic = false;
                            for (int i = 0; i < __instance.ViewPorts.Length; i++)
                            {
                                __instance.ViewPorts[i].sharedMaterial = __instance.DefaultMaterial;
                                __instance.SabText[i].gameObject.SetActive(false);
                                if (page * 4 + i < __instance.textures.Length)
                                    __instance.ViewPorts[i].material.SetTexture("_MainTex", __instance.textures[page * 4 + i]);
                                else
                                    __instance.ViewPorts[i].sharedMaterial = __instance.StaticMaterial;
                            }
                        }
                        else if (!__instance.isStatic && PlayerTask.PlayerHasTaskOfType<HudOverrideTask>(PlayerControl.LocalPlayer))
                        {
                            __instance.isStatic = true;
                            for (int j = 0; j < __instance.ViewPorts.Length; j++)
                            {
                                __instance.ViewPorts[j].sharedMaterial = __instance.StaticMaterial;
                                __instance.SabText[j].gameObject.SetActive(true);
                            }
                        }
                        return false;
                    }
                    return true;
                }
            }

            [HarmonyPatch(typeof(SurveillanceMinigame), nameof(SurveillanceMinigame.Close))]
            class SurveillanceMinigameClosePatch
            {
                static void Prefix(SurveillanceMinigame __instance)
                {
                    UseCameraTime();
                }
            }
        }

        [HarmonyPatch]
        class PlanetSurveillanceMinigamePatch
        {
            static TMPro.TextMeshPro TimeRemaining;

            public static void ResetData()
            {
                if (TimeRemaining != null)
                {
                    UnityEngine.Object.Destroy(TimeRemaining);
                    TimeRemaining = null;
                }
            }

            [HarmonyPatch(typeof(PlanetSurveillanceMinigame), nameof(PlanetSurveillanceMinigame.Begin))]
            class PlanetSurveillanceMinigameBeginPatch
            {
                public static void Prefix(PlanetSurveillanceMinigame __instance)
                {
                    cameraTimer = 0f;
                }
            }

            [HarmonyPatch(typeof(PlanetSurveillanceMinigame), nameof(PlanetSurveillanceMinigame.Update))]
            class PlanetSurveillanceMinigameUpdatePatch
            {
                public static bool Prefix(PlanetSurveillanceMinigame __instance)
                {
                    cameraTimer += Time.deltaTime;
                    if (cameraTimer > 0.1f)
                        UseCameraTime();

                    if (MapOptions.MapOption.RestrictCamera.getBool())
                    {
                        if (TimeRemaining == null)
                        {
                            TimeRemaining = UnityEngine.Object.Instantiate(FastDestroyableSingleton<HudManager>.Instance.TaskText, __instance.transform);
                            TimeRemaining.alignment = TMPro.TextAlignmentOptions.BottomRight;
                            TimeRemaining.transform.position = Vector3.zero;
                            TimeRemaining.transform.localPosition = new Vector3(0.95f, 4.45f);
                            TimeRemaining.transform.localScale *= 1.8f;
                            TimeRemaining.color = Palette.White;
                        }

                        if (RestrictCameraTime <= 0f)
                        {
                            __instance.Close();
                            return false;
                        }

                        string timeString = TimeSpan.FromSeconds(RestrictCameraTime).ToString(@"mm\:ss\.ff");
                        TimeRemaining.text = String.Format(ModTranslation.getString("timeRemaining"), timeString);
                        TimeRemaining.gameObject.SetActive(true);
                    }

                    return true;
                }
            }


            [HarmonyPatch(typeof(PlanetSurveillanceMinigame), nameof(PlanetSurveillanceMinigame.Close))]
            class PlanetSurveillanceMinigameClosePatch
            {
                static void Prefix(PlanetSurveillanceMinigame __instance)
                {
                    UseCameraTime();
                }
            }
        }

        [HarmonyPatch]
        class DoorLogPatch
        {
            static TMPro.TextMeshPro TimeRemaining;

            public static void ResetData()
            {
                if (TimeRemaining != null)
                {
                    UnityEngine.Object.Destroy(TimeRemaining);
                    TimeRemaining = null;
                }
            }

            [HarmonyPatch(typeof(Minigame), nameof(Minigame.Begin))]
            class SecurityLogGameBeginPatch
            {
                public static void Prefix(Minigame __instance)
                {
                    if (__instance is SecurityLogGame)
                        cameraTimer = 0f;
                }
            }

            [HarmonyPatch(typeof(SecurityLogGame), nameof(SecurityLogGame.Update))]
            class SecurityLogGameUpdatePatch
            {
                public static bool Prefix(SecurityLogGame __instance)
                {
                    cameraTimer += Time.deltaTime;
                    if (cameraTimer > 0.1f)
                        UseCameraTime();

                    if (MapOptions.MapOption.RestrictCamera.getBool())
                    {
                        if (TimeRemaining == null)
                        {
                            TimeRemaining = UnityEngine.Object.Instantiate(FastDestroyableSingleton<HudManager>.Instance.TaskText, __instance.transform);
                            TimeRemaining.alignment = TMPro.TextAlignmentOptions.BottomRight;
                            TimeRemaining.transform.position = Vector3.zero;
                            TimeRemaining.transform.localPosition = new Vector3(1.0f, 4.25f);
                            TimeRemaining.transform.localScale *= 1.6f;
                            TimeRemaining.color = Palette.White;
                        }

                        if (RestrictCameraTime <= 0f)
                        {
                            __instance.Close();
                            return false;
                        }

                        string timeString = TimeSpan.FromSeconds(RestrictCameraTime).ToString(@"mm\:ss\.ff");
                        TimeRemaining.text = String.Format(ModTranslation.getString("timeRemaining"), timeString);
                        TimeRemaining.gameObject.SetActive(true);
                    }

                    return true;
                }
            }


            [HarmonyPatch]
            class SecurityLogGameClosePatch
            {
                private static IEnumerable<MethodBase> TargetMethods()
                {
                    return typeof(Minigame).GetMethods().Where(x => x.Name == "Close");
                }

                static void Prefix(Minigame __instance)
                {
                    if (__instance is SecurityLogGame)
                        UseCameraTime();
                }
            }
        }
    }
}