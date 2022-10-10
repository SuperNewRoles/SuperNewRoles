using SuperNewRoles.Patches;

namespace SuperNewRoles.MapCustoms
{
    class MapCustom
    {
        public static CustomOption MapCustomOption;

        public static CustomOption SkeldSetting;//スケルド


        public static CustomOption MiraSetting;//ミラ
        public static CustomOption MiraAdditionalVents;
        public static CustomOption AddVitalsMira;


        public static CustomOption PolusSetting;//ポーラス
        public static CustomOption PolusAdditionalVents;
        public static CustomOption SpecimenVital;
        public static CustomOption PolusRandomSpawn;


        public static CustomOption AirshipSetting;//エアーシップ
        public static CustomOption SecretRoomOption;
        public static CustomOption AirShipAdditionalVents;
        public static CustomOption AirshipDisableMovingPlatform;
        public static CustomOption RecordsAdminDestroy;
        public static CustomOption MoveElecPad;
        public static CustomOption AddWireTask;


        public static void CreateOption()
        {
            MapCustomOption = CustomOption.Create(623, false, CustomOptionType.Generic, "MapCustom", false, null, true);

            /*===============スケルド===============*/
            SkeldSetting = CustomOption.Create(624, false, CustomOptionType.Generic, "<color=#8fbc8f>Skeld</color>", false, MapCustomOption);

            /*===============ミラ===============*/
            MiraSetting = CustomOption.Create(660, false, CustomOptionType.Generic, "<color=#cd5c5c>Mira</color>", false, MapCustomOption);
            MiraAdditionalVents = CustomOption.Create(631, false, CustomOptionType.Generic, "MiraAdditionalVents", false, MiraSetting);
            AddVitalsMira = CustomOption.Create(472, false, CustomOptionType.Generic, "AddVitalsMiraSetting", false, MiraSetting);

            /*===============ポーラス===============*/
            PolusSetting = CustomOption.Create(677, false, CustomOptionType.Generic, "<color=#4b0082>Polus</color>", false, MapCustomOption);
            PolusAdditionalVents = CustomOption.Create(662, false, CustomOptionType.Generic, "PolusAdditionalVents", false, PolusSetting);
            SpecimenVital = CustomOption.Create(613, false, CustomOptionType.Generic, "SpecimenVitalSetting", false, PolusSetting);
            PolusRandomSpawn = CustomOption.Create(670, false, CustomOptionType.Generic, "PolusrandomSpawn", false, PolusSetting);

            /*===============エアーシップ===============*/
            AirshipSetting = CustomOption.Create(663, false, CustomOptionType.Generic, "<color=#ff0000>Airship</color>", false, MapCustomOption);
            SecretRoomOption = CustomOption.Create(664, false, CustomOptionType.Generic, "SecretRoom", false, AirshipSetting);
            AirShipAdditionalVents = CustomOption.Create(605, false, CustomOptionType.Generic, "AirShipAdditionalVents", false, AirshipSetting);
            AirshipDisableMovingPlatform = CustomOption.Create(665, false, CustomOptionType.Generic, "AirshipDisableMovingPlatformSetting", false, AirshipSetting);
            RecordsAdminDestroy = CustomOption.Create(612, false, CustomOptionType.Generic, "RecordsAdminDestroySetting", false, AirshipSetting);
            MoveElecPad = CustomOption.Create(645, false, CustomOptionType.Generic, "MoveElecPadSetting", false, AirshipSetting);
            AddWireTask = CustomOption.Create(646, false, CustomOptionType.Generic, "AddWireTaskSetting", false, AirshipSetting);
        }
    }
}