using System;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using HarmonyLib;
using SuperNewRoles.Mode;
using UnityEngine;
using static SuperNewRoles.MapCustoms.MapCustomHandler;

namespace SuperNewRoles.MapCustoms;

public class MapCustomHandler
{
  
    public static bool IsMapCustom(MapCustomId mapCustomId, bool isDefaultOnly=true)
    {
        bool isCommonDecision = MapCustom.MapCustomOption.GetBool() && (ModeHandler.IsMode(ModeId.Default) || !isDefaultOnly);
        if (!isCommonDecision) return false; // 共通条件を満たしていなかったら, 早期リターン

        byte isMapId = GameManager.Instance.LogicOptions.currentGameOptions.GetByte(ByteOptionNames.MapId);
        return mapCustomId switch
        {
            MapCustomId.Skeld => isMapId == 0 && MapCustom.SkeldSetting.GetBool(),
            MapCustomId.Mira => isMapId == 1 && MapCustom.MiraSetting.GetBool(),
            MapCustomId.Polus => isMapId == 2 && MapCustom.PolusSetting.GetBool(),
            MapCustomId.Airship => isMapId == 4 && MapCustom.AirshipSetting.GetBool(),
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
[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Start))]
class IntroCutsceneOnDestroyPatch
{
    public static void Postfix(ShipStatus __instance)
    {
        if (ModeHandler.IsMode(ModeId.BattleRoyal))
        {
            //SelectRoleSystem.OnEndIntro(); Logger.Info("StartOnEndIntro");
        }

        // 壁越しにタスクを無効化する
        if (IsMapCustom(MapCustomId.Airship) && MapCustom.AntiTaskOverWall.GetBool())
        {
            // シャワー 写真
            var array = new[] { "task_shower", "task_developphotos", "task_garbage1", "task_garbage2", "task_garbage3", "task_garbage4", "task_garbage5" };
            foreach (var c in GameObject.FindObjectsOfType<Console>())
            {
                if (c == null) continue;
                if (array.Any(x => c.name == x)) c.checkWalls = true;

                // 武器庫カチ メインカチ
                if (c.name == "DivertRecieve" && (c.Room == SystemTypes.Armory || c.Room == SystemTypes.MainHall)) c.checkWalls = true;
            }
        }

        // ベントを追加する
        AdditionalVents.AddAdditionalVents();

        // スペシメンにバイタルを移動する
        SpecimenVital.MoveVital();

        //配電盤を移動させる
        MoveElecPad.MoveElecPads();

        if (__instance.FastRooms.ContainsKey(SystemTypes.GapRoom))
        {
            GameObject gapRoom = __instance.AllRooms.ToList().Find(n => n.RoomId == SystemTypes.GapRoom).gameObject;
            // ぬ～んを消す
            if (MapCustomHandler.IsMapCustom(MapCustomHandler.MapCustomId.Airship) && MapCustom.AirshipDisableMovingPlatform.GetBool())
            {
                gapRoom.GetComponentInChildren<MovingPlatformBehaviour>().gameObject.SetActive(false);
                gapRoom.GetComponentsInChildren<PlatformConsole>().ForEach(x => x.gameObject.SetActive(false));
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
[HarmonyPatch(typeof(AirshipExileController), nameof(AirshipExileController.WrapUpAndSpawn))]
class AirshipExileControllerWrapUpAndSpawnPatch
{
    static void Prefix()
    { // エアーシップ電気室のドアをシャッフルする
        if (MapCustoms.MapCustomHandler.IsMapCustom(MapCustomHandler.MapCustomId.Airship)
            && MapCustom.ShuffleElectricalDoors.GetBool()
            && AmongUsClient.Instance.AmHost)
            AirshipStatus.Instance.Systems[SystemTypes.Decontamination].Cast<ElectricalDoors>().Initialize();
    }
}