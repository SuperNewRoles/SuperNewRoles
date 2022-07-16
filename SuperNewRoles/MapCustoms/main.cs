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
        public static bool isMapCustom(MapCustomId mapCustomId, bool IsChache = true)
        {
            return mapCustomId switch
            {
                MapCustomId.Skeld => GameOptions.MapId == 0 && MapCustom.MapCustomOption.getBool() && MapCustom.SkeldSetting.getBool() && ModeHandler.isMode(ModeId.Default),
                MapCustomId.Mira => GameOptions.MapId == 1 && MapCustom.MapCustomOption.getBool() && MapCustom.MiraSetting.getBool() && ModeHandler.isMode(ModeId.Default),
                MapCustomId.Polus => GameOptions.MapId == 2 && MapCustom.MapCustomOption.getBool() && MapCustom.PolusSetting.getBool() && ModeHandler.isMode(ModeId.Default),
                MapCustomId.Airship => GameOptions.MapId == 4 && MapCustom.MapCustomOption.getBool() && MapCustom.AirshipSetting.getBool() && ModeHandler.isMode(ModeId.Default),
                _ => false,
            };
        }
        public enum MapCustomId
        {
            Skeld,
            Mira,
            Polus,
            Airship,
        }
    }
    [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.OnDestroy))]
    class IntroCutsceneOnDestroyPatch
    {
        public static void Prefix(IntroCutscene __instance)
        {
            // ベントを追加する
            AdditionalVents.AddAdditionalVents();

            // スペシメンにバイタルを移動する
            SpecimenVital.moveVital();

            //配電盤を移動させる
            MoveElecPad.MoveElecPads();

            GameObject gapRoom = DestroyableSingleton<ShipStatus>.Instance.FastRooms[SystemTypes.GapRoom].gameObject;
            // ぬ～んを消す
            if (MapCustomHandler.isMapCustom(MapCustomHandler.MapCustomId.Airship) && MapCustom.AirshipDisableMovingPlatform.getBool())
            {
                gapRoom.GetComponentInChildren<MovingPlatformBehaviour>().gameObject.SetActive(false);
                gapRoom.GetComponentsInChildren<PlatformConsole>().ForEach(x => x.gameObject.SetActive(false));
            }
        }
    }
    public static class Extensions
    {
        public static void ForEach<T>(this IList<T> self, Action<T> todo)
        {
            for (int i = 0; i < self.Count; i++)
            {
                todo(self[i]);
            }
        }
        public static T Random<T>(this IList<T> self)
        {
            if (self.Count > 0)
            {
                return self[UnityEngine.Random.Range(0, self.Count)];
            }
            return default;
        }
    }
}
