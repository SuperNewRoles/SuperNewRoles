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

        public static void RpcShowGuardEffect(this PlayerControl shower, PlayerControl target)
        {
            if (shower.IsMod())
            {// mod導入者ならCustomRpcSenderを使用しなくても正しくRpcを送れる。
                Logger.Info($"Mod導入者{shower.name}({shower.GetRole()})=>{target.name}({target.GetRole()})", "RpcShowGuardEffect");
                shower.ProtectPlayer(target, 0);
                shower.RpcMurderPlayer(target);
            }
            else
            {
                var crs = CustomRpcSender.Create("RpcShowGuardEffect", SendOption.Reliable);
                var clientId = shower.GetClientId();
                Logger.Info($"非Mod導入者{shower.name}({shower.GetRole()})=>{target.name}({target.GetRole()})", "RpcShowGuardEffect");
                crs.StartMessage(clientId);
                crs.StartRpc(shower.NetId, (byte)RpcCalls.ProtectPlayer)// 守護を始める
                    .WriteNetObject(target) // targetを対象に
                    .Write(0) // ProtectPlayerの引数2の、coloridを0で実行
                    .EndRpc(); // 守護終わり

                crs.StartRpc(shower.NetId, (byte)RpcCalls.MurderPlayer) // キルを始める
                    .WriteNetObject(target) // targetを対象に
                    .EndRpc(); // キル終わり

                crs.EndMessage(); // RpcShowGuardEffect終わり
                crs.SendMessage(); // ログへ出力(のはず)
            }
        }
    }
}