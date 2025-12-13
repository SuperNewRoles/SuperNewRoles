using System;
using HarmonyLib;
using UnityEngine;
using SuperNewRoles.Modules;
using SuperNewRoles.CustomObject;
using System.Collections.Generic;

namespace SuperNewRoles.Patches.CursedTasks;

public static class CursedMushroom
{
    private static readonly List<Vector2> FungleMushroomPositions = new()
    {
        new(6.654f, 4.6485f),
        new(13.9424f, 9.2511f),
        new(15.2154f, 3.6173f),
        new(20.3487f, 7.472f),
        new(26.0282f, 12.9952f),
        new(-15.7754f, 6.0697f),
        new(-8.3219f, -14.3521f),
        new(16.9672f, 13.9916f),
        new(8.4608f, 9.9998f),

        /*
        10.3346 16.61 0.0166
        13.9837 18.8841 0.0189
        11.1516 18.177 0.0182
        9.7766 18.052 0.0181
        8.5846 18.4939 0.0185
        */
        new(10.3346f, 16.61f),
        new(13.9837f, 18.8841f),
        new(11.1516f, 18.177f),
        new(9.7766f, 18.052f),
        new(8.5846f, 18.4939f),
        new(6.1528f, 18.4484f)
    };
    public static void SpawnCustomMushroomFungle()
    {
        if (!MapNames.Fungle.IsMap()) return;
        foreach (Vector2 position in FungleMushroomPositions)
        {
            CustomSpores.AddMushroom(position, (mushroom) => { });
        }
    }
    // Cursed モード時に、マッシュルームが元の位置から一定範囲内で
    // プレイヤーに向かって追いかける動作を追加するパッチ。
    [HarmonyPatch(typeof(Mushroom), nameof(Mushroom.TriggerSpores))]
    public static class MushroomTriggerSporesPatc
    {
        private static Vector3 _lastPosition;
        public static void Prefix(Mushroom __instance)
        {
            _lastPosition = __instance.transform.position;
        }
        public static void Postfix(Mushroom __instance)
        {
            if (!Main.IsCursed) return;
            __instance.transform.position = _lastPosition;
            MushroomFixedUpdatePatch.UpdatePosition(__instance);
        }
    }

    [HarmonyPatch(typeof(Mushroom), nameof(Mushroom.FixedUpdate))]
    public static class MushroomFixedUpdatePatch
    {
        private const float TriggerRange = 3.0f; // プレイヤーを追いかけるトリガー距離
        private const float MaxDisplacement = 2.0f; // 元位置からこれ以上離れない
        private const float MoveSpeed = 1.2f; // 移動速度 (units/sec)
        private const float RecheckIntervalSeconds = 0.15f; // 近傍プレイヤー探索を間引く
        private const float EpsilonSqr = 0.000001f;

        private static readonly List<PlayerControl> AlivePlayersCache = new(16);
        private static float _alivePlayersCacheFixedTime = -1f;

        private struct TargetCacheEntry
        {
            public PlayerControl Target;
            public float NearestSqrDist;
            public float LastRecheckTime;
        }

        private static readonly Dictionary<int, TargetCacheEntry> TargetCache = new();

        private static void EnsureAlivePlayersCache()
        {
            if (Time.fixedTime - _alivePlayersCacheFixedTime <= 0.1f) return;
            _alivePlayersCacheFixedTime = Time.fixedTime;

            AlivePlayersCache.Clear();
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                if (player == null) continue;
                if (player.Data == null) continue;
                if (player.Data.IsDead) continue;
                AlivePlayersCache.Add(player);
            }
        }

        private static PlayerControl GetMostNearPlayer(Vector2 origin, out float nearestSqrDist)
        {
            PlayerControl targetPlayer = null;
            nearestSqrDist = float.MaxValue;

            EnsureAlivePlayersCache();
            for (int i = 0; i < AlivePlayersCache.Count; i++)
            {
                PlayerControl player = AlivePlayersCache[i];
                Vector3 p = player.transform.position;
                float dx = origin.x - p.x;
                float dy = origin.y - p.y;
                float d2 = (dx * dx) + (dy * dy);
                if (d2 < nearestSqrDist)
                {
                    nearestSqrDist = d2;
                    targetPlayer = player;
                    if (nearestSqrDist <= EpsilonSqr) break;
                }
            }
            return targetPlayer;
        }

        private static bool TryGetTarget(Mushroom mushroom, Vector2 origin, float triggerRangeSqr, out PlayerControl target, out float nearestSqrDist)
        {
            int key = mushroom.GetInstanceID();
            bool hasCache = TargetCache.TryGetValue(key, out TargetCacheEntry entry);

            bool cacheValid =
                hasCache &&
                entry.Target != null &&
                entry.Target.Data != null &&
                !entry.Target.Data.IsDead;

            bool shouldRecheck = !cacheValid || (Time.time - entry.LastRecheckTime) >= RecheckIntervalSeconds;
            if (shouldRecheck)
            {
                entry.Target = GetMostNearPlayer(origin, out entry.NearestSqrDist);
                entry.LastRecheckTime = Time.time;
                TargetCache[key] = entry;
            }

            target = entry.Target;
            nearestSqrDist = entry.NearestSqrDist;
            return target != null && nearestSqrDist <= triggerRangeSqr;
        }

        public static void UpdatePosition(Mushroom __instance)
        {
            if (__instance == null) return;
            if (__instance.gameObject == null) return;
            if (!__instance.gameObject.activeInHierarchy)
            {
                TargetCache.Remove(__instance.GetInstanceID());
                return;
            }

            // origPosition は Vector2 で保存されているマップ上の元位置
            Vector2 origin = __instance.origPosition;
            Vector3 currentPos = __instance.transform.position;
            Vector3 origin3 = new(origin.x, origin.y, currentPos.z);

            float triggerRangeSqr = TriggerRange * TriggerRange;
            float maxDisplacementSqr = MaxDisplacement * MaxDisplacement;

            // 生存プレイヤーから最も近いものを探す（探索は間引く）
            bool shouldChase = TryGetTarget(__instance, origin, triggerRangeSqr, out PlayerControl targetPlayer, out float nearestSqrDist);

            float step = MoveSpeed * Time.deltaTime;
            float stepSqr = step * step;

            if (shouldChase)
            {
                // プレイヤーが範囲内にいる -> 追いかける
                Vector3 targetPos = targetPlayer.transform.position;
                targetPos.z = currentPos.z;
                Vector3 dir = targetPos - currentPos;
                float dirSqr = dir.sqrMagnitude;
                if (dirSqr > EpsilonSqr)
                {
                    if (dirSqr > stepSqr)
                        dir *= step / Mathf.Sqrt(dirSqr);

                    Vector3 next = currentPos + dir;
                    // 元位置からの最大距離でクランプ
                    Vector3 fromOrigin = next - origin3;
                    float fromOriginSqr = fromOrigin.sqrMagnitude;
                    if (fromOriginSqr > maxDisplacementSqr)
                    {
                        fromOrigin *= MaxDisplacement / Mathf.Sqrt(fromOriginSqr);
                        next = origin3 + fromOrigin;
                    }
                    __instance.transform.position = next;
                }
            }
            else
            {
                // いない場合は元に戻る
                Vector3 dir = origin3 - currentPos;
                float dirSqr = dir.sqrMagnitude;
                if (dirSqr > EpsilonSqr)
                {
                    if (dirSqr > stepSqr)
                        dir *= step / Mathf.Sqrt(dirSqr);

                    Vector3 next = currentPos + dir;
                    __instance.transform.position = next;
                }
            }
        }
        private static int counter = 0;
        public static void Postfix(Mushroom __instance)
        {
            try
            {
                if (!Main.IsCursed) return;
                UpdatePosition(__instance);
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
            }
        }
    }
}
