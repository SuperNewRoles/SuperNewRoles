
namespace SuperNewRoles.Mode.SuperHostRoles
{
    class RPCSetTask
    {/*
                if (!ModeHandler.IsMode(ModeId.SuperHostRoles)) return true;
                PlayerControl player = ModHelpers.PlayerById(playerId);
                if (player == null) return false;
                if (player.IsClearTask() && !player.IsRole(RoleId.Workperson))
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
                            MessageWriter messageWriter2 = AmongUsClient.Instance.StartRpcImmediately(__instance.NetId, (byte)RpcCalls.SetTasks, SendOption.Reliable,p.GetClientId());
                            messageWriter2.Write(playerId);
                            messageWriter2.WriteBytesAndSize(tasks);
                            messageWriter2.EndRPC();
                        }
                    }
                    return false;
                } else if (player.IsRole(RoleId.Workperson))
                {
                    var tasks = ModHelpers.GenerateTasks(CustomOptions.WorkpersonCommonTask.GetInt(), CustomOptions.WorkpersonShortTask.GetInt(), CustomOptions.WorkpersonLongTask.GetInt()).ToArray();
                    MessageWriter messageWriter = AmongUsClient.Instance.StartRpc(player.NetId, (byte)29);
                    messageWriter.Write(playerId);
                    messageWriter.WriteBytesAndSize(tasks);
                    messageWriter.EndMessage();
                    return false;
                }*/
    }
}