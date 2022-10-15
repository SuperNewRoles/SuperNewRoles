using System;
using System.Collections.Generic;
using HarmonyLib;
using SuperNewRoles.Mode.SuperHostRoles;
using SuperNewRoles.Patches;
using UnityEngine;
using static SuperNewRoles.Modules.CustomOptions;

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
                //SuperNewRoles.Patches.AdminPatch.ClearAndReload();
                //SuperNewRoles.Patches.CameraPatch.ClearAndReload();
                //SuperNewRoles.Patches.VitalsPatch.ClearAndReload();
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
        public static CustomOption MapOptionSetting;
        public static CustomOption DeviceOptions;
        public static CustomOption DeviceUseAdmin;

        //public static CustomOption DeviceUseAdminTime;
        public static CustomOption DeviceUseVitalOrDoorLog;
        //public static CustomOption DeviceUseVitalOrDoorLogTime;
        public static CustomOption DeviceUseCamera;
        //public static CustomOption DeviceUseCameraTime;
        public static CustomOption NotUseReportDeadBody;
        public static CustomOption NotUseMeetingButton;
        public static CustomOption RandomMapOption;
        public static CustomOption RandomMapSkeld;
        public static CustomOption RandomMapMira;
        public static CustomOption RandomMapPolus;
        public static CustomOption RandomMapAirship;

        public static CustomOption RestrictDevicesOption;
        public static CustomOption RestrictAdmin;
        public static CustomOption IsYkundesuBeplnEx;
        public static CustomOption CanUseAdminTime;
        public static CustomOption RestrictCamera;
        public static CustomOption CanUseCameraTime;
        public static CustomOption RestrictVital;
        public static CustomOption CanUseVitalTime;

        public static CustomOption RandomSpawnOption;

        public static CustomOption ReactorDurationOption;
        public static CustomOption PolusReactorTimeLimit;
        public static CustomOption MiraReactorTimeLimit;
        public static CustomOption AirshipReactorTimeLimit;
        public static Dictionary<byte, PoolablePlayer> playerIcons = new();
        public static CustomOption WireTaskIsRandomOption;
        public static CustomOption WireTaskNumOption;

        public static CustomOption VentAnimation;


        public static void LoadOption()
        {
            MapOptionSetting = CustomOption.Create(527, true, CustomOptionType.Generic, "MapOptionSetting", false, null, isHeader: true);
            DeviceOptions = CustomOption.Create(528, true, CustomOptionType.Generic, "DeviceOptionsSetting", false, MapOptionSetting);
            DeviceUseAdmin = CustomOption.Create(446, true, CustomOptionType.Generic, "DeviceUseAdminSetting", true, DeviceOptions);
            //DeviceUseAdminTime = CustomOption.Create(447, Cs(Color.white, "DeviceTimeSetting"), 10f, 0f, 60f, 1f, DeviceUseAdmin);
            DeviceUseVitalOrDoorLog = CustomOption.Create(448, true, CustomOptionType.Generic, "DeviceUseVitalOrDoorLogSetting", true, DeviceOptions);
            //DeviceUseVitalOrDoorLogTime = CustomOption.Create(449, Cs(Color.white, "DeviceTimeSetting"), 10f, 0f, 60f, 1f, DeviceUseVitalOrDoorLog);
            DeviceUseCamera = CustomOption.Create(450, true, CustomOptionType.Generic, "DeviceUseCameraSetting", true, DeviceOptions);
            //DeviceUseCameraTime = CustomOption.Create(451, Cs(Color.white, "DeviceTimeSetting"), 10f,0f,60f,1f, DeviceUseCamera);
            NotUseReportDeadBody = CustomOption.Create(452, true, CustomOptionType.Generic, "NotUseReportSetting", false, MapOptionSetting);
            NotUseMeetingButton = CustomOption.Create(453, true, CustomOptionType.Generic, "NotUseMeetingSetting", false, MapOptionSetting);

            RandomMapOption = CustomOption.Create(454, true, CustomOptionType.Generic, "RamdomMapSetting", true, MapOptionSetting);
            RandomMapSkeld = CustomOption.Create(455, true, CustomOptionType.Generic, "RMSkeldSetting", true, RandomMapOption);
            RandomMapMira = CustomOption.Create(456, true, CustomOptionType.Generic, "RMMiraSetting", true, RandomMapOption);
            RandomMapPolus = CustomOption.Create(457, true, CustomOptionType.Generic, "RMPolusSetting", true, RandomMapOption);
            RandomMapAirship = CustomOption.Create(458, true, CustomOptionType.Generic, "RMAirshipSetting", true, RandomMapOption);

            RandomSpawnOption = CustomOption.Create(955, false, CustomOptionType.Generic, "RandomSpawnOption", false, MapOptionSetting);

            ReactorDurationOption = CustomOption.Create(468, true, CustomOptionType.Generic, "ReactorDurationSetting", false, MapOptionSetting);
            PolusReactorTimeLimit = CustomOption.Create(469, true, CustomOptionType.Generic, "PolusReactorTime", 30f, 0f, 100f, 1f, ReactorDurationOption);
            MiraReactorTimeLimit = CustomOption.Create(470, true, CustomOptionType.Generic, "MiraReactorTime", 30f, 0f, 100f, 1f, ReactorDurationOption);
            AirshipReactorTimeLimit = CustomOption.Create(471, true, CustomOptionType.Generic, "AirshipReactorTime", 30f, 0f, 100f, 1f, ReactorDurationOption);

            VentAnimation = CustomOption.Create(600, false, CustomOptionType.Generic, "VentAnimation", false, MapOptionSetting);

            WireTaskIsRandomOption = CustomOption.Create(956, false, CustomOptionType.Generic, "WireTaskIsRandom", false, MapOptionSetting);
            WireTaskNumOption = CustomOption.Create(957, false, CustomOptionType.Generic, "WireTaskNum", 5f, 1f, 8f, 1f, WireTaskIsRandomOption);

            CustomOptions.LadderDead = CustomOption.Create(637, true, CustomOptionType.Generic, "LadderDead", false, isHeader: true);
            LadderDeadChance = CustomOption.Create(625, true, CustomOptionType.Generic, "LadderDeadChance", rates[1..], CustomOptions.LadderDead);
        }
    }
}