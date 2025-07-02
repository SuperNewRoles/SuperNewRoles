using System.Collections.Generic;
using HarmonyLib;
using SuperNewRoles.CustomOptions.Categories;
using SuperNewRoles.Modules;
using UnityEngine;
using System.Linq;
using Hazel;

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
    private static Dictionary<byte, Vector2> externalImpulses;

    // 次のBatchMovementをスキップするプレイヤーIDセット
    public static HashSet<byte> skipNextBatchPlayers = new HashSet<byte>();

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
        externalImpulses = new Dictionary<byte, Vector2>();
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

    public static void ResetState(PlayerControl player)
    {
        finishedMovementPosition.Remove(player.PlayerId);
        smoothingTimeLeft.Remove(player.PlayerId);
        smoothingVelocity.Remove(player.PlayerId);
        stopDetectionCounter.Remove(player.PlayerId);
        externalImpulses.Remove(player.PlayerId);
        if (movementQueues.TryGetValue(player.PlayerId, out var queue))
        {
            queue.Clear();
        }
    }
    public static void FixedUpdate(PlayerControl player)
    {
        if (player.NetTransform.isPaused || player.onLadder || (ShipStatus.Instance != null && ShipStatus.Instance.Type == ShipStatus.MapType.Fungle && ShipStatus.Instance is FungleShipStatus fungleShipStatus && fungleShipStatus.Zipline.playerIdHands.ContainsKey(player.PlayerId)))
        {
            // Clear state for paused players
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

                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(player.NetId, CustomRPCManager.SNRNetworkTransformRpc, SendOption.Reliable);
                writer.Write((byte)MovementRpcType.StopMovement);
                writer.Write(player.PlayerId);
                writer.Write(player.transform.position.x);
                writer.Write(player.transform.position.y);
                writer.Write(player.transform.position.z);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
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
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(player.NetId, CustomRPCManager.SNRNetworkTransformRpc, SendOption.Reliable);
            writer.Write((byte)MovementRpcType.StartMovement);
            writer.Write(player.PlayerId);
            writer.Write(player.transform.position.x);
            writer.Write(player.transform.position.y);
            writer.Write(player.transform.position.z);
            writer.Write(currentVelocity.x);
            writer.Write(currentVelocity.y);
            // Add initial movement data immediately
            movementBuffer.Add(new MovementData { position = player.transform.position, velocity = currentVelocity });
        }
        else // Continuing movement or changed direction
        {
            if (directionChangedSignificantly)
            {
                // Direction Change: Send current buffer immediately, reset timers
                Logger.Info($"Direction Change: sendTimer = {sendTimer}, SEND_TIMER_THRESHOLD = {SEND_TIMER_THRESHOLD}, {(SEND_TIMER_THRESHOLD - sendTimer) / 3f}");
                sendTimer += (SEND_TIMER_THRESHOLD - sendTimer) / 2f;
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
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(player.NetId, CustomRPCManager.SNRNetworkTransformRpc, SendOption.Reliable);
            writer.Write((byte)MovementRpcType.BatchMovement);
            writer.Write(player.PlayerId);
            writer.Write(movementBuffer.Count); // List の Count を送信
            // List を直接ループして書き込む
            foreach (var data in movementBuffer)
            {
                writer.Write(data.position.x);
                writer.Write(data.position.y);
                writer.Write(data.position.z);
                writer.Write(data.velocity.x);
                writer.Write(data.velocity.y);
            }
            AmongUsClient.Instance.FinishRpcImmediately(writer);

            movementBuffer.Clear();
        }
    }

    public static void ReceivedNetworkTransform(MessageReader reader)
    {
        byte rpcType = reader.ReadByte();
        byte playerId;
        switch (rpcType)
        {
            case (byte)MovementRpcType.BatchMovement:
                playerId = reader.ReadByte();
                int count = Mathf.Min(reader.ReadInt32(), 20); // Read count, capped at 20
                if (skipNextBatchPlayers.Remove(playerId))
                {
                    reader.Position += count * 4 * 5;
                    break;
                }

                var queue = GetOrCreateQueue(playerId);
                for (int i = 0; i < count; i++)
                {
                    float x = reader.ReadSingle();
                    float y = reader.ReadSingle();
                    float z = reader.ReadSingle();
                    float vx = reader.ReadSingle();
                    float vy = reader.ReadSingle();
                    queue.Enqueue(new MovementData { position = new Vector3(x, y, z), velocity = new Vector2(vx, vy) });
                }
                break;
            case (byte)MovementRpcType.StartMovement:
                playerId = reader.ReadByte();
                Vector3 position = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                Vector2 velocity = new Vector2(reader.ReadSingle(), reader.ReadSingle());
                RpcStartMovement(playerId, position, velocity);
                break;
            case (byte)MovementRpcType.StopMovement:
                playerId = reader.ReadByte();
                position = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                RpcStopMovement(playerId, position);
                break;
            case (byte)MovementRpcType.ApplyExternalImpulse:
                playerId = reader.ReadByte();
                Vector2 impulse = new Vector2(reader.ReadSingle(), reader.ReadSingle());
                externalImpulses[playerId] = externalImpulses.GetValueOrDefault(playerId, Vector2.zero) + impulse;
                // 次のバッチ移動をスキップ
                skipNextBatchPlayers.Add(playerId);
                break;
        }
    }

    private static void FixedUpdateRemote(PlayerControl player)
    {
        // Apply external impulses first if any
        if (externalImpulses.Remove(player.PlayerId, out var impulse) && impulse.sqrMagnitude > 0.0001f) // C# 7.0+
        {
            player.MyPhysics.body.velocity += impulse;
            // インパルスの方向に少しだけ位置を補正
            float adjustmentMagnitude = Mathf.Min(impulse.magnitude * Time.fixedDeltaTime * 2.0f, 0.1f);
            Vector3 positionAdjustment = (Vector3)impulse.normalized * adjustmentMagnitude;
            player.transform.position += positionAdjustment;

            // ネットワーク同期のバッファをクリアして、即座に位置を固定
            if (movementQueues.TryGetValue(player.PlayerId, out var queue)) queue.Clear();
            finishedMovementPosition.Remove(player.PlayerId);
            smoothingTimeLeft.Remove(player.PlayerId);
            smoothingVelocity.Remove(player.PlayerId);
            // externalImpulses.Remove(player.PlayerId); // Already removed by .Remove(key, out value)
        }

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
        if (!finishedMovementPosition.TryGetValue(player.PlayerId, out var target) || !target.HasValue)
        {
            return false; // Not handling smoothing stop
        }

        // Target position exists, proceed with smoothing logic
        if (!smoothingTimeLeft.TryGetValue(player.PlayerId, out float remainingTime))
        {
            // This case should ideally not happen if RpcStopMovement initializes smoothingTimeLeft correctly.
            // However, as a fallback, or if initialization logic changes, we can set it here.
            remainingTime = STOP_SMOOTHING_TIME; // Or SMOOTHING_TOTAL_TIME depending on desired behavior on first entry
            smoothingTimeLeft[player.PlayerId] = remainingTime;
        }

        if (!smoothingVelocity.TryGetValue(player.PlayerId, out Vector2 vel)) // Calculate velocity only once or if not present
        {
            var currPos = (Vector2)player.transform.position;
            // Avoid division by zero if totalTime is very small. Use remainingTime if it's the first setup.
            float timeForCalc = (remainingTime > 0.001f) ? remainingTime : STOP_SMOOTHING_TIME; // Fallback to STOP_SMOOTHING_TIME
            if (timeForCalc <= 0.001f && SMOOTHING_TOTAL_TIME > 0.001f) timeForCalc = SMOOTHING_TOTAL_TIME; // Prefer total time if available for initial calc

            vel = (timeForCalc > 0.001f) ? (target.Value - currPos) / timeForCalc : Vector2.zero;
            smoothingVelocity[player.PlayerId] = vel;
            // If we just calculated velocity, and smoothingTimeLeft was not set by RpcStopMovement (e.g. from an older state),
            // ensure it's set to a full duration for this new smoothing action.
            // This aligns with the original logic of setting SMOOTHING_TOTAL_TIME when vel was first calculated.
            if (!smoothingTimeLeft.ContainsKey(player.PlayerId) || smoothingTimeLeft[player.PlayerId] <= 0)
            { // Check if it was not properly set or expired
                smoothingTimeLeft[player.PlayerId] = SMOOTHING_TOTAL_TIME;
                remainingTime = SMOOTHING_TOTAL_TIME;
            }
        }


        if (remainingTime > 0)
        {
            player.MyPhysics.body.velocity = vel;
            smoothingTimeLeft[player.PlayerId] = remainingTime - Time.fixedDeltaTime;
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
    public static void RpcStartMovement(byte playerId, Vector3 startPosition, Vector2 velocity)
    {
        PlayerControl player = ExPlayerControl.ById(playerId);
        // Clear any potential stopping state
        finishedMovementPosition.Remove(playerId);
        smoothingTimeLeft.Remove(playerId);
        smoothingVelocity.Remove(playerId);

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
        syncPositionLimit[playerId] = SYNC_POSITION_LIMIT_INITIAL_VALUE;
    }
    public static void RpcStopMovement(byte playerId, Vector3 position)
    {
        // Set the target position for smoothing stop
        finishedMovementPosition[playerId] = position;
        // Initialize smoothing time, velocity will be calculated on first FixedUpdateRemote call
        smoothingTimeLeft[playerId] = STOP_SMOOTHING_TIME;
        smoothingVelocity.Remove(playerId); // Ensure previous smoothing velocity is cleared

        // Clear the queue on stop command
        if (movementQueues.TryGetValue(playerId, out var queue))
        {
            queue.Clear();
        }
    }
    public static void RpcBatchMovement(byte playerId, Vector3[] positions, Vector2[] velocities)
    {
        var queue = GetOrCreateQueue(playerId);

        for (int i = 0; i < positions.Length; i++)
        {
            queue.Enqueue(new MovementData { position = positions[i], velocity = velocities[i] });
        }
    }
    [HarmonyPatch(typeof(CustomNetworkTransform), nameof(CustomNetworkTransform.OnEnable))]
    public static class CustomNetworkTransformOnEnablePatch
    {
        public static void Postfix(CustomNetworkTransform __instance)
        {
            if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started) return;
            if (CustomNetworkTransformPatch.IsVanillaServer()) return;
            if (GeneralSettingOptions.NetworkTransformType == NetworkTransformType.ModdedLowLatency)
                ResetState(__instance.myPlayer);

        }
    }
    [HarmonyPatch(typeof(CustomNetworkTransform), nameof(CustomNetworkTransform.FixedUpdate))]
    public static class CustomNetworkTransformPatch
    {
        private static bool IsVanillaServerCache;
        private static int lastShipStatusInstanceId;
        public static bool IsVanillaServer()
            => ShipStatus.Instance != null && lastShipStatusInstanceId == ShipStatus.Instance.GetInstanceID() ? IsVanillaServerCache : (IsVanillaServerCache = AmongUsClient.Instance.NetworkMode != NetworkModes.LocalGame && !ModHelpers.IsCustomServer());
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
public enum MovementRpcType
{
    StartMovement,
    StopMovement,
    BatchMovement,
    ApplyExternalImpulse
}
