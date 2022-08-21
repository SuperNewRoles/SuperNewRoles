using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SuperNewRoles.Roles.Impostor
{
    public static class Slugger
    {
        //ここにコードを書きこんでください
        public static List<PlayerControl> SetTarget()
        {
            List<PlayerControl> Targets = new();
            foreach (CachedPlayer player in CachedPlayer.AllPlayers)
            {
                if (player.IsDead()) continue;
                if (player.PlayerId == CachedPlayer.LocalPlayer.PlayerId) continue;
                if (Vector2.Distance(CachedPlayer.LocalPlayer.transform.position, player.transform.position) > 1.5f) continue;
                Targets.Add(player);
            }
            return Targets;
        }
    }
}