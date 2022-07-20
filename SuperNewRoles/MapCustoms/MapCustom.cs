using SuperNewRoles.CustomOption;

namespace SuperNewRoles.MapCustoms
{
    class MapCustom
    {
        public static CustomOption.CustomOption MapCustomOption;

        public static CustomOption.CustomOption SkeldSetting;//スケルド


        public static CustomOption.CustomOption MiraSetting;//ミラ
        public static CustomOption.CustomOption MiraAdditionalVents;
        public static CustomOption.CustomOption AddVitalsMira;


        public static CustomOption.CustomOption PolusSetting;//ポーラス
        public static CustomOption.CustomOption PolusAdditionalVents;
        public static CustomOption.CustomOption SpecimenVital;
        public static CustomOption.CustomOption PolusRandomSpawn;


        public static CustomOption.CustomOption AirshipSetting;//エアーシップ
        public static CustomOption.CustomOption SecretRoomOption;
        public static CustomOption.CustomOption AirShipAdditionalVents;
        public static CustomOption.CustomOption AirshipDisableMovingPlatform;
        public static CustomOption.CustomOption RecordsAdminDestroy;
        public static CustomOption.CustomOption MoveElecPad;
        public static CustomOption.CustomOption AddWireTask;


        public static void CreateOption()
        {
            MapCustomOption = CustomOption.CustomOption.Create(623, false, CustomOptionType.Generic, "MapCustom", false, null, true);

            /*===============スケルド===============*/
            SkeldSetting = CustomOption.CustomOption.Create(624, false, CustomOptionType.Generic, "<color=#8fbc8f>Skeld</color>", false, MapCustomOption);

            /*===============ミラ===============*/
            MiraSetting = CustomOption.CustomOption.Create(660, false, CustomOptionType.Generic, "<color=#cd5c5c>Mira</color>", false, MapCustomOption);
            MiraAdditionalVents = CustomOption.CustomOption.Create(631, false, CustomOptionType.Generic, "MiraAdditionalVents", false, MiraSetting);
            AddVitalsMira = CustomOption.CustomOption.Create(472, false, CustomOptionType.Generic, "AddVitalsMiraSetting", false, MiraSetting);

            /*===============ポーラス===============*/
            PolusSetting = CustomOption.CustomOption.Create(677, false, CustomOptionType.Generic, "<color=#4b0082>Polus</color>", false, MapCustomOption);
            PolusAdditionalVents = CustomOption.CustomOption.Create(662, false, CustomOptionType.Generic, "PolusAdditionalVents", false, PolusSetting);
            SpecimenVital = CustomOption.CustomOption.Create(613, false, CustomOptionType.Generic, "SpecimenVitalSetting", false, PolusSetting);
            PolusRandomSpawn = CustomOption.CustomOption.Create(670, false, CustomOptionType.Generic, "PolusrandomSpawn", false, PolusSetting);

            /*===============エアーシップ===============*/
            AirshipSetting = CustomOption.CustomOption.Create(663, false, CustomOptionType.Generic, "<color=#ff0000>Airship</color>", false, MapCustomOption);
            SecretRoomOption = CustomOption.CustomOption.Create(664, false, CustomOptionType.Generic, "SecretRoom", false, AirshipSetting);
            AirShipAdditionalVents = CustomOption.CustomOption.Create(605, false, CustomOptionType.Generic, "AirShipAdditionalVents", false, AirshipSetting);
            AirshipDisableMovingPlatform = CustomOption.CustomOption.Create(665, false, CustomOptionType.Generic, "AirshipDisableMovingPlatformSetting", false, AirshipSetting);
            RecordsAdminDestroy = CustomOption.CustomOption.Create(612, false, CustomOptionType.Generic, "RecordsAdminDestroySetting", false, AirshipSetting);
            MoveElecPad = CustomOption.CustomOption.Create(645, false, CustomOptionType.Generic, "MoveElecPadSetting", false, AirshipSetting);
            AddWireTask = CustomOption.CustomOption.Create(646, false, CustomOptionType.Generic, "AddWireTaskSetting", false, AirshipSetting);
        }
    }
}