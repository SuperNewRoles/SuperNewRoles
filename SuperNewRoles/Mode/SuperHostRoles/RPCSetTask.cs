
namespace SuperNewRoles.Mode.SuperHostRoles
{
    class RPCSetTask
    {/*
                if (!ModeHandler.IsMode(ModeId.SuperHostRoles)) return true;
                PlayerControl player = ModHelpers.playerById(playerId);
                if (player == null) return false;
                if (player.isClearTask() && !player.isRole(RoleId.Workperson))
                {
                    foreach (PlayerControl p in CachedPlayer.AllPlayers)
                    {
                        if (!p.Data.Disconnected)
                        {
                            var tasks = taskTypeIds;
                            if (p.PlayerId != player.PlayerId)
                            {
                                tasks = (new() { }).ToArray();
                            }
                            MessageWriter messageWriter2 = AmongUsClient.Instance.StartRpcImmediately(__instance.NetId, (byte)RpcCalls.SetTasks, SendOption.Reliable,p.getClientId());
                            messageWriter2.Write(playerId);
                            messageWriter2.WriteBytesAndSize(tasks);
                            messageWriter2.EndRPC();
                        }
                    }
                    return false;
                } else if (player.isRole(RoleId.Workperson))
                {
                    var tasks = ModHelpers.generateTasks((int)CustomOptions.WorkpersonCommonTask.GetFloat(), (int)CustomOptions.WorkpersonShortTask.GetFloat(), (int)CustomOptions.WorkpersonLongTask.GetFloat()).ToArray();
                    MessageWriter messageWriter = AmongUsClient.Instance.StartRpc(player.NetId, (byte)29);
                    messageWriter.Write(playerId);
                    messageWriter.WriteBytesAndSize(tasks);
                    messageWriter.EndMessage();
                    return false;
                }*/
    }
}