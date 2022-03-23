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
        [HarmonyPatch(typeof(Console),nameof(Console.CanUse))]
        class MapConsoleCanuse
        {
            public static void Postfix(Console __instance)
            {
                UsableDistance = __instance.usableDistance;
            }
        }
        private static float UsableDistance;
        private static int Count = 0;
        public static bool IsCom;
        public static void FixedUpdate()
        {
            Count--;
            if (Count >= 1) return;
            Count = 5;
            if (!MapOptions.MapOption.UseAdmin && !ModeHandler.isMode(ModeId.Default))
            {
                foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                {
                    try
                    {
                        if (p.isAlive() && !p.IsMod() && !p.inVent)
                        {
                            var Distance = Vector2.Distance(p.GetTruePosition(), GetAdminTransform());
                            var cid = p.getClientId();
                            if (Distance <= UsableDistance)
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
            else if (PlayerControl.GameOptions.MapId == 4)
            {
                return new Vector2(-22.323f, 0.9099998f);
            }
            return new Vector2(0, 0);
        }
    }
}
