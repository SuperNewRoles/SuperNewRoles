using HarmonyLib;
using System.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SuperNewRoles.Roles;
using SuperNewRoles.CustomOption;
using static SuperNewRoles.CustomOption.CustomOptions;

namespace SuperNewRoles.MapOptions
{ 
    [HarmonyPatch]
    public class MapOption
    {
        public static bool UseAdmin;
        public static bool UseVitalOrDoorLog;
        public static bool UseCamera;
        public static bool UseDeadBodyReport;
        public static bool UseMeetingButton;
        public static void ClearAndReload()
        {
            if (MapOptionSetting.getBool())
            {
                if (DeviceOptions.getBool())
                {
                    UseAdmin = DeviceUseAdmin.getBool();
                    UseVitalOrDoorLog = DeviceUseVitalOrDoorLog.getBool();
                    UseCamera = DeviceUseCamera.getBool();
                }
                else
                {
                    UseAdmin = true;
                    UseVitalOrDoorLog = true;
                    UseCamera = true;
                }
                UseDeadBodyReport = !NotUseReportDeadBody.getBool();
                UseMeetingButton = !NotUseMeetingButton.getBool();
            } else
            {
                UseAdmin = true;
                UseVitalOrDoorLog = true;
                UseCamera = true;
                UseDeadBodyReport = true;
                UseMeetingButton = true;
            }
        }
        public static CustomOption.CustomOption MapOptionSetting;
        public static CustomOption.CustomOption DeviceOptions;
        public static CustomOption.CustomOption DeviceUseAdmin;
        public static CustomOption.CustomOption DeviceUseVitalOrDoorLog;
        public static CustomOption.CustomOption DeviceUseCamera;
        public static CustomOption.CustomOption NotUseReportDeadBody;
        public static CustomOption.CustomOption NotUseMeetingButton;
        public static void LoadOption() {
            MapOptionSetting = CustomOption.CustomOption.Create(246, cs(Color.white, "MapOptionSetting"), false, null, isHeader: true);
            DeviceOptions = CustomOption.CustomOption.Create(115, cs(Color.white, "DeviceOptionsSetting"), false, MapOptionSetting);
            DeviceUseAdmin = CustomOption.CustomOption.Create(116, cs(Color.white, "DeviceUseAdminSetting"), true, DeviceOptions);
            DeviceUseVitalOrDoorLog = CustomOption.CustomOption.Create(117, cs(Color.white, "DeviceUseVitalOrDoorLogSetting"), true, DeviceOptions);
            DeviceUseCamera = CustomOption.CustomOption.Create(118, cs(Color.white, "DeviceUseCameraSetting"), true, DeviceOptions);
            NotUseReportDeadBody = CustomOption.CustomOption.Create(247, cs(Color.white, "NotUseReportSetting"), false, MapOptionSetting);
            NotUseMeetingButton = CustomOption.CustomOption.Create(248, cs(Color.white, "NotUseMeetingSetting"), false, MapOptionSetting);
        }
    }
}