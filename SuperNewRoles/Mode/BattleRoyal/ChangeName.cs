using System;
using System.Collections.Generic;
using System.Text;
using SuperNewRoles.Helpers;
using SuperNewRoles.Mode.SuperHostRoles;
using SuperNewRoles.Roles.RoleBases;
using UnityEngine;

namespace SuperNewRoles.Mode.BattleRoyal
{
    public static class ChangeName
    {
        public static List<(string, float)> Notifications;
        public static void SetNotification(string text, float time)
        {
            Notifications.Add((text, time));
        }
        public static void FixedUpdate()
        {
            foreach (var notification in Notifications.ToArray())
            {
                int index = Notifications.IndexOf(notification);
                float timer = notification.Item2 - Time.fixedDeltaTime;
                Logger.Info(timer.ToString());
                if (timer <= 0)
                {
                    Logger.Info("REMOVE");
                    Notifications.RemoveAt(index);
                    OnNotificationUpdate();
                    continue;
                }
                Notifications[index] = (notification.Item1, timer);
            }
        }
        public static void OnNotificationUpdate()
        {
            UpdateName(true);
        }
        public static void ClearAll()
        {
            Notifications = new();
        }
        public static void UpdateName(PlayerControl player, bool SelfOnly = false)
        {
            string name, selfname;
            (name, selfname) = GetName(player);
            if (!SelfOnly)
            {
                BattleTeam team = BattleTeam.GetTeam(player);
                if (team is null) return;
                foreach (PlayerControl seeplayer in PlayerControl.AllPlayerControls)
                {
                    if (seeplayer.IsBot()) continue;
                    if (seeplayer.PlayerId == player.PlayerId) continue;
                    if (!team.IsTeam(seeplayer) && seeplayer.IsAlive()) continue;
                    player.RpcSetNamePrivate(name, seeplayer);
                }
            }
            player.RpcSetNamePrivate(selfname, player);
        }
        public static void UpdateName(bool SelfOnly = false)
        {
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                Logger.Info(player.GetDefaultName());
            }
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                UpdateName(player, SelfOnly);
            }
        }
        public static (string, string) GetName(PlayerControl player)
        {
            string name = player.GetDefaultName();
            name = "<size=75%>" + ModHelpers.Cs(CustomRoles.GetRoleColor(player.GetRole(), IsImpostorReturn: true), CustomRoles.GetRoleName(player.GetRole(), IsImpostorReturn: true)) + "</size>\n" + name + "\n\n";
            string selfname = name;
            selfname = "\n\n\n\n" + selfname + "\n\n\n\n";
            foreach (var notifi in Notifications)
            {
                Logger.Info("SETNOTIF:" + notifi.Item1);
                selfname = "<size=200%>" + notifi.Item1 + "</size>" + selfname + "\n";
            }
            return (name, selfname);
        }
    }
}