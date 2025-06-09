using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using SuperNewRoles.Patches;
using SuperNewRoles.CustomOptions.Categories;
using System;

namespace SuperNewRoles.Modules
{
    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.CoStartGame))]
    public static class CoStartGameSumouPatch
    {
        public static void Postfix()
        {
            // Always clear lists on game start, regardless of SumouMode setting,
            // to ensure clean state if the mode is toggled.
            for (int i = 0; i < SumouPatch.playerColliders.Length; i++)
            {
                SumouPatch.playerColliders[i] = null;
            }
            for (int i = 0; i < SumouPatch.playerStopCountdown.Length; i++)
            {
                SumouPatch.playerStopCountdown[i] = 0;
            }

            if (!GeneralSettingOptions.SumouMode) return;

            // SumouPatch.playerColliders.Clear(); // Already cleared above
            // SumouPatch.playerStopCountdown.Clear(); // Already cleared above
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                GameObject playerCollider = new("PlayerCollider");
                playerCollider.transform.SetParent(player.transform);
                playerCollider.transform.localPosition = new(0, 0.125f, 0);
                playerCollider.transform.localScale = Vector3.one;
                BoxCollider2D boxCollider = playerCollider.AddComponent<BoxCollider2D>();
                boxCollider.isTrigger = true;
                boxCollider.size = new Vector2(0.6f, 0.8f);
                SumouPatch.playerColliders[player.PlayerId] = boxCollider; // キャッシュに登録
            }
        }
    }

    [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.FixedUpdate))]
    public static class SumouPatch
    {
        public static Collider2D[] playerColliders = new Collider2D[byte.MaxValue]; // コライダーキャッシュ
        internal static float[] playerStopCountdown = new float[byte.MaxValue]; // PlayerId -> frames to wait until stop
        private const float PushMultiplier = 2.5f;
        private const float DeadBodyKickDistance = 0.2f;
        private const float DeadBodyKickRange = 0.6f;
        private const float StopFramesDuration = 10 / 60f; // Frames to wait before stopping
        public static bool HighPerformance => GeneralSettingOptions.SumouHighPerformance;

        public static void Prefix(PlayerPhysics __instance, out Vector2 __state)
        {
            __state = Vector2.zero;
            if (!GeneralSettingOptions.SumouMode || __instance.myPlayer == null || !__instance.myPlayer.AmOwner) return;
            __state = __instance.transform.position;
        }

        public static void Postfix(PlayerPhysics __instance, Vector2 __state)
        {
            if (!GeneralSettingOptions.SumouMode) return;
            if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started) return;

            PlayerControl selfPlayer = __instance.myPlayer;
            if (selfPlayer == null) return;
            if (!__instance.myPlayer.AmOwner && Vector3.Distance(PlayerControl.LocalPlayer.transform.position, __instance.transform.position) > 6f) return;

            // --- Timer countdown and velocity reset logic ---
            if (playerStopCountdown[selfPlayer.PlayerId] > 0)
            {
                playerStopCountdown[selfPlayer.PlayerId] -= Time.fixedDeltaTime;
                if (playerStopCountdown[selfPlayer.PlayerId] <= 0)
                {
                    __instance.body.velocity = Vector2.zero;
                    playerStopCountdown[selfPlayer.PlayerId] = 0;
                    if (selfPlayer.AmOwner) // Only the owner should dictate their final velocity state for sync
                    {
                        ModdedNetworkTransform.skipNextBatchPlayers.Add(selfPlayer.PlayerId);
                    }
                }
                else
                {
                    playerStopCountdown[selfPlayer.PlayerId] = StopFramesDuration;
                }
            }

            if (playerColliders[selfPlayer.PlayerId] == null) return; // キャッシュから取得

            if (HighPerformance && !__instance.myPlayer.AmOwner)
            {
                CheckAndPushPlayer(selfPlayer, ExPlayerControl.LocalPlayer, __instance, playerColliders[selfPlayer.PlayerId]);
            }
            else
            {
                foreach (var other in PlayerControl.AllPlayerControls)
                {
                    CheckAndPushPlayer(selfPlayer, other, __instance, playerColliders[selfPlayer.PlayerId]);
                }
            }

            // 壁の外に出た場合のロールバック処理
            if (selfPlayer.AmOwner && __state != Vector2.zero)
            {
                int shipAndObjectsMask = Constants.ShipAndObjectsMask;
                Collider2D[] hitColliders = Physics2D.OverlapPointAll(selfPlayer.GetTruePosition(), shipAndObjectsMask);
                bool isInsideWall = false;
                foreach (var hitCollider in hitColliders)
                {
                    if (hitCollider != null && !hitCollider.isTrigger)
                    {
                        isInsideWall = true;
                        break;
                    }
                }
                if (isInsideWall)
                {
                    __instance.transform.position = __state;
                    __instance.body.velocity = Vector2.zero;
                    // ネットワーク同期をスキップして即座に反映
                    ModdedNetworkTransform.skipNextBatchPlayers.Add(selfPlayer.PlayerId);
                }
            }
        }
        private static void CheckAndPushPlayer(PlayerControl selfPlayer, PlayerControl other, PlayerPhysics __instance, Collider2D myCollider)
        {
            if (other == selfPlayer || other.Data.IsDead) return;

            // パフォーマンス対策で近くにいなかったら処理しない
            if (Vector3.Distance(selfPlayer.transform.position, other.transform.position) > 10f) return;

            if (playerColliders[other.PlayerId] == null) return; // キャッシュから取得

            if (other.Collider == null || !other.Collider.enabled) return;
            if (selfPlayer.Collider == null || !selfPlayer.Collider.enabled) return;

            if (myCollider.IsTouching(playerColliders[other.PlayerId]))
            {
                var myVelBeforeThisInteraction = __instance.body.velocity; // Velocity of selfPlayer *before* this specific push
                var otherVel = other.MyPhysics.body.velocity;
                var direction = ((Vector2)__instance.transform.position - (Vector2)other.transform.position).normalized;

                var otherSpeedAlong = Vector2.Dot(otherVel, direction);
                if (otherSpeedAlong <= 0) return;

                var relSpeed = Vector2.Dot(otherVel - myVelBeforeThisInteraction, direction);
                if (relSpeed > 0)
                {
                    // Push処理
                    Vector2 selfChange = direction * relSpeed * (__instance.myPlayer.AmOwner ? PushMultiplier : 0.405f);
                    __instance.body.velocity = myVelBeforeThisInteraction + selfChange; // Apply push

                    // --- Condition to start the stop timer ---
                    bool shouldStartTimer = selfPlayer.AmOwner &&
                                            (!selfPlayer.CanMove) &&
                                            myVelBeforeThisInteraction.sqrMagnitude < 0.001f && // Check if velocity *before this push* was (near) zero
                                            playerStopCountdown[selfPlayer.PlayerId] == 0; // Only start if not already counting down

                    if (shouldStartTimer)
                    {
                        playerStopCountdown[selfPlayer.PlayerId] = StopFramesDuration;
                    }
                    // --- End of condition to start the stop timer ---

                    // 相手側はネットワーク同期をスキップして即座に反映
                    ModdedNetworkTransform.skipNextBatchPlayers.Add(__instance.myPlayer.PlayerId);
                }
            }
        }
    }
}