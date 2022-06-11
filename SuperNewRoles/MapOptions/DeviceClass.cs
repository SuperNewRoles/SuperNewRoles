using HarmonyLib;
using SuperNewRoles.Helpers;
using System;
using System.Collections.Generic;
using System.Text;
using SuperNewRoles.CustomRPC;
using Hazel;
using SuperNewRoles.Mode.SuperHostRoles;
using UnityEngine;

namespace SuperNewRoles.MapOptions
{
    public class DeviceClass
    {
        [HarmonyPatch(typeof(MapConsole), nameof(MapConsole.Use))]
        public static class MapConsoleUsePatch
        {
            public static bool Prefix(MapConsole __instance)
            {
                bool IsUse = MapOption.UseAdmin;/*
                if (MapOption.UseAdmin)
                {
                    if (BlockTool.AdminTime != -10 && BlockTool.AdminTime <= 0)
                    {
                        IsUse = false;
                    }
                }*/
                return IsUse;
            }
        }
        [HarmonyPatch(typeof(MapCountOverlay), nameof(MapCountOverlay.Update))]
        class MapCountOverlayUpdatePatch
        {
            public static bool Prefix(MapConsole __instance)
            {
                bool IsUse = MapOption.UseAdmin;
                /*
                if (MapOption.UseAdmin)
                {
                    if (BlockTool.AdminTime > 0)
                    {
                        BlockTool.AdminTime -= Time.fixedDeltaTime;
                        MessageWriter writer = RPCHelper.StartRPC(CustomRPC.CustomRPC.SetDeviceTime);
                        writer.Write(BlockTool.AdminTime);
                        writer.Write((byte)SystemTypes.Admin);
                        writer.EndRPC();
                    }
                    else if (BlockTool.AdminTime != -10 && BlockTool.AdminTime < 0.1)
                    {
                        SuperNewRolesPlugin.Logger.LogInfo("アドミンを無効に設定！");
                        IsUse = false;
                    }
                }*/
                return IsUse;
            }
        }
        [HarmonyPatch(typeof(VitalsMinigame), nameof(VitalsMinigame.Update))]
        class VitalsDevice
        {
            static void Postfix(VitalsMinigame __instance)
            {
                if (!MapOption.UseVitalOrDoorLog)
                {
                    __instance.Close();
                } else
                {
                    /*
                    if (BlockTool.VitalTime > 0)
                    {
                        BlockTool.VitalTime -= Time.fixedDeltaTime;
                        MessageWriter writer = RPCHelper.StartRPC(CustomRPC.CustomRPC.SetDeviceTime);
                        writer.Write(BlockTool.VitalTime);
                        writer.Write((byte)SystemTypes.Medical);
                        writer.EndRPC();
                    }
                    else if (BlockTool.CameraTime != -10)
                    {
                        __instance.Close();
                    }
                    */
                }
            }
        }
        [HarmonyPatch(typeof(SurveillanceMinigame), nameof(SurveillanceMinigame.Update))]
        class SurveillanceMinigameUpdatePatch
        {
            public static void Postfix(SurveillanceMinigame __instance)
            {
                if (MapOption.UseCamera == false)
                {
                    __instance.Close();
                } else
                {
                    /*
                    if (BlockTool.CameraTime > 0)
                    {
                        BlockTool.CameraTime -= Time.fixedDeltaTime;
                        MessageWriter writer = RPCHelper.StartRPC(CustomRPC.CustomRPC.SetDeviceTime);
                        writer.Write(BlockTool.CameraTime);
                        writer.Write((byte)SystemTypes.Security);
                        writer.EndRPC();
                    }
                    else if (BlockTool.CameraTime != -10)
                    {
                        __instance.Close();
                    }*/
                }
            }
        }

        [HarmonyPatch(typeof(PlanetSurveillanceMinigame), nameof(PlanetSurveillanceMinigame.Update))]
        class PlanetSurveillanceMinigameUpdatePatch
        {
            public static void Postfix(PlanetSurveillanceMinigame __instance)
            {
                if (MapOption.UseCamera == false)
                {
                    __instance.Close();
                } else
                {/*
                    if (BlockTool.CameraTime > 0)
                    {
                        BlockTool.CameraTime -= Time.fixedDeltaTime;
                        MessageWriter writer = RPCHelper.StartRPC(CustomRPC.CustomRPC.SetDeviceTime);
                        writer.Write(BlockTool.CameraTime);
                        writer.Write((byte)SystemTypes.Security);
                        writer.EndRPC();
                    }
                    else if (BlockTool.CameraTime != -10)
                    {
                        __instance.Close();
                    }*/
                }
            }
        }

        [HarmonyPatch(typeof(SecurityLogGame), nameof(SecurityLogGame.Update))]
        class SecurityLogGameUpdatePatch
        {
            public static void Postfix(SecurityLogGame __instance)
            {
                if (MapOption.UseVitalOrDoorLog == false)
                {
                    __instance.Close();
                } else
                {/*
                    if (BlockTool.VitalTime > 0)
                    {
                        BlockTool.VitalTime -= Time.fixedDeltaTime;
                        MessageWriter writer = RPCHelper.StartRPC(CustomRPC.CustomRPC.SetDeviceTime);
                        writer.Write(BlockTool.VitalTime);
                        writer.Write((byte)SystemTypes.Medical);
                        writer.EndRPC();
                    } else if(BlockTool.VitalTime != -10){
                        __instance.Close();
                    }*/
                }
            }
        }
    }
}
