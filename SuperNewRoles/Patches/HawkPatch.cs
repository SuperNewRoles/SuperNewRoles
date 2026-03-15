using System;
using AmongUs.GameOptions;
using HarmonyLib;
using SuperNewRoles.CustomOptions.Categories;
using SuperNewRoles.Events;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles;
using SuperNewRoles.Roles.Ability;
using UnityEngine;

namespace SuperNewRoles.Patches;

// https://github.com/KYMario/TownOfHost-K/blob/321f445294fd91ab1027a9e5dfe55fa9355764cb/Modules/Zoom.cs#L29
[HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
public static class HawkZoom
{
    // 手動ズームが無効な状態へ戻すときの基準倍率。
    internal const float DefaultManualTargetSize = 3f;
    public static float size = HudManager.Instance.UICamera.orthographicSize;
    private static float last = 0;
    private static float zoomSpeed = 0f; // SmoothDampによって管理されるようになります
    private static float mouseScrollDeltaSensitivity = 0.5f; // マウスホイールの感度調整用
    private const float smoothTime = 0.1f; // SmoothDampのスムーズ時間（値を小さくするとシャープに、大きくするとより滑らかに）

    public static float manualTargetSize = DefaultManualTargetSize; // 手動ズームの目標サイズ
    private static bool manualTargetInitialized = false;

    public static void Postfix()
    {
        // 試合中だけズーム更新を行う。
        if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started)
        {
            return;
        }

        // HUD カメラが初めて取れた時点の倍率を手動ズームの初期値に合わせる。
        if (!manualTargetInitialized && HudManager.Instance?.UICamera != null)
        {
            manualTargetSize = HudManager.Instance.UICamera.orthographicSize;
            size = manualTargetSize; // 初期サイズを合わせる
            manualTargetInitialized = true;
        }
        else if (HudManager.Instance?.UICamera == null && manualTargetInitialized)
        {
            // カメラが利用できなくなった場合、初期化フラグをリセット
            manualTargetInitialized = false;
        }

        // Hawk 系など「能力側が提示するズーム要求」を先に集約する。
        HawkEventData data = HawkEvent.Invoke(false, (int)manualTargetSize, false);
        ExPlayerControl localPlayer = ExPlayerControl.LocalPlayer;
        bool enabledZoomOnDead = GameSettingOptions.EnabledZoomOnDead;
        bool isDead = localPlayer != null && localPlayer.IsDead();
        bool isMeetingOpen = MeetingHud.Instance != null;
        bool requireCompletedTasks = GameSettingOptions.EnabledZoomOnDeadRequireCompletedTasks;
        bool disableForGhostOrGuardianAngel = GameSettingOptions.EnabledZoomOnDeadDisableForGhostOrGuardianAngel;
        bool isBuskerFakeDeath = ExPlayerControl.LocalPlayer?.GetAbility<BuskerPseudocideAbility>()?.isEffectActive == true;

        // バスカーの擬似死中は死亡後ズームの対象外として扱う。
        bool isEligibleDeadState = isDead && !isBuskerFakeDeath;
        // 毎フレーム呼ばれるので、親条件を満たさない間は重めの補助判定を先に計算しない。
        bool shouldEvaluateChildRestrictions = enabledZoomOnDead && isEligibleDeadState && !isMeetingOpen;

        bool isTaskTriggerRole = false;
        int completedTasks = 0;
        int totalTasks = 0;
        if (shouldEvaluateChildRestrictions && requireCompletedTasks)
        {
            // IsTaskTriggerRole は内部で能力一覧を舐めるので、子設定が有効な時だけ評価する。
            isTaskTriggerRole = localPlayer.IsTaskTriggerRole();
            if (isTaskTriggerRole)
            {
                // 実際にタスク条件を課す役職にだけ進捗取得を行う。
                (completedTasks, totalTasks) = localPlayer.GetAllTaskForShowProgress();
            }
        }

        GhostRoleId ghostRole = GhostRoleId.None;
        bool isGuardianAngel = false;
        if (shouldEvaluateChildRestrictions && disableForGhostOrGuardianAngel)
        {
            ghostRole = localPlayer.GhostRole;
            isGuardianAngel = localPlayer.Data != null
                && localPlayer.Data.Role != null
                && localPlayer.Data.Role.Role == RoleTypes.GuardianAngel;
        }

        // 死亡後手動ズーム可否を pure helper に寄せ、テストからも同じ判定を叩けるようにする。
        DeadZoomState deadZoomState = DeadZoomHelper.EvaluateManualDeadZoom(
            enabledZoomOnDead,
            isEligibleDeadState,
            isMeetingOpen,
            requireCompletedTasks,
            isTaskTriggerRole,
            completedTasks,
            totalTasks,
            disableForGhostOrGuardianAngel,
            ghostRole,
            isGuardianAngel
        );

        // 能力起因のズームは手動ズームより常に優先する。
        if (data.RefAcceleration) // アビリティがSmoothDampを要求する場合
        {
            float abilityTargetSize = Mathf.Clamp(data.RefZoomSize, 2.46f, 15f);
            size = Mathf.SmoothDamp(size, abilityTargetSize, ref zoomSpeed, smoothTime);
        }
        else if (data.RefCancelZoom) // アビリティが固定ズームを要求する場合
        {
            size = Mathf.Clamp(data.RefZoomSize, 2.46f, 15f);
            zoomSpeed = 0f;
        }
        // ユーザーが編集した手動ズーム条件
        else if (deadZoomState.CanUseManualZoom)
        {
            if (!PlayerControl.LocalPlayer.CanMove)
            {
                // 操作不能中は拡大縮小の入力を受け付けず、既定倍率へ戻す。
                manualTargetSize = DefaultManualTargetSize;
            }
            else if (ModHelpers.IsAndroid())
            {
                // Android はピンチ操作で目標倍率を更新する。
                if (Input.touchCount == 2)
                {
                    Touch touch0 = Input.GetTouch(0);
                    Touch touch1 = Input.GetTouch(1);

                    Vector2 prevTouch0 = touch0.position - touch0.deltaPosition;
                    Vector2 prevTouch1 = touch1.position - touch1.deltaPosition;

                    float prevDistance = Vector2.Distance(prevTouch0, prevTouch1);
                    float currentDistance = Vector2.Distance(touch0.position, touch1.position);

                    float pinchDelta = prevDistance - currentDistance;

                    manualTargetSize += pinchDelta * 0.01f; // 感度調整
                    manualTargetSize = Mathf.Clamp(manualTargetSize, 2.46f, 15f);
                }
            }
            else // PCの手動ズーム
            {
                // PC はホイールで目標倍率を上下させる。
                float scrollDelta = Input.mouseScrollDelta.y;
                if (scrollDelta != 0)
                {
                    manualTargetSize -= scrollDelta * mouseScrollDeltaSensitivity;
                    manualTargetSize = Mathf.Clamp(manualTargetSize, 2.46f, 15f);
                }

                if (Input.GetKeyDown(KeyCode.LeftShift))
                {
                    // Shift で能力側が提案した基準倍率へ戻す。
                    manualTargetSize = Mathf.Clamp(data.RefZoomSize, 2.46f, 15f);
                }
            }
            // 手動ズームも急に飛ばさず、通常時と同じく補間で追従させる。
            size = Mathf.SmoothDamp(size, manualTargetSize, ref zoomSpeed, smoothTime);
        }
        else // その他の場合（生存プレイヤーでアビリティ干渉なし、など）はデフォルトの挙動
        {
            // 会議開始時か子設定でブロックされた時は、以前の手動倍率を持ち越さない。
            if (MeetingHud.Instance != null || deadZoomState.ShouldResetToDefault)
                manualTargetSize = DefaultManualTargetSize;
            size = Mathf.SmoothDamp(size, manualTargetSize, ref zoomSpeed, smoothTime);
        }

        if (last != size)
        {
            // 実カメラと UI カメラを同じ倍率で更新する。
            if (HudManager.Instance?.UICamera != null)
            {
                HudManager.Instance.UICamera.orthographicSize = size;
            }
            if (Camera.main != null)
            {
                Camera.main.orthographicSize = size;
            }
            ResolutionManager.ResolutionChanged?.Invoke((float)Screen.width / Screen.height, Screen.width, Screen.height, Screen.fullScreen);
            last = size;
        }
    }
}

internal readonly struct DeadZoomState
{
    // このフレームで死亡後手動ズーム分岐へ入ってよいか。
    public bool CanUseManualZoom { get; }
    // 直前まで手動ズームしていた倍率を捨てて既定値へ戻すべきか。
    public bool ShouldResetToDefault { get; }

    public DeadZoomState(bool canUseManualZoom, bool shouldResetToDefault)
    {
        CanUseManualZoom = canUseManualZoom;
        ShouldResetToDefault = shouldResetToDefault;
    }
}

internal static class DeadZoomHelper
{
    // 死亡後手動ズームの「親設定 / 子設定 / プレイヤー状態」を 1 箇所で評価する。
    public static DeadZoomState EvaluateManualDeadZoom(
        bool enabledZoomOnDead,
        bool isDead,
        bool isMeetingOpen,
        bool requireCompletedTasks,
        bool isTaskTriggerRole,
        int completedTasks,
        int totalTasks,
        bool disableForGhostOrGuardianAngel,
        GhostRoleId ghostRole,
        bool isGuardianAngel)
    {
        if (!enabledZoomOnDead || !isDead || isMeetingOpen)
        {
            return new DeadZoomState(false, false);
        }

        // タスク条件はタスクトリガー役職にだけ適用する。
        bool blockedByTasks = requireCompletedTasks && isTaskTriggerRole && !AreRequiredTasksCompleted(completedTasks, totalTasks);
        // 幽霊役職条件は Mod 幽霊役職と GuardianAngel のどちらも対象。
        bool blockedByGhostRole = disableForGhostOrGuardianAngel && (ghostRole != GhostRoleId.None || isGuardianAngel);
        bool isBlockedByAdditionalRestrictions = blockedByTasks || blockedByGhostRole;

        return new DeadZoomState(!isBlockedByAdditionalRestrictions, isBlockedByAdditionalRestrictions);
    }

    // タスクを持たない役職や必要数 0 のケースは完了扱いにする。
    public static bool AreRequiredTasksCompleted(int completedTasks, int totalTasks)
        => totalTasks <= 0 || completedTasks >= totalTasks;
}

[HarmonyPatch(typeof(AspectPosition), nameof(AspectPosition.AdjustPosition), new Type[] { typeof(float) })]
public static class AspectPositionAdjustPositionPatch
{
    public static bool Prefix(AspectPosition __instance, float aspect)
    {
        if (__instance == null || __instance.gameObject == null)
        {
            // Logger.Error("AspectPosition is null"); // Loggerがない場合コメントアウト
            return false;
        }
        return true;
    }
}

[HarmonyPatch(typeof(HudManager), nameof(HudManager.Start))]
public static class HudManagerStartPatch
{
    public static void Postfix(HudManager __instance)
    {
        // 試合開始時に前試合のズーム倍率が残らないよう初期化する。
        HawkZoom.size = HawkZoom.manualTargetSize = HawkZoom.DefaultManualTargetSize;
        HawkZoom.manualTargetInitialized = false;
        HawkZoom.last = 0f;
        HawkZoom.zoomSpeed = 0f;
    }
}