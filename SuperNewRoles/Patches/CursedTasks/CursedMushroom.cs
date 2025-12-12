using System;
using HarmonyLib;
using UnityEngine;
using SuperNewRoles.Modules;
using SuperNewRoles.CustomObject;
using System.Collections.Generic;

namespace SuperNewRoles.Patches.CursedTasks;

public static class CursedMushroom
{
    private static readonly List<Vector3> FungleMushroomPositions = new()
    {
        new(6.654f, 4.6485f, 0.1046f),
        new(13.9424f, 9.2511f, 0.1093f),
        new(15.2154f, 3.6173f, 0.1036f),
        new(20.3487f, 7.472f, 0.1075f),
        new(26.0282f, 12.9952f, 0.113f),
        new(-15.7754f, 6.0697f, 0.1061f),
        new(-8.3219f, -14.3521f, 0.1144f),
        new(16.9672f, 13.9916f, 0.113f),
        new(8.4608f, 9.9998f, 0.11f),

        /*
        10.3346 16.61 0.0166
        13.9837 18.8841 0.0189
        11.1516 18.177 0.0182
        9.7766 18.052 0.0181
        8.5846 18.4939 0.0185
        */
        new(10.3346f, 16.61f, 0.0166f),
        new(13.9837f, 18.8841f, 0.1189f),
        new(11.1516f, 18.177f, 0.1182f),
        new(9.7766f, 18.052f, 0.1181f),
        new(8.5846f, 18.4939f, 0.1185f),
        new(6.1528f, 18.4484f, 0.1184f)
    };
    public static void SpawnCustomMushroomFungle()
    {
        if (!MapNames.Fungle.IsMap()) return;
        foreach (Vector3 position in FungleMushroomPositions)
        {
            CustomSpores.AddMushroom(position, (mushroom) =>
            {
            });
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

        private static PlayerControl GetMostNearPlayer(Mushroom mushroom, out float nearestDist)
        {
            PlayerControl targetPlayer = null;
            nearestDist = float.MaxValue;
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                if (player == null) continue;
                if (player.Data == null) continue;
                if (player.Data.IsDead) continue;
                float d = Vector2.Distance(mushroom.origPosition, player.transform.position);
                if (d < nearestDist)
                {
                    nearestDist = d;
                    targetPlayer = player;
                }
            }
            return targetPlayer;
        }
        public static void UpdatePosition(Mushroom __instance)
        {
            if (__instance == null) return;
            if (__instance.gameObject == null) return;
            if (!__instance.gameObject.activeInHierarchy) return;

            // origPosition は Vector2 で保存されているマップ上の元位置
            Vector2 origin = __instance.origPosition;
            Vector3 origin3 = new(origin.x, origin.y, __instance.transform.position.z);

            // 生存プレイヤーから最も近いものを探す
            PlayerControl targetPlayer = GetMostNearPlayer(__instance, out float nearestDist);

            Vector3 currentPos = __instance.transform.position;

            if (targetPlayer != null && nearestDist <= TriggerRange)
            {
                // プレイヤーが範囲内にいる -> 追いかける
                Vector3 targetPos = targetPlayer.transform.position;
                targetPos.z = currentPos.z;
                Vector3 dir = targetPos - currentPos;
                if (dir.sqrMagnitude > 0.0001f)
                {
                    float step = MoveSpeed * Time.deltaTime;
                    Vector3 next = currentPos + Vector3.ClampMagnitude(dir, step);
                    // 元位置からの最大距離でクランプ
                    Vector3 fromOrigin = next - origin3;
                    if (fromOrigin.magnitude > MaxDisplacement)
                    {
                        next = origin3 + fromOrigin.normalized * MaxDisplacement;
                    }
                    __instance.transform.position = next;
                }
            }
            else
            {
                // いない場合は元に戻る
                Vector3 dir = origin3 - currentPos;
                if (dir.sqrMagnitude > 0.0001f)
                {
                    float step = MoveSpeed * Time.deltaTime;
                    Vector3 next = currentPos + Vector3.ClampMagnitude(dir, step);
                    __instance.transform.position = next;
                }
            }
        }
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
