using System;
using HarmonyLib;
using SuperNewRoles.Modules.Events.Bases;
using AmongUs.GameOptions;

namespace SuperNewRoles.Events;

public class DisconnectEventData : IEventData
{
    public PlayerControl disconnectedPlayer { get; }
    public DisconnectReasons reason { get; }
    public DisconnectEventData(PlayerControl disconnectedPlayer, DisconnectReasons reason)
    {
        this.disconnectedPlayer = disconnectedPlayer;
        this.reason = reason;
    }
}

public class DisconnectEvent : EventTargetBase<DisconnectEvent, DisconnectEventData>
{
    public static void Invoke(PlayerControl disconnectedPlayer, DisconnectReasons reason)
    {
        var data = new DisconnectEventData(disconnectedPlayer, reason);
        Instance.Awake(data);
    }
}

[HarmonyPatch(typeof(GameData), nameof(GameData.HandleDisconnect), [typeof(PlayerControl), typeof(DisconnectReasons)])]
public static class DisconnectPatch
{
    public static void Postfix(PlayerControl player, DisconnectReasons reason)
    {
        if (player == null) return;
        DisconnectEvent.Invoke(player, reason);
    }
}