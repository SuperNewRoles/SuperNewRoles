using System.Collections.Generic;
using HarmonyLib;
using SuperNewRoles.CustomOptions.Categories;
using SuperNewRoles.Events;
using SuperNewRoles.Events.PCEvents;
using SuperNewRoles.Roles;
using SuperNewRoles.Roles.Ability;
using SuperNewRoles.Roles.CrewMate;
using SuperNewRoles.Roles.Impostor;
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
    private static bool GetRoleInfoVisibility(ExPlayerControl player, HideRoleOnGhostAbility localPlayerHrg)
    {
        if (player == null || player.Player == null || !player.Player.Visible)
        {
            return false;
        }

        if (ExPlayerControl.LocalPlayer.PlayerId == player.PlayerId)
        {
            return true;
        }

        if (!ExPlayerControl.LocalPlayer.IsDead())
        {
            return false;
        }

        // バスカーの偽装死時は他のプレイヤーの役職を見えないようにする
        bool isBuskerFakeDeath = ExPlayerControl.LocalPlayer.GetAbility<BuskerPseudocideAbility>()?.isEffectActive == true;
        if (isBuskerFakeDeath && ExPlayerControl.LocalPlayer.PlayerId != player.PlayerId)
        {
            return false;
        }

        // Local player is ghost
        bool canSeeGhostRoles = !GameSettingOptions.HideGhostRoles ||
                                (ExPlayerControl.LocalPlayer.IsImpostor() && GameSettingOptions.ShowGhostRolesToImpostor);

        if (!canSeeGhostRoles)
        {
            return false;
        }

        return true;
    }
    private static void SetPlayerNameColor(ExPlayerControl player, bool isRoleInfoVisible)
    {
        if (player.TryGetAbility<HideMyRoleWhenAliveAbility>(out var hmr) && hmr.IsHide(player).role)
        {
            // 生きている時は役職を自覚できない役の名前色を設定
            var roleColor = player.Data.Role.IsImpostor ? Impostor.Instance.RoleColor : Crewmate.Instance.RoleColor;
            player.Data.Role.NameColor = roleColor;
            SetNameTextColor(player, roleColor);
        }
        else
        { // 通常の役職表示
            if (isRoleInfoVisible)
            {
                player.Data.Role.NameColor = player.roleBase.RoleColor;
                SetNameTextColor(player, player.roleBase.RoleColor, true);
            }
            else if (ExPlayerControl.LocalPlayer.IsImpostor() && player.IsImpostor())
            {
                player.Data.Role.NameColor = Palette.ImpostorRed;
                SetNameTextColor(player, Palette.ImpostorRed, true);
            }
            else
            {
                player.Data.Role.NameColor = Color.white;
                SetNameTextColor(player, Color.white, true);
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

        // 生きている時は役職を自覚できない役の役職名を上書き
        var hideMyRoleAbility = !player.AmOwner || player.IsDead() ? null : player.GetAbility<HideMyRoleWhenAliveAbility>();
        hideMyRoleAbility?.DisplayRoleName(player, ref roleName);

        // 幽霊役職の表示は役職可視性チェックに従う
        var hrg = ExPlayerControl.LocalPlayer.GetAbility<HideRoleOnGhostAbility>();
        bool isRoleVisible = GetRoleInfoVisibility(player, hrg);

        if (player.GhostRole != GhostRoleId.None && player.GhostRoleBase != null && isRoleVisible)
            roleName = $"{ModHelpers.CsWithTranslation(player.GhostRoleBase.RoleColor, player.GhostRole.ToString())} ({roleName}) ";
        if (player.ModifierRoleBases.Count > 0)
            roleName += " ";
        foreach (var modifier in player.ModifierRoleBases)
        {
            // 生きている時は役職を自覚できないモディファイアは処理をスキップ
            if (hideMyRoleAbility != null && hideMyRoleAbility.IsCheckTargetModifierRoleHidden(player, modifier.ModifierRole)) continue;
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

        SetPlayerNameColor(player, isRoleVisible);

        UpdateVisible(player, hrg);
        NameTextUpdateEvent.Invoke(player, isRoleVisible);
        NameTextUpdateVisiableEvent.Invoke(player, isRoleVisible);
    }
    public static void SetCustomTaskCount(ExPlayerControl player, int completed, int total, bool showOnMeeting = false, bool showCompletedOnComms = false)
    {
        string text = ModHelpers.Cs(Color.yellow, "(" + (showOnMeeting ? (ModHelpers.IsComms() ? "?" : completed.ToString()) : (showCompletedOnComms ? completed.ToString() : "?")) + "/" + total.ToString() + ")");
        if (player.PlayerInfoText != null)
            player.PlayerInfoText.text += text;
        if (player.MeetingInfoText != null && showOnMeeting)
            player.MeetingInfoText.text += text;
    }
    public static void SetNameTextColor(ExPlayerControl player, Color color, bool nonLog = false)
    {
        if (!nonLog)
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
        FixedUpdateEvent.Instance.AddListener(UpdateAllVisible);
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
    private static void UpdateAllVisible()
    {
        HideRoleOnGhostAbility hideRoleOnGhostAbility = ExPlayerControl.LocalPlayer.GetAbility<HideRoleOnGhostAbility>();
        foreach (var player in ExPlayerControl.ExPlayerControls)
        {
            UpdateVisible(player, hideRoleOnGhostAbility);
            NameTextUpdateVisiableEvent.Invoke(player, player.Player.Visible);
        }
    }
    public static void UpdateVisible(ExPlayerControl player, HideRoleOnGhostAbility localHideRoleOnGhostAbility)
    {
        if (player == null || player.Player == null)
            return;

        // バスカーの偽装死時は他のプレイヤーの役職を見えないようにする
        bool isBuskerFakeDeath = ExPlayerControl.LocalPlayer.GetAbility<BuskerPseudocideAbility>()?.isEffectActive == true;

        bool visiable = !isBuskerFakeDeath && GetRoleInfoVisibility(player, localHideRoleOnGhostAbility);
        UpdateVisible(player, visiable);
        if (!visiable && localHideRoleOnGhostAbility != null && localHideRoleOnGhostAbility.IsHideRole(player))
        {
            // When role info is not visible, the name color may need to be updated (e.g. to red for fellow impostors).
            SetPlayerNameColor(player, visiable);
        }
    }
    public static void UpdateVisible(ExPlayerControl player, bool visiable)
    {
        if (player == null || player.Player == null)
            return;
        if (!player.Player.Visible)
            visiable = false;
        if (visiable && player.PlayerInfoText == null)
            Initialize(player);

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