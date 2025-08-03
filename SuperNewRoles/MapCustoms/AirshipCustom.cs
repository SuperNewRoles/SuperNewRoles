using System.Linq;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using SuperNewRoles.CustomOptions.Categories;
using SuperNewRoles.Modules;
using SuperNewRoles.Events;

namespace SuperNewRoles.MapCustoms;

public static class AirshipCustom
{
    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Awake))]
    public static class ShipStatusAwakePatch
    {
        public static void Postfix(ShipStatus __instance)
        {
            if (!MapCustomHandler.IsMapCustom(MapCustomHandler.MapCustomId.Airship))
                return;

            // アーカイブアドミン封印
            if (MapEditSettingsOptions.RecordsAdminDestroy)
            {
                Transform Admin = GameObject.Find("Airship(Clone)").transform.FindChild("Records").FindChild("records_admin_map");
                Object.Destroy(Admin.gameObject);
            }
        }
    }

    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Start))]
    public static class ShipStatusStartPatch
    {
        public static void Postfix(ShipStatus __instance)
        {
            if (!MapCustomHandler.IsMapCustom(MapCustomHandler.MapCustomId.Airship))
                return;

            // 壁越しタスク禁止
            if (MapEditSettingsOptions.AntiTaskOverWall)
            {
                // シャワー 写真
                var array = new[] { "task_shower", "task_developphotos", "task_garbage1", "task_garbage2", "task_garbage3", "task_garbage4", "task_garbage5" };
                foreach (var c in Object.FindObjectsOfType<Console>())
                {
                    if (c == null) continue;
                    if (array.Any(x => c.name == x)) c.checkWalls = true;

                    // 武器庫カチ メインカチ
                    if (c.name == "DivertRecieve" && (c.Room == SystemTypes.Armory || c.Room == SystemTypes.MainHall)) c.checkWalls = true;
                }
            }
            SetRoleEvent.Instance.AddListener((data) => OnSetRole(data));
        }
        private static void OnSetRole(SetRoleEventData data)
        {
            if (!data.player.AmOwner) return;
            if (MapEditSettingsOptions.ModifyGapRoomOneWayShadow && ShipStatus.Instance.FastRooms.TryGetValue(SystemTypes.GapRoom, out var gapRoom))
            {
                var gapRoomShadow = gapRoom.GetComponentInChildren<OneWayShadows>();
                if (gapRoomShadow == null) return;
                var amImpostorLight = ExPlayerControl.LocalPlayer.IsImpostor() || ExPlayerControl.LocalPlayer.HasImpostorVision();
                if (MapEditSettingsOptions.GapRoomShadowIgnoresImpostors && amImpostorLight)
                {
                    // オブジェクトを非アクティブにすると影判定自体が消えるのでどちらからでも見通せる
                    gapRoomShadow.gameObject.SetActive(false);
                }
                else if (MapEditSettingsOptions.DisableGapRoomShadowForNonImpostor && !amImpostorLight)
                {
                    // OneWayShadowsを無効にしても影判定は残るので普通の壁のような双方向の影になる
                    gapRoomShadow.enabled = false;
                }
            }
        }
    }

    // ランダムスポーン機能
    [HarmonyPatch(typeof(SpawnInMinigame), nameof(SpawnInMinigame.Begin))]
    public static class SpawnInMinigameBeginPatch
    {
        public static void Postfix(SpawnInMinigame __instance)
        {
            if (!MapCustomHandler.IsMapCustom(MapCustomHandler.MapCustomId.Airship) ||
                !MapEditSettingsOptions.AirshipRandomSpawn)
                return;

            // ランダムなスポーン位置を選択
            new LateTask(() => __instance.LocationButtons[ModHelpers.GetRandomIndex(__instance.LocationButtons)].ReceiveClickUp(), 0f);
        }
    }
}