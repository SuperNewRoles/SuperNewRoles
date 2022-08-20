using System.Collections.Generic;
using System.Linq;
using InnerNet;
using SuperNewRoles.CustomRPC;
using SuperNewRoles.Intro;
using SuperNewRoles.Roles;
using UnityEngine;
using static SuperNewRoles.Patch.ShareGameVersion;

namespace SuperNewRoles
{
    public static class PlayerControlHepler
    {
        public static bool IsMod(this PlayerControl player)
        {
            return player != null && IsMod(player.GetClientId());
        }
        public static bool IsMod(this ClientData player)
        {
            return player != null && IsMod(player.Id);
        }
        public static bool IsMod(this int player)
        {
            return (player == AmongUsClient.Instance.HostId && AmongUsClient.Instance.AmHost)
|| GameStartManagerUpdatePatch.VersionPlayers.ContainsKey(player);
        }
        public static void ClearAllTasks(this PlayerControl player)
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
        public static void RefreshRoleDescription(PlayerControl player)
        {
            if (player == null) return;

            List<Intro.IntroDate> infos = new() { Intro.IntroDate.GetIntroDate(player.GetRole(), player) };

            var toRemove = new List<PlayerTask>();
            var aaa = false;
            var mytxt = "";
            foreach (PlayerTask t in player.myTasks)
            {
                var textTask = t.gameObject.GetComponent<ImportantTextTask>();
                if (textTask != null)
                {
                    if (aaa == false)
                    {
                        mytxt = textTask.Text;
                    }
                    var info = infos.FirstOrDefault(x => textTask.Text.StartsWith(ModTranslation.GetString(x.NameKey + "Name")));
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

                task.Text = CustomOption.CustomOptions.Cs(roleInfo.color, $"{ModTranslation.GetString(roleInfo.NameKey + "Name")}: {roleInfo.TitleDesc}");
                if (player.IsLovers())
                {
                    task.Text += "\n" + ModHelpers.Cs(RoleClass.Lovers.color, ModTranslation.GetString("LoversName") + ": " + string.Format(ModTranslation.GetString("LoversIntro"), PlayerControl.LocalPlayer.GetOneSideLovers()?.Data?.PlayerName ?? ""));
                }
                if (!player.IsGhostRole(RoleId.DefaultRole))
                {
                    var GhostRoleInfo = IntroDate.GetIntroDate(player.GetGhostRole(), player);
                    task.Text += "\n" + CustomOption.CustomOptions.Cs(GhostRoleInfo.color, $"{ModTranslation.GetString(GhostRoleInfo.NameKey + "Name")}: {GhostRoleInfo.TitleDesc}");
                }
                /**
                if (player.IsQuarreled())
                {
                    task.Text += "\n" + ModHelpers.Cs(RoleClass.Quarreled.color, String.Format(ModTranslation.GetString("QuarreledIntro"), SetNamesClass.AllNames[PlayerControl.LocalPlayer.GetOneSideQuarreled().PlayerId]));
                }
                **/

                player.myTasks.Insert(0, task);
            }
        }
    }
}