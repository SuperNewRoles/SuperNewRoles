using System;
using SuperNewRoles.Events;
using SuperNewRoles.Events.PCEvents;

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
        switch (deathType)
        {
            case CustomDeathType.Exile:
                player.Player.Exiled();
                ExileEvent.Invoke(player);
                break;
            case CustomDeathType.FalseCharge:
                player.Player.Exiled();
                ExileEvent.Invoke(player);
                break;
            case CustomDeathType.Kill:
                if (source == null)
                    throw new Exception("Source is null");
                source.Player.MurderPlayer(player.Player, MurderResultFlags.Succeeded);
                break;
            case CustomDeathType.Suicide:
                player.Player.MurderPlayer(player.Player, MurderResultFlags.Succeeded);
                break;
            case CustomDeathType.WaveCannon:
                player.Player.MurderPlayer(player.Player, MurderResultFlags.Succeeded);
                player.FinalStatus = FinalStatus.WaveCannon;
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
    FalseCharge,
    Suicide,
    WaveCannon,
}
