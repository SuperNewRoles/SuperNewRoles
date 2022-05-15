
using Hazel;
using SuperNewRoles.CustomRPC;
using SuperNewRoles.Roles;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SuperNewRoles.Mode.SuperHostRoles
{
    class CoEnterVent
    {
        public static bool Prefix(PlayerPhysics __instance, int id)
        {
            if (!AmongUsClient.Instance.AmHost || !ModeHandler.isMode(ModeId.SuperHostRoles)) return true;
            if (!RoleClass.Minimalist.UseVent && __instance.myPlayer.isRole(RoleId.Minimalist))
            {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(__instance.NetId, (byte)RpcCalls.BootFromVent, SendOption.Reliable, -1);
                writer.WritePacked(127);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                new LateTask(() =>
                {
                    int clientId = __instance.myPlayer.getClientId();
                    MessageWriter writer2 = AmongUsClient.Instance.StartRpcImmediately(__instance.NetId, (byte)RpcCalls.BootFromVent, SendOption.Reliable, clientId);
                    writer2.Write(id);
                    AmongUsClient.Instance.FinishRpcImmediately(writer2);
                }, 0.5f, "Anti Vent");
                return false;
            } else if (!RoleClass.Egoist.UseVent && __instance.myPlayer.isRole(RoleId.Egoist))
            {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(__instance.NetId, (byte)RpcCalls.BootFromVent, SendOption.Reliable, -1);
                writer.WritePacked(127);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                new LateTask(() =>
                {
                    int clientId = __instance.myPlayer.getClientId();
                    MessageWriter writer2 = AmongUsClient.Instance.StartRpcImmediately(__instance.NetId, (byte)RpcCalls.BootFromVent, SendOption.Reliable, clientId);
                    writer2.Write(id);
                    AmongUsClient.Instance.FinishRpcImmediately(writer2);
                }, 0.5f, "Anti Vent");
                return false;
            } else if ((__instance.myPlayer.isRole(RoleId.Jackal) && !RoleClass.Jackal.IsUseVent) || __instance.myPlayer.isRole(RoleId.RemoteSheriff) || __instance.myPlayer.isRole(RoleId.Sheriff) || __instance.myPlayer.isRole(RoleId.truelover) || __instance.myPlayer.isRole(RoleId.FalseCharges))
            {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(__instance.NetId, (byte)RpcCalls.BootFromVent, SendOption.Reliable, -1);
                writer.WritePacked(127);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                new LateTask(() =>
                {
                    int clientId = __instance.myPlayer.getClientId();
                    MessageWriter writer2 = AmongUsClient.Instance.StartRpcImmediately(__instance.NetId, (byte)RpcCalls.BootFromVent, SendOption.Reliable, clientId);
                    writer2.Write(id);
                    AmongUsClient.Instance.FinishRpcImmediately(writer2);
                }, 0.5f, "Anti Vent");
                return false;
            } else if (__instance.myPlayer.isRole(RoleId.Technician) && !RoleHelpers.IsSabotage())
            {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(__instance.NetId, (byte)RpcCalls.BootFromVent, SendOption.Reliable, -1);
                writer.WritePacked(127);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                new LateTask(() =>
                {
                    int clientId = __instance.myPlayer.getClientId();
                    MessageWriter writer2 = AmongUsClient.Instance.StartRpcImmediately(__instance.NetId, (byte)RpcCalls.BootFromVent, SendOption.Reliable, clientId);
                    writer2.Write(id);
                    AmongUsClient.Instance.FinishRpcImmediately(writer2);
                }, 0.5f, "Anti Vent");
                return false;
            }
            return true;
        }
    }
}
