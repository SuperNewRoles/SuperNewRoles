using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNewRoles.Patch
{
    public class DeadPlayer
    {
        public static List<DeadPlayer> deadPlayers = new List<DeadPlayer>();
        public PlayerControl player;
        public DateTime timeOfDeath;
        public DeathReason deathReason;
        public PlayerControl killerIfExisting;

        public DeadPlayer(PlayerControl player, DateTime timeOfDeath, DeathReason deathReason, PlayerControl killerIfExisting)
        {
            this.player = player;
            this.timeOfDeath = timeOfDeath;
            this.deathReason = deathReason;
            this.killerIfExisting = killerIfExisting;
        }
    }
}
