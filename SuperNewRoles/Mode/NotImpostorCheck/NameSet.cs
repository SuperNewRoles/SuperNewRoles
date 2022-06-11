using SuperNewRoles.Mode.SuperHostRoles;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SuperNewRoles.Mode.NotImpostorCheck
{
    class NameSet
    {
        public static void Postfix()
        {
            int LocalId = CachedPlayer.LocalPlayer.PlayerId;
            if (main.Impostors.Contains(LocalId)) {
                foreach (PlayerControl p in CachedPlayer.AllPlayers)
                {
                    if (p.isAlive() && p.PlayerId != LocalId)
                    {
                        p.nameText.color = Color.white;
                    }
                }
            }
        }
    }
}
