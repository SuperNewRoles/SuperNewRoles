using System;
using System.Collections.Generic;

namespace SuperNewRoles.Modules;

public class DeadPlayer
{
    public static List<DeadPlayer> deadPlayers = new();
    public static Dictionary<byte, (DateTime, PlayerControl)> ActualDeathTime;
    public PlayerControl player;
    public byte playerId;
    public DateTime timeOfDeath;
    public DeathReason deathReason;
    public PlayerControl killerIfExisting;
    public byte killerIfExistingId;

    public DeadPlayer(PlayerControl player, byte playerId, DateTime timeOfDeath, DeathReason deathReason, PlayerControl killerIfExisting)
    {
        this.player = player;
        this.playerId = playerId;
        this.timeOfDeath = timeOfDeath;
        this.deathReason = deathReason;
        this.killerIfExisting = killerIfExisting;
        if (killerIfExisting != null) killerIfExistingId = killerIfExisting.PlayerId;
    }

    internal static void ClearAndReloads()
    {
        deadPlayers = new();
        ActualDeathTime = new();
    }
}