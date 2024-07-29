using AmongUs.GameOptions;
using Hazel;
using InnerNet;
using SuperNewRoles.Helpers;
using SuperNewRoles.Roles;

namespace SuperNewRoles.Mode.SuperHostRoles;

public static class Helpers
{
    public static void UnCheckedRpcSetRole(this PlayerControl player, RoleTypes role)
    {
        foreach (PlayerControl p in CachedPlayer.AllPlayers.AsSpan())
        {
            player.RpcSetRoleDesync(role, p);
        }
    }
    public static void ResetToCrewAndSetRole(this PlayerControl player, RoleTypes role, bool isNotModOnly = true)
    {
        CustomRpcSender sender = CustomRpcSender.Create("ResetToCrewAndSetRole", sendOption:SendOption.Reliable);
        sender.RpcSetRole(player, RoleTypes.Tracker, true);
        if (!(player.IsMod() && isNotModOnly))
        {
            if (player.PlayerId != PlayerControl.LocalPlayer.PlayerId)
                sender.RpcSetRole(player, role, true, player.GetClientId());
            else
                player.SetRole(role, true);
        }
        sender.SendMessage();
    }
    public static void RpcSetRoleImmediately(this PlayerControl player, RoleTypes role, bool canOverride = true, int targetClientId = -1)
    {
        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(player.NetId, (byte)RpcCalls.SetRole, SendOption.Reliable, targetClientId);
        writer.Write((ushort)role);
        writer.Write(canOverride);
        AmongUsClient.Instance.FinishRpcImmediately(writer);
        player.SetRole(role, canOverride);
    }
    //TownOfHostより！！
    public static void RpcSetRoleDesync(this PlayerControl player, RoleTypes role, bool canOverride, PlayerControl seer = null)
    {
        //player: 名前の変更対象
        //seer: 上の変更を確認することができるプレイヤー

        if (player == null) return;
        if (seer == null) seer = player;
        var clientId = seer.GetClientId();
        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(player.NetId, (byte)RpcCalls.SetRole, SendOption.Reliable, clientId);
        writer.Write((ushort)role);
        writer.Write(canOverride);
        AmongUsClient.Instance.FinishRpcImmediately(writer);
    }
    public static void RpcSetRoleDesync(this PlayerControl player, CustomRpcSender sender, RoleTypes role, bool canOverride, PlayerControl seer = null)
    {
        //player: 名前の変更対象
        //seer: 上の変更を確認することができるプレイヤー
        if (player == null) return;
        if (seer == null) seer = player;
        var clientId = seer.GetClientId();
        SuperNewRolesPlugin.Logger.LogInfo("(Desync => " + seer.Data.PlayerName + " ) " + player.Data.PlayerName + " => " + role);
        sender.AutoStartRpc(player.NetId, (byte)RpcCalls.SetRole)
            .Write((ushort)role)
            .Write(canOverride)
            .EndRpc();
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
    /// 守護ガードのエフェクトを表示する。
    /// showerのキルクール及びアビリティクールのリセットも行う。
    /// </summary>
    /// <param name="shower">エフェクトを見れる人</param>
    /// <param name="target">エフェクトをかける人</param>
    public static void RpcShowGuardEffect(this PlayerControl shower, PlayerControl target)
    {
        if (shower.IsMod())
        {// mod導入者ならCustomRpcSenderを使用しなくても正しくRpcを送れる。
            Logger.Info($"Mod導入者{shower.name}({shower.GetRole()})=>{target.name}({target.GetRole()})", "RpcShowGuardEffect");
            MessageWriter writer = RPCHelper.StartRPC(CustomRPC.ShowGuardEffect);
            writer.Write(shower.PlayerId);
            writer.Write(target.PlayerId);
            writer.EndRPC();
            RPCProcedure.ShowGuardEffect(shower.PlayerId, target.PlayerId);
        }
        else
        {
            var clientId = shower.GetClientId();
            Logger.Info($"非Mod導入者{shower.name}({shower.GetRole()})=>{target.name}({target.GetRole()})", "RpcShowGuardEffect");
            if (!ModeHandler.IsMode(ModeId.SuperHostRoles))
            {
                MurderHelpers.RpcForceMurderAndGuard(shower, target, shower);
                MurderHelpers.RpcForceGuard(shower, target, shower);
                return;
            }
            SyncSetting.CustomSyncSettings(target, isCooldownTwice: true);
            new LateTask(() =>
            {
                SyncSetting.CustomSyncSettings(target, isCooldownTwice: true);
                MurderHelpers.RpcForceGuard(shower, target, shower);
                MurderHelpers.RpcMurderPlayerFlags(shower, target,
                    MurderResultFlags.FailedProtected, shower);
                new LateTask(() => SyncSetting.CustomSyncSettings(target), 0.15f);
            }, 0.15f);
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

        byte reactorId = (byte)SystemTypes.Reactor;
        MapNames mapName = (MapNames)GameManager.Instance.LogicOptions.currentGameOptions.MapId;

        if (mapName == MapNames.Polus) reactorId = (byte)SystemTypes.Laboratory;
        else if (mapName == MapNames.Airship) reactorId = (byte)SystemTypes.HeliSabotage;

        // ReactorサボをDesyncで発動
        SuperNewRolesPlugin.Logger.LogInfo("SetDesyncSabotage");
        MessageWriter SabotageWriter = AmongUsClient.Instance.StartRpcImmediately(MapUtilities.CachedShipStatus.NetId, (byte)RpcCalls.UpdateSystem, SendOption.Reliable, clientId);
        SabotageWriter.Write(reactorId);
        MessageExtensions.WriteNetObject(SabotageWriter, shower);
        SabotageWriter.Write((byte)128);
        AmongUsClient.Instance.FinishRpcImmediately(SabotageWriter);

        new LateTask(() =>
        { // Reactorサボを修理
            MessageWriter SabotageFixWriter = AmongUsClient.Instance.StartRpcImmediately(MapUtilities.CachedShipStatus.NetId, (byte)RpcCalls.UpdateSystem, SendOption.Reliable, clientId);
            SabotageFixWriter.Write(reactorId);
            MessageExtensions.WriteNetObject(SabotageFixWriter, shower);
            SabotageFixWriter.Write((byte)16);
            AmongUsClient.Instance.FinishRpcImmediately(SabotageFixWriter);
        }, 0.1f + duration, "Fix Desync Reactor");

        if (mapName == MapNames.Airship) //Airship用
            new LateTask(() =>
            { // Reactorサボを修理
                MessageWriter SabotageFixWriter = AmongUsClient.Instance.StartRpcImmediately(MapUtilities.CachedShipStatus.NetId, (byte)RpcCalls.UpdateSystem, SendOption.Reliable, clientId);
                SabotageFixWriter.Write(reactorId);
                MessageExtensions.WriteNetObject(SabotageFixWriter, shower);
                SabotageFixWriter.Write((byte)17);
                AmongUsClient.Instance.FinishRpcImmediately(SabotageFixWriter);
            }, 0.1f + duration, "Fix Desync Reactor 2");
    }
    /// <summary>
    /// リアクターのフラッシュを全員に見せる
    /// </summary>
    /// <param name="shower">見る人</param>
    /// <param name="duration">継続時間</param>
    public static void ShowReactorFlash(float duration = 0f)
    {
        if (!AmongUsClient.Instance.AmHost) return;

        byte reactorId = (byte)SystemTypes.Reactor;
        MapNames mapName = (MapNames)GameManager.Instance.LogicOptions.currentGameOptions.MapId;

        if (mapName == MapNames.Polus) reactorId = (byte)SystemTypes.Laboratory;
        else if (mapName == MapNames.Airship) reactorId = (byte)SystemTypes.HeliSabotage;

        // ReactorサボをDesyncで発動
        SuperNewRolesPlugin.Logger.LogInfo("SetDesyncSabotage");
        MessageWriter SabotageWriter = AmongUsClient.Instance.StartRpcImmediately(MapUtilities.CachedShipStatus.NetId, (byte)RpcCalls.UpdateSystem, SendOption.Reliable);
        SabotageWriter.Write(reactorId);
        MessageExtensions.WriteNetObject(SabotageWriter, PlayerControl.LocalPlayer);
        SabotageWriter.Write((byte)128);
        AmongUsClient.Instance.FinishRpcImmediately(SabotageWriter);

        new LateTask(() =>
        { // Reactorサボを修理
            MessageWriter SabotageFixWriter = AmongUsClient.Instance.StartRpcImmediately(MapUtilities.CachedShipStatus.NetId, (byte)RpcCalls.UpdateSystem, SendOption.Reliable);
            SabotageFixWriter.Write(reactorId);
            MessageExtensions.WriteNetObject(SabotageFixWriter, PlayerControl.LocalPlayer);
            SabotageFixWriter.Write((byte)16);
            AmongUsClient.Instance.FinishRpcImmediately(SabotageFixWriter);
        }, 0.1f + duration, "Fix Desync Reactor");

        if (mapName == MapNames.Airship) //Airship用
            new LateTask(() =>
            { // Reactorサボを修理
                MessageWriter SabotageFixWriter = AmongUsClient.Instance.StartRpcImmediately(MapUtilities.CachedShipStatus.NetId, (byte)RpcCalls.UpdateSystem, SendOption.Reliable);
                SabotageFixWriter.Write(reactorId);
                MessageExtensions.WriteNetObject(SabotageFixWriter, PlayerControl.LocalPlayer);
                SabotageFixWriter.Write((byte)17);
                AmongUsClient.Instance.FinishRpcImmediately(SabotageFixWriter);
            }, 0.1f + duration, "Fix Desync Reactor 2");
    }
}