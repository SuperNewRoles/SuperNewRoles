using HarmonyLib;
using Hazel;
using InnerNet;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SuperNewRoles.Mode.SuperHostRoles
{
    class BlockTool
    {
        [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.RepairSystem))]
        class RepairSystemPatch
        {
            public static void Prefix(ShipStatus __instance,
                [HarmonyArgument(0)] SystemTypes systemType,
                [HarmonyArgument(1)] PlayerControl player,
                [HarmonyArgument(2)] byte amount)
            {
                if (systemType == SystemTypes.Comms)
                {
                    IsCom = !IsCom;
                }
            }
        }
        private static float UsableDistance = 1.5f;
        private static int Count = 0;
        public static bool IsCom;
        public static void FixedUpdate()
        {
            Count--;
            if (Count >= 1) return;
            Count = 7;
            SuperNewRolesPlugin.Logger.LogInfo("場所:" + PlayerControl.LocalPlayer.transform.position.x + "、" + PlayerControl.LocalPlayer.transform.position.y);
            if ((!MapOptions.MapOption.UseAdmin || !MapOptions.MapOption.UseCamera) && !ModeHandler.isMode(ModeId.Default))
            {
                foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                {
                    try
                    {
                        if (p.isAlive() && !p.IsMod() && !p.inVent)
                        {
                            var cid = p.getClientId();
                            bool IsGuard = false;
                            //アドミンチェック
                            if (!MapOptions.MapOption.UseAdmin)
                            {
                                var AdminDistance = Vector2.Distance(p.GetTruePosition(), GetAdminTransform());
                                if (AdminDistance <= UsableDistance)
                                {
                                    IsGuard = true;
                                }
                            }
                            //Polus用のアドミンチェック。Polusはアドミンが2つあるから
                            if (!IsGuard && PlayerControl.GameOptions.MapId == 2 && !MapOptions.MapOption.UseAdmin)
                            {
                                var AdminDistance = Vector2.Distance(p.GetTruePosition(), new Vector2(24.66107f,-21.523f));
                                if (AdminDistance <= UsableDistance)
                                {
                                    IsGuard = true;
                                }
                            }
                            //カメラチェック
                            if (!IsGuard && !MapOptions.MapOption.UseCamera)
                            {
                                var VitalDistance = Vector2.Distance(p.GetTruePosition(), GetCameraTransform());
                                if (VitalDistance <= UsableDistance)
                                {
                                    IsGuard = true;
                                }
                            }
                            //バイタルもしくはドアログを防ぐ
                            if (!IsGuard && !MapOptions.MapOption.UseVitalOrDoorLog)
                            {
                                float distance = UsableDistance;
                                if (PlayerControl.GameOptions.MapId == 2)
                                {
                                    distance += 0.5f;
                                }
                                var VitalDistance = Vector2.Distance(p.GetTruePosition(),GetVitalOrDoorLogTransform());
                                if (VitalDistance <= distance)
                                {
                                    IsGuard = true;
                                }
                            }
                            if (IsGuard)
                            {
                                MessageWriter SabotageWriter = AmongUsClient.Instance.StartRpcImmediately(ShipStatus.Instance.NetId, (byte)RpcCalls.RepairSystem, SendOption.Reliable, cid);
                                SabotageWriter.Write((byte)SystemTypes.Comms);
                                MessageExtensions.WriteNetObject(SabotageWriter, p);
                                SabotageWriter.Write((byte)128);
                                AmongUsClient.Instance.FinishRpcImmediately(SabotageWriter);
                            }
                            else
                            {
                                if (!IsCom)
                                {
                                    MessageWriter SabotageFixWriter = AmongUsClient.Instance.StartRpcImmediately(ShipStatus.Instance.NetId, (byte)RpcCalls.RepairSystem, SendOption.Reliable, cid);
                                    SabotageFixWriter.Write((byte)SystemTypes.Comms);
                                    MessageExtensions.WriteNetObject(SabotageFixWriter, p);
                                    SabotageFixWriter.Write((byte)16);
                                    AmongUsClient.Instance.FinishRpcImmediately(SabotageFixWriter);

                                    if (PlayerControl.GameOptions.MapId == 4)
                                    {
                                        SabotageFixWriter = AmongUsClient.Instance.StartRpcImmediately(ShipStatus.Instance.NetId, (byte)RpcCalls.RepairSystem, SendOption.Reliable, cid);
                                        SabotageFixWriter.Write((byte)SystemTypes.Comms);
                                        MessageExtensions.WriteNetObject(SabotageFixWriter, p);
                                        SabotageFixWriter.Write((byte)17);
                                        AmongUsClient.Instance.FinishRpcImmediately(SabotageFixWriter);
                                    }
                                }
                            }
                        }
                    }
                    catch { }
                }
            }
        }
        public static Vector2 GetAdminTransform()
        {
            if (PlayerControl.GameOptions.MapId == 0)
            {
                return new Vector2(3.48f, -8.624401f);
            }
            else if (PlayerControl.GameOptions.MapId == 1)
            {
                return new Vector2(21.024f, 19.095f);
            }
            else if (PlayerControl.GameOptions.MapId == 2)
            {
                return new Vector2(23.13707f, -21.523f);
            }
            else if (PlayerControl.GameOptions.MapId == 3)
            {
                return new Vector2(-3.48f, -8.624401f);
            }
            else if (PlayerControl.GameOptions.MapId == 4)
            {
                return new Vector2(-22.323f, 0.9099998f);
            }
            return new Vector2(1000, 1000);
        }

        public static Vector2 GetCameraTransform()
        {
            if (PlayerControl.GameOptions.MapId == 0)
            {
                return new Vector2(-12.93658f, -2.790947f);
            }
            else if (PlayerControl.GameOptions.MapId == 2)
            {
                return new Vector2(2.428533f, -12.52964f);
            }
            else if (PlayerControl.GameOptions.MapId == 3)
            {
                return new Vector2(13.07439f, -3.215496f);
            }
            else if (PlayerControl.GameOptions.MapId == 4)
            {
                return new Vector2(8.018572f, -9.942375f);
            }
            return new Vector2(1000, 1000);
        }
        public static Vector2 GetVitalOrDoorLogTransform()
        {
            if (PlayerControl.GameOptions.MapId == 1)
            {
                return new Vector2(15.51107f,-2.897387f);
            }
            else if (PlayerControl.GameOptions.MapId == 2)
            {
                return new Vector2(26.20935f,-16.04406f);
            }
            else if (PlayerControl.GameOptions.MapId == 4)
            {
                return new Vector2(25.28237f,-8.145635f);
            }
            return new Vector2(1000, 1000);
        }
    }
}
