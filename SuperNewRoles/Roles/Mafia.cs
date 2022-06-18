using SuperNewRoles.CustomRPC;
using SuperNewRoles.Mode;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SuperNewRoles.Roles
{
    internal class Mafia
    {
        public static bool IsKillFlag()
        {
            if (RoleClass.Mafia.CachedIs) return true;
            SuperNewRolesPlugin.Logger.LogInfo("IsKillMafia!!!");
            foreach (CachedPlayer player in CachedPlayer.AllPlayers)
            {
                SuperNewRolesPlugin.Logger.LogInfo(player.Data.PlayerName + " => " + player.PlayerControl.isAlive());
                SuperNewRolesPlugin.Logger.LogInfo(player.Data.PlayerName+" => "+player.PlayerControl.IsPlayer());
                SuperNewRolesPlugin.Logger.LogInfo(player.Data.PlayerName + " => " + player.PlayerControl.isImpostor());
                SuperNewRolesPlugin.Logger.LogInfo(!player.PlayerControl.isRole(RoleId.Mafia) && !player.PlayerControl.isRole(RoleId.Egoist));
                if (player.PlayerControl.IsPlayer() && player.PlayerControl.isAlive() && player.PlayerControl.isImpostor() && !player.PlayerControl.isRole(RoleId.Mafia) && !player.PlayerControl.isRole(RoleId.Egoist))
                {
                    SuperNewRolesPlugin.Logger.LogInfo("ƒAƒEƒg:"+player.Data.PlayerName);
                    return false;
                }
            }
            SuperNewRolesPlugin.Logger.LogInfo("‚Â‚¤‚©");
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
                SuperNewRolesPlugin.Logger.LogInfo(PlayerControl.LocalPlayer.killTimer);
            }
        }
    }
}