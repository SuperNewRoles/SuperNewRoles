using UnityEngine;

namespace SuperNewRoles.Roles;

internal class Mafia
{
    public static bool IsKillFlag()
    {
        if (RoleClass.Mafia.CachedIs) return true;
        foreach (CachedPlayer player in CachedPlayer.AllPlayers)
        {
            if (!player.PlayerControl.IsBot() && player.PlayerControl.IsAlive() && player.PlayerControl.IsImpostor() && !player.PlayerControl.IsRole(RoleId.Mafia) && !player.PlayerControl.IsRole(RoleId.Egoist))
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
                if (!FastDestroyableSingleton<HudManager>.Instance.KillButton.isActiveAndEnabled)
                {
                    FastDestroyableSingleton<HudManager>.Instance.KillButton.Show();
                }
            }
        }
        else
        {
            if (FastDestroyableSingleton<HudManager>.Instance.KillButton.isActiveAndEnabled)
            {
                FastDestroyableSingleton<HudManager>.Instance.KillButton.Hide();
            }
            if (!RoleClass.IsMeeting)
            {
                PlayerControl.LocalPlayer.SetKillTimer(PlayerControl.LocalPlayer.killTimer - Time.fixedDeltaTime);
            }
        }
    }
}