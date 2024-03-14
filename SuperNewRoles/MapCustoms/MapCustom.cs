using SuperNewRoles.Mode;
using UnityEngine;
using static SuperNewRoles.Modules.CustomOption;
using static SuperNewRoles.Modules.CustomOptionHolder;

namespace SuperNewRoles.MapCustoms;

class MapCustom
{
    public static CustomOption MapCustomOption;

    /*===============スケルド===============*/
    public static CustomOption SkeldSetting;
    // public static CustomOption SkeldRandomSpawn;

    /*===============ミラ===============*/
    public static CustomOption MiraSetting;
    // public static CustomOption MiraRandomSpawn;
    public static CustomOption MiraAdditionalVents;
    public static CustomOption AddVitalsMira;


    /*===============ポーラス===============*/
    public static CustomOption PolusSetting;
    public static CustomOption PolusRandomSpawn;
    public static CustomOption PolusAdditionalVents;
    public static CustomOption SpecimenVital;


    /*===============エアーシップ===============*/
    public static CustomOption AirshipSetting;
    public static CustomOption AirshipRandomSpawn;
    public static CustomOption SecretRoomOption;
    public static CustomOption AirShipAdditionalVents;
    public static CustomOption AirshipDisableMovingPlatform;
    public static CustomOption RecordsAdminDestroy;
    public static CustomOption MoveElecPad;
    public static CustomOption AddWireTask;
    public static CustomOption AntiTaskOverWall;
    internal static CustomOption ShuffleElectricalDoors;
    /// <summary>昇降機右の上下の影を変更する設定</summary>
    public static CustomOption ModifyGapRoomOneWayShadow;
    /// <summary>インポスターが下から上を見ることができる設定</summary>
    public static CustomOption GapRoomShadowIgnoresImpostors;
    /// <summary>非インポスターが上から下を見ることができない設定</summary>
    public static CustomOption DisableGapRoomShadowForNonImpostor;
    /// <summary>昇降右のダウンロードをはしごの下に移動する設定</summary>
    public static CustomOption MoveGapRoomDownload;

    /*===============ファングル===============*/
    public static CustomOption TheFungleSetting;
    public static CustomOption TheFungleSpawnType;
    public static CustomOption TheFungleZiplineOption;
    public static CustomOption TheFungleCanUseZiplineOption;
    public static CustomOption TheFungleZiplineUpTime;
    public static CustomOption TheFungleZiplineDownTime;
    public static CustomOption TheFungleZiplineUpOrDown;
    public static CustomOption TheFungleCameraOption;
    public static CustomOption TheFungleCameraChangeRange;
    public static CustomOption TheFungleCameraSpeed;
    public static CustomOption TheFungleMushroomMixupOption;
    public static CustomOption TheFungleMushroomMixupCantOpenMeeting;
    public static CustomOption TheFungleMushroomMixupTime;
    public static CustomOption TheFungleAdditionalAdmin;
    public static CustomOption TheFunglePowerOutageSabotage;
    public static CustomOption TheFungleHideSporeMask;
    public static CustomOption TheFungleHideSporeMaskOnlyImpostor;

    /*===============アガルタ===============*/
    public static CustomOption AgarthaSetting;
    public static CustomOption AgarthaRandomSpawn;
    public static CustomOption AgarthaRandomSpawnIsFirstSpawn;
    public static CustomOption AgarthaRandomSpawnIsAddSpawnWay;

    public static void CreateOption()
    {
        MapCustomOption = Create(102800, true, CustomOptionType.Generic, Cs(new Color(132f / 187f, 162f / 255f, 212f / 255f, 1f), "MapCustom"), false, null, true);

        /*===============スケルド===============*/
        SkeldSetting = Create(102900, true, CustomOptionType.Generic, "<color=#8fbc8f>The Skeld</color>", false, MapCustomOption);
        // SkeldRandomSpawn = Create(1248, false, CustomOptionType.Generic, "RandomSpawnOption", false, SkeldSetting);

        /*===============ミラ===============*/
        MiraSetting = Create(103000, true, CustomOptionType.Generic, "<color=#cd5c5c>Mira HQ</color>", false, MapCustomOption);
        // MiraRandomSpawn = Create(103001, false, CustomOptionType.Generic, "RandomSpawnOption", false, MiraSetting);
        MiraAdditionalVents = Create(103002, false, CustomOptionType.Generic, "AdditionalVents", false, MiraSetting);
        AddVitalsMira = Create(103003, false, CustomOptionType.Generic, "AdditionalVitals", false, MiraSetting);

        /*===============ポーラス===============*/
        PolusSetting = Create(103100, true, CustomOptionType.Generic, "<color=#4b0082>Polus</color>", false, MapCustomOption);
        PolusRandomSpawn = Create(103101, true, CustomOptionType.Generic, "RandomSpawnOption", false, PolusSetting);
        PolusAdditionalVents = Create(103102, false, CustomOptionType.Generic, "AdditionalVents", false, PolusSetting);
        SpecimenVital = Create(103103, false, CustomOptionType.Generic, "SpecimenVitalSetting", false, PolusSetting);

        /*===============エアーシップ===============*/
        AirshipSetting = Create(103200, true, CustomOptionType.Generic, "<color=#ff0000>Airship</color>", false, MapCustomOption);
        AirshipRandomSpawn = Create(103201, true, CustomOptionType.Generic, "RandomSpawnOption", false, AirshipSetting);
        SecretRoomOption = Create(103202, false, CustomOptionType.Generic, "SecretRoom", false, AirshipSetting);
        AirShipAdditionalVents = Create(103203, false, CustomOptionType.Generic, "AdditionalVents", false, AirshipSetting);
        AirshipDisableMovingPlatform = Create(103204, false, CustomOptionType.Generic, "AirshipDisableMovingPlatformSetting", false, AirshipSetting);
        RecordsAdminDestroy = Create(103205, false, CustomOptionType.Generic, "RecordsAdminDestroySetting", false, AirshipSetting);
        MoveElecPad = Create(103206, false, CustomOptionType.Generic, "MoveElecPadSetting", false, AirshipSetting);
        AddWireTask = Create(103207, false, CustomOptionType.Generic, "AddWireTaskSetting", false, AirshipSetting);
        AntiTaskOverWall = Create(103208, false, CustomOptionType.Generic, "AntiTaskOverWallSetting", false, AirshipSetting);
        ShuffleElectricalDoors = Create(103209, false, CustomOptionType.Generic, "ShuffleElectricalDoorsSetting", false, AirshipSetting);
        ModifyGapRoomOneWayShadow = Create(103210, false, CustomOptionType.Generic, "ModifyGapRoomOneWayShadow", false, AirshipSetting);
        GapRoomShadowIgnoresImpostors = Create(103211, false, CustomOptionType.Generic, "GapRoomShadowIgnoresImpostors", true, ModifyGapRoomOneWayShadow);
        DisableGapRoomShadowForNonImpostor = Create(103212, false, CustomOptionType.Generic, "DisableGapRoomShadowForNonImpostor", true, ModifyGapRoomOneWayShadow);
        MoveGapRoomDownload = Create(103213, false, CustomOptionType.Generic, "MoveGapRoomDownload", false, AirshipSetting);

        /*===============ファングル===============*/
        TheFungleSetting = Create(104800, true, CustomOptionType.Generic, "<color=#fd7e00>The Fungle</color>", false, MapCustomOption);
        TheFungleSpawnType = Create(104817, true, CustomOptionType.Generic, "TheFungleSpawnType", new string[] { "TheFungleSpawnTypeNormal", "RandomSpawnOption", "TheFungleSpawnTypeSelect" }, TheFungleSetting);
        TheFungleZiplineOption = Create(104802, true, CustomOptionType.Generic, "TheFungleZiplineOption", false, TheFungleSetting);
        TheFungleCanUseZiplineOption = Create(104803, true, CustomOptionType.Generic, "TheFungleCanUseZiplineOption", true, TheFungleZiplineOption);
        TheFungleZiplineUpTime = Create(104804, false, CustomOptionType.Generic, "TheFungleZiplineUpTime", 4f, 0.5f, 12f, 0.5f, TheFungleCanUseZiplineOption);
        TheFungleZiplineDownTime = Create(104805, false, CustomOptionType.Generic, "TheFungleZiplineDownTime", 1.75f, 0.5f, 12f, 0.5f, TheFungleCanUseZiplineOption);
        TheFungleZiplineUpOrDown = Create(104806, true, CustomOptionType.Generic, "TheFungleZiplineUpOrDown", new string[] { "TheFungleZiplineAlways", "TheFungleZiplineOnlyUp", "TheFungleZiplineOnlyDown" }, TheFungleCanUseZiplineOption); TheFungleCameraOption = Create(104807, false, CustomOptionType.Generic, "TheFungleCameraOption", false, TheFungleSetting);
        TheFungleCameraChangeRange = Create(104808, false, CustomOptionType.Generic, "TheFungleCameraChangeRange", 7.5f, 0.5f, 15f, 0.5f, TheFungleCameraOption);
        TheFungleCameraSpeed = Create(104809, false, CustomOptionType.Generic, "TheFungleCameraSpeed", 1f, 0f, 10f, 0.25f, TheFungleCameraOption);
        TheFungleMushroomMixupOption = Create(104810, true, CustomOptionType.Generic, "TheFungleMushroomMixupOption", false, TheFungleSetting);
        TheFungleMushroomMixupCantOpenMeeting = Create(104811, true, CustomOptionType.Generic, "TheFungleMushroomMixupCantOpenMeeting", false, TheFungleMushroomMixupOption);
        TheFungleMushroomMixupTime = Create(104812, true, CustomOptionType.Generic, "TheFungleMushroomMixupTime", 10f, 1f, 30f, 0.5f, TheFungleMushroomMixupOption);
        TheFungleAdditionalAdmin = Create(104813, false, CustomOptionType.Generic, "TheFungleAdditionalAdmin", false, TheFungleSetting);
        TheFunglePowerOutageSabotage = Create(104814, false, CustomOptionType.Generic, "TheFunglePowerOutageSabotage", false, TheFungleSetting);
        TheFungleHideSporeMask = Create(104815, false, CustomOptionType.Generic, "TheFungleHideSporeMask", false, TheFungleSetting);
        TheFungleHideSporeMaskOnlyImpostor = Create(104816, false, CustomOptionType.Generic, "TheFungleHideSporeMaskOnlyImpostor", false, TheFungleHideSporeMask);

        /*===============アガルタ===============*/
        AgarthaSetting = Create(103300, false, CustomOptionType.Generic, "<color=#a67646>Agartha</color>", false, MapCustomOption);
        AgarthaRandomSpawn = Create(103301, false, CustomOptionType.Generic, "RandomSpawnOption", true, AgarthaSetting);
        AgarthaRandomSpawnIsFirstSpawn = Create(103302, false, CustomOptionType.Generic, "AgarthaRandomSpawnIsFirstSpawn", false, AgarthaRandomSpawn);
        AgarthaRandomSpawnIsAddSpawnWay = Create(103303, false, CustomOptionType.Generic, "AgarthaRandomSpawnIsAddSpawnWay", false, AgarthaRandomSpawn);
    }
}
public class MapCustomClearAndReload
{
    /*===============エアーシップ===============*/
    internal static bool AirshipSetting;
    public static bool AirshipRandomSpawn;

    /*===============ファングル===============*/
    internal static bool FungleSetting;
    public static bool FungleRandomSpawn;

    /*===============アガルタ===============*/
    internal static bool AgarthaSetting;
    public static bool AgarthaRandomSpawn;
    public static bool AgarthaRandomSpawnIsFirstSpawn;
    public static bool AgarthaRandomSpawnIsAddSpawnWay;
    public static void ClearAndReload()
    {
        if (!MapCustom.MapCustomOption.GetBool() || ModeHandler.IsMode(ModeId.SuperHostRoles))
        {
            AirshipSetting = false;
            FungleSetting = false;
            AgarthaSetting = false;
        }
        else
        {
            AirshipSetting = MapCustom.AirshipSetting.GetBool();
            AgarthaSetting = MapCustom.AgarthaSetting.GetBool();
        }
        FungleSetting = MapCustom.TheFungleSetting.GetBool();

        /*===============エアーシップ===============*/
        AirshipRandomSpawn = AirshipSetting && MapCustom.AirshipRandomSpawn.GetBool();

        /*===============ファングル===============*/
        FungleRandomSpawn = FungleHandler.IsFungleSpawnType(FungleHandler.FungleSpawnType.Random);

        /*===============アガルタ===============*/
        // TODO:仮の設定取得方式
        AgarthaRandomSpawn = AgarthaSetting && MapCustom.AgarthaRandomSpawn.GetBool();
        AgarthaRandomSpawnIsFirstSpawn = AgarthaSetting && MapCustom.AgarthaRandomSpawnIsFirstSpawn.GetBool();
        AgarthaRandomSpawnIsAddSpawnWay = AgarthaSetting && MapCustom.AgarthaRandomSpawnIsAddSpawnWay.GetBool();
    }
}