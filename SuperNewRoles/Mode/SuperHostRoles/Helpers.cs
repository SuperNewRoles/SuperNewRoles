using Hazel;

namespace SuperNewRoles.Mode.SuperHostRoles
{
    public static class Helpers
    {
        public static void UnCheckedRpcSetRole(this PlayerControl player, RoleTypes role)
        {
            foreach (PlayerControl p in CachedPlayer.AllPlayers)
            {
                player.RpcSetRoleDesync(role, p);
            }
        }
        //TownOfHostより！！
        public static void RpcSetRoleDesync(this PlayerControl player, RoleTypes role, PlayerControl seer = null)
        {
            //player: 名前の変更対象
            //seer: 上の変更を確認することができるプレイヤー

            if (player == null) return;
            if (seer == null) seer = player;
            var clientId = seer.GetClientId();
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(player.NetId, (byte)RpcCalls.SetRole, SendOption.Reliable, clientId);
            writer.Write((ushort)role);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }
        public static void RpcSetRoleDesync(this PlayerControl player, CustomRpcSender sender, RoleTypes role, PlayerControl seer = null)
        {
            //player: 名前の変更対象
            //seer: 上の変更を確認することができるプレイヤー
            if (player == null) return;
            if (seer == null) seer = player;
            var clientId = seer.GetClientId();
            SuperNewRolesPlugin.Logger.LogInfo("(Desync => " + seer.Data.PlayerName + " ) " + player.Data.PlayerName + " => " + role);
            sender.StartMessage(clientId)
                .StartRpc(player.NetId, RpcCalls.SetRole)
                .Write((ushort)role)
                .EndRpc()
                .EndMessage();
        }
        public static void RpcSetRole(this PlayerControl player, CustomRpcSender sender, RoleTypes role)
        {
            SuperNewRolesPlugin.Logger.LogInfo(player.Data.PlayerName + " => " + role);
            if (player == null) return;
            sender.StartRpc(player.NetId, RpcCalls.SetRole);
            sender.Write((ushort)role);
            sender.EndRpc();
            player.SetRole(role);
        }
    }
}