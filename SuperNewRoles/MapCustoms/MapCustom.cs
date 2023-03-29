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

    /*===============アガルタ===============*/
    public static CustomOption AgarthaSetting;
    public static CustomOption AgarthaRandomSpawn;
    public static CustomOption AgarthaRandomSpawnIsFirstSpawn;
    public static CustomOption AgarthaRandomSpawnIsAddSpawnWay;

    public static void CreateOption()
    {
        MapCustomOption = Create(623, false, CustomOptionType.Generic, Cs(new Color(132f / 187f, 162f / 255f, 212f / 255f, 1f), "MapCustom"), false, null, true);

        /*===============スケルド===============*/
        SkeldSetting = Create(624, false, CustomOptionType.Generic, "<color=#8fbc8f>Skeld</color>", false, MapCustomOption);
        // SkeldRandomSpawn = Create(1248, false, CustomOptionType.Generic, "RandomSpawnOption", false, SkeldSetting);

        /*===============ミラ===============*/
        MiraSetting = Create(660, false, CustomOptionType.Generic, "<color=#cd5c5c>Mira</color>", false, MapCustomOption);
        // MiraRandomSpawn = Create(1249, false, CustomOptionType.Generic, "RandomSpawnOption", false, MiraSetting);
        MiraAdditionalVents = Create(631, false, CustomOptionType.Generic, "MiraAdditionalVents", false, MiraSetting);
        AddVitalsMira = Create(472, false, CustomOptionType.Generic, "AddVitalsMiraSetting", false, MiraSetting);

        /*===============ポーラス===============*/
        PolusSetting = Create(677, false, CustomOptionType.Generic, "<color=#4b0082>Polus</color>", false, MapCustomOption);
        PolusRandomSpawn = Create(670, false, CustomOptionType.Generic, "RandomSpawnOption", false, PolusSetting);
        PolusAdditionalVents = Create(662, false, CustomOptionType.Generic, "PolusAdditionalVents", false, PolusSetting);
        SpecimenVital = Create(613, false, CustomOptionType.Generic, "SpecimenVitalSetting", false, PolusSetting);

        /*===============エアーシップ===============*/
        AirshipSetting = Create(663, false, CustomOptionType.Generic, "<color=#ff0000>Airship</color>", false, MapCustomOption);
        AirshipRandomSpawn = Create(955, false, CustomOptionType.Generic, "RandomSpawnOption", false, AirshipSetting);
        SecretRoomOption = Create(664, false, CustomOptionType.Generic, "SecretRoom", false, AirshipSetting);
        AirShipAdditionalVents = Create(605, false, CustomOptionType.Generic, "AirShipAdditionalVents", false, AirshipSetting);
        AirshipDisableMovingPlatform = Create(665, false, CustomOptionType.Generic, "AirshipDisableMovingPlatformSetting", false, AirshipSetting);
        RecordsAdminDestroy = Create(612, false, CustomOptionType.Generic, "RecordsAdminDestroySetting", false, AirshipSetting);
        MoveElecPad = Create(645, false, CustomOptionType.Generic, "MoveElecPadSetting", false, AirshipSetting);
        AddWireTask = Create(646, false, CustomOptionType.Generic, "AddWireTaskSetting", false, AirshipSetting);
        AntiTaskOverWall = Create(1106, false, CustomOptionType.Generic, "AntiTaskOverWallSetting", false, AirshipSetting);
        ShuffleElectricalDoors = Create(1110, false, CustomOptionType.Generic, "ShuffleElectricalDoorsSetting", false, AirshipSetting);

        /*===============アガルタ===============*/
        AgarthaSetting = Create(1250, false, CustomOptionType.Generic, "<color=#a67646>Agartha</color>", false, MapCustomOption);
        AgarthaRandomSpawn = Create(1084, false, CustomOptionType.Generic, "RandomSpawnOption", true, AgarthaSetting);
        AgarthaRandomSpawnIsFirstSpawn = Create(1085, false, CustomOptionType.Generic, "AgarthaRandomSpawnIsFirstSpawn", false, AgarthaRandomSpawn);
        AgarthaRandomSpawnIsAddSpawnWay = Create(1086, false, CustomOptionType.Generic, "AgarthaRandomSpawnIsAddSpawnWay", false, AgarthaRandomSpawn);
    }
}

public class MapCustomClearAndReload
{
    /*===============エアーシップ===============*/
    internal static bool AirshipSetting;
    public static bool AirshipRandomSpawn;

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
            AgarthaSetting = false;
        }
        else
        {
            AirshipSetting = MapCustom.AirshipSetting.GetBool();
            AgarthaSetting = MapCustom.AgarthaSetting.GetBool();
        }
        /*===============エアーシップ===============*/
        // FIXME:CustomMapIdをSHR時はfalseにするのがうまく動作していないようなので此処で取得している。そちらを直したら移動する。
        AirshipRandomSpawn = AirshipSetting && MapCustom.AirshipRandomSpawn.GetBool();

        /*===============アガルタ===============*/
        // TODO:仮の設定取得方式
        AgarthaRandomSpawn = AgarthaSetting && MapCustom.AgarthaRandomSpawn.GetBool();
        AgarthaRandomSpawnIsFirstSpawn = AgarthaSetting && MapCustom.AgarthaRandomSpawnIsFirstSpawn.GetBool();
        AgarthaRandomSpawnIsAddSpawnWay = AgarthaSetting && MapCustom.AgarthaRandomSpawnIsAddSpawnWay.GetBool();
    }
}