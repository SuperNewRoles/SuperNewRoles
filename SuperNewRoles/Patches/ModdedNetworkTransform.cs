using System.Collections.Generic;
using HarmonyLib;
using SuperNewRoles.CustomOptions.Categories;
using SuperNewRoles.Modules;
using UnityEngine;
using System.Linq;

namespace SuperNewRoles.Patches;
public static class ModdedNetworkTransform
{
    public static readonly Dictionary<byte, float> SEND_TIMER_THRESHOLD_LEVELS =
    new()
    {
        {(byte)NetworkTransformTypeLowLatencyLevel.Max, 0.01f},
        {(byte)NetworkTransformTypeLowLatencyLevel.High, 0.05f},
        {(byte)NetworkTransformTypeLowLatencyLevel.Medium, 0.1f},
        {(byte)NetworkTransformTypeLowLatencyLevel.Low, 0.15f},
        {(byte)NetworkTransformTypeLowLatencyLevel.SuperLow, 0.2f},
        {(byte)NetworkTransformTypeLowLatencyLevel.Bad, 0.5f},
    };
    // Constants
    // 移動データの一括送信間隔
    private static float SEND_TIMER_THRESHOLD => SEND_TIMER_THRESHOLD_LEVELS[(byte)GeneralSettingOptions.NetworkTransformTypeLowLatencyLevel];
    // 移動データ保存をスキップする最初の時間
    private const float DONT_SAVE_MOVEMENT_TIMER_THRESHOLD = 0.1f;
    // 滑らかな停止にかける合計時間
    private const float SMOOTHING_TOTAL_TIME = 0.12f;
    // 位置同期を強制するまでのバッファデータ数
    private const int SYNC_POSITION_LIMIT_INITIAL_VALUE = 5;
    // 停止時の滑らかな動きにかける時間
    private const float STOP_SMOOTHING_TIME = 0.05f;
    // 方向転換検出の閾値 (ドット積)
    private const float DIRECTION_CHANGE_DOT_THRESHOLD = 0.0f;
    // 停止確定までのフレーム数
    private const int STOP_CONFIRM_FRAMES = 5;
    // 速度ゼロ判定の閾値 (平方)
    private const float VELOCITY_ZERO_THRESHOLD_SQR = 0.0001f; // 0.01f * 0.01f
    // ハードシンクを実行する距離の閾値
    private const float HARD_SYNC_DISTANCE_THRESHOLD = 0.5f;
    // ワープを実行する距離の閾値 (RpcStartMovement)
    private const float START_WARP_DISTANCE_THRESHOLD = 1.5f;
    // Lerp係数 (位置)
    private const float POSITION_LERP_FACTOR = 0.7f;
    // Lerp係数 (速度)
    private const float VELOCITY_LERP_FACTOR = 0.8f;
    // Lerp係数 (アイドル減速)
    private const float IDLE_DECELERATION_LERP_FACTOR = 0.15f;


    // フレームごとの位置と速度をまとめるデータ
    public struct MovementData
    {
        public Vector3 position; public Vector2 velocity;
    }
    // 移動同期用のバッファとキュー
    private static List<MovementData> movementBuffer;
    private static Dictionary<byte, Queue<MovementData>> movementQueues;

    public static Dictionary<byte, Vector2?> finishedMovementPosition;

    public static Vector2 lastVelocity;

    public static float sendTimer;
    public static float dontSaveMovementTimerTimer;


    private static Dictionary<byte, float> smoothingTimeLeft;
    private static Dictionary<byte, Vector2> smoothingVelocity;
    private static Dictionary<byte, int> syncPositionLimit;
    private static Dictionary<byte, int> stopDetectionCounter;

    public static void Initialize()
    {
        finishedMovementPosition = new();
        sendTimer = 0f;
        movementBuffer = new List<MovementData>();
        movementQueues = new Dictionary<byte, Queue<MovementData>>();
        smoothingTimeLeft = new Dictionary<byte, float>();
        smoothingVelocity = new Dictionary<byte, Vector2>();
        syncPositionLimit = new Dictionary<byte, int>();
        stopDetectionCounter = new Dictionary<byte, int>();
        lastVelocity = Vector2.zero;
        // Ensure all players have queues initialized? Or handle dynamically.
    }

    // Helper to get or add a queue for a player
    private static Queue<MovementData> GetOrCreateQueue(byte playerId)
    {
        if (!movementQueues.TryGetValue(playerId, out var queue))
        {
            queue = new Queue<MovementData>();
            movementQueues[playerId] = queue;
        }
        return queue;
    }


    public static void FixedUpdate(PlayerControl player)
    {
        if (player.NetTransform.isPaused)
        {
            // Clear state for paused players
            finishedMovementPosition.Remove(player.PlayerId);
            smoothingTimeLeft.Remove(player.PlayerId);
            smoothingVelocity.Remove(player.PlayerId);
            stopDetectionCounter.Remove(player.PlayerId);
            if (movementQueues.TryGetValue(player.PlayerId, out var queue))
            {
                queue.Clear();
            }
            return;
        }

        if (player.AmOwner)
        {
            FixedUpdateOwner(player);
        }
        else
        {
            FixedUpdateRemote(player);
        }
    }

    private static void FixedUpdateOwner(PlayerControl player)
    {
        Vector2 currentVelocity = player.MyPhysics.body.velocity;
        bool isCurrentlyStopped = currentVelocity.sqrMagnitude < VELOCITY_ZERO_THRESHOLD_SQR; // Use squared magnitude for efficiency
        bool wasMovingLastFrame = lastVelocity.sqrMagnitude >= VELOCITY_ZERO_THRESHOLD_SQR;
        bool isCountingDown = stopDetectionCounter.ContainsKey(player.PlayerId); // No need for TryGetValue here

        if (isCurrentlyStopped)
        {
            HandleOwnerStop(player, wasMovingLastFrame, isCountingDown);
        }
        else // Moving
        {
            HandleOwnerMove(player, currentVelocity, isCountingDown);
        }
    }

    private static void HandleOwnerStop(PlayerControl player, bool wasMovingLastFrame, bool isCountingDown)
    {
        int currentStopCount = stopDetectionCounter.TryGetValue(player.PlayerId, out var count) ? count : 0;

        if (wasMovingLastFrame || isCountingDown) // Start stopping or continue countdown
        {
            int nextCount = currentStopCount + 1;
            stopDetectionCounter[player.PlayerId] = nextCount;

            if (nextCount >= STOP_CONFIRM_FRAMES) // Stop confirmed
            {
                // --- Confirmed Stop ---
                lastVelocity = Vector2.zero; // Confirm stopped state
                sendTimer = 0f;
                dontSaveMovementTimerTimer = 0f;

                // Send remaining buffer if any
                SendMovementBuffer(player);

                RpcStopMovement(player, player.transform.position);
                stopDetectionCounter.Remove(player.PlayerId); // Reset counter
                // --- End Confirmed Stop ---
            }
            // else: Still counting down, do nothing (no RPCs, no buffer changes)
        }
        // else: Was already stopped last frame, do nothing
    }

    private static void HandleOwnerMove(PlayerControl player, Vector2 currentVelocity, bool isCountingDown)
    {
        if (isCountingDown)
        {
            // Moved while counting down -> Reset counter
            stopDetectionCounter.Remove(player.PlayerId);
            // lastVelocity remains from before the countdown started, allowing correct detection below.
        }

        // --- Determine Movement State ---
        // Started moving only if not previously counting down and was stopped.
        bool startedMoving = !isCountingDown && lastVelocity.sqrMagnitude < VELOCITY_ZERO_THRESHOLD_SQR;

        // Significant direction change check
        bool directionChangedSignificantly = !startedMoving &&
                                             lastVelocity.sqrMagnitude > VELOCITY_ZERO_THRESHOLD_SQR && // Check if last velocity was significant
                                             currentVelocity.sqrMagnitude > VELOCITY_ZERO_THRESHOLD_SQR && // Check if current velocity is significant
                                             Vector2.Dot(lastVelocity.normalized, currentVelocity.normalized) < DIRECTION_CHANGE_DOT_THRESHOLD;

        // --- Handle Movement State ---
        if (startedMoving)
        {
            // Start Movement: Reset timers, clear buffer, send start RPC
            sendTimer = 0f;
            dontSaveMovementTimerTimer = DONT_SAVE_MOVEMENT_TIMER_THRESHOLD;
            movementBuffer.Clear();
            RpcStartMovement(player, player.transform.position, currentVelocity);
            // Add initial movement data immediately
            movementBuffer.Add(new MovementData { position = player.transform.position, velocity = currentVelocity });
        }
        else // Continuing movement or changed direction
        {
            if (directionChangedSignificantly)
            {
                // Direction Change: Send current buffer immediately, reset timers
                Logger.Info($"Direction Change: sendTimer = {sendTimer}, SEND_TIMER_THRESHOLD = {SEND_TIMER_THRESHOLD}, {(SEND_TIMER_THRESHOLD - sendTimer) / 3f}");
                sendTimer += (SEND_TIMER_THRESHOLD - sendTimer) / 3f;
                // Add data for the new direction immediately after sending old buffer
                movementBuffer.Add(new MovementData { position = player.transform.position, velocity = currentVelocity });
            }
            else // Continuing straight or minor adjustments
            {
                // Buffer Movement Data (respecting dontSave timer)
                if (dontSaveMovementTimerTimer <= 0f)
                {
                    movementBuffer.Add(new MovementData { position = player.transform.position, velocity = currentVelocity });
                }
                else
                {
                    dontSaveMovementTimerTimer -= Time.fixedDeltaTime;
                }
            }

            // Periodic Send Logic
            sendTimer += Time.fixedDeltaTime;
            if (sendTimer >= SEND_TIMER_THRESHOLD) // Send if timer threshold reached
            {
                SendMovementBuffer(player); // Sends and clears buffer
                sendTimer = 0f; // Reset timer after sending
            }
        }

        lastVelocity = currentVelocity; // Update last velocity only when moving
    }
    private static void SendMovementBuffer(PlayerControl player)
    {
        if (movementBuffer.Count > 0)
        {
            var positions = new Vector3[movementBuffer.Count];
            var velocities = new Vector2[movementBuffer.Count];
            for (int i = 0; i < movementBuffer.Count; i++)
            {
                positions[i] = movementBuffer[i].position;
                velocities[i] = movementBuffer[i].velocity;
            }
            RpcBatchMovement(player, positions, velocities);
            movementBuffer.Clear();
        }
    }

    private static void FixedUpdateRemote(PlayerControl player)
    {
        // Priority 1: Handle smooth stop if a finish position is set
        if (HandleRemoteSmoothingStop(player))
        {
            return; // Stop processing if smoothing
        }

        // Priority 2: Handle movement playback from the queue
        if (HandleRemoteMovementPlayback(player))
        {
            return; // Stop processing if playing back
        }

        // Priority 3: If no stop target and no queue data, gently decelerate
        HandleRemoteIdleDeceleration(player);
    }
    private static bool HandleRemoteSmoothingStop(PlayerControl player)
    {
        if (finishedMovementPosition.TryGetValue(player.PlayerId, out var target) && target.HasValue)
        {
            // Calculate velocity for smoothing only once
            if (!smoothingVelocity.TryGetValue(player.PlayerId, out var vel))
            {
                smoothingTimeLeft[player.PlayerId] = SMOOTHING_TOTAL_TIME;
                var currPos = (Vector2)player.transform.position;
                // Avoid division by zero if totalTime is very small
                vel = (SMOOTHING_TOTAL_TIME > 0.001f) ? (target.Value - currPos) / SMOOTHING_TOTAL_TIME : Vector2.zero;
                smoothingVelocity[player.PlayerId] = vel;
            }

            var remaining = smoothingTimeLeft.TryGetValue(player.PlayerId, out var time) ? time : 0f;

            if (remaining > 0)
            {
                player.MyPhysics.body.velocity = vel;
                smoothingTimeLeft[player.PlayerId] = remaining - Time.fixedDeltaTime;
            }
            else // Smoothing finished
            {
                player.transform.position = new Vector3(target.Value.x, target.Value.y, player.transform.position.z);
                player.MyPhysics.body.velocity = Vector2.zero;
                // Clean up state
                finishedMovementPosition.Remove(player.PlayerId);
                smoothingTimeLeft.Remove(player.PlayerId);
                smoothingVelocity.Remove(player.PlayerId);
            }
            return true; // Is handling smoothing stop
        }
        return false; // Not handling smoothing stop
    }
    private static bool HandleRemoteMovementPlayback(PlayerControl player)
    {
        if (movementQueues.TryGetValue(player.PlayerId, out var queue) && queue.Count > 0)
        {
            var record = queue.Dequeue();
            Vector3 targetPosition3D = new Vector3(record.position.x, record.position.y, player.transform.position.z);
            float distance = Vector2.Distance(player.transform.position, record.position);

            // Interpolate position smoothly
            float speed = record.velocity.magnitude;
            // Avoid division by zero or very small update steps for lerp factor
            float lerpFactorBase = (speed * Time.fixedDeltaTime > 0.01f) ? Mathf.Clamp01(distance / (speed * Time.fixedDeltaTime)) : 1.0f;
            player.transform.position = Vector3.Lerp(player.transform.position, targetPosition3D, lerpFactorBase * POSITION_LERP_FACTOR);

            // Interpolate velocity smoothly
            player.MyPhysics.body.velocity = Vector2.Lerp(player.MyPhysics.body.velocity, record.velocity, VELOCITY_LERP_FACTOR);

            // Position Sync Logic
            int currentSyncLimit = syncPositionLimit.TryGetValue(player.PlayerId, out var limit) ? limit : SYNC_POSITION_LIMIT_INITIAL_VALUE;
            currentSyncLimit--;
            if (currentSyncLimit <= 0)
            {
                currentSyncLimit = SYNC_POSITION_LIMIT_INITIAL_VALUE;
                // Hard sync only if drift is significant
                if (distance > HARD_SYNC_DISTANCE_THRESHOLD)
                {
                    player.transform.position = targetPosition3D;
                    player.MyPhysics.body.velocity = record.velocity; // Reset velocity on hard sync
                }
            }
            syncPositionLimit[player.PlayerId] = currentSyncLimit; // Update the counter

            return true; // Is handling playback
        }
        return false; // Not handling playback (queue empty or doesn't exist)
    }

    private static void HandleRemoteIdleDeceleration(PlayerControl player)
    {
        // Gently decelerate if no other actions are being handled
        player.MyPhysics.body.velocity = Vector2.Lerp(player.MyPhysics.body.velocity, Vector2.zero, IDLE_DECELERATION_LERP_FACTOR);
    }

    // --- RPC Handlers ---
    // 後でCustomRPC側でいい感じにする
    [CustomRPC]
    public static void RpcStartMovement(PlayerControl player, Vector3 startPosition, Vector2 velocity)
    {
        if (player.AmOwner) return;

        // Clear any potential stopping state
        finishedMovementPosition.Remove(player.PlayerId);
        smoothingTimeLeft.Remove(player.PlayerId);
        smoothingVelocity.Remove(player.PlayerId);

        // Warp if too far away
        if (Vector2.Distance(player.transform.position, startPosition) > START_WARP_DISTANCE_THRESHOLD)
        {
            player.transform.position = startPosition;
        }

        player.MyPhysics.body.velocity = velocity;

        // Ensure queue exists and clear old data
        var queue = GetOrCreateQueue(player.PlayerId);
        queue.Clear();

        // Reset sync counter
        syncPositionLimit[player.PlayerId] = SYNC_POSITION_LIMIT_INITIAL_VALUE;
    }
    [CustomRPC]
    public static void RpcStopMovement(PlayerControl player, Vector3 position)
    {
        if (player.AmOwner) return;

        // Set the target position for smoothing stop
        finishedMovementPosition[player.PlayerId] = position;
        // Initialize smoothing time, velocity will be calculated on first FixedUpdateRemote call
        smoothingTimeLeft[player.PlayerId] = STOP_SMOOTHING_TIME;
        smoothingVelocity.Remove(player.PlayerId); // Ensure previous smoothing velocity is cleared

        // Clear the queue on stop command
        if (movementQueues.TryGetValue(player.PlayerId, out var queue))
        {
            queue.Clear();
        }
    }
    [CustomRPC]
    public static void RpcBatchMovement(PlayerControl player, Vector3[] positions, Vector2[] velocities)
    {
        if (player.AmOwner) return;

        var queue = GetOrCreateQueue(player.PlayerId);

        for (int i = 0; i < positions.Length; i++)
        {
            queue.Enqueue(new MovementData { position = positions[i], velocity = velocities[i] });
        }
    }
    [HarmonyPatch(typeof(CustomNetworkTransform), nameof(CustomNetworkTransform.FixedUpdate))]
    public static class CustomNetworkTransformPatch
    {
        private static bool IsVanillaServerCache;
        private static int lastShipStatusInstanceId;
        private static bool IsVanillaServer()
            => lastShipStatusInstanceId == ShipStatus.Instance.GetInstanceID() ? IsVanillaServerCache : (IsVanillaServerCache = AmongUsClient.Instance.NetworkMode != NetworkModes.LocalGame && !ModHelpers.IsCustomServer());
        public static bool Prefix(CustomNetworkTransform __instance)
        {
            // 開始時のみ
            if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started) return true;
            if (IsVanillaServer()) return true;
            if (GeneralSettingOptions.NetworkTransformType == NetworkTransformType.ModdedLowLatency)
            {
                // PlayerControl が null でないことを確認
                if (__instance.myPlayer != null)
                {
                    FixedUpdate(__instance.myPlayer);
                }
                return false; // 元のFixedUpdateをスキップ
            }
            return true; // それ以外の場合は元のFixedUpdateを実行
        }
    }
    [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Start))]
    public static class GameStartManagerPatch
    {
        public static void Postfix()
        {
            Initialize();
        }
    }
}

public enum NetworkTransformType
{
    Vanilla,
    ModdedLowLatency,
}
public enum NetworkTransformTypeLowLatencyLevel
{
    Bad,
    SuperLow,
    Low,
    Medium,
    High,
    Max,
}

// Deleted code: Original FixedUpdate logic which is now refactored into smaller methods.
// Deleted code: Redundant checks for queue existence inside loops, now handled by GetOrCreateQueue or TryGetValue.
// Deleted code: LINQ ToArray calls inside FixedUpdateOwner, replaced with direct array creation in SendMovementBuffer.

