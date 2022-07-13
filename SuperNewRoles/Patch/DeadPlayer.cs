using System;
using System.Collections.Generic;

namespace SuperNewRoles.Patch
{
    public class DeadPlayer
    {
        public static List<DeadPlayer> deadPlayers = new();
        public PlayerControl player;
        public DateTime timeOfDeath;
        public DeathReason deathReason;
        public PlayerControl killerIfExisting;
        public byte killerIfExistingId;

        public DeadPlayer(PlayerControl player, DateTime timeOfDeath, DeathReason deathReason, PlayerControl killerIfExisting)
        {
            this.player = player;
            this.timeOfDeath = timeOfDeath;
            this.deathReason = deathReason;
            this.killerIfExisting = killerIfExisting;
            if (killerIfExisting != null) this.killerIfExistingId = killerIfExisting.PlayerId;
        }
    }
}
