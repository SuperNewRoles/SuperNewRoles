using System.Collections.Generic;

namespace SuperNewRoles.Map
{
    public enum CustomMapNames
    {
        Skeld,
        Mira,
        Polus,
        Dleks,
        Airship,
        Agartha
    }
    public static class Data
    {
        public static CustomMapNames ThisMap = CustomMapNames.Skeld;//CustomMapNames.Agartha;
        public static string[] MapStringNames = new string[6] { "The Skeld", "MIRA HQ", "Polus", "dlekS ehT", "Airship", "Agartha" };
        public static Dictionary<string, CustomMapNames> CustomMapNameData = new()
        {
            { MapStringNames[0], CustomMapNames.Skeld },
            { MapStringNames[1], CustomMapNames.Mira }
            ,
            { MapStringNames[2], CustomMapNames.Polus },
            { MapStringNames[3], CustomMapNames.Dleks }
            ,
            { MapStringNames[4], CustomMapNames.Agartha },
            { MapStringNames[5], CustomMapNames.Agartha }
        };
        public static void MainMenu() { }
        public static void ClearAndReloads()
        {
            //ThisMap = CustomMapNames.Skeld;
            ThisMap = CustomMapNameData[MapStringNames[PlayerControl.GameOptions.MapId]];
            ThisMap = CustomMapNames.Skeld;
        }
        public static bool IsMap(CustomMapNames map)
        {
            return ThisMap == map;
        }
    }
}
