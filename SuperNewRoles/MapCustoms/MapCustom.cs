using System;
using System.Collections.Generic;
using System.Text;
using SuperNewRoles.CustomOption;
using SuperNewRoles.MapCustoms.Airship;

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


        public static CustomOption.CustomOption AirshipSetting;//エアーシップ
        public static CustomOption.CustomOption SecretRoomOption;
        public static CustomOption.CustomOption AirShipAdditionalVents;
        public static CustomOption.CustomOption AirshipDisableMovingPlatform;
        public static CustomOption.CustomOption RecordsAdminDestroy;
        public static CustomOption.CustomOption MoveElecPad;


        public static void CreateOption()
        {
            MapCustomOption = CustomOption.CustomOption.Create(623, false, CustomOptionType.Generic, "MapCustom", false, null, true);

            /*===============スケルド===============*/
            SkeldSetting = CustomOption.CustomOption.Create(624, false, CustomOptionType.Generic, "Skeld", false, MapCustomOption);

            /*===============ミラ===============*/
            MiraSetting = CustomOption.CustomOption.Create(606, false, CustomOptionType.Generic, "Mira", false, MapCustomOption);
            MiraAdditionalVents = CustomOption.CustomOption.Create(631, false, CustomOptionType.Generic, "MiraAdditionalVents", false, MiraSetting);
            AddVitalsMira = CustomOption.CustomOption.Create(472, false, CustomOptionType.Generic, "AddVitalsMiraSetting", false, MiraSetting);

            /*===============ポーラス===============*/
            PolusSetting = CustomOption.CustomOption.Create(606, false, CustomOptionType.Generic, "Polus", false, MapCustomOption);
            PolusAdditionalVents = CustomOption.CustomOption.Create(606, false, CustomOptionType.Generic, "PolusAdditionalVents", false, PolusSetting);
            SpecimenVital = CustomOption.CustomOption.Create(613, false, CustomOptionType.Generic, "SpecimenVitalSetting", false, PolusSetting);

            /*===============エアーシップ===============*/
            AirshipSetting = CustomOption.CustomOption.Create(624, false, CustomOptionType.Generic, "Airship", false, MapCustomOption);
            SecretRoomOption = CustomOption.CustomOption.Create(624, false, CustomOptionType.Generic, "SecretRoom", false, AirshipSetting);
            AirShipAdditionalVents = CustomOption.CustomOption.Create(605, false, CustomOptionType.Generic, "AirShipAdditionalVents", false, AirshipSetting);
            AirshipDisableMovingPlatform = CustomOption.CustomOption.Create(623, false, CustomOptionType.Generic, "AirshipDisableMovingPlatformSetting", false, AirshipSetting);
            RecordsAdminDestroy = CustomOption.CustomOption.Create(612, false, CustomOptionType.Generic, "RecordsAdminDestroySetting", false, AirshipSetting);
            MoveElecPad = CustomOption.CustomOption.Create(612, false, CustomOptionType.Generic, "MoveElecPadSetting", false, AirshipSetting);
        }
    }
}
