using System.Linq;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using SuperNewRoles.CustomOptions.Categories;
using SuperNewRoles.Modules;

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

    // ランダムスポーン機能
    [HarmonyPatch(typeof(SpawnInMinigame), nameof(SpawnInMinigame.Begin))]
    public static class SpawnInMinigameBeginPatch
    {
        public static void Postfix(SpawnInMinigame __instance)
        {
            if (!MapCustomHandler.IsMapCustom(MapCustomHandler.MapCustomId.Airship) ||
                !MapEditSettingsOptions.AirshipRandomSpawn)
                return;
            /*

            // スポーン位置を保存
            RandomSpawnLocations = __instance.Locations.ToList().ConvertAll(x => (Vector2)x.Location);
            IsRandomSpawnLoading = true;
            RandomSpawnLastCount = -1;

            // ホストの場合、プレイヤーを一時的な位置に移動
            if (AmongUsClient.Instance.AmHost)
            {
                foreach (ExPlayerControl p in ExPlayerControl.ExPlayerControls)
                {
                    p.RpcCustomSnapTo(new Vector2(3, 6));
                }
            }

            // ローカルプレイヤーを画面外に移動
            ExPlayerControl.LocalPlayer.RpcCustomSnapTo(new Vector2(-30, 30));*/

            // ランダムなスポーン位置を選択
            __instance.LocationButtons[ModHelpers.GetRandomIndex(__instance.LocationButtons)].OnClick.Invoke();

            // スポーン選択画面を閉じる
            __instance.Close();
        }
    }

    // ランダムスポーン処理の更新
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    public static class HudManagerUpdatePatch
    {
        public static void Postfix()
        {/*
            return;
            if (!AmongUsClient.Instance.AmHost ||
                !IsRandomSpawnLoading ||
                AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started ||
                !MapCustomHandler.IsMapCustom(MapCustomHandler.MapCustomId.Airship) ||
                !MapEditSettingsOptions.AirshipRandomSpawn)
                return;

            List<PlayerControl> loadedPlayers = new();
            bool allPlayersLoaded = true;
            int notLoadedCount = 0;

            // プレイヤーのロード状態を確認
            foreach (ExPlayerControl p in ExPlayerControl.ExPlayerControls)
            {
                if (p.IsDead())
                    continue;

                // 一時的な位置にいるプレイヤーはまだロードされていない
                if (ModHelpers.IsPositionDistance(p.transform.position, new Vector2(3, 6), 0.5f) ||
                    ModHelpers.IsPositionDistance(p.transform.position, new Vector2(-25, 40), 0.5f) ||
                    ModHelpers.IsPositionDistance(p.transform.position, new Vector2(-1.4f, 2.3f), 0.5f))
                {
                    allPlayersLoaded = false;
                    notLoadedCount++;
                }
                else
                {
                    loadedPlayers.Add(p);
                    p.RpcCustomSnapTo(new Vector2(-30, 30));
                }
            }

            // ロード中のプレイヤー数が変わった場合、名前を更新
            if (RandomSpawnLastCount != loadedPlayers.Count)
            {
                RandomSpawnLastCount = loadedPlayers.Count;
                string loadingMessage = $"\n\n\n\n\n\n\n\n<size=300%><color=white>SuperNewRoles</size>\n\n\n\n\n\n\n\n\n\n\n\n\n\n<size=200%><color=white>ロード中... {notLoadedCount}人";

                foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                {
                    if (!p.AmOwner)
                    {
                        p.RpcSetNamePrivate(loadingMessage);
                    }
                    else
                    {
                        p.SetName(loadingMessage);
                    }
                }
            }

            // 全プレイヤーがロードされた場合、ランダムな位置にスポーン
            if (allPlayersLoaded)
            {
                IsRandomSpawnLoading = false;
                RandomSpawnLastCount = -1;

                foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                {
                    string name = p.GetDefaultName();
                    p.RpcSetNamePrivate(name);

                    if (!p.IsBot())
                    {
                        // ランダムな位置にスポーン
                        p.RpcSnapTo(RandomSpawnLocations.GetRandom());
                    }
                }
            }*/
        }
    }
}