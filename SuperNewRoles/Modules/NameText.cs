using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using SuperNewRoles.CustomOptions.Categories;
using SuperNewRoles.Events;
using SuperNewRoles.Events.PCEvents;
using SuperNewRoles.Roles;
using SuperNewRoles.Roles.CrewMate;
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

        playerInfoText.transform.localPosition = player.Player.cosmetics.nameText.transform.localPosition + Vector3.up * 0.2f;

        var meetingInfo = player.MeetingInfoText;
        if (MeetingHud.Instance != null && player.VoteArea != null)
        {
            if (meetingInfo == null)
            {
                meetingInfo = Object.Instantiate(player.VoteArea.NameText, player.VoteArea.NameText.transform.parent);
                meetingInfo.transform.localPosition += Vector3.down * 0.1f;
                meetingInfo.fontSize = 1.5f;
                meetingInfo.gameObject.name = "Info";
                player.MeetingInfoText = meetingInfo;
            }
            if (meetingInfo != null)
            {
                var playerName = player.VoteArea.NameText;
                playerName.transform.localPosition = new Vector3(0.3384f, 0.0311f + 0.0683f, -0.1f);
            }
        }
    }
    public static void UpdateNameInfo(ExPlayerControl player)
    {
        if (player == null || player.Player == null)
            return;
        Initialize(player);
        string TaskText = "";
        try
        {
            if (player.IsTaskTriggerRole())
            {
                var (complete, all) = player.GetAllTaskForShowProgress();
                TaskText += ModHelpers.Cs(Color.yellow, "(" + (ModHelpers.IsComms() ? "?" : complete.ToString()) + "/" + all.ToString() + ")");
            }
        }
        catch { }
        string playerInfoText = "";
        string meetingInfoText = "";
        string roleName = $"{ModHelpers.CsWithTranslation(player.roleBase.RoleColor, player.roleBase.Role.ToString())}";
        // ベスト冤罪ヤーは生きてる時は自覚できない
        if (player.Role == RoleId.BestFalseCharge && player.AmOwner && player.IsAlive())
        {
            roleName = $"{ModHelpers.CsWithTranslation(Crewmate.Instance.RoleColor, Crewmate.Instance.Role.ToString())}";
        }
        if (player.GhostRole != GhostRoleId.None && player.GhostRoleBase != null)
            roleName = $"{ModHelpers.CsWithTranslation(player.GhostRoleBase.RoleColor, player.GhostRole.ToString())} ({roleName}) ";
        if (player.ModifierRoleBases.Count > 0)
            roleName += " ";
        foreach (var modifier in player.ModifierRoleBases)
        {
            roleName = modifier.ModifierMark(player).Replace("{0}", roleName);
        }
        playerInfoText = roleName;
        playerInfoText += TaskText;
        meetingInfoText = playerInfoText.Trim();
        player.PlayerInfoText.text = playerInfoText;
        if (player.MeetingInfoText != null)
            player.MeetingInfoText.text = meetingInfoText;
        player.cosmetics.nameText.text = player.Player.CurrentOutfit.PlayerName;
        if (player.VoteArea != null)
            player.VoteArea.NameText.text = player.Player.Data.DefaultOutfit.PlayerName;
        bool visiable = ExPlayerControl.LocalPlayer.PlayerId == player.PlayerId ||
                        (ExPlayerControl.LocalPlayer.IsDead() && !GameSettingOptions.HideGhostRoles);
        if (visiable)
        {
            player.Data.Role.NameColor = player.roleBase.RoleColor;
            SetNameTextColor(player, player.roleBase.RoleColor);
        }
        else if (ExPlayerControl.LocalPlayer.IsImpostor() && player.IsImpostor())
        {
            player.Data.Role.NameColor = Palette.ImpostorRed;
            SetNameTextColor(player, Palette.ImpostorRed);
        }
        else
        {
            player.Data.Role.NameColor = Color.white;
            SetNameTextColor(player, Color.white);
        }
        UpdateVisiable(player);
        NameTextUpdateEvent.Invoke(player, visiable);
        NameTextUpdateVisiableEvent.Invoke(player, visiable);
    }
    public static void SetCustomTaskCount(ExPlayerControl player, int completed, int total, bool showOnMeeting = false, bool showCompletedOnComms = false)
    {
        string text = ModHelpers.Cs(Color.yellow, "(" + (showOnMeeting ? (ModHelpers.IsComms() ? "?" : completed.ToString()) : (showCompletedOnComms ? completed.ToString() : "?")) + "/" + total.ToString() + ")");
        if (player.PlayerInfoText != null)
            player.PlayerInfoText.text += text;
        if (player.MeetingInfoText != null && showOnMeeting)
            player.MeetingInfoText.text += text;
    }
    public static void SetNameTextColor(ExPlayerControl player, Color color)
    {
        Logger.Info($"SetNameTextColor: {player.Data.PlayerName} {color}");
        player.Player.cosmetics.nameText.color = color;
        if (player.VoteArea != null)
            player.VoteArea.NameText.color = color;
    }
    public static void AddNameText(ExPlayerControl player, string text, bool checkContains = false)
    {
        if (checkContains)
        {
            if (player.Player.cosmetics.nameText.text.Contains(text))
                return;
        }
        player.Player.cosmetics.nameText.text += text;
        if (player.VoteArea != null && player.VoteArea.PlayerIcon?.cosmetics?.nameText != null)
            player.VoteArea.NameText.text += text;
    }
    public static void RegisterNameTextUpdateEvent()
    {
        TaskCompleteEvent.Instance.AddListener(new(x => UpdateNameInfo(x.player)));
        MurderEvent.Instance.AddListener(new(x =>
        {
            UpdateNameInfo(x.killer);
            UpdateNameInfo(x.target);
        }));
        DieEvent.Instance.AddListener(x => { if (x.player?.PlayerId == ExPlayerControl.LocalPlayer?.PlayerId) new LateTask(() => UpdateAllNameInfo(), 0.5f); });
        WrapUpEvent.Instance.AddListener(x => UpdateAllNameInfo());
        MeetingStartEvent.Instance.AddListener(x => UpdateAllNameInfo());
        FixedUpdateEvent.Instance.AddListener(UpdateAllVisiable);
        _lastDead = new();
    }
    [HarmonyPatch(typeof(HudOverrideSystemType), nameof(HudOverrideSystemType.UpdateSystem))]
    public static class HudOverrideSystemTypePatch
    {
        private static bool _lastActive = false;
        public static void Prefix(HudOverrideSystemType __instance)
        {
            _lastActive = __instance.IsActive;
        }
        public static void Postfix(HudOverrideSystemType __instance)
        {
            if (__instance.IsActive && !_lastActive)
                UpdateAllNameInfo();
        }
    }
    private static Dictionary<ExPlayerControl, bool> _lastDead = new();
    private static void UpdateAllVisiable()
    {
        foreach (var player in ExPlayerControl.ExPlayerControls)
        {
            UpdateVisiable(player);
            NameTextUpdateVisiableEvent.Invoke(player, player.Player.Visible);
        }
    }
    public static void UpdateVisiable(ExPlayerControl player)
    {
        if (player == null || player.Player == null)
            return;
        bool visiable = player.Player.Visible &&
                        (ExPlayerControl.LocalPlayer.PlayerId == player.PlayerId ||
                        (ExPlayerControl.LocalPlayer.IsDead() &&
                            (!GameSettingOptions.HideGhostRoles || (ExPlayerControl.LocalPlayer.IsImpostor() && GameSettingOptions.ShowGhostRolesToImpostor)))
                        );
        UpdateVisiable(player, visiable);
    }
    public static void UpdateVisiable(ExPlayerControl player, bool visiable)
    {
        if (player == null || player.Player == null)
            return;
        if (!player.Player.Visible)
            visiable = false;
        if (visiable && player.PlayerInfoText == null)
            UpdateNameInfo(player);
        player.PlayerInfoText.gameObject.SetActive(visiable);
        if (player.MeetingInfoText != null)
            player.MeetingInfoText.gameObject.SetActive(visiable);
    }
    public static void UpdateAllNameInfo()
    {
        foreach (var player in ExPlayerControl.ExPlayerControls)
            UpdateNameInfo(player);
    }
}
