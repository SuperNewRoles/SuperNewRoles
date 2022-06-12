using HarmonyLib;
using Hazel;
using SuperNewRoles.CustomOption;
using SuperNewRoles.Helpers;
using SuperNewRoles.Roles;
using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNewRoles.Mode.SuperHostRoles
{
    class RPCSetTask
    {/*
                if (!ModeHandler.isMode(ModeId.SuperHostRoles)) return true;
                PlayerControl player = ModHelpers.playerById(playerId);
                if (player == null) return false;
                if (player.isClearTask() && !player.isRole(CustomRPC.RoleId.Workperson))
                {
                    foreach (PlayerControl p in CachedPlayer.AllPlayers)
                    {
                        if (!p.Data.Disconnected)
                        {
                            var tasks = taskTypeIds;
                            if (p.PlayerId != player.PlayerId)
                            {
                                tasks = (new List<byte>() { }).ToArray();
                            }
                            MessageWriter messageWriter2 = AmongUsClient.Instance.StartRpcImmediately(__instance.NetId, (byte)RpcCalls.SetTasks, SendOption.Reliable,p.getClientId());
                            messageWriter2.Write(playerId);
                            messageWriter2.WriteBytesAndSize(tasks);
                            messageWriter2.EndRPC();
                        }
                    }
                    return false;
                } else if (player.isRole(CustomRPC.RoleId.Workperson))
                {
                    var tasks = ModHelpers.generateTasks((int)CustomOptions.WorkpersonCommonTask.getFloat(), (int)CustomOptions.WorkpersonShortTask.getFloat(), (int)CustomOptions.WorkpersonLongTask.getFloat()).ToArray();
                    MessageWriter messageWriter = AmongUsClient.Instance.StartRpc(player.NetId, (byte)29);
                    messageWriter.Write(playerId);
                    messageWriter.WriteBytesAndSize(tasks);
                    messageWriter.EndMessage();
                    return false;
                }*/

    }
}
