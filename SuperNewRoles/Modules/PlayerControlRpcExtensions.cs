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
    [CustomRPC]
    public static void RpcCustomReportDeadBody(this PlayerControl player, NetworkedPlayerInfo target)
    {
        player.ReportDeadBody(target);
    }
    [CustomRPC]
    public static void RpcCustomMurderPlayer(this PlayerControl player, PlayerControl target, bool didSucceed)
    {
        player.MurderPlayer(target, didSucceed ? MurderResultFlags.Succeeded : MurderResultFlags.FailedError);
    }
}