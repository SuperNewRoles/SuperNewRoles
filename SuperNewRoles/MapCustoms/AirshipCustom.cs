using System.Linq;
using HarmonyLib;
using UnityEngine;
using SuperNewRoles.CustomOptions.Categories;

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

            // 昇降機の影変更
            if (MapEditSettingsOptions.ModifyGapRoomOneWayShadow)
            {
                var gapRoom = __instance.AllRooms.ToList().Find(n => n.RoomId == SystemTypes.GapRoom).gameObject;
                var shadowObj = gapRoom.transform.FindChild("Shadow");
                if (shadowObj != null)
                {
                    var shadow = shadowObj.GetComponent<SpriteRenderer>();
                    if (shadow != null)
                    {
                        // インポスターが下から上を見ることができる設定
                        if (MapEditSettingsOptions.GapRoomShadowIgnoresImpostors)
                        {
                            shadow.material.SetInt("_Mask", 2);
                        }

                        // 非インポスターが上から下を見ることができない設定
                        if (MapEditSettingsOptions.DisableGapRoomShadowForNonImpostor)
                        {
                            shadow.material.SetInt("_Mask", 1);
                        }
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcUsePlatform))]
    public static class PlayerControlRpcUsePlatformPatch
    {
        public static bool Prefix()
        {
            // ランダムスポーン
            if (MapCustomHandler.IsMapCustom(MapCustomHandler.MapCustomId.Airship) &&
                MapEditSettingsOptions.AirshipRandomSpawn)
            {
                return false;
            }
            return true;
        }
    }
}