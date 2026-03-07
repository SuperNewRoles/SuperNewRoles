using System;
using System.Collections.Generic;
using UnityEngine;

namespace SuperNewRoles.Modules;

public sealed class AndroidRightStickAimCoreState
{
    private readonly HashSet<ulong> _requesters = new();
    private float _lastInputX = 1f;
    private float _lastInputY;
    private bool _hasLastInputDirection;

    public bool HasActiveRequesters => _requesters.Count > 0;

    public void SetRequesterVisible(ulong requesterId, bool visible)
    {
        if (requesterId == 0)
            return;

        if (visible)
            _requesters.Add(requesterId);
        else
            _requesters.Remove(requesterId);
    }

    public void ClearRequesters()
    {
        _requesters.Clear();
    }

    public void ResetAim()
    {
        _lastInputX = 1f;
        _lastInputY = 0f;
        _hasLastInputDirection = false;
    }

    public void ResetAll()
    {
        ClearRequesters();
        ResetAim();
    }

    public (float x, float y) ResolveDirection(float rawX, float rawY, float fallbackX, float fallbackY)
    {
        (float x, float y) normalizedInput = NormalizeOrZero(rawX, rawY);
        if (IsNonZero(normalizedInput.x, normalizedInput.y))
        {
            _lastInputX = normalizedInput.x;
            _lastInputY = normalizedInput.y;
            _hasLastInputDirection = true;
            return normalizedInput;
        }

        if (_hasLastInputDirection)
            return (_lastInputX, _lastInputY);

        (float x, float y) normalizedFallback = NormalizeOrZero(fallbackX, fallbackY);
        return IsNonZero(normalizedFallback.x, normalizedFallback.y) ? normalizedFallback : (1f, 0f);
    }

    private static (float x, float y) NormalizeOrZero(float x, float y)
    {
        float magnitudeSquared = x * x + y * y;
        if (magnitudeSquared <= 0.0001f)
            return (0f, 0f);

        float magnitude = MathF.Sqrt(magnitudeSquared);
        return (x / magnitude, y / magnitude);
    }

    private static bool IsNonZero(float x, float y)
    {
        return x * x + y * y > 0f;
    }
}

public sealed class AndroidRightStickAimState
{
    private readonly AndroidRightStickAimCoreState _core = new();

    public bool HasActiveRequesters => _core.HasActiveRequesters;

    public void SetRequesterVisible(ulong requesterId, bool visible)
    {
        _core.SetRequesterVisible(requesterId, visible);
    }

    public void ClearRequesters()
    {
        _core.ClearRequesters();
    }

    public void ResetAim()
    {
        _core.ResetAim();
    }

    public void ResetAll()
    {
        _core.ResetAll();
    }

    public Vector2 ResolveDirection(Vector2 rawDirection, Vector2 fallbackDirection)
    {
        (float x, float y) resolved = _core.ResolveDirection(rawDirection.x, rawDirection.y, fallbackDirection.x, fallbackDirection.y);
        return new Vector2(resolved.x, resolved.y);
    }
}

public static class AndroidAimVisibilityPolicy
{
    public static bool ShouldShowForTriggerHappy(
        bool isAndroid,
        bool isOwner,
        bool isAlive,
        bool canMove,
        bool inVent,
        bool meetingOpen,
        bool introDisplayed,
        bool gatlingGunActive)
    {
        return isAndroid
            && isOwner
            && isAlive
            && canMove
            && !inVent
            && !meetingOpen
            && !introDisplayed
            && gatlingGunActive;
    }

    public static bool ShouldShowForKunoichi(
        bool isAndroid,
        bool isOwner,
        bool isAlive,
        bool canMove,
        bool inVent,
        bool meetingOpen,
        bool introDisplayed,
        bool invisibleBlocked)
    {
        return isAndroid
            && isOwner
            && isAlive
            && canMove
            && !inVent
            && !meetingOpen
            && !introDisplayed
            && !invisibleBlocked;
    }
}

public static class AndroidRightStickAim
{
    private static readonly AndroidRightStickAimState State = new();

    public static bool IsActive => ModHelpers.IsAndroid() && State.HasActiveRequesters;

    public static void SetVisible(ulong requesterId, bool visible)
    {
        if (!ModHelpers.IsAndroid())
            return;

        State.SetRequesterVisible(requesterId, visible);
    }

    public static void ResetAll()
    {
        State.ResetAll();
    }

    public static Vector2 GetAimDirection(Vector2 fallbackDirection)
    {
        if (!ModHelpers.IsAndroid())
            return GetMouseDirection(fallbackDirection);

        Vector2 rawDirection = GetSecondTouchDirection();
        return State.ResolveDirection(rawDirection, fallbackDirection);
    }

    public static bool ShouldSuppressAdjustLighting()
    {
        return ModHelpers.IsAndroid() && State.HasActiveRequesters;
    }

    private static Vector2 GetMouseDirection(Vector2 fallbackDirection)
    {
        Vector3 mouseDirection = Input.mousePosition - new Vector3(Screen.width / 2f, Screen.height / 2f);
        Vector2 aimDirection = new(mouseDirection.x, mouseDirection.y);
        return NormalizeOrFallback(aimDirection, fallbackDirection);
    }

    private static Vector2 GetSecondTouchDirection()
    {
        if (Input.touchCount < 2)
            return new Vector2(0f, 0f);

        Touch touch = Input.GetTouch(1);
        if (touch.phase is TouchPhase.Canceled or TouchPhase.Ended)
            return new Vector2(0f, 0f);

        Vector2 screenCenter = new(Screen.width * 0.5f, Screen.height * 0.5f);
        Vector2 touchDirection = touch.position - screenCenter;
        if (touchDirection.sqrMagnitude <= 0.0001f)
        {
            return new Vector2(0f, 0f);
        }

        return touchDirection.normalized;
    }

    private static Vector2 NormalizeOrFallback(Vector2 vector, Vector2 fallbackDirection)
    {
        if (vector.sqrMagnitude > 0.0001f)
            return vector.normalized;

        if (fallbackDirection.sqrMagnitude > 0.0001f)
            return fallbackDirection.normalized;

        return new Vector2(1f, 0f);
    }
}
