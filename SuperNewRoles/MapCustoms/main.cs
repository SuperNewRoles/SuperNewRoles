using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;
using System;
using static PlayerControl;
using SuperNewRoles.Mode;

namespace SuperNewRoles.MapCustoms
{
    public static class MapCustomHandler
    {
        //if簡略化
        public static int isMap()
        {
            //値はMapidに準拠
            //設定してないのに有効になる問題修正の簡略化
            //通常モードしか対応していない
            if (GameOptions.MapId == 0 && MapCustom.MapCustomOption.getBool() && MapCustom.SkeldSetting.getBool() && ModeHandler.isMode(ModeId.Default))
            {
                //スケルド
                return 0;
            }
            if (GameOptions.MapId == 1 && MapCustom.MapCustomOption.getBool() && MapCustom.MiraSetting.getBool() && ModeHandler.isMode(ModeId.Default))
            {
                //ミラ
                return 1;
            }
            //ドレクスは使用しないので省略
            if (GameOptions.MapId == 2 && MapCustom.MapCustomOption.getBool() && MapCustom.PolusSetting.getBool() && ModeHandler.isMode(ModeId.Default))
            {
                //ポーラス
                return 2;
            }
            if (GameOptions.MapId == 4 && MapCustom.MapCustomOption.getBool() && MapCustom.AirshipSetting.getBool() && ModeHandler.isMode(ModeId.Default))
            {
                //エアーシップ
                return 4;
            }
            //この100の意味はない
            return 100;
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

            GameObject gapRoom = DestroyableSingleton<ShipStatus>.Instance.FastRooms[SystemTypes.GapRoom].gameObject;
            // ぬ～んを消す
            if (Booler.IsAirshipOn() && MapCustom.AirshipDisableMovingPlatform.getBool())
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
