using HarmonyLib;
using System.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SuperNewRoles.Roles;
using SuperNewRoles.CustomOption;
using static SuperNewRoles.CustomOption.CustomOptions;
using SuperNewRoles.Mode.SuperHostRoles;

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
        public static bool ValidationSkeld;
        public static bool ValidationMira;
        public static bool ValidationPolus;
        public static bool ValidationAirship;
        public static bool ValidationSubmerged;
        public static void ClearAndReload()
        {
            if (MapOptionSetting.getBool())
            {
                if (DeviceOptions.getBool())
                {
                    UseAdmin = DeviceUseAdmin.getBool();
                    UseVitalOrDoorLog = DeviceUseVitalOrDoorLog.getBool();
                    UseCamera = DeviceUseCamera.getBool();
                    ValidationSkeld = RandomMapSkeld.getBool();
                    ValidationMira = RandomMapMira.getBool();
                    ValidationPolus = RandomMapPolus.getBool();
                    ValidationAirship = RandomMapAirship.getBool();
                    ValidationSubmerged = RandomMapSubmerged.getBool();
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
            BlockTool.CameraPlayers = new List<byte>();
            //BlockTool.VitalPlayers = new List<byte>();
            //BlockTool.AdminPlayers = new List<byte>();
            /*
            if (DeviceUseCameraTime.getFloat() == 0 || !UseCamera)
            {
                BlockTool.CameraTime = -10;
            } else
            {
                BlockTool.CameraTime = DeviceUseCameraTime.getFloat();
            }
            if (DeviceUseVitalOrDoorLogTime.getFloat() == 0 || !UseVitalOrDoorLog)
            {
                BlockTool.VitalTime = -10;
            }
            else
            {
                BlockTool.VitalTime = DeviceUseVitalOrDoorLogTime.getFloat();
            }
            if (DeviceUseAdminTime.getFloat() == 0 || !UseAdmin)
            {
                BlockTool.AdminTime = -10;
            }
            else
            {
                BlockTool.AdminTime = DeviceUseAdminTime.getFloat();
            }*/
        }
        public static CustomOption.CustomOption MapOptionSetting;
        public static CustomOption.CustomOption DeviceOptions;
        public static CustomOption.CustomOption DeviceUseAdmin;
        //public static CustomOption.CustomOption DeviceUseAdminTime;
        public static CustomOption.CustomOption DeviceUseVitalOrDoorLog;
        //public static CustomOption.CustomOption DeviceUseVitalOrDoorLogTime;
        public static CustomOption.CustomOption DeviceUseCamera;
        //public static CustomOption.CustomOption DeviceUseCameraTime;
        public static CustomOption.CustomOption NotUseReportDeadBody;
        public static CustomOption.CustomOption NotUseMeetingButton;
        public static CustomOption.CustomOption RandomMapOption;
        public static CustomOption.CustomOption RandomMapSkeld;
        public static CustomOption.CustomOption RandomMapMira;
        public static CustomOption.CustomOption RandomMapPolus;
        public static CustomOption.CustomOption RandomMapAirship;
        public static CustomOption.CustomOption RandomMapSubmerged;

        public static void LoadOption() {
            MapOptionSetting = CustomOption.CustomOption.Create(246, true, CustomOptionType.Generic, "MapOptionSetting", false, null, isHeader: true);
            DeviceOptions = CustomOption.CustomOption.Create(115, true, CustomOptionType.Generic, "DeviceOptionsSetting", false, MapOptionSetting);
            DeviceUseAdmin = CustomOption.CustomOption.Create(116, true, CustomOptionType.Generic, "DeviceUseAdminSetting", true, DeviceOptions);
            //DeviceUseAdminTime = CustomOption.CustomOption.Create(274, cs(Color.white, "DeviceTimeSetting"), 10f, 0f, 60f, 1f, DeviceUseAdmin);
            DeviceUseVitalOrDoorLog = CustomOption.CustomOption.Create(117, true, CustomOptionType.Generic, "DeviceUseVitalOrDoorLogSetting", true, DeviceOptions);
            //DeviceUseVitalOrDoorLogTime = CustomOption.CustomOption.Create(273, cs(Color.white, "DeviceTimeSetting"), 10f, 0f, 60f, 1f, DeviceUseVitalOrDoorLog);
            DeviceUseCamera = CustomOption.CustomOption.Create(118, true, CustomOptionType.Generic, "DeviceUseCameraSetting", true, DeviceOptions);
            //DeviceUseCameraTime = CustomOption.CustomOption.Create(272, cs(Color.white, "DeviceTimeSetting"), 10f,0f,60f,1f, DeviceUseCamera);
            NotUseReportDeadBody = CustomOption.CustomOption.Create(247, true, CustomOptionType.Generic, "NotUseReportSetting", false, MapOptionSetting);
            NotUseMeetingButton = CustomOption.CustomOption.Create(248, true, CustomOptionType.Generic, "NotUseMeetingSetting", false, MapOptionSetting);
            RandomMapOption = CustomOption.CustomOption.Create(369, true, CustomOptionType.Generic, "RamdomMapSetting", true, MapOptionSetting);
            RandomMapSkeld = CustomOption.CustomOption.Create(370, true, CustomOptionType.Generic, "RMSkeldSetting", true, RandomMapOption);
            RandomMapMira = CustomOption.CustomOption.Create(371, true, CustomOptionType.Generic, "RMMiraSetting", true, RandomMapOption);
            RandomMapPolus = CustomOption.CustomOption.Create(372, true, CustomOptionType.Generic, "RMPolusSetting", true, RandomMapOption);
            RandomMapAirship = CustomOption.CustomOption.Create(373, true, CustomOptionType.Generic, "RMAirshipSetting", true, RandomMapOption);
            RandomMapSubmerged = CustomOption.CustomOption.Create(373, true, CustomOptionType.Generic, "RMSubmergedSetting", true, RandomMapOption);
            //RM‚ÍRandomMap‚Ì—ª‚Å‚·()
        }
    }
}