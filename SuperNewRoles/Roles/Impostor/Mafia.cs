
using UnityEngine;

namespace SuperNewRoles.Roles
{
    internal class Mafia
    {
        public static bool IsKillFlag()
        {
            if (RoleClass.Mafia.CachedIs) return true;
            foreach (CachedPlayer player in CachedPlayer.AllPlayers)
            {
                if (player.PlayerControl.IsPlayer() && player.PlayerControl.IsAlive() && player.PlayerControl.IsImpostor() && !player.PlayerControl.IsRole(RoleId.Mafia) && !player.PlayerControl.IsRole(RoleId.Egoist))
                {
                    return false;
                }
            }
            RoleClass.Mafia.CachedIs = true;
            return true;
        }
        public static void FixedUpdate()
        {
            if (IsKillFlag())
            {
                if (!RoleClass.IsMeeting)
                {
                    if (!HudManager.Instance.KillButton.isActiveAndEnabled)
                    {
                        HudManager.Instance.KillButton.Show();
                    }
                }
            }
            else
            {
                if (HudManager.Instance.KillButton.isActiveAndEnabled)
                {
                    HudManager.Instance.KillButton.Hide();
                }
                if (!RoleClass.IsMeeting)
                {
                    PlayerControl.LocalPlayer.SetKillTimer(PlayerControl.LocalPlayer.killTimer - Time.fixedDeltaTime);
                }
            }
        }
    }
}