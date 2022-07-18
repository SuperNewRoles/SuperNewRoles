using UnityEngine;

namespace SuperNewRoles.Mode.NotImpostorCheck
{
    class NameSet
    {
        public static void Postfix()
        {
            int LocalId = CachedPlayer.LocalPlayer.PlayerId;
            if (Main.Impostors.Contains(LocalId))
            {
                foreach (PlayerControl p in CachedPlayer.AllPlayers)
                {
                    if (p.IsAlive() && p.PlayerId != LocalId)
                    {
                        p.NameText().color = Color.white;
                    }
                }
            }
        }
    }
}