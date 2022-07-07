using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;
using System;

namespace SuperNewRoles.MapCustoms
{
    [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.OnDestroy))]
    class IntroCutsceneOnDestroyPatch
    {
        public static void Prefix(IntroCutscene __instance)
        {
            // ベントを追加する
            AdditionalVents.AddAdditionalVents();

            // スペシメンにバイタルを移動する
            SpecimenVital.moveVital();

            GameObject gapRoom = DestroyableSingleton<ShipStatus>.Instance.FastRooms[SystemTypes.GapRoom].gameObject;
            // ぬ～んを消す
            if (PlayerControl.GameOptions.MapId == 4 && MapCustom.AirshipDisableMovingPlatform.getBool())
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
    }
}
