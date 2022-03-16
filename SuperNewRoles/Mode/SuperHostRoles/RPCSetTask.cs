using HarmonyLib;
using Hazel;
using SuperNewRoles.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNewRoles.Mode.SuperHostRoles
{
    class RPCSetTask
    {
        [HarmonyPatch(typeof(GameData),nameof(GameData.RpcSetTasks))]
        class RPCSetTasksPatch
        {
            public static bool Prefix(GameData __instance,
                [HarmonyArgument(0)] byte playerId,
                [HarmonyArgument(1)] ref UnhollowerBaseLib.Il2CppStructArray<byte> taskTypeIds)
            {
                if (!ModeHandler.isMode(ModeId.SuperHostRoles)) return true;
                PlayerControl player = ModHelpers.playerById(playerId);
                if (player == null) return false;
                if (player.isClearTask())
                {
                    foreach (PlayerControl p in PlayerControl.AllPlayerControls)
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
                }
                return true;
            }
        }
    }
}
