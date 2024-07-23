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
            bool needNotificationUpdate = false;
            for (int i = Notifications.Count - 1; i >= 0; i--)
            {
                var notification = Notifications[i];
                float timer = notification.Item2 - Time.fixedDeltaTime;
                Logger.Info(timer.ToString());
                if (timer <= 0)
                {
                    Logger.Info("REMOVE");
                    Notifications.RemoveAt(i);
                    needNotificationUpdate = true;
                }
                else
                {
                    Notifications[i] = (notification.Item1, timer);
                }
            }
            if (needNotificationUpdate) OnNotificationUpdate();
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
                foreach (PlayerControl seeplayer in CachedPlayer.AllPlayers.AsSpan())
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
            foreach (PlayerControl player in CachedPlayer.AllPlayers.AsSpan())
            {
                Logger.Info(player.GetDefaultName());
                UpdateName(player, SelfOnly);
            }
        }
        public static (string, string) GetName(PlayerControl player)
        {
            string name = $"<size=75%>{ModHelpers.Cs(CustomRoles.GetRoleColor(player.GetRole(), IsImpostorReturn: true), CustomRoles.GetRoleName(player.GetRole(), IsImpostorReturn: true))}</size>\n{player.GetDefaultName()}\n\n";
            string selfname = $"\n\n\n\n{name}\n\n\n\n";
            foreach (var notifi in Notifications.AsSpan())
            {
                Logger.Info("SETNOTIF:" + notifi.Item1);
                selfname = $"<size=200%>{notifi.Item1}</size>{selfname}\n";
            }
            return (name, selfname);
        }
    }
}