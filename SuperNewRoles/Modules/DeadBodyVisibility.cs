using System;
using UnityEngine;

namespace SuperNewRoles.Modules;

public static class DeadBodyVisibility
{
    private const float HiddenPositionX = 9999f;
    private const float HiddenPositionY = 9999f;
    private const float HiddenPositionTolerance = 0.01f;

    public static Vector3 HiddenPosition => new(HiddenPositionX, HiddenPositionY, 0f);

    public static bool IsHiddenPosition(Vector3 position)
    {
        return IsHiddenPosition(position.x, position.y);
    }

    internal static bool IsHiddenPosition(float x, float y)
    {
        return Math.Abs(x - HiddenPositionX) <= HiddenPositionTolerance &&
            Math.Abs(y - HiddenPositionY) <= HiddenPositionTolerance;
    }
}
