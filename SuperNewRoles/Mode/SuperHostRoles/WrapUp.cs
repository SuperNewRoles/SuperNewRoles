using Hazel;
using InnerNet;
using SuperNewRoles.CustomRPC;
using SuperNewRoles.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SuperNewRoles.Mode.SuperHostRoles
{
    class WrapUpClass
    {
        public static void WrapUp(GameData.PlayerInfo exiled)
        {
            if (!AmongUsClient.Instance.AmHost) return;
            /*
            new LateTask(() =>
            {
                foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                {
                    byte reactorId = 3;
                    if (PlayerControl.GameOptions.MapId == 2) reactorId = 21;
                    MessageWriter MurderWriter = AmongUsClient.Instance.StartRpcImmediately(p.NetId, (byte)RpcCalls.MurderPlayer, SendOption.Reliable, p.getClientId());
                    MessageExtensions.WriteNetObject(MurderWriter, BotHandler.Bot);
                    AmongUsClient.Instance.FinishRpcImmediately(MurderWriter);
                    MessageWriter SabotageWriter = AmongUsClient.Instance.StartRpcImmediately(ShipStatus.Instance.NetId, (byte)RpcCalls.RepairSystem, SendOption.Reliable, p.getClientId());
                    SabotageWriter.Write(reactorId);
                    MessageExtensions.WriteNetObject(SabotageWriter, p);
                    SabotageWriter.Write((byte)128);
                    AmongUsClient.Instance.FinishRpcImmediately(SabotageWriter);
                    MessageWriter SabotageFixWriter = AmongUsClient.Instance.StartRpcImmediately(ShipStatus.Instance.NetId, (byte)RpcCalls.RepairSystem, SendOption.Reliable, p.getClientId());
                    SabotageFixWriter.Write(reactorId);
                    MessageExtensions.WriteNetObject(SabotageFixWriter, p);
                    SabotageFixWriter.Write((byte)16);
                    AmongUsClient.Instance.FinishRpcImmediately(SabotageFixWriter);
                    if (PlayerControl.GameOptions.MapId == 4)
                    {
                        MessageWriter SabotageFixWriter2 = AmongUsClient.Instance.StartRpcImmediately(ShipStatus.Instance.NetId, (byte)RpcCalls.RepairSystem, SendOption.Reliable, p.getClientId());
                        SabotageFixWriter2.Write(reactorId);
                        MessageExtensions.WriteNetObject(SabotageFixWriter2, p);
                        SabotageFixWriter2.Write((byte)17);
                        AmongUsClient.Instance.FinishRpcImmediately(SabotageFixWriter2);
                    }
                }
            }, 5f, "AntiBlack");*/
            AmongUsClient.Instance.StartCoroutine(nameof(ResetName));
            IEnumerator ResetName()
            {
                yield return new WaitForSeconds(1);
                FixedUpdate.SetRoleNames();
            }
            Roles.BestFalseCharge.WrapUp();
            if (exiled == null) return;
            exiled.IsDead = true;
            exiled.Object.Exiled();
            if (exiled.Object.isRole(RoleId.Sheriff) || exiled.Object.isRole(RoleId.truelover))
            {
                exiled.Object.RpcSetRoleDesync(RoleTypes.GuardianAngel);
            }
            Roles.Jester.WrapUp(exiled);
            Roles.Nekomata.WrapUp(exiled);
        }
    }
}
