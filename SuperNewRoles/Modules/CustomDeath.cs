using System;
using System.Linq;
using SuperNewRoles.Events;
using SuperNewRoles.Events.PCEvents;
using UnityEngine;
using UnityEngine.Rendering;

namespace SuperNewRoles.Modules;

public static class CustomDeathExtensions
{
    [CustomRPC]
    public static void RpcCustomDeath(this ExPlayerControl player, CustomDeathType deathType)
    {
        player.CustomDeath(deathType);
    }
    [CustomRPC]
    public static void RpcCustomDeath(this ExPlayerControl source, ExPlayerControl target, CustomDeathType deathType)
    {
        CustomDeath(target, deathType, source);
    }
    public static void CustomDeath(this ExPlayerControl player, CustomDeathType deathType, ExPlayerControl source = null)
    {
        Logger.Info($"CustomDeath: {deathType}, Source: {source?.Player.Data.PlayerName ?? "NoPlayer"}, Target: {player.Player.Data.PlayerName}");
        switch (deathType)
        {
            case CustomDeathType.Exile:
                player.Player.Exiled();
                ExileEvent.Invoke(player);
                FinalStatusManager.SetFinalStatus(player, FinalStatus.Exiled);
                break;
            case CustomDeathType.FalseCharge:
                player.Player.Exiled();
                ExileEvent.Invoke(player);
                FinalStatusManager.SetFinalStatus(player, FinalStatus.FalseCharge);
                break;
            case CustomDeathType.Revange:
                player.Player.Exiled();
                ExileEvent.Invoke(player);
                FinalStatusManager.SetFinalStatus(player, FinalStatus.Revange);
                break;
            case CustomDeathType.Kill:
                if (!TryKillEvent.Invoke(source, ref player).RefSuccess)
                    break;
                if (source == null)
                    throw new Exception("Source is null");
                source.Player.MurderPlayer(player.Player, MurderResultFlags.Succeeded);
                FinalStatusManager.SetFinalStatus(player, FinalStatus.Kill);
                break;
            case CustomDeathType.KilLWithoutDeadbodyAndTeleport:
                if (!TryKillEvent.Invoke(source, ref player).RefSuccess)
                    break;
                if (source == null)
                    throw new Exception("Source is null");
                Vector2 pos = source.Player.GetTruePosition();
                source.Player.MurderPlayer(player.Player, MurderResultFlags.Succeeded);
                FinalStatusManager.SetFinalStatus(source, FinalStatus.Kill);
                new LateTask(() =>
                {
                    DeadBody deadBody = GameObject.FindObjectsOfType<DeadBody>().FirstOrDefault(x => x.ParentId == source.PlayerId);
                    if (deadBody != null)
                        GameObject.Destroy(deadBody.gameObject);
                    source.Player.NetTransform.SnapTo(pos);
                    source.Player.MyPhysics.body.velocity = Vector2.zero;
                }, 0.1f);
                break;
            case CustomDeathType.Suicide:
                player.Player.MurderPlayer(player.Player, MurderResultFlags.Succeeded);
                FinalStatusManager.SetFinalStatus(player, FinalStatus.Suicide);
                break;
            case CustomDeathType.LoversSuicide:
                player.Player.MurderPlayer(player.Player, MurderResultFlags.Succeeded);
                FinalStatusManager.SetFinalStatus(player, FinalStatus.LoversSuicide);
                break;
            case CustomDeathType.LoversSuicideMurderWithoutDeadbody:
                player.Player.Exiled();
                MurderEvent.Invoke(source, player, MurderResultFlags.Succeeded);
                FinalStatusManager.SetFinalStatus(player, FinalStatus.LoversSuicide);
                break;
            case CustomDeathType.WaveCannon:
                player.Player.MurderPlayer(player.Player, MurderResultFlags.Succeeded);
                FinalStatusManager.SetFinalStatus(player, FinalStatus.WaveCannon);
                break;
            case CustomDeathType.Samurai:
                if (!TryKillEvent.Invoke(source, ref player).RefSuccess)
                    break;
                var pos2 = player.Player.GetTruePosition();
                player.Player.MurderPlayer(player.Player, MurderResultFlags.Succeeded);
                player.Player.NetTransform.SnapTo(pos2);
                player.Player.MyPhysics.body.velocity = Vector2.zero;
                FinalStatusManager.SetFinalStatus(player, FinalStatus.Samurai);
                break;
            case CustomDeathType.BombBySelfBomb:
                if (!TryKillEvent.Invoke(source, ref player).RefSuccess)
                    break;
                player.Player.MurderPlayer(player.Player, MurderResultFlags.Succeeded);
                FinalStatusManager.SetFinalStatus(player, FinalStatus.BombBySelfBomb);
                break;
            case CustomDeathType.SelfBomb:
                player.Player.MurderPlayer(player.Player, MurderResultFlags.Succeeded);
                FinalStatusManager.SetFinalStatus(player, FinalStatus.SelfBomb);
                break;
            case CustomDeathType.Tuna:
                player.Player.MurderPlayer(player.Player, MurderResultFlags.Succeeded);
                FinalStatusManager.SetFinalStatus(player, FinalStatus.Tuna);
                break;
            case CustomDeathType.Push:
                if (!TryKillEvent.Invoke(source, ref player).RefSuccess)
                    break;
                player.Player.Exiled();
                FinalStatusManager.SetFinalStatus(player, FinalStatus.Push);
                break;
            case CustomDeathType.Ignite:
                player.Player.Exiled();
                FinalStatusManager.SetFinalStatus(player, FinalStatus.Ignite);
                break;
            case CustomDeathType.FalseCharges:
                if (!TryKillEvent.Invoke(source, ref player).RefSuccess)
                    break;
                source.Player.MurderPlayer(player.Player, MurderResultFlags.Succeeded);
                FinalStatusManager.SetFinalStatus(player, FinalStatus.FalseCharges);
                break;
            case CustomDeathType.LaunchByRocket:
                if (!TryKillEvent.Invoke(source, ref player).RefSuccess)
                    break;
                player.Player.Exiled();
                FinalStatusManager.SetFinalStatus(player, FinalStatus.LaunchByRocket);
                break;
            case CustomDeathType.VampireKill:
                if (!TryKillEvent.Invoke(source, ref player).RefSuccess)
                    break;
                var pos3 = source.Player.GetTruePosition();
                source.Player.MurderPlayer(player.Player, MurderResultFlags.Succeeded);
                FinalStatusManager.SetFinalStatus(player, FinalStatus.VampireKill);
                source.Player.NetTransform.SnapTo(pos3);
                break;
            case CustomDeathType.VampireWithDead:
                player.Player.MurderPlayer(player.Player, MurderResultFlags.Succeeded);
                FinalStatusManager.SetFinalStatus(player, FinalStatus.VampireWithDead);
                break;
            case CustomDeathType.PenguinAfterMeeting:
                if (!TryKillEvent.Invoke(source, ref player).RefSuccess)
                    break;
                player.Player.Exiled();
                MurderEvent.Invoke(source, player, MurderResultFlags.Succeeded);
                FinalStatusManager.SetFinalStatus(player, FinalStatus.Kill);
                break;
            case CustomDeathType.SuicideSecrets:
                player.Player.Exiled();
                FinalStatusManager.SetFinalStatus(player, FinalStatus.Suicide);
                break;
            default:
                throw new Exception($"Invalid death type: {deathType}");
        }
    }
    public static void Register()
    {
        WrapUpEvent.Instance.AddListener(x =>
        {
            if (x.exiled == null)
                return;
            ExPlayerControl exPlayer = (ExPlayerControl)x.exiled;
            exPlayer.CustomDeath(CustomDeathType.Exile);
        });
    }
}
public enum CustomDeathType
{
    Exile,
    Kill,
    Revange,
    FalseCharge,
    Suicide,
    WaveCannon,
    Samurai,
    BombBySelfBomb,
    SelfBomb,
    Tuna,
    Push,
    Ignite,
    FalseCharges,
    LoversSuicide,
    LoversSuicideMurderWithoutDeadbody,
    LaunchByRocket,
    VampireKill,
    VampireWithDead,
    KilLWithoutDeadbodyAndTeleport,
    PenguinAfterMeeting,
    SuicideSecrets,
}
