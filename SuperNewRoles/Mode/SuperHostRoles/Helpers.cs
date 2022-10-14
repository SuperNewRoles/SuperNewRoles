using Hazel;
using InnerNet;

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

        /// <summary>
        /// 守護ガードのエフェクトを表示する(キルクールもリセット)
        /// </summary>
        /// <param name="shower">エフェクトを見れる人</param>
        /// <param name="target">エフェクトをかける人</param>
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
                var crs = CustomRpcSender.Create("RpcShowGuardEffect");
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
        /// <summary>
        /// リアクターのフラッシュをshowerのみに見せる
        /// </summary>
        /// <param name="shower">見る人</param>
        /// <param name="duration">継続時間</param>
        public static void ShowReactorFlash(this PlayerControl shower, float duration = 0f)
        {
            if (shower == null || !AmongUsClient.Instance.AmHost || shower.AmOwner) return;
            int clientId = shower.GetClientId();

            byte reactorId = 3;
            if (PlayerControl.GameOptions.MapId == 2) reactorId = 21;

            // ReactorサボをDesyncで発動
            SuperNewRolesPlugin.Logger.LogInfo("SetDesyncSabotage");
            MessageWriter SabotageWriter = AmongUsClient.Instance.StartRpcImmediately(MapUtilities.CachedShipStatus.NetId, (byte)RpcCalls.RepairSystem, SendOption.Reliable, clientId);
            SabotageWriter.Write(reactorId);
            MessageExtensions.WriteNetObject(SabotageWriter, shower);
            SabotageWriter.Write((byte)128);
            AmongUsClient.Instance.FinishRpcImmediately(SabotageWriter);

            new LateTask(() =>
            { // Reactorサボを修理
                MessageWriter SabotageFixWriter = AmongUsClient.Instance.StartRpcImmediately(MapUtilities.CachedShipStatus.NetId, (byte)RpcCalls.RepairSystem, SendOption.Reliable, clientId);
                SabotageFixWriter.Write(reactorId);
                MessageExtensions.WriteNetObject(SabotageFixWriter, shower);
                SabotageFixWriter.Write((byte)16);
                AmongUsClient.Instance.FinishRpcImmediately(SabotageFixWriter);
            }, 0.1f + duration, "Fix Desync Reactor");

            if (PlayerControl.GameOptions.MapId == 4) //Airship用
                new LateTask(() =>
                { // Reactorサボを修理
                    MessageWriter SabotageFixWriter = AmongUsClient.Instance.StartRpcImmediately(MapUtilities.CachedShipStatus.NetId, (byte)RpcCalls.RepairSystem, SendOption.Reliable, clientId);
                    SabotageFixWriter.Write(reactorId);
                    MessageExtensions.WriteNetObject(SabotageFixWriter, shower);
                    SabotageFixWriter.Write((byte)17);
                    AmongUsClient.Instance.FinishRpcImmediately(SabotageFixWriter);
                }, 0.1f + duration, "Fix Desync Reactor 2");
        }
    }
}