using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using SuperNewRoles.Patches;
using SuperNewRoles.CustomOptions.Categories;

namespace SuperNewRoles.Modules
{
    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.CoStartGame))]
    public static class CoStartGameSumouPatch
    {
        public static void Postfix()
        {
            // Always clear lists on game start, regardless of SumouMode setting,
            // to ensure clean state if the mode is toggled.
            SumouPatch.playerColliders.Clear();
            SumouPatch.playerStopCountdown.Clear(); // Clear the new dictionary

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
        public static Dictionary<byte, Collider2D> playerColliders = new Dictionary<byte, Collider2D>(); // コライダーキャッシュ
        internal static Dictionary<byte, float> playerStopCountdown = new Dictionary<byte, float>(); // PlayerId -> frames to wait until stop
        private const float PushMultiplier = 2.5f;
        private const float DeadBodyKickDistance = 0.2f;
        private const float DeadBodyKickRange = 0.6f;
        private const float StopFramesDuration = 10 / 60f; // Frames to wait before stopping

        public static void Postfix(PlayerPhysics __instance)
        {
            if (!GeneralSettingOptions.SumouMode) return;
            if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started) return;

            PlayerControl selfPlayer = __instance.myPlayer;
            if (selfPlayer == null) return;

            // --- Timer countdown and velocity reset logic ---
            if (playerStopCountdown.TryGetValue(selfPlayer.PlayerId, out float currentTimerValue))
            {
                currentTimerValue -= Time.fixedDeltaTime;
                if (currentTimerValue <= 0)
                {
                    __instance.body.velocity = Vector2.zero;
                    playerStopCountdown.Remove(selfPlayer.PlayerId);
                    if (selfPlayer.AmOwner) // Only the owner should dictate their final velocity state for sync
                    {
                        ModdedNetworkTransform.skipNextBatchPlayers.Add(selfPlayer.PlayerId);
                    }
                }
                else
                {
                    playerStopCountdown[selfPlayer.PlayerId] = currentTimerValue;
                }
            }
            // --- End of timer logic ---

            // ローカルプレイヤーのみ処理
            // PlayerControl selfPlayer = __instance.myPlayer; // Already defined above
            // if (selfPlayer == null) return; // Already checked above

            if (!playerColliders.TryGetValue(selfPlayer.PlayerId, out var myCollider) || myCollider == null) return; // キャッシュから取得

            foreach (var other in PlayerControl.AllPlayerControls)
            {
                if (other == selfPlayer || other.Data.IsDead) continue;

                if (!playerColliders.TryGetValue(other.PlayerId, out var otherCollider) || otherCollider == null) continue; // キャッシュから取得

                if (myCollider.IsTouching(otherCollider))
                {
                    var myVelBeforeThisInteraction = __instance.body.velocity; // Velocity of selfPlayer *before* this specific push
                    var otherVel = other.MyPhysics.body.velocity;
                    var direction = ((Vector2)__instance.transform.position - (Vector2)other.transform.position).normalized;

                    var otherSpeedAlong = Vector2.Dot(otherVel, direction);
                    if (otherSpeedAlong <= 0) continue;

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
                                                !playerStopCountdown.ContainsKey(selfPlayer.PlayerId); // Only start if not already counting down

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
            /*
                        // DeadBodyをキックする処理 (CheckEndGame.csから移動)
                        foreach (DeadBody deadBody in UnityEngine.Object.FindObjectsOfType<DeadBody>())
                        {
                            Vector2 diff = (Vector2)deadBody.transform.position - (Vector2)__instance.transform.position;
                            if (diff.sqrMagnitude <= DeadBodyKickRange * DeadBodyKickRange)
                            {
                                var direction = diff.normalized;
                                var bodyCollider = deadBody.GetComponent<Collider2D>();
                                if (bodyCollider == null)
                                {
                                    var col = deadBody.gameObject.AddComponent<CircleCollider2D>();
                                    col.isTrigger = false;
                                    bodyCollider = col;
                                }
                                RaycastHit2D[] hits = new RaycastHit2D[1];
                                int hitCount = bodyCollider.Cast(direction, hits, DeadBodyKickDistance);
                                float maxDist = DeadBodyKickDistance;
                                if (hitCount > 0)
                                    maxDist = Mathf.Max(0, hits[0].distance - 0.01f);
                                Vector2 actualOffset = direction * maxDist;
                                if (actualOffset.sqrMagnitude > 0.0001f)
                                {
                                    deadBody.transform.position += (Vector3)actualOffset;
                                    // RpcApplyDeadBodyImpulseを呼び出すためにCustomRpcExtsが必要
                                    CustomRpcExts.RpcApplyDeadBodyImpulse(deadBody.ParentId, actualOffset.x, actualOffset.y);
                                }
                            }
                        }*/
        }
    }
}