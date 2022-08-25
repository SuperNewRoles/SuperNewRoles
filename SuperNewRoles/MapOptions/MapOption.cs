using System;
using System.Collections.Generic;
using HarmonyLib;
using SuperNewRoles.CustomOption;
using SuperNewRoles.Mode.SuperHostRoles;
using UnityEngine;
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
        public static bool IsRandomMap;
        public static bool ValidationSkeld;
        public static bool ValidationMira;
        public static bool ValidationPolus;
        public static bool ValidationAirship;
        public static bool ValidationSubmerged;
        public static bool IsRestrict;

        public static bool RandomSpawn;
        //タスク関連
        public static bool WireTaskIsRandom;
        public static int WireTaskNum;

        //千里眼・ズーム関連
        public static bool MouseZoom;
        public static bool ClairvoyantZoom;
        public static float CoolTime;
        public static float DurationTime;
        public static bool IsZoomOn;
        public static float Timer;
        public static DateTime ButtonTimer;
        //private static Sprite buttonSprite;
        public static float Default;
        public static float CameraDefault;
        public static void ClearAndReload()
        {
            if (MapOptionSetting.GetBool())
            {
                if (DeviceOptions.GetBool())
                {
                    UseAdmin = DeviceUseAdmin.GetBool();
                    UseVitalOrDoorLog = DeviceUseVitalOrDoorLog.GetBool();
                    UseCamera = DeviceUseCamera.GetBool();
                }
                else
                {
                    UseAdmin = true;
                    UseVitalOrDoorLog = true;
                    UseCamera = true;
                }
                if (RandomMapOption.GetBool())
                {
                    IsRandomMap = true;
                    ValidationSkeld = RandomMapSkeld.GetBool();
                    ValidationMira = RandomMapMira.GetBool();
                    ValidationPolus = RandomMapPolus.GetBool();
                    ValidationAirship = RandomMapAirship.GetBool();
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
                RandomSpawn = (MapNames)PlayerControl.GameOptions.MapId == MapNames.Airship && RandomSpawnOption.GetBool();
                WireTaskIsRandom = WireTaskIsRandomOption.GetBool();
                WireTaskNum = WireTaskNumOption.GetInt();
                UseDeadBodyReport = !NotUseReportDeadBody.GetBool();
                UseMeetingButton = !NotUseMeetingButton.GetBool();
                //SuperNewRoles.Patch.AdminPatch.ClearAndReload();
                //SuperNewRoles.Patch.CameraPatch.ClearAndReload();
                //SuperNewRoles.Patch.VitalsPatch.ClearAndReload();
            }
            else
            {
                RandomSpawn = false;
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
                WireTaskIsRandom = false;
            }
            BlockTool.OldDesyncCommsPlayers = new();
            BlockTool.CameraPlayers = new();
            //BlockTool.VitalPlayers = new();
            //BlockTool.AdminPlayers = new();
            /*
            if (DeviceUseCameraTime.GetFloat() == 0 || !UseCamera)
            {
                BlockTool.CameraTime = -10;
            } else
            {
                BlockTool.CameraTime = DeviceUseCameraTime.GetFloat();
            }
            if (DeviceUseVitalOrDoorLogTime.GetFloat() == 0 || !UseVitalOrDoorLog)
            {
                BlockTool.VitalTime = -10;
            }
            else
            {
                BlockTool.VitalTime = DeviceUseVitalOrDoorLogTime.GetFloat();
            }
            if (DeviceUseAdminTime.GetFloat() == 0 || !UseAdmin)
            {
                BlockTool.AdminTime = -10;
            }
            else
            {
                BlockTool.AdminTime = DeviceUseAdminTime.GetFloat();
            }*/
            PolusReactorTimeLimit.GetFloat();
            MiraReactorTimeLimit.GetFloat();
            AirshipReactorTimeLimit.GetFloat();

            //千里眼・ズーム関連
            ClairvoyantZoom = CustomOptions.ClairvoyantZoom.GetBool();
            MouseZoom = CustomOptions.MouseZoom.GetBool();
            CoolTime = ZoomCoolTime.GetFloat();
            DurationTime = ZoomDurationTime.GetFloat();
            IsZoomOn = false;
            Timer = 0;
            ButtonTimer = DateTime.Now;
            CameraDefault = Camera.main.orthographicSize;
            Default = FastDestroyableSingleton<HudManager>.Instance.UICamera.orthographicSize;
            playerIcons = new();
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

        public static CustomOption.CustomOption RestrictDevicesOption;
        public static CustomOption.CustomOption RestrictAdmin;
        public static CustomOption.CustomOption IsYkundesuBeplnEx;
        public static CustomOption.CustomOption CanUseAdminTime;
        public static CustomOption.CustomOption RestrictCamera;
        public static CustomOption.CustomOption CanUseCameraTime;
        public static CustomOption.CustomOption RestrictVital;
        public static CustomOption.CustomOption CanUseVitalTime;

        public static CustomOption.CustomOption RandomSpawnOption;

        public static CustomOption.CustomOption ReactorDurationOption;
        public static CustomOption.CustomOption PolusReactorTimeLimit;
        public static CustomOption.CustomOption MiraReactorTimeLimit;
        public static CustomOption.CustomOption AirshipReactorTimeLimit;
        public static Dictionary<byte, PoolablePlayer> playerIcons = new();
        public static CustomOption.CustomOption WireTaskIsRandomOption;
        public static CustomOption.CustomOption WireTaskNumOption;

        public static CustomOption.CustomOption VentAnimation;


        public static void LoadOption()
        {
            MapOptionSetting = CustomOption.CustomOption.Create(527, true, CustomOptionType.Generic, "MapOptionSetting", false, null, isHeader: true);
            DeviceOptions = CustomOption.CustomOption.Create(528, true, CustomOptionType.Generic, "DeviceOptionsSetting", false, MapOptionSetting);
            DeviceUseAdmin = CustomOption.CustomOption.Create(446, true, CustomOptionType.Generic, "DeviceUseAdminSetting", true, DeviceOptions);
            //DeviceUseAdminTime = CustomOption.CustomOption.Create(447, Cs(Color.white, "DeviceTimeSetting"), 10f, 0f, 60f, 1f, DeviceUseAdmin);
            DeviceUseVitalOrDoorLog = CustomOption.CustomOption.Create(448, true, CustomOptionType.Generic, "DeviceUseVitalOrDoorLogSetting", true, DeviceOptions);
            //DeviceUseVitalOrDoorLogTime = CustomOption.CustomOption.Create(449, Cs(Color.white, "DeviceTimeSetting"), 10f, 0f, 60f, 1f, DeviceUseVitalOrDoorLog);
            DeviceUseCamera = CustomOption.CustomOption.Create(450, true, CustomOptionType.Generic, "DeviceUseCameraSetting", true, DeviceOptions);
            //DeviceUseCameraTime = CustomOption.CustomOption.Create(451, Cs(Color.white, "DeviceTimeSetting"), 10f,0f,60f,1f, DeviceUseCamera);
            NotUseReportDeadBody = CustomOption.CustomOption.Create(452, true, CustomOptionType.Generic, "NotUseReportSetting", false, MapOptionSetting);
            NotUseMeetingButton = CustomOption.CustomOption.Create(453, true, CustomOptionType.Generic, "NotUseMeetingSetting", false, MapOptionSetting);

            RandomMapOption = CustomOption.CustomOption.Create(454, true, CustomOptionType.Generic, "RamdomMapSetting", true, MapOptionSetting);
            RandomMapSkeld = CustomOption.CustomOption.Create(455, true, CustomOptionType.Generic, "RMSkeldSetting", true, RandomMapOption);
            RandomMapMira = CustomOption.CustomOption.Create(456, true, CustomOptionType.Generic, "RMMiraSetting", true, RandomMapOption);
            RandomMapPolus = CustomOption.CustomOption.Create(457, true, CustomOptionType.Generic, "RMPolusSetting", true, RandomMapOption);
            RandomMapAirship = CustomOption.CustomOption.Create(458, true, CustomOptionType.Generic, "RMAirshipSetting", true, RandomMapOption);
            /*
                        RestrictDevicesOption = CustomOption.CustomOption.Create(460, false, CustomOptionType.Generic, "RestrictDevicesSetting", true, MapOptionSetting);
                        RestrictAdmin = CustomOption.CustomOption.Create(461, false, CustomOptionType.Generic, "RestrictAdminSetting", false, RestrictDevicesOption);
                        IsYkundesuBeplnEx = CustomOption.CustomOption.Create(462, false, CustomOptionType.Generic, "IsYkundesuBeplnExSetting", false, RestrictAdmin);
                        CanUseAdminTime = CustomOption.CustomOption.Create(463, false, CustomOptionType.Generic, "DeviceTimeSetting", 10f, 0f, 300f, 2.5f, RestrictAdmin);
                        RestrictCamera = CustomOption.CustomOption.Create(464, false, CustomOptionType.Generic, "RestrictCameraSetting", false, RestrictDevicesOption);
                        CanUseCameraTime = CustomOption.CustomOption.Create(465, false, CustomOptionType.Generic, "DeviceTimeSetting", 10f, 0f, 300f, 2.5f, RestrictCamera);
                        RestrictVital = CustomOption.CustomOption.Create(466, false, CustomOptionType.Generic, "RestrictVitalSetting", false, RestrictDevicesOption);
                        CanUseVitalTime = CustomOption.CustomOption.Create(467, false, CustomOptionType.Generic, "DeviceTimeSetting", 10f, 0f, 300f, 2.5f, RestrictVital);
            */

            RandomSpawnOption = CustomOption.CustomOption.Create(955, false, CustomOptionType.Generic, "RandomSpawnOption", false, null);

            ReactorDurationOption = CustomOption.CustomOption.Create(468, true, CustomOptionType.Generic, "ReactorDurationSetting", false, MapOptionSetting);
            PolusReactorTimeLimit = CustomOption.CustomOption.Create(469, true, CustomOptionType.Generic, "PolusReactorTime", 30f, 0f, 100f, 1f, ReactorDurationOption);
            MiraReactorTimeLimit = CustomOption.CustomOption.Create(470, true, CustomOptionType.Generic, "MiraReactorTime", 30f, 0f, 100f, 1f, ReactorDurationOption);
            AirshipReactorTimeLimit = CustomOption.CustomOption.Create(471, true, CustomOptionType.Generic, "AirshipReactorTime", 30f, 0f, 100f, 1f, ReactorDurationOption);

            VentAnimation = CustomOption.CustomOption.Create(600, false, CustomOptionType.Generic, "VentAnimation", false, MapOptionSetting);

            WireTaskIsRandomOption = CustomOption.CustomOption.Create(956, false, CustomOptionType.Generic, "WireTaskIsRandom", false, MapOptionSetting);
            WireTaskNumOption = CustomOption.CustomOption.Create(957, false, CustomOptionType.Generic, "WireTaskNum", 5f,1f,8f,1f, WireTaskIsRandomOption);

            LadderDead = CustomOption.CustomOption.Create(637, true, CustomOptionType.Generic, "LadderDead", false, isHeader: true);
            LadderDeadChance = CustomOption.CustomOption.Create(625, true, CustomOptionType.Generic, "LadderDeadChance", rates[1..], LadderDead);
        }
    }
}