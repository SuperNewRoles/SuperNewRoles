using System;
using Hazel;
using SuperNewRoles.CustomObject;
using SuperNewRoles.Helpers;
using UnityEngine;

namespace SuperNewRoles.Roles.Crewmate;

public static class Painter
{
    public enum ActionType
    {
        TaskComplete,
        SabotageRepair,
        InVent,
        ExitVent,
        CheckVital,
        CheckAdmin,
        Death
    }
    public static void SetTarget(PlayerControl Target)
    {
        bool Is = true;
        if (Target == null)
        {
            Target = RoleClass.Painter.CurrentTarget;
            RoleClass.Painter.CurrentTarget = null;
            Is = false;
        }
        else
        {
            RoleClass.Painter.CurrentTarget = Target;
        }
        MessageWriter writer = RPCHelper.StartRPC(CustomRPC.PainterSetTarget);
        writer.Write(Target.PlayerId);
        writer.Write(Is);
        writer.EndRPC();
        RPCProcedure.PainterSetTarget(Target.PlayerId, Is);
    }
    public static void WrapUp()
    {
        Logger.Info("WrapUp");
        if (RoleClass.Painter.Prints.Count > 0 && RoleClass.Painter.IsFootprintMeetingDestroy) DestroyPrints();
        Logger.Info($"{RoleClass.Painter.CurrentTarget != null}");
        if (RoleClass.Painter.CurrentTarget != null)
        {
            SpawnFootprints();
            SetTarget(null);
        }
        foreach (ActionType type in Enum.GetValues(typeof(ActionType)))
        {
            RoleClass.Painter.ActionData[type] = new();
        }
    }
    public static void DestroyPrints()
    {
        RoleClass.Painter.Prints.RemoveAll(print =>
        {
            if (print != null && print.footprint != null)
            {
                GameObject.Destroy(print.footprint);
            }
            return true;
        });
    }
    public static void SpawnFootprints()
    {
        if (RoleClass.Painter.CurrentTarget == null) throw new Exception("RoleClass.Painter.CurrentTargetがnullです");
        foreach (var data in RoleClass.Painter.ActionData)
        {
            Logger.Info($"{data.Key}の数は{data.Value.Count}です");
            foreach (var pos in data.Value)
            {
                Footprint print = new(-1f, false, RoleClass.Painter.CurrentTarget, new(pos.x, pos.y, 0.01f));
                print.footprint.transform.localScale *= 2;
                if (data.Key == ActionType.Death && RoleClass.Painter.IsDeathFootpointBig) print.footprint.transform.localScale *= 2f;
                RoleClass.Painter.Prints.Add(print);
            }
        }
    }
    public static void Handle(ActionType type, Vector2? nullpos = null)
    {
        Logger.Info($"ハンドル:{type}");
        if (!RoleClass.Painter.IsEnables[type]) return;
        if (RoleClass.Painter.CurrentTarget == null) return;
        Logger.Info($"ハンドル:{type}が通過");
        Vector2 pos = nullpos == null ? RoleClass.Painter.CurrentTarget.GetTruePosition() : (Vector2)nullpos;
        RoleClass.Painter.ActionData[type].Add(pos);
    }
    public static void HandleRpc(ActionType type)
    {
        if (RoleClass.Painter.IsLocalActionSend && PlayerControl.LocalPlayer.IsAlive())
        {
            var pos = CachedPlayer.LocalPlayer.transform.position;
            byte[] buff = new byte[sizeof(float) * 2];
            Buffer.BlockCopy(BitConverter.GetBytes(pos.x), 0, buff, 0 * sizeof(float), sizeof(float));
            Buffer.BlockCopy(BitConverter.GetBytes(pos.y), 0, buff, 1 * sizeof(float), sizeof(float));

            MessageWriter writer = RPCHelper.StartRPC(CustomRPC.PainterPaintSet);
            writer.Write(CachedPlayer.LocalPlayer.PlayerId);
            writer.Write((byte)type);
            writer.WriteBytesAndSize(buff);
            writer.EndRPC();
            RPCProcedure.PainterPaintSet(CachedPlayer.LocalPlayer.PlayerId, (byte)type, buff);
        }
    }
}