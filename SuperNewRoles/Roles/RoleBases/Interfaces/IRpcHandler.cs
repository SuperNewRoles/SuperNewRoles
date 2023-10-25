using Hazel;
using SuperNewRoles.Helpers;

namespace SuperNewRoles.Roles.RoleBases.Interfaces;
public interface IRpcHandler
{
    public void RpcReader(MessageReader reader);
}