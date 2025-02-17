using SuperNewRoles.Roles;

namespace SuperNewRoles.Modules;

public static class PlayerControlRpcExtensions
{
    [CustomRPC]
    public static void RpcExiledCustom(this PlayerControl player)
    {
        player.Exiled();
    }
    [CustomRPC]
    public static void RpcCustomSetRole(this ExPlayerControl player, RoleId roleId)
    {
        player.SetRole(roleId);
    }
}