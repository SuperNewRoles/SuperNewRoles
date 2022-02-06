using HarmonyLib;
using System.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SuperNewRoles.Roles;
using SuperNewRoles.CustomOption;

namespace SuperNewRoles.MapOptions
{ 
    [HarmonyPatch]
    public class MapOption
    {
        public static bool UseAdmin;
        public static bool UseVitalOrDoorLog;
        public static bool UseCamera;
        public static void ClearAndReload()
        {
            if (CustomOptions.DeviceOptions.getBool() == true)
            {
                UseAdmin = CustomOptions.DeviceUseAdmin.getBool();
                UseVitalOrDoorLog = CustomOptions.DeviceUseVitalOrDoorLog.getBool();
                UseCamera = CustomOptions.DeviceUseCamera.getBool();
            } else
            {
                UseAdmin = true;
                UseVitalOrDoorLog = true;
                UseCamera = true;
            }
        }
    }
}