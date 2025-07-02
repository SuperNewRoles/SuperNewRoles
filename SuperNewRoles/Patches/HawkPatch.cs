using System;
using HarmonyLib;
using SuperNewRoles.CustomOptions.Categories;
using SuperNewRoles.Events;
using SuperNewRoles.Modules;
using UnityEngine;

namespace SuperNewRoles.Patches;

// https://github.com/KYMario/TownOfHost-K/blob/321f445294fd91ab1027a9e5dfe55fa9355764cb/Modules/Zoom.cs#L29
[HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
public static class HawkZoom
{
    public static float size = HudManager.Instance.UICamera.orthographicSize;
    private static float last = 0;
    private static float zoomSpeed = 0f; // SmoothDampによって管理されるようになります
    private static float mouseScrollDeltaSensitivity = 0.5f; // マウスホイールの感度調整用
    private const float smoothTime = 0.1f; // SmoothDampのスムーズ時間（値を小さくするとシャープに、大きくするとより滑らかに）

    public static float manualTargetSize = 3f; // 手動ズームの目標サイズ
    private static bool manualTargetInitialized = false;

    public static void Postfix()
    {
        if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started)
        {
            return;
        }

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

        HawkEventData data = HawkEvent.Invoke(false, (int)manualTargetSize, false);

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
        else if (GameSettingOptions.EnabledZoomOnDead && !data.RefCancelZoom && ExPlayerControl.LocalPlayer != null && ExPlayerControl.LocalPlayer.IsDead() && MeetingHud.Instance == null)
        {
            if (!PlayerControl.LocalPlayer.CanMove)
            {
                // CanMoveでない場合は手動ズームターゲットを現在のサイズに維持（動かさない）
                manualTargetSize = 3f;
            }
            else if (ModHelpers.IsAndroid())
            {
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
                float scrollDelta = Input.mouseScrollDelta.y;
                if (scrollDelta != 0)
                {
                    manualTargetSize -= scrollDelta * mouseScrollDeltaSensitivity;
                    manualTargetSize = Mathf.Clamp(manualTargetSize, 2.46f, 15f);
                }

                if (Input.GetKeyDown(KeyCode.LeftShift))
                {
                    // Shiftキーでアビリティが提示する基準ズーム値（data.RefZoomSize）に戻す
                    // もしアビリティがズーム値を提供しない場合は、一般的なデフォルト値(3fなど)に戻すことを検討
                    manualTargetSize = Mathf.Clamp(data.RefZoomSize, 2.46f, 15f);
                    // zoomSpeed = 0f; // SmoothDampが次のフレームで調整するので、必須ではない
                }
            }
            // 手動ズームの場合もSmoothDampを適用
            size = Mathf.SmoothDamp(size, manualTargetSize, ref zoomSpeed, smoothTime);
        }
        else // その他の場合（生存プレイヤーでアビリティ干渉なし、など）はデフォルトの挙動
        {
            if (MeetingHud.Instance != null)
                manualTargetSize = 3f;
            size = Mathf.SmoothDamp(size, manualTargetSize, ref zoomSpeed, smoothTime);
        }

        if (last != size)
        {
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
        HawkZoom.size = HawkZoom.manualTargetSize = 3f;
    }
}