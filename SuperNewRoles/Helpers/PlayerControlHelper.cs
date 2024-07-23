using System.Collections.Generic;
using System.Linq;
using InnerNet;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles;
using SuperNewRoles.Roles.RoleBases;
using UnityEngine;
using static SuperNewRoles.Patches.ShareGameVersion;

namespace SuperNewRoles;

public static class PlayerControlHelper
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
            Object.Destroy(playerTask.gameObject);
        }
        player.myTasks.Clear();

        if (player.Data != null && player.Data.Tasks != null)
            player.Data.Tasks.Clear();
    }
    public static void RefreshRoleDescription(PlayerControl player)
    {
        if (player == null) return;
        Logger.Info($"Set Role Description. player : {player.name}", "RefreshRoleDescription");

        RoleId playerRole = player.GetRole();
        if (playerRole == RoleId.Bestfalsecharge && player.IsAlive())
        {
            playerRole = RoleId.DefaultRole;
        }

        for (int i = player.myTasks.Count - 1; i >= 0; i--)
        {
            var Task = player.myTasks[i];
            var textTask = Task.gameObject.GetComponent<ImportantTextTask>();
            if (textTask == null) continue;
            if (textTask.Text.StartsWith(CustomRoles.GetRoleName(player)))
                playerRole = RoleId.None; // TextTask for this RoleInfo does not have to be added, as it already exists
            else
            {
                player.myTasks.RemoveAt(i); // TextTask does not have a corresponding RoleInfo and will hence be deleted
                Object.Destroy(Task.gameObject);
            }
        }

        if (playerRole == RoleId.None) return;

        Logger.Info($"Set Role Description. infos : {playerRole}", "RefreshRoleDescription");
        // Add TextTask for remaining RoleInfos
        var task = new GameObject("RoleTask").AddComponent<ImportantTextTask>();
        task.transform.SetParent(player.transform, false);

        task.Text = ModHelpers.Cs(CustomRoles.GetRoleColor(playerRole), $"{CustomRoles.GetRoleName(playerRole)}: {CustomRoles.GetRoleIntro(playerRole)}");
        if (player.IsLovers() || player.IsFakeLovers())
        {
            task.Text += $"\n{ModHelpers.Cs(RoleClass.Lovers.color, $"{ModTranslation.GetString("LoversName")}: {string.Format(ModTranslation.GetString("LoversIntro"), PlayerControl.LocalPlayer.GetOneSideLovers()?.Data?.PlayerName ?? "")}")}";
        }
        if (!player.IsGhostRole(RoleId.DefaultRole))
        {
            task.Text += $"\n{ModHelpers.Cs(CustomRoles.GetRoleColor(player.GetGhostRole(), player), $"{CustomRoles.GetRoleName(player.GetGhostRole(), player)}: {CustomRoles.GetRoleIntro(player.GetGhostRole(), player)}")}";
        }

        player.myTasks.Insert(0, task);
    }
}