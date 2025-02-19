using System.Linq;
using SuperNewRoles.Events;
using SuperNewRoles.Events.PCEvents;
using UnityEngine;

namespace SuperNewRoles.Modules;

public static class NameText
{
    public static void Initialize(ExPlayerControl player)
    {
        bool commsActive = ModHelpers.IsComms();
        var playerInfoText = player.PlayerInfoText;
        if (playerInfoText == null)
        {
            var nameText = player.Player.cosmetics.nameText;
            if (nameText == null)
            {
                Logger.Error($"nameText is null: {player.Player.name}");
            }
            playerInfoText = UnityEngine.Object.Instantiate(nameText, nameText.transform.parent);
            playerInfoText.fontSize *= 0.75f;
            playerInfoText.gameObject.name = "Info";
            player.PlayerInfoText = playerInfoText;
        }

        // Set the position every time bc it sometimes ends up in the wrong place due to camoflauge
        playerInfoText.transform.localPosition = player.Player.cosmetics.nameText.transform.localPosition + Vector3.up * 0.2f;

        var meetingInfo = player.MeetingInfoText;
        if (MeetingHud.Instance != null)
        {
            var playerVoteArea = MeetingHud.Instance.playerStates.FirstOrDefault(x => x.TargetPlayerId == player.PlayerId);
            if (meetingInfo == null && playerVoteArea != null)
            {
                meetingInfo = UnityEngine.Object.Instantiate(playerVoteArea.NameText, playerVoteArea.NameText.transform.parent);
                meetingInfo.transform.localPosition += Vector3.down * 0.1f;
                meetingInfo.fontSize = 1.5f;
                meetingInfo.gameObject.name = "Info";
                player.MeetingInfoText = meetingInfo;
            }
            // Set player name higher to align in middle
            if (meetingInfo != null && playerVoteArea != null)
            {
                var playerName = playerVoteArea.NameText;
                playerName.transform.localPosition = new Vector3(0.3384f, 0.0311f + 0.0683f, -0.1f);
            }
        }
    }
    public static (int completed, int total) TaskCompletedData(NetworkedPlayerInfo playerInfo)
    {
        if (playerInfo?.Tasks == null)
            return (-1, -1);

        int TotalTasks = 0;
        int CompletedTasks = 0;

        for (int j = 0; j < playerInfo.Tasks.Count; j++)
        {
            if (playerInfo.Tasks[j] == null)
                continue;
            TotalTasks++;
            if (playerInfo.Tasks[j].Complete)
            {
                CompletedTasks++;
            }
        }
        return (CompletedTasks, TotalTasks);
    }
    public static void UpdateNameInfo(ExPlayerControl player)
    {
        Initialize(player);
        string TaskText = "";
        try
        {
            if (player.IsTaskTriggerRole())
            {
                var (complete, all) = TaskCompletedData(player.Data);
                TaskText += ModHelpers.Cs(Color.yellow, "(" + (ModHelpers.IsComms() ? "?" : complete.ToString()) + "/" + all.ToString() + ")");
            }
        }
        catch { }
        string playerInfoText = "";
        string meetingInfoText = "";
        playerInfoText = $"{ModHelpers.Cs(player.roleBase.RoleColor, ModTranslation.GetString(player.Role.ToString()))}";
        /*if (GhostRoleNames != "")
        {
            playerInfoText = $"{CustomOptionHolder.Cs((Color)GhostRoleColor, GhostRoleNames)}({playerInfoText})";
        }

        if (attributeRoles.Count != 0)
        {
            foreach (var kvp in attributeRoles)
            {
                if (!kvp.Value.Item2) continue;
                playerInfoText += $" + {CustomOptionHolder.Cs(kvp.Value.Item1, kvp.Key)}";
            }
        }*/
        playerInfoText += TaskText;
        meetingInfoText = playerInfoText.Trim();
        player.PlayerInfoText.text = playerInfoText;
        bool visiable = ExPlayerControl.LocalPlayer.PlayerId == player.PlayerId || ExPlayerControl.LocalPlayer.IsDead();
        player.PlayerInfoText.gameObject.SetActive(visiable);
        if (player.MeetingInfoText != null)
            player.MeetingInfoText.text = MeetingHud.Instance.state == MeetingHud.VoteStates.Results ? "" : meetingInfoText;
    }
    public static void RegisterNameTextUpdateEvent()
    {
        TaskCompleteEvent.Instance.AddListener(new(x => UpdateNameInfo(x.player)));
        MurderEvent.Instance.AddListener(new(x =>
        {
            if (x.target?.PlayerId == ExPlayerControl.LocalPlayer?.PlayerId)
            {
                UpdateAllNameInfo();
            }
            UpdateNameInfo(x.killer);
            UpdateNameInfo(x.target);
        }));
        WrapUpEvent.Instance.AddListener(new(x => UpdateAllNameInfo()));
        MeetingStartEvent.Instance.AddListener(new(x => UpdateAllNameInfo()));
    }
    private static void UpdateAllNameInfo()
    {
        foreach (var player in ExPlayerControl.ExPlayerControls)
            UpdateNameInfo(player);
    }
}
