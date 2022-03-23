using InnerNet;
using SuperNewRoles.Patch;
using SuperNewRoles.Roles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static SuperNewRoles.Patch.ShareGameVersion;

namespace SuperNewRoles
{
    public static class PlayerControlHepler
    {
        public static bool IsMod(this PlayerControl player)
        {
            if (player == null) return false;
            return IsMod(player.getClientId());
        }
        public static bool IsMod(this ClientData player)
        {
            if (player == null) return false;
            return IsMod(player.Id);
        }
        public static bool IsMod(this int player)
        {
            if (player == AmongUsClient.Instance.HostId && AmongUsClient.Instance.AmHost) return true;
            return GameStartManagerUpdatePatch.VersionPlayers.ContainsKey(player);
        }
        public static void clearAllTasks(this PlayerControl player)
        {
            if (player == null) return;
            for (int i = 0; i < player.myTasks.Count; i++)
            {
                PlayerTask playerTask = player.myTasks[i];
                playerTask.OnRemove();
                UnityEngine.Object.Destroy(playerTask.gameObject);
            }
            player.myTasks.Clear();

            if (player.Data != null && player.Data.Tasks != null)
                player.Data.Tasks.Clear();
        }
        public static void refreshRoleDescription(PlayerControl player)
        {
            if (player == null) return;

            List<Intro.IntroDate> infos = new List<Intro.IntroDate>() { Intro.IntroDate.GetIntroDate(player.getRole(),player) };

            var toRemove = new List<PlayerTask>();
            var aaa = false;
            var mytxt = "";
            foreach (PlayerTask t in player.myTasks)
            {
                var textTask = t.gameObject.GetComponent<ImportantTextTask>();
                if (textTask != null)
                {
                    if (aaa == false) {
                        mytxt = textTask.Text;
                    }
                    var info = infos.FirstOrDefault(x => textTask.Text.StartsWith(ModTranslation.getString(x.NameKey+"Name")));
                    if (info != null)
                        infos.Remove(info); // TextTask for this RoleInfo does not have to be added, as it already exists
                    else
                        toRemove.Add(t); // TextTask does not have a corresponding RoleInfo and will hence be deleted
                }
            }

            foreach (PlayerTask t in toRemove)
            {
                t.OnRemove();
                player.myTasks.Remove(t);
                UnityEngine.Object.Destroy(t.gameObject);
            }

            // Add TextTask for remaining RoleInfos
            foreach (Intro.IntroDate roleInfo in infos)
            {
                var task = new GameObject("RoleTask").AddComponent<ImportantTextTask>();
                task.transform.SetParent(player.transform, false);
                
                task.Text = CustomOption.CustomOptions.cs(roleInfo.color, $"{ModTranslation.getString(roleInfo.NameKey + "Name")}: {roleInfo.TitleDesc}");
                if (player.IsLovers())
                {
                    task.Text += "\n" + ModHelpers.cs(RoleClass.Lovers.color,ModTranslation.getString("LoversName")+": "+ string.Format(ModTranslation.getString("LoversIntro"), PlayerControl.LocalPlayer.GetOneSideLovers()?.Data?.PlayerName ?? ""));
                }
                /**
                if (player.IsQuarreled())
                {
                    task.Text += "\n" + ModHelpers.cs(RoleClass.Quarreled.color, String.Format(ModTranslation.getString("QuarreledIntro"), SetNamesClass.AllNames[PlayerControl.LocalPlayer.GetOneSideQuarreled().PlayerId]));
                }
                **/

                player.myTasks.Insert(0, task);
            }
        }
    }
}
