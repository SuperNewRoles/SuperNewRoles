using System;
using System.Collections.Generic;
using HarmonyLib;
using SuperNewRoles.Mode;
using SuperNewRoles.Mode.SuperHostRoles;
using UnityEngine;
using static SuperNewRoles.Modules.CustomOption;
using static SuperNewRoles.Modules.CustomOptionHolder;

namespace SuperNewRoles.MapOption;

[HarmonyPatch]
public class MapOption
{
    // |:========== マップの設定 ==========:|
    public static CustomOption MapOptionSetting;

    // |:========== 情報機器制限の設定 ==========:|
    public static CustomOption DeviceOptions;

    // |:========== ON/OFFの設定 ==========:|
    public static CustomOption CanUseDeviceSetting;
    public static CustomOption DeviceUseAdmin;
    public static CustomOption DeviceUseVitalOrDoorLog;
    public static CustomOption DeviceUseCamera;

    // |:========== 時間制限の設定 ==========:|
    public static CustomOption RestrictDevicesTimeOption;
    public static CustomOption RestrictAdmin;
    public static CustomOption DeviceUseAdminTime;
    public static CustomOption RestrictVital;
    public static CustomOption DeviceUseVitalOrDoorLogTime;
    public static CustomOption RestrictCamera;
    public static CustomOption DeviceUseCameraTime;

    // |:========== リアクター継続時間の設定 ==========:|
    public static CustomOption ReactorDurationOption;
    public static CustomOption SkeldReactorTimeLimit;
    public static CustomOption SkeldLifeSuppTimeLimit;
    public static CustomOption MiraReactorTimeLimit;
    public static CustomOption MiraLifeSuppTimeLimit;
    public static CustomOption PolusReactorTimeLimit;
    public static CustomOption AirshipReactorTimeLimit;
    public static CustomOption FungleReactorTimeLimit;

    // |:========== ベントアニメーション有効化の設定 ==========:|
    public static CustomOption VentAnimationPlaySetting;

    // |:========== 配線タスクランダムの設定 ==========:|
    public static CustomOption WireTaskIsRandomOption;
    public static CustomOption WireTaskNumOption;

    // |:========== ランダムマップの設定 ==========:|
    public static CustomOption RandomMapOption;
    public static CustomOption RandomMapSkeld;
    public static CustomOption RandomMapMira;
    public static CustomOption RandomMapPolus;
    public static CustomOption RandomMapAirship;

    // |:========== 反転マップを有効化の設定 ==========:|
    public static CustomOption enableMirrorMap;

    // |:========== 梯子クールダウンの設定 ==========:|
    public static CustomOption LadderCoolChangeOption;
    public static CustomOption LadderCoolTimeOption;
    public static CustomOption LadderImpostorCoolChangeOption;
    public static CustomOption LadderImpostorCoolTimeOption;

    // |:========== ジップラインクールダウンの設定 ==========:|
    public static CustomOption ZiplineCoolChangeOption;
    public static CustomOption ZiplineCoolTimeOption;
    public static CustomOption ZiplineImpostorCoolChangeOption;
    public static CustomOption ZiplineImpostorCoolTimeOption;

    // |:========== その他 ==========:|
    public static Dictionary<byte, PoolablePlayer> playerIcons = new();


    public static void LoadOption()
    {
        // |:========== マップの設定 ==========:|

        MapOptionSetting = Create(101900, true, CustomOptionType.Generic, Cs(new Color(170f / 255f, 76f / 255f, 143f / 255f, 1), "MapOptionSetting"), false, null, isHeader: true);

        // |:========== 情報機器制限の設定 ==========:|
        DeviceOptions = Create(102000, true, CustomOptionType.Generic, "DeviceOptionsSetting", false, MapOptionSetting, isHeader: true);

        // |:========== ON/OFFの設定 ==========:|
        CanUseDeviceSetting = Create(102100, true, CustomOptionType.Generic, "CanUseDeviceSetting", true, DeviceOptions);
        DeviceUseAdmin = Create(102101, true, CustomOptionType.Generic, "DeviceUseAdminSetting", true, CanUseDeviceSetting);
        DeviceUseVitalOrDoorLog = Create(102103, true, CustomOptionType.Generic, "DeviceUseVitalOrDoorLogSetting", true, CanUseDeviceSetting);
        DeviceUseCamera = Create(102104, true, CustomOptionType.Generic, "DeviceUseCameraSetting", true, CanUseDeviceSetting);

        // |:========== 時間制限の設定 ==========:|
        RestrictDevicesTimeOption = Create(102200, false, CustomOptionType.Generic, "RestrictDevicesTimeOption", false, DeviceOptions);
        RestrictAdmin = Create(102201, false, CustomOptionType.Generic, "RestrictAdmin", false, RestrictDevicesTimeOption);
        DeviceUseAdminTime = Create(102202, false, CustomOptionType.Generic, "DeviceTimeSetting", 10f, 0f, 120f, 1f, RestrictAdmin);
        RestrictVital = Create(102203, false, CustomOptionType.Generic, "RestrictVital", false, RestrictDevicesTimeOption);
        DeviceUseVitalOrDoorLogTime = Create(102204, false, CustomOptionType.Generic, "DeviceTimeSetting", 10f, 0f, 120f, 1f, RestrictVital);
        RestrictCamera = Create(102205, false, CustomOptionType.Generic, "RestrictCamera", false, RestrictDevicesTimeOption);
        DeviceUseCameraTime = Create(102206, false, CustomOptionType.Generic, "DeviceTimeSetting", 10f, 0f, 120f, 1f, RestrictCamera);

        // |:========== 緊急タスク継続時間の設定 ==========:|
        ReactorDurationOption = Create(102300, true, CustomOptionType.Generic, "ReactorDurationSetting", false, MapOptionSetting, isHeader: true);
        SkeldReactorTimeLimit = Create(102304, true, CustomOptionType.Generic, "SkeldReactorTime", 20f, 0f, 30f, 1f, ReactorDurationOption);
        SkeldLifeSuppTimeLimit = Create(102305, true, CustomOptionType.Generic, "SkeldLifeSuppTime", 20f, 0f, 30f, 1f, ReactorDurationOption);
        MiraReactorTimeLimit = Create(102301, true, CustomOptionType.Generic, "MiraReactorTime", 30f, 0f, 45f, 1f, ReactorDurationOption);
        MiraLifeSuppTimeLimit = Create(102306, true, CustomOptionType.Generic, "MiraLifeSuppTime", 30f, 0f, 45f, 1f, ReactorDurationOption);
        PolusReactorTimeLimit = Create(102302, true, CustomOptionType.Generic, "PolusReactorTime", 40f, 0f, 60f, 1f, ReactorDurationOption);
        AirshipReactorTimeLimit = Create(102303, true, CustomOptionType.Generic, "AirshipReactorTime", 60f, 0f, 90f, 1f, ReactorDurationOption);
        FungleReactorTimeLimit = Create(102307, true, CustomOptionType.Generic, "FungleReactorTime", 40f, 0f, 60f, 1f, ReactorDurationOption);

        // |:========== ベントアニメーション有効化の設定 ==========:|
        VentAnimationPlaySetting = Create(102400, false, CustomOptionType.Generic, "VentAnimationPlaySetting", true, MapOptionSetting, isHeader: true);

        // |:========== 配線タスクランダムの設定 ==========:|
        WireTaskIsRandomOption = Create(102500, false, CustomOptionType.Generic, "WireTaskIsRandom", false, MapOptionSetting, isHeader: true);
        WireTaskNumOption = Create(102501, false, CustomOptionType.Generic, "WireTaskNum", 5f, 1f, 8f, 1f, WireTaskIsRandomOption);

        // |:========== ランダムマップの設定 ==========:|
        RandomMapOption = Create(102600, true, CustomOptionType.Generic, "RamdomMapSetting", false, MapOptionSetting, isHeader: true);
        RandomMapSkeld = Create(102601, true, CustomOptionType.Generic, "RMSkeldSetting", true, RandomMapOption);
        RandomMapMira = Create(102602, true, CustomOptionType.Generic, "RMMiraSetting", true, RandomMapOption);
        RandomMapPolus = Create(102603, true, CustomOptionType.Generic, "RMPolusSetting", true, RandomMapOption);
        RandomMapAirship = Create(102604, true, CustomOptionType.Generic, "RMAirshipSetting", true, RandomMapOption);

        // |:========== 反転マップ有効化の設定 ==========:|
        enableMirrorMap = Create(102700, false, CustomOptionType.Generic, "enableMirrorMap", false, MapOptionSetting, isHeader: true);

        // |:========== 梯子クールダウンの設定 ==========:|
        LadderCoolChangeOption = Create(105100, false, CustomOptionType.Generic, "LadderCoolChangeSetting", false, MapOptionSetting, isHeader: true);
        LadderCoolTimeOption = Create(105101, false, CustomOptionType.Generic, "LadderCoolTimeSetting", 2.5f, 0f, 60f, 2.5f, LadderCoolChangeOption);
        LadderImpostorCoolChangeOption = Create(105102, false, CustomOptionType.Generic, "LadderImpostorCoolChangeSetting", false, LadderCoolChangeOption);
        LadderImpostorCoolTimeOption = Create(105103, false, CustomOptionType.Generic, "LadderImpostorCoolTimeSetting", 2.5f, 0f, 60f, 2.5f, LadderImpostorCoolChangeOption);

        // |:========== 梯子クールダウンの設定 ==========:|
        ZiplineCoolChangeOption = Create(105200, false, CustomOptionType.Generic, "ZiplineCoolChangeSetting", false, MapOptionSetting, isHeader: true);
        ZiplineCoolTimeOption = Create(105201, false, CustomOptionType.Generic, "ZiplineCoolTimeSetting", 7.5f, 0f, 60f, 2.5f, ZiplineCoolChangeOption);
        ZiplineImpostorCoolChangeOption = Create(105202, false, CustomOptionType.Generic, "ZiplineImpostorCoolChangeSetting", false, ZiplineCoolChangeOption);
        ZiplineImpostorCoolTimeOption = Create(105203, false, CustomOptionType.Generic, "ZiplineImpostorCoolTimeSetting", 7.5f, 0f, 60f, 2.5f, ZiplineImpostorCoolChangeOption);
    }

    #region 設定取得に使用している変数の定義

    // |:========== 変数:ON/OFFの設定 ==========:|
    public static bool CanUseAdmin;
    public static bool CanUseVitalOrDoorLog;
    public static bool CanUseCamera;
    // |:========== 変数:時間制限の設定 ==========:|
    public static bool IsUsingRestrictDevicesTime;
    // その他はDeviceClassに存在

    // |:========== 変数:リアクター継続時間の設定 ==========:|
    public static bool IsReactorDurationSetting;

    // |:========== 変数:ベントアニメーションを有効にする ==========:|
    public static bool CanPlayVentAnimation;

    // |:========== 変数:配線タスクランダム ==========:|
    public static bool WireTaskIsRandom;
    public static int WireTaskNum;

    // |:========== 変数:ランダムマップ ==========:|
    public static bool IsRandomMap;
    public static bool ValidationSkeld;
    public static bool ValidationMira;
    public static bool ValidationPolus;
    public static bool ValidationAirship;

    // |:========== 変数:反転マップを有効にする ==========:|
    public static bool IsenableMirrorMap;

    // |:========== 梯子クールダウンの設定 ==========:|
    public static bool IsLadderCoolChange;
    public static float LadderCoolTime;
    public static bool IsLadderImpostorCoolChange;
    public static float LadderImpostorCoolTime;

    // |:========== 梯子クールダウンの設定 ==========:|
    public static bool IsZiplineCoolChange;
    public static float ZiplineCoolTime;
    public static bool IsZiplineImpostorCoolChange;
    public static float ZiplineImpostorCoolTime;

    // |:========== 変数:その他 ==========:|
    public static float Default;
    public static float CameraDefault;

    #endregion

    public static void ClearAndReload()
    {
        #region マップの設定全てを初期値で初期化

        CanUseAdmin = true;
        CanUseVitalOrDoorLog = true;
        CanUseCamera = true;

        IsUsingRestrictDevicesTime = false;

        IsReactorDurationSetting = false;

        CanPlayVentAnimation = true;

        WireTaskIsRandom = false;

        IsRandomMap = false;
        ValidationSkeld = true;
        ValidationMira = true;
        ValidationPolus = true;
        ValidationAirship = true;

        IsenableMirrorMap = false;

        IsLadderCoolChange = false;
        LadderCoolTime = 5f;
        IsLadderImpostorCoolChange = false;
        LadderImpostorCoolTime = 5f;

        IsZiplineCoolChange = false;
        ZiplineCoolTime = 7.5f;
        IsZiplineImpostorCoolChange = false;
        ZiplineImpostorCoolTime = 7.5f;

        #endregion

        #region マップの設定のon/offに関わらない初期化

        BlockTool.OldDesyncCommsPlayers = new();
        BlockTool.CameraPlayers = new();

        WireTaskNum = WireTaskNumOption.GetInt();

        CameraDefault = Camera.main.orthographicSize;
        Default = FastDestroyableSingleton<HudManager>.Instance.UICamera.orthographicSize;
        playerIcons = new();

        #endregion

        if (MapOptionSetting.GetBool()) // 早期return使用していない理由有。閉じ括弧の下に理由を記載しています。
        {
            if (DeviceOptions.GetBool())
            {
                if (CanUseDeviceSetting.GetBool())
                {
                    CanUseAdmin = DeviceUseAdmin.GetBool();
                    CanUseVitalOrDoorLog = DeviceUseVitalOrDoorLog.GetBool();
                    CanUseCamera = DeviceUseCamera.GetBool();
                }

                if (!ModeHandler.IsMode(ModeId.SuperHostRoles) && RestrictDevicesTimeOption.GetBool())
                {
                    IsUsingRestrictDevicesTime = true;
                    // 他の変数は[DeviceClass.ClearAndReload();]で初期化を行っている。
                }
            }

            IsReactorDurationSetting = ReactorDurationOption.GetBool();

            if (!ModeHandler.IsMode(ModeId.SuperHostRoles)) CanPlayVentAnimation = VentAnimationPlaySetting.GetBool();

            WireTaskIsRandom = WireTaskIsRandomOption.GetBool();

            if (RandomMapOption.GetBool())
            {
                IsRandomMap = true;

                ValidationSkeld = RandomMapSkeld.GetBool();
                ValidationMira = RandomMapMira.GetBool();
                ValidationPolus = RandomMapPolus.GetBool();
                ValidationAirship = RandomMapAirship.GetBool();
            }

            IsenableMirrorMap = enableMirrorMap.GetBool();

            IsLadderCoolChange = LadderCoolChangeOption.GetBool();
            LadderCoolTime = LadderCoolTimeOption.GetFloat();
            IsLadderImpostorCoolChange = LadderImpostorCoolChangeOption.GetBool();
            LadderImpostorCoolTime = LadderImpostorCoolTimeOption.GetFloat();

            IsZiplineCoolChange = ZiplineCoolChangeOption.GetBool();
            ZiplineCoolTime = ZiplineCoolTimeOption.GetFloat();
            IsZiplineImpostorCoolChange = ZiplineImpostorCoolChangeOption.GetBool();
            ZiplineImpostorCoolTime = ZiplineImpostorCoolTimeOption.GetFloat();
        }
        /*
            [MapOptionSetting.GetBool()]に早期returnを使用していない理由。
            [DeviceClass.ClearAndReload();]を[IsUsingRestrictDevicesTime]の初期化の後に行う必要がある為。
            又　後に設定が増えた時に区切りをわかりやすくして追加しやすくする為
        */
        DeviceClass.ClearAndReload();
    }
}