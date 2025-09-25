using UnityEngine;
using Hazel;
using AmongUs.GameOptions;

namespace SuperNewRoles.Mode;

/// <summary>
/// バトルロイヤルモード用のプレイヤー拡張メソッド
/// </summary>
public static class BattleRoyalPlayerExtensions
{
    /// <summary>
    /// 特定のプレイヤーから見て、特定のプレイヤーの名前を変更する関数
    /// </summary>
    /// <param name="targetPlayer">変更する名前のプレイヤー</param>
    /// <param name="newName">変更後の名前</param>
    /// <param name="seePlayer">変更後の名前を見れるプレイヤー（nullの場合は自分自身）</param>
    public static void RpcSetNamePrivate(this PlayerControl targetPlayer, string newName, PlayerControl seePlayer = null)
    {
        if (targetPlayer == null || newName == null || !AmongUsClient.Instance.AmHost) return;
        if (seePlayer == null) seePlayer = targetPlayer;

        if (seePlayer.PlayerId == PlayerControl.LocalPlayer.PlayerId)
        {
            targetPlayer.SetName(newName);
            return;
        }

        var clientId = AmongUsClient.Instance.GetClientIdFromCharacter(seePlayer);
        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(targetPlayer.NetId, (byte)RpcCalls.SetName, SendOption.Reliable, clientId);
        writer.Write(targetPlayer.Data.NetId);
        writer.Write(newName);
        AmongUsClient.Instance.FinishRpcImmediately(writer);
    }

    /// <summary>
    /// 特定のプレイヤーから見て、特定のプレイヤーの役職を変更する関数（Desync）
    /// </summary>
    /// <param name="targetPlayer">役職を変更するプレイヤー</param>
    /// <param name="role">変更後の役職</param>
    /// <param name="seePlayer">変更後の役職を見れるプレイヤー（nullの場合は自分自身）</param>
    public static void RpcSetRoleDesync(this PlayerControl targetPlayer, RoleTypes role, PlayerControl seePlayer = null)
    {
        Logger.Info($"{seePlayer.Data.PlayerName} : {targetPlayer.Data.PlayerName} => {role}を実行", "RpcSetRoleDesync");
        if (targetPlayer == null || !AmongUsClient.Instance.AmHost) return;
        if (seePlayer == null) seePlayer = targetPlayer;

        if (seePlayer.PlayerId == PlayerControl.LocalPlayer.PlayerId)
        {
            DestroyableSingleton<RoleManager>.Instance.SetRole(targetPlayer, role);
            return;
        }

        var clientId = AmongUsClient.Instance.GetClientIdFromCharacter(seePlayer);
        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(targetPlayer.NetId, (byte)RpcCalls.SetRole, SendOption.Reliable, clientId);
        writer.Write((ushort)role);
        writer.Write(false); // canOverride
        AmongUsClient.Instance.FinishRpcImmediately(writer);
    }
}
