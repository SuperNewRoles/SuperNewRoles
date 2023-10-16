using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hazel;
using SuperNewRoles.Helpers;

namespace SuperNewRoles.Roles.RoleBases.Interfaces;
public interface IRpcHandler
{
    public sealed MessageWriter GetRpcWriter()
    {
        MessageWriter writer = RPCHelper.StartRPC(CustomRPC.RoleRpcHandler);
        // 自身のPlayerIdを送信
        writer.Write((this as RoleBase)?.Player.PlayerId ?? 255);
        return writer;
    }
    public void RpcReader(MessageReader reader);
}