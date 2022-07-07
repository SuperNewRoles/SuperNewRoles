using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;
using System;
using static PlayerControl;
using SuperNewRoles.Mode;

namespace SuperNewRoles.MapCustoms
{
    public class MapCustomHandler
    {
        public static bool isMapCustom(Ids.MapCustomId CMId, bool IsChache = true)
        {
            return CMId switch
            {
                Ids.MapCustomId.Skeld => GameOptions.MapId == 0 && MapCustom.MapCustomOption.getBool() && MapCustom.SkeldSetting.getBool() && ModeHandler.isMode(ModeId.Default),
                Ids.MapCustomId.Mira => GameOptions.MapId == 1 && MapCustom.MapCustomOption.getBool() && MapCustom.MiraSetting.getBool() && ModeHandler.isMode(ModeId.Default),
                Ids.MapCustomId.Polus => GameOptions.MapId == 2 && MapCustom.MapCustomOption.getBool() && MapCustom.PolusSetting.getBool() && ModeHandler.isMode(ModeId.Default),
                Ids.MapCustomId.Airship => GameOptions.MapId == 4 && MapCustom.MapCustomOption.getBool() && MapCustom.AirshipSetting.getBool() && ModeHandler.isMode(ModeId.Default),
                _ => false,
            };
        }
    }
    public class Ids
    {
        public enum MapCustomId
        {
            Skeld,
            Mira,
            Polus,
            Airship,
        }
    }
}
