namespace SuperNewRoles.Modules;

public static class PlayerControlRpcExtensions
{
    [CustomRPC]
    public static void RpcExiledCustom(this PlayerControl player)
    {
        player.Exiled();
    }
}