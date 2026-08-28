using System;
using System.Collections.Generic;
using Hazel;
using SuperNewRoles.Modules;
using UnityEngine;

namespace SuperNewRoles.Roles.Ability;

public sealed class TriggerHappyBulletBatchData : ICustomRpcObject
{
    private const float PositionScale = 64f;
    private const float PositionInvScale = 1f / PositionScale;
    private const float TwoPi = Mathf.PI * 2f;
    private const float DirectionToByte = 255f / TwoPi;
    private const float ByteToDirection = TwoPi / 255f;
    private const float AngleToUShort = 65535f / TwoPi;
    private const float UShortToAngle = TwoPi / 65535f;
    private const float TimeToByte = 255f / TriggerHappyAbility.BatchInterval;
    private const float ByteToTime = TriggerHappyAbility.BatchInterval / 255f;

    public ushort AnglePacked { get; private set; }
    public short[] Xs { get; private set; } = Array.Empty<short>();
    public short[] Ys { get; private set; } = Array.Empty<short>();
    public byte[] DirectionAngles { get; private set; } = Array.Empty<byte>();
    public byte[] TimeOffsets { get; private set; } = Array.Empty<byte>();

    public int Count => Xs.Length;
    public float AngleRadians => UnpackAngle(AnglePacked);

    public TriggerHappyBulletBatchData()
    {
    }

    public TriggerHappyBulletBatchData(
        IReadOnlyList<Vector3> positionsWithTime,
        IReadOnlyList<Vector2> directions,
        float angleRad)
    {
        if (positionsWithTime == null || directions == null)
            return;

        int count = Math.Min(positionsWithTime.Count, directions.Count);
        AnglePacked = PackAngle(angleRad);
        Xs = new short[count];
        Ys = new short[count];
        DirectionAngles = new byte[count];
        TimeOffsets = new byte[count];

        for (int i = 0; i < count; i++)
        {
            Vector3 entry = positionsWithTime[i];
            Vector2 direction = directions[i];
            Xs[i] = PackPosition(entry.x);
            Ys[i] = PackPosition(entry.y);
            DirectionAngles[i] = PackDirection(direction);
            TimeOffsets[i] = PackTime(entry.z);
        }
    }

    public Vector2 GetPosition(int index)
    {
        return new Vector2(Xs[index] * PositionInvScale, Ys[index] * PositionInvScale);
    }

    public Vector2 GetDirection(int index)
    {
        float angle = DirectionAngles[index] * ByteToDirection;
        return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
    }

    public float GetDelay(int index)
    {
        return TimeOffsets[index] * ByteToTime;
    }

    public void Serialize(MessageWriter writer)
    {
        if (writer == null)
            return;

        writer.Write(AnglePacked);
        writer.Write((ushort)Count);
        for (int i = 0; i < Count; i++)
        {
            ModHelpers.Write(writer, Xs[i]);
            ModHelpers.Write(writer, Ys[i]);
            writer.Write(DirectionAngles[i]);
            writer.Write(TimeOffsets[i]);
        }
    }

    public void Deserialize(MessageReader reader)
    {
        if (reader == null)
            return;

        AnglePacked = reader.ReadUInt16();
        ushort count = reader.ReadUInt16();
        if (count == 0)
        {
            Xs = Array.Empty<short>();
            Ys = Array.Empty<short>();
            DirectionAngles = Array.Empty<byte>();
            TimeOffsets = Array.Empty<byte>();
            return;
        }

        Xs = new short[count];
        Ys = new short[count];
        DirectionAngles = new byte[count];
        TimeOffsets = new byte[count];
        for (int i = 0; i < count; i++)
        {
            Xs[i] = reader.ReadInt16();
            Ys[i] = reader.ReadInt16();
            DirectionAngles[i] = reader.ReadByte();
            TimeOffsets[i] = reader.ReadByte();
        }
    }

    private static short PackPosition(float value)
    {
        int scaled = Mathf.RoundToInt(value * PositionScale);
        scaled = Mathf.Clamp(scaled, short.MinValue, short.MaxValue);
        return (short)scaled;
    }

    private static byte PackDirection(Vector2 direction)
    {
        if (direction == Vector2.zero)
            return 0;

        float angle = Mathf.Atan2(direction.y, direction.x);
        angle = Mathf.Repeat(angle, TwoPi);
        int packed = Mathf.RoundToInt(angle * DirectionToByte);
        return (byte)Mathf.Clamp(packed, 0, 255);
    }

    private static byte PackTime(float time)
    {
        float clamped = Mathf.Clamp(time, 0f, TriggerHappyAbility.BatchInterval);
        int packed = Mathf.RoundToInt(clamped * TimeToByte);
        return (byte)Mathf.Clamp(packed, 0, 255);
    }

    private static ushort PackAngle(float angle)
    {
        angle = Mathf.Repeat(angle, TwoPi);
        int packed = Mathf.RoundToInt(angle * AngleToUShort);
        return (ushort)Mathf.Clamp(packed, 0, 65535);
    }

    private static float UnpackAngle(ushort packed)
    {
        return packed * UShortToAngle;
    }
}
