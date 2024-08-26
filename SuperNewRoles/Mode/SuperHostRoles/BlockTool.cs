using System;
using System.Collections.Generic;
using HarmonyLib;
using Hazel;
using InnerNet;
using SuperNewRoles.MapOption;
using UnityEngine;

namespace SuperNewRoles.Mode.SuperHostRoles;

class BlockTool
{
    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.UpdateSystem), new Type[] { typeof(SystemTypes), typeof(PlayerControl), typeof(byte) })]
    class UpdateSystemPatch
    {
        public static void Prefix(
            [HarmonyArgument(0)] SystemTypes systemType,
            [HarmonyArgument(1)] PlayerControl player,
            [HarmonyArgument(2)] byte amount)
        {
            if (systemType == SystemTypes.Security)
            {
                if (amount == 1)
                {
                    if (!CameraPlayers.Contains(player.PlayerId)) CameraPlayers.Add(player.PlayerId);
                }
                else
                {
                    if (CameraPlayers.Contains(player.PlayerId)) CameraPlayers.Remove(player.PlayerId);
                }
            }
            else if (systemType == SystemTypes.Comms)
            {
                IsCom = !IsCom;
            }
        }
    }
    public static List<byte> CameraPlayers;
    public static List<byte> OldDesyncCommsPlayers;
    private static readonly float UsableDistance = 1.5f;
    private static int Count = 0;
    public static bool IsCom;
    public static float CameraTime = 0;
    public static float AdminTime = 0;
    public static float VitalTime = 0;

    private static bool CheckGuardAndDistance(Vector2 player, Vector2 target, float distance)
        => Vector2.Distance(player, target) <= UsableDistance;

    public static void FixedUpdate()
    {
        Count--;
        if (Count > 0) return;
        Count = 3;
        if (ModeHandler.IsMode(ModeId.Default) ||
            (
              MapOption.MapOption.CanUseAdmin &&
              MapOption.MapOption.CanUseVitalOrDoorLog &&
              MapOption.MapOption.CanUseCamera
            ))
            return;
        foreach (PlayerControl p in CachedPlayer.AllPlayers)
        {
            if (p.IsDead() || p.IsMod())
                continue;
            var cid = p.GetClientId();

            bool IsGuard = false;
            Vector2 playerposition = p.GetTruePosition();
            int currentMapId = GameManager.Instance.LogicOptions.currentGameOptions.MapId;

            //カメラチェック
            if (!MapOption.MapOption.CanUseCamera &&
                CameraPlayers.Contains(p.PlayerId))
                IsGuard = true;

            //アドミンチェック
            if (!IsGuard && !MapOption.MapOption.CanUseAdmin)
            {
                IsGuard = CheckGuardAndDistance(playerposition, GetAdminTransform(), UsableDistance);
            }

            //Polus用のアドミンチェック。Polusはアドミンが2つあるから
            if (!IsGuard &&
                currentMapId == 2 &&
                !MapOption.MapOption.CanUseAdmin)
            {
                IsGuard = CheckGuardAndDistance(playerposition, new Vector2(24.66107f, -21.523f), UsableDistance);
            }

            //AirShip(アーカイブ)用のアドミンチェック。AirShipはアドミンが2つあるから
            if (!IsGuard &&
                currentMapId == 4 &&
                // アドミンを使えない設定 or 
                (!MapOption.MapOption.CanUseAdmin ||
                  // アーカイブアドミンのみ使えない設定か
                  (MapCustoms.MapCustom.RecordsAdminDestroy.GetBool()
                    && MapOption.MapOption.MapOptionSetting.GetBool()
                  )
                ))
            {
                IsGuard = CheckGuardAndDistance(playerposition, new Vector2(19.9f, 12.9f), UsableDistance);
            }

            //バイタルもしくはドアログを防ぐ
            if (!IsGuard && !MapOption.MapOption.CanUseVitalOrDoorLog)
            {
                float distance = UsableDistance;
                if (currentMapId == 2)
                    distance += 0.5f;
                IsGuard = CheckGuardAndDistance(playerposition, GetVitalOrDoorLogTransform(), distance);
            }
            if (IsGuard && !p.inVent && MeetingHud.Instance == null)
            {
                if (!OldDesyncCommsPlayers.Contains(p.PlayerId))
                    OldDesyncCommsPlayers.Add(p.PlayerId);
                SendCommsRpc(128, p, cid);
            }
            else if (!IsCom && OldDesyncCommsPlayers.Contains(p.PlayerId))
            {
                OldDesyncCommsPlayers.Remove(p.PlayerId);
                SendCommsRpc(16, p, cid);
                if (currentMapId == 4)
                    SendCommsRpc(17, p, cid);
            }
        }
    }
    private static void SendCommsRpc(byte type, PlayerControl p, int cid = -1)
    {
        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(MapUtilities.CachedShipStatus.NetId, (byte)RpcCalls.UpdateSystem, SendOption.Reliable, cid);
        writer.Write((byte)SystemTypes.Comms);
        MessageExtensions.WriteNetObject(writer, p);
        writer.Write(type);
        AmongUsClient.Instance.FinishRpcImmediately(writer);
    }
    public static Vector2 GetAdminTransform()
    {
        return GameManager.Instance.LogicOptions.currentGameOptions.MapId switch
        {
            0 => new(3.48f, -8.624401f),
            1 => new(21.024f, 19.095f),
            2 => new(23.13707f, -21.523f),
            3 => new(-3.48f, -8.624401f),
            4 => new(-22.323f, 0.9099998f),
            _ => new(1000, 1000)
        };
    }

    public static Vector2 GetCameraTransform()
    {
        return GameManager.Instance.LogicOptions.currentGameOptions.MapId switch
        {
            0 => new(-12.93658f, -2.790947f),
            2 => new(-12.93658f, -2.790947f),
            3 => new Vector2(13.07439f, -3.215496f),
            4 => new Vector2(8.018572f, -9.942375f),
            _ => new(1000, 1000)
        };
    }
    public static Vector2 GetVitalOrDoorLogTransform()
    {
        return GameManager.Instance.LogicOptions.currentGameOptions.MapId switch
        {
            1 => new Vector2(15.51107f, -2.897387f),
            2 => new Vector2(26.20935f, -16.04406f),
            4 => new Vector2(25.28237f, -8.145635f),
            5 => new Vector2(1.535f, -0.419f),
            _ => new(1000, 1000)
        };
    }
}