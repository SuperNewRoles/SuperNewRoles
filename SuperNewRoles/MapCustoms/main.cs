using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using SuperNewRoles.Mode;
using UnityEngine;
using static PlayerControl;
using static SuperNewRoles.MapCustoms.MapCustomHandler;

namespace SuperNewRoles.MapCustoms
{
    public class MapCustomHandler
    {
        public static bool IsMapCustom(MapCustomId mapCustomId)
        {
            return mapCustomId switch
            {
                MapCustomId.Skeld => GameOptions.MapId == 0 && MapCustom.MapCustomOption.GetBool() && MapCustom.SkeldSetting.GetBool() && ModeHandler.IsMode(ModeId.Default),
                MapCustomId.Mira => GameOptions.MapId == 1 && MapCustom.MapCustomOption.GetBool() && MapCustom.MiraSetting.GetBool() && ModeHandler.IsMode(ModeId.Default),
                MapCustomId.Polus => GameOptions.MapId == 2 && MapCustom.MapCustomOption.GetBool() && MapCustom.PolusSetting.GetBool() && ModeHandler.IsMode(ModeId.Default),
                MapCustomId.Airship => GameOptions.MapId == 4 && MapCustom.MapCustomOption.GetBool() && MapCustom.AirshipSetting.GetBool() && ModeHandler.IsMode(ModeId.Default),
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
            SpecimenVital.MoveVital();

            //配電盤を移動させる
            MoveElecPad.MoveElecPads();

            if (ShipStatus.Instance.FastRooms.ContainsKey(SystemTypes.GapRoom))
            {
                GameObject gapRoom = FastDestroyableSingleton<ShipStatus>.Instance.FastRooms[SystemTypes.GapRoom].gameObject;
                // ぬ～んを消す
                if (MapCustomHandler.IsMapCustom(MapCustomHandler.MapCustomId.Airship) && MapCustom.AirshipDisableMovingPlatform.GetBool())
                {
                    gapRoom.GetComponentInChildren<MovingPlatformBehaviour>().gameObject.SetActive(false);
                    gapRoom.GetComponentsInChildren<PlatformConsole>().ForEach(x => x.gameObject.SetActive(false));
                }
            }

            // 壁越しにタスクを無効化する
            if (IsMapCustom(MapCustomId.Airship) && MapCustom.AntiTaskOverWall.GetBool())
            {
                // シャワー 写真 武器庫カチ トイレゴミ 医務室ゴミ
                var array = new[] { "task_shower", "task_developphotos","panel_data","task_garbage5","task_garbage1" };
                foreach(var c in  GameObject.FindObjectsOfType<Console>()) {
                    if (c == null) continue;
                    if (array.Any(x => c.name == x)) c.checkWalls = true;
                }
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
            return self.Count > 0 ? self[UnityEngine.Random.Range(0, self.Count)] : default;
        }
    }
}