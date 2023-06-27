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
    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.RepairSystem))]
    class RepairSystemPatch
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
    public static void FixedUpdate()
    {
        Count--;
        if (Count > 0) return;
        Count = 3;
        if ((!MapOption.MapOption.CanUseAdmin ||
            !MapOption.MapOption.CanUseVitalOrDoorLog ||
            !MapOption.MapOption.CanUseCamera)
            && !ModeHandler.IsMode(ModeId.Default))
        {
            foreach (PlayerControl p in CachedPlayer.AllPlayers)
            {
                try
                {
                    if (p.IsAlive() && !p.IsMod())
                    {
                        var cid = p.GetClientId();
                        bool IsGuard = false;
                        Vector2 playerposition = p.GetTruePosition();
                        //カメラチェック
                        if (!MapOption.MapOption.CanUseCamera && CameraPlayers.Contains(p.PlayerId)) IsGuard = true;
                        //アドミンチェック
                        if (!MapOption.MapOption.CanUseAdmin)
                        {
                            var AdminDistance = Vector2.Distance(playerposition, GetAdminTransform());
                            if (AdminDistance <= UsableDistance) IsGuard = true;
                        }
                        //Polus用のアドミンチェック。Polusはアドミンが2つあるから
                        if (!IsGuard && GameManager.Instance.LogicOptions.currentGameOptions.MapId == 2 && !MapOption.MapOption.CanUseAdmin)
                        {
                            var AdminDistance = Vector2.Distance(playerposition, new Vector2(24.66107f, -21.523f));
                            if (AdminDistance <= UsableDistance) IsGuard = true;
                        }
                        //AirShip(アーカイブ)用のアドミンチェック。AirShipはアドミンが2つあるから
                        if ((!IsGuard && GameManager.Instance.LogicOptions.currentGameOptions.MapId == 4 && !MapOption.MapOption.CanUseAdmin) || (!IsGuard && GameManager.Instance.LogicOptions.currentGameOptions.MapId == 4 && MapCustoms.MapCustom.RecordsAdminDestroy.GetBool() && MapOption.MapOption.MapOptionSetting.GetBool()))
                        {
                            var AdminDistance = Vector2.Distance(playerposition, new Vector2(19.9f, 12.9f));
                            if (AdminDistance <= UsableDistance) IsGuard = true;
                        }
                        //バイタルもしくはドアログを防ぐ
                        if (!IsGuard && !MapOption.MapOption.CanUseVitalOrDoorLog)
                        {
                            float distance = UsableDistance;
                            if (GameManager.Instance.LogicOptions.currentGameOptions.MapId == 2) distance += 0.5f;
                            var AdminDistance = Vector2.Distance(playerposition, GetVitalOrDoorLogTransform());
                            if (AdminDistance <= distance) IsGuard = true;
                        }
                        if (IsGuard && !p.inVent && MeetingHud.Instance == null)
                        {
                            if (!OldDesyncCommsPlayers.Contains(p.PlayerId))
                                OldDesyncCommsPlayers.Add(p.PlayerId);
                            MessageWriter SabotageWriter = AmongUsClient.Instance.StartRpcImmediately(MapUtilities.CachedShipStatus.NetId, (byte)RpcCalls.RepairSystem, SendOption.Reliable, cid);
                            SabotageWriter.Write((byte)SystemTypes.Comms);
                            MessageExtensions.WriteNetObject(SabotageWriter, p);
                            SabotageWriter.Write((byte)128);
                            AmongUsClient.Instance.FinishRpcImmediately(SabotageWriter);
                        }
                        else
                        {
                            if (!IsCom && OldDesyncCommsPlayers.Contains(p.PlayerId))
                            {
                                OldDesyncCommsPlayers.Remove(p.PlayerId);
                                MessageWriter SabotageFixWriter = AmongUsClient.Instance.StartRpcImmediately(MapUtilities.CachedShipStatus.NetId, (byte)RpcCalls.RepairSystem, SendOption.Reliable, cid);
                                SabotageFixWriter.Write((byte)SystemTypes.Comms);
                                MessageExtensions.WriteNetObject(SabotageFixWriter, p);
                                SabotageFixWriter.Write((byte)16);
                                AmongUsClient.Instance.FinishRpcImmediately(SabotageFixWriter);

                                if (GameManager.Instance.LogicOptions.currentGameOptions.MapId == 4)
                                {
                                    SabotageFixWriter = AmongUsClient.Instance.StartRpcImmediately(MapUtilities.CachedShipStatus.NetId, (byte)RpcCalls.RepairSystem, SendOption.Reliable, cid);
                                    SabotageFixWriter.Write((byte)SystemTypes.Comms);
                                    MessageExtensions.WriteNetObject(SabotageFixWriter, p);
                                    SabotageFixWriter.Write((byte)17);
                                    AmongUsClient.Instance.FinishRpcImmediately(SabotageFixWriter);
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    SuperNewRolesPlugin.Logger.LogError(e);
                }
            }
        }
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
            _ => new(1000, 1000)
        };
    }
}