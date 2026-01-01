using System;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Events;
using SuperNewRoles.Modules;
using SuperNewRoles.Modules.Events.Bases;
using SuperNewRoles.Roles.Ability;
using SuperNewRoles.Roles.Ability.CustomButton;
using UnityEngine;

namespace SuperNewRoles.Roles.Neutral;

class Moira : RoleBase<Moira>
{
    public override RoleId Role { get; } = RoleId.Moira;
    public override Color32 RoleColor { get; } = new(201, 127, 219, byte.MaxValue);
    public override List<Func<AbilityBase>> Abilities { get; } = [() => new MoiraMeetingAbility(MoiraAbilityLimit)];

    public override QuoteMod QuoteMod { get; } = QuoteMod.SuperNewRoles;
    public override RoleTypes IntroSoundType { get; } = RoleTypes.Shapeshifter;
    public override short IntroNum { get; } = 1;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Neutral;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Neutral;
    public override TeamTag TeamTag { get; } = TeamTag.Neutral;
    public override RoleTag[] RoleTags { get; } = [RoleTag.Information];
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Neutral;
    public override RoleId[] RelatedRoleIds { get; } = [];

    [CustomOptionInt("MoiraAbilityLimit", 1, 10, 1, 5)]
    public static int MoiraAbilityLimit;
}

public class MoiraMeetingAbility : CustomMeetingButtonBase, IAbilityCount
{
    private const float LimitTextScale = 0.5f;
    private static readonly Color SelectedColor = new(0.6f, 0.6f, 0.6f, 1f);

    private ExPlayerControl firstTarget;
    private GameObject selectedButton;
    private Color? defaultButtonColor;
    private TMPro.TextMeshPro limitText;
    private bool usedThisMeeting;
    private bool lastMeetingDead;
    private List<(byte, byte)> swapData;

    public MoiraMeetingAbility(int maxUses)
    {
        Count = maxUses;
        swapData = new();
    }

    public override Sprite Sprite => AssetManager.GetAsset<Sprite>("MoiraButton.png");
    public override bool HasButtonLocalPlayer => false;
    private EventListener<WrapUpEventData> wrapUpEvent;
    private EventListener<MeetingHudCalculateVotesOnPlayerOnlyHostEventData> calculateVotesEvent;
    private EventListener<VotingCompleteEventData> votingCompleteEvent;
    private EventListener<NameTextUpdateEventData> nameTextUpdateEvent;

    public override bool CheckHasButton(ExPlayerControl player)
    {
        return ExPlayerControl.LocalPlayer.IsAlive()
            && player.IsAlive()
            && player.PlayerId != ExPlayerControl.LocalPlayer.PlayerId
            && HasCount
            && !usedThisMeeting;
    }

    public override void AttachToLocalPlayer()
    {
        base.AttachToLocalPlayer();
        lastMeetingDead = ExPlayerControl.LocalPlayer.IsDead();
        wrapUpEvent = WrapUpEvent.Instance.AddListener(OnWrapUp);
        calculateVotesEvent = MeetingHudCalculateVotesOnPlayerOnlyHostEvent.Instance.AddListener(OnCalculateVotes);
    }

    public override void AttachToAlls()
    {
        base.AttachToAlls();
        votingCompleteEvent = VotingCompleteEvent.Instance.AddListener(OnVotingComplete);
        nameTextUpdateEvent = NameTextUpdateEvent.Instance.AddListener(OnNameTextUpdate);
    }

    public override void DetachToLocalPlayer()
    {
        base.DetachToLocalPlayer();
        wrapUpEvent?.RemoveListener();
        calculateVotesEvent?.RemoveListener();
    }

    public override void DetachToAlls()
    {
        base.DetachToAlls();
        votingCompleteEvent?.RemoveListener();
        nameTextUpdateEvent?.RemoveListener();
    }

    private void OnNameTextUpdate(NameTextUpdateEventData data)
    {
        if (data.Player != Player) return;
        if (data.Player.IsDead()) return;
        // 使い切ってないとダメ
        if (HasCount) return;
        data.Player.cosmetics.nameText.text = ModHelpers.Cs(Moira.Instance.RoleColor, data.Player.cosmetics.nameText.text);
        if (data.Player.VoteArea != null)
            data.Player.VoteArea.NameText.text = ModHelpers.Cs(Moira.Instance.RoleColor, data.Player.VoteArea.NameText.text);
    }

    private void OnWrapUp(WrapUpEventData data)
    {
        bool isDead = ExPlayerControl.LocalPlayer.IsDead() || data.exiled?.PlayerId == ExPlayerControl.LocalPlayer.PlayerId;
        if (!lastMeetingDead && isDead)
        {
            // 死んだでリセットする時
            RpcResetMoira();
        }
        // 生き返りの場合を想定して毎回設定し直す
        lastMeetingDead = isDead;
    }

    public override bool CheckIsAvailable(ExPlayerControl player)
    {
        return HasCount && !usedThisMeeting && player.IsAlive();
    }

    public override void OnMeetingStart()
    {
        usedThisMeeting = false;
        ClearSelection();
        RefreshLimitText();
    }

    public override void OnMeetingUpdate()
    {
        if (ExPlayerControl.LocalPlayer.IsDead())
        {
            ClearSelection();
            DestroyLimitText();
        }
        else
        {
            RefreshLimitText();
        }

        base.OnMeetingUpdate();
    }

    public override void OnMeetingClose()
    {
        if (!lastMeetingDead && ExPlayerControl.LocalPlayer.IsDead())
        {
        }
        else if (usedThisMeeting)
        {
            RpcSwapMoira();
        }
        ClearSelection();
        DestroyLimitText();
    }

    [CustomRPC]
    public void RpcResetMoira()
    {
        foreach (var data in swapData)
        {
            var player1 = ExPlayerControl.ById(data.Item1);
            var player2 = ExPlayerControl.ById(data.Item2);
            player1.ReverseRole(player2);

            RoleTypes player1Role = player1.Data.Role.Role;
            RoleTypes player2Role = player2.Data.Role.Role;

            var player1TargetRole = AdjustRoleForTarget(player2Role, player2, player1);
            var player2TargetRole = AdjustRoleForTarget(player1Role, player1, player2);

            RoleManager.Instance.SetRole(player1.Player, player1TargetRole);
            RoleManager.Instance.SetRole(player2.Player, player2TargetRole);
        }
        swapData.Clear();
    }

    [CustomRPC]
    public void RpcSwapMoira()
    {
        var data = swapData.Last();

        var player1 = ExPlayerControl.ById(data.Item1);
        var player2 = ExPlayerControl.ById(data.Item2);

        player1.ReverseRole(player2);

        RoleTypes player1Role = player1.Data.Role.Role;
        RoleTypes player2Role = player2.Data.Role.Role;

        var player1TargetRole = AdjustRoleForTarget(player2Role, player2, player1);
        var player2TargetRole = AdjustRoleForTarget(player1Role, player1, player2);

        RoleManager.Instance.SetRole(player1.Player, player1TargetRole);
        RoleManager.Instance.SetRole(player2.Player, player2TargetRole);
    }

    private void OnCalculateVotes(MeetingHudCalculateVotesOnPlayerOnlyHostEventData data)
    {
        if (!AmongUsClient.Instance.AmHost) return;
        if (!usedThisMeeting) return;
        if (swapData.Count == 0) return;

        var current = swapData[^1];
        if (current.Item1 == byte.MaxValue || current.Item2 == byte.MaxValue) return;

        if (data.Target == current.Item1)
        {
            data.Target = current.Item2;
        }
        else if (data.Target == current.Item2)
        {
            data.Target = current.Item1;
        }
    }

    private void OnVotingComplete(VotingCompleteEventData data)
    {
        if (!usedThisMeeting) return;
        if (swapData.Count == 0) return;
        if (MeetingHud.Instance == null) return;

        var last = swapData[^1];
        PlayerVoteArea swapped1 = null;
        PlayerVoteArea swapped2 = null;
        foreach (PlayerVoteArea playerVoteArea in MeetingHud.Instance.playerStates)
        {
            if (playerVoteArea.TargetPlayerId == last.Item1) swapped1 = playerVoteArea;
            if (playerVoteArea.TargetPlayerId == last.Item2) swapped2 = playerVoteArea;
        }
        if (swapped1 != null && swapped2 != null)
        {
            MeetingHud.Instance.StartCoroutine(Effects.Slide3D(swapped1.transform, swapped1.transform.localPosition, swapped2.transform.localPosition, 1.5f));
            MeetingHud.Instance.StartCoroutine(Effects.Slide3D(swapped2.transform, swapped2.transform.localPosition, swapped1.transform.localPosition, 1.5f));
        }
    }

    public override void OnClick(ExPlayerControl exPlayer, GameObject button)
    {
        var meetingHud = MeetingHud.Instance;
        if (meetingHud == null) return;
        if (!(meetingHud.state == MeetingHud.VoteStates.Voted ||
              meetingHud.state == MeetingHud.VoteStates.NotVoted ||
              meetingHud.state == MeetingHud.VoteStates.Discussion))
        {
            return;
        }
        if (exPlayer.IsDead() || !HasCount || usedThisMeeting) return;

        if (firstTarget == null)
        {
            firstTarget = exPlayer;
            SetSelectedButton(button);
            return;
        }

        if (firstTarget.PlayerId == exPlayer.PlayerId)
        {
            ClearSelection();
            return;
        }

        this.UseAbilityCount();

        RpcUsedMoira(firstTarget, exPlayer);

        DestroyAllButton();

        ClearSelection();
        RefreshLimitText();
    }

    [CustomRPC]
    public void RpcUsedMoira(ExPlayerControl firstTarget, ExPlayerControl exPlayer)
    {
        usedThisMeeting = true;
        swapData.Add((firstTarget.PlayerId, exPlayer.PlayerId));
    }

    private void RefreshLimitText()
    {
        if (ExPlayerControl.LocalPlayer == null || ExPlayerControl.LocalPlayer.IsDead()) return;

        if (limitText == null)
        {
            limitText = GameObject.Instantiate(FastDestroyableSingleton<HudManager>.Instance.KillButton.cooldownTimerText, MeetingHud.Instance.transform);
            limitText.enableWordWrapping = false;
            limitText.transform.localScale = Vector3.one * LimitTextScale;
            limitText.transform.localPosition = new Vector3(-3.58f, 2.27f, -10);
            limitText.alignment = TMPro.TextAlignmentOptions.Left;
        }

        limitText.text = ModTranslation.GetString("RemainingText", Math.Max(0, Count));
        limitText.gameObject.SetActive(true);
    }

    private void DestroyLimitText()
    {
        if (limitText == null) return;
        GameObject.Destroy(limitText.gameObject);
        limitText = null;
    }

    private void SetSelectedButton(GameObject button)
    {
        if (selectedButton == button) return;

        ClearSelectedButton();

        selectedButton = button;
        if (selectedButton.TryGetComponent<SpriteRenderer>(out var renderer))
        {
            defaultButtonColor = renderer.color;
            renderer.color = SelectedColor;
        }
    }

    private void ClearSelectedButton()
    {
        if (selectedButton != null && selectedButton.TryGetComponent<SpriteRenderer>(out var renderer))
        {
            renderer.color = defaultButtonColor ?? Color.white;
        }

        selectedButton = null;
        defaultButtonColor = null;
    }

    private void ClearSelection()
    {
        ClearSelectedButton();
        firstTarget = null;
    }

    private RoleTypes AdjustRoleForTarget(RoleTypes incomingRole, ExPlayerControl sourcePlayer, ExPlayerControl targetPlayer)
    {
        if (targetPlayer != null && targetPlayer.IsAlive() && RoleManager.IsGhostRole(incomingRole))
        {
            return ToAliveRole(sourcePlayer);
        }

        if (targetPlayer != null && targetPlayer.IsDead() && !RoleManager.IsGhostRole(incomingRole))
        {
            return ToGhostRole(sourcePlayer);
        }

        return incomingRole;
    }

    private RoleTypes ToAliveRole(ExPlayerControl sourcePlayer)
    {
        if (sourcePlayer.Data.Role.IsImpostor)
        {
            return RoleTypes.Impostor;
        }
        return RoleTypes.Crewmate;
    }

    private RoleTypes ToGhostRole(ExPlayerControl sourcePlayer)
    {
        var sourceData = sourcePlayer?.Data;

        if (sourceData != null)
        {
            return sourceData.Role.DefaultGhostRole;
        }

        // フォールバック: インポスター系はImpostorGhost、それ以外はCrewmateGhost
        return sourcePlayer?.Data?.Role?.IsImpostor == true ? RoleTypes.ImpostorGhost : RoleTypes.CrewmateGhost;
    }
}

