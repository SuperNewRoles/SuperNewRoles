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
        public static bool IsRandomMap;
        public static bool ValidationSkeld;
        public static bool ValidationMira;
        public static bool ValidationPolus;
        public static bool ValidationAirship;
        public static bool ValidationSubmerged;
        public static bool IsRestrict;
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
                if (RandomMapOption.getBool())
                {
                    IsRandomMap = true;
                    ValidationSkeld = RandomMapSkeld.getBool();
                    ValidationMira = RandomMapMira.getBool();
                    ValidationPolus = RandomMapPolus.getBool();
                    ValidationAirship = RandomMapAirship.getBool();
                    ValidationSubmerged = RandomMapSubmerged.getBool();
                }
                else
                {
                    IsRandomMap = false;
                    ValidationSkeld = false;
                    ValidationMira = false;
                    ValidationPolus = false;
                    ValidationAirship = false;
                    ValidationSubmerged = false;
                }
                UseDeadBodyReport = !NotUseReportDeadBody.getBool();
                UseMeetingButton = !NotUseMeetingButton.getBool();
                SuperNewRoles.Patch.AdminPatch.ClearAndReload();
                SuperNewRoles.Patch.CameraPatch.ClearAndReload();
                SuperNewRoles.Patch.VitalsPatch.ClearAndReload();
            }
            else
            {
                UseAdmin = true;
                UseVitalOrDoorLog = true;
                UseCamera = true;
                UseDeadBodyReport = true;
                UseMeetingButton = true;
                IsRandomMap = false;
                ValidationSkeld = false;
                ValidationMira = false;
                ValidationPolus = false;
                ValidationAirship = false;
                ValidationSubmerged = false;
            }
            RandomMap.Prefix();
            BlockTool.OldDesyncCommsPlayers = new List<byte>();
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
            PolusReactorTimeLimit.getFloat();
            MiraReactorTimeLimit.getFloat();
            AirshipReactorTimeLimit.getFloat();
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

        public static CustomOption.CustomOption RestrictDevicesOption;
        public static CustomOption.CustomOption RestrictAdmin;
        public static CustomOption.CustomOption IsYkundesuBeplnEx;
        public static CustomOption.CustomOption CanUseAdminTime;
        public static CustomOption.CustomOption RestrictCamera;
        public static CustomOption.CustomOption CanUseCameraTime;
        public static CustomOption.CustomOption RestrictVital;
        public static CustomOption.CustomOption CanUseVitalTime;

        public static CustomOption.CustomOption AddVitalsMira;
        public static CustomOption.CustomOption VentAnimation;

        public static CustomOption.CustomOption ReactorDurationOption;
        public static CustomOption.CustomOption PolusReactorTimeLimit;
        public static CustomOption.CustomOption MiraReactorTimeLimit;
        public static CustomOption.CustomOption AirshipReactorTimeLimit;

        public static CustomOption.CustomOption MapRemodelingOption;
        public static CustomOption.CustomOption AirShipAdditionalVents;

        public static void LoadOption()
        {
            MapOptionSetting = CustomOption.CustomOption.Create(527, true, CustomOptionType.Generic, "MapOptionSetting", false, null, isHeader: true);
            DeviceOptions = CustomOption.CustomOption.Create(528, true, CustomOptionType.Generic, "DeviceOptionsSetting", false, MapOptionSetting);
            DeviceUseAdmin = CustomOption.CustomOption.Create(446, true, CustomOptionType.Generic, "DeviceUseAdminSetting", true, DeviceOptions);
            //DeviceUseAdminTime = CustomOption.CustomOption.Create(447, cs(Color.white, "DeviceTimeSetting"), 10f, 0f, 60f, 1f, DeviceUseAdmin);
            DeviceUseVitalOrDoorLog = CustomOption.CustomOption.Create(448, true, CustomOptionType.Generic, "DeviceUseVitalOrDoorLogSetting", true, DeviceOptions);
            //DeviceUseVitalOrDoorLogTime = CustomOption.CustomOption.Create(449, cs(Color.white, "DeviceTimeSetting"), 10f, 0f, 60f, 1f, DeviceUseVitalOrDoorLog);
            DeviceUseCamera = CustomOption.CustomOption.Create(450, true, CustomOptionType.Generic, "DeviceUseCameraSetting", true, DeviceOptions);
            //DeviceUseCameraTime = CustomOption.CustomOption.Create(451, cs(Color.white, "DeviceTimeSetting"), 10f,0f,60f,1f, DeviceUseCamera);
            NotUseReportDeadBody = CustomOption.CustomOption.Create(452, true, CustomOptionType.Generic, "NotUseReportSetting", false, MapOptionSetting);
            NotUseMeetingButton = CustomOption.CustomOption.Create(453, true, CustomOptionType.Generic, "NotUseMeetingSetting", false, MapOptionSetting);

            RandomMapOption = CustomOption.CustomOption.Create(454, true, CustomOptionType.Generic, "RamdomMapSetting", true, MapOptionSetting);
            RandomMapSkeld = CustomOption.CustomOption.Create(455, true, CustomOptionType.Generic, "RMSkeldSetting", true, RandomMapOption);
            RandomMapMira = CustomOption.CustomOption.Create(456, true, CustomOptionType.Generic, "RMMiraSetting", true, RandomMapOption);
            RandomMapPolus = CustomOption.CustomOption.Create(457, true, CustomOptionType.Generic, "RMPolusSetting", true, RandomMapOption);
            RandomMapAirship = CustomOption.CustomOption.Create(458, true, CustomOptionType.Generic, "RMAirshipSetting", true, RandomMapOption);
            RandomMapSubmerged = CustomOption.CustomOption.Create(459, true, CustomOptionType.Generic, "RMSubmergedSetting", true, RandomMapOption);
            //RM??��?��??��?��RandomMap??��?��̗�??��?��ł�()

            RestrictDevicesOption = CustomOption.CustomOption.Create(460, false, CustomOptionType.Generic, "RestrictDevicesSetting", true, MapOptionSetting);
            RestrictAdmin = CustomOption.CustomOption.Create(461, false, CustomOptionType.Generic, "RestrictAdminSetting", false, RestrictDevicesOption);
            IsYkundesuBeplnEx = CustomOption.CustomOption.Create(462, false, CustomOptionType.Generic, "IsYkundesuBeplnExSetting", false, RestrictAdmin);
            CanUseAdminTime = CustomOption.CustomOption.Create(463, false, CustomOptionType.Generic, "DeviceTimeSetting", 10f, 0f, 300f, 2.5f, RestrictAdmin);
            RestrictCamera = CustomOption.CustomOption.Create(464, false, CustomOptionType.Generic, "RestrictCameraSetting", false, RestrictDevicesOption);
            CanUseCameraTime = CustomOption.CustomOption.Create(465, false, CustomOptionType.Generic, "DeviceTimeSetting", 10f, 0f, 300f, 2.5f, RestrictCamera);
            RestrictVital = CustomOption.CustomOption.Create(466, false, CustomOptionType.Generic, "RestrictVitalSetting", false, RestrictDevicesOption);
            CanUseVitalTime = CustomOption.CustomOption.Create(467, false, CustomOptionType.Generic, "DeviceTimeSetting", 10f, 0f, 300f, 2.5f, RestrictVital);

            ReactorDurationOption = CustomOption.CustomOption.Create(468, true, CustomOptionType.Generic, "ReactorDurationSetting", false, MapOptionSetting);
            PolusReactorTimeLimit = CustomOption.CustomOption.Create(469, true, CustomOptionType.Generic, "PolusReactorTime", 30f, 0f, 100f, 1f, ReactorDurationOption);
            MiraReactorTimeLimit = CustomOption.CustomOption.Create(470, true, CustomOptionType.Generic, "MiraReactorTime", 30f, 0f, 100f, 1f, ReactorDurationOption);
            AirshipReactorTimeLimit = CustomOption.CustomOption.Create(471, true, CustomOptionType.Generic, "AirshipReactorTime", 30f, 0f, 100f, 1f, ReactorDurationOption);

            AddVitalsMira = CustomOption.CustomOption.Create(472, false, CustomOptionType.Generic, "AddVitalsMiraSetting", false, MapOptionSetting);

            VentAnimation = CustomOption.CustomOption.Create(529, false, CustomOptionType.Generic, "VentAnimation", false, MapOptionSetting);
            MapRemodelingOption = CustomOption.CustomOption.Create(473, false, CustomOptionType.Generic, "MapRemodelingOptionSetting", false, MapOptionSetting);
            AirShipAdditionalVents = CustomOption.CustomOption.Create(474, false, CustomOptionType.Generic, "AirShipAdditionalVents", false, MapRemodelingOption);

        }
    }
}