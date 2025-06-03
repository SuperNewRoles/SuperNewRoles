using System;
using System.Collections.Generic;
using UnityEngine;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Roles.Ability;
using SuperNewRoles.Roles.Ability.CustomButton;
using SuperNewRoles.Modules;
using SuperNewRoles.Events.PCEvents;
using SuperNewRoles.Modules.Events.Bases;

namespace SuperNewRoles.Roles.Crewmate;

class MeetingSheriff : RoleBase<MeetingSheriff>
{
    public override RoleId Role { get; } = RoleId.MeetingSheriff;
    public override Color32 RoleColor { get; } = Sheriff.Instance.RoleColor;
    public override List<Func<AbilityBase>> Abilities { get; } = [() => new MeetingSheriffAbility(new MeetingSheriffAbilityData(
        killCount: MeetingSheriffMaxKillCount,
        shotsPerMeeting: MeetingSheriffShotsPerMeeting,
        canKillNeutral: MeetingSheriffCanKillNeutral,
        canKillImpostor: MeetingSheriffCanKillImpostor,
        canKillMadRoles: MeetingSheriffCanKillMadRoles,
        canKillFriendRoles: MeetingSheriffCanKillFriendRoles,
        canKillLovers: MeetingSheriffCanKillLovers))];

    public override QuoteMod QuoteMod { get; } = QuoteMod.TheOtherRoles;
    public override RoleTypes IntroSoundType { get; } = RoleTypes.Crewmate;
    public override short IntroNum { get; } = 1;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Crewmate;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Crewmate;
    public override TeamTag TeamTag { get; } = TeamTag.Crewmate;
    public override RoleTag[] RoleTags { get; } = [];
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Crewmate;

    [CustomOptionInt("MeetingSheriffMaxKillCount", 1, 10, 1, 1, translationName: "SheriffMaxKillCount")]
    public static int MeetingSheriffMaxKillCount;

    [CustomOptionInt("MeetingSheriffShotsPerMeeting", 1, 5, 1, 1)]
    public static int MeetingSheriffShotsPerMeeting;

    [CustomOptionBool("MeetingSheriffCanKillImpostor", true, translationName: "SheriffCanKillImpostor")]
    public static bool MeetingSheriffCanKillImpostor;

    [CustomOptionBool("MeetingSheriffCanKillMadRoles", true, translationName: "SheriffCanKillMadRoles")]
    public static bool MeetingSheriffCanKillMadRoles;

    [CustomOptionBool("MeetingSheriffCanKillNeutral", true, translationName: "SheriffCanKillNeutral")]
    public static bool MeetingSheriffCanKillNeutral;

    [CustomOptionBool("MeetingSheriffCanKillFriendRoles", true, translationName: "SheriffCanKillFriendRoles")]
    public static bool MeetingSheriffCanKillFriendRoles;

    [CustomOptionBool("MeetingSheriffCanKillLovers", true, translationName: "SheriffCanKillLovers")]
    public static bool MeetingSheriffCanKillLovers;
}

public class MeetingSheriffAbilityData
{
    public int KillCount { get; set; }
    public int ShotsPerMeeting { get; set; }
    public bool CanKillNeutral { get; set; }
    public bool CanKillImpostor { get; set; }
    public bool CanKillMadRoles { get; set; }
    public bool CanKillFriendRoles { get; set; }
    public bool CanKillLovers { get; set; }

    public MeetingSheriffAbilityData(int killCount, int shotsPerMeeting, bool canKillNeutral, bool canKillImpostor, bool canKillMadRoles, bool canKillFriendRoles, bool canKillLovers)
    {
        KillCount = killCount;
        ShotsPerMeeting = shotsPerMeeting;
        CanKillNeutral = canKillNeutral;
        CanKillImpostor = canKillImpostor;
        CanKillMadRoles = canKillMadRoles;
        CanKillFriendRoles = canKillFriendRoles;
        CanKillLovers = canKillLovers;
    }

    public bool CanKill(PlayerControl killer, ExPlayerControl target)
    {
        if (target.IsImpostor())
            return CanKillImpostor;
        else if (target.IsNeutral())
            return CanKillNeutral;
        else if (target.IsMadRoles())
            return CanKillMadRoles;
        else if (target.IsFriendRoles())
            return CanKillFriendRoles;
        else if (target.IsLovers())
            return CanKillLovers;
        return false;
    }
}

public class MeetingSheriffAbility : CustomMeetingButtonBase, IAbilityCount
{
    public MeetingSheriffAbilityData MeetingSheriffAbilityData { get; set; }
    public override Sprite Sprite => AssetManager.GetAsset<Sprite>("SheriffKillButton.png");
    public override bool HasButtonLocalPlayer => false;
    private int ShotThisMeeting;
    private int MeetingCount = -1;
    private TMPro.TextMeshPro limitText;

    public MeetingSheriffAbility(MeetingSheriffAbilityData meetingSheriffAbilityData)
    {
        MeetingSheriffAbilityData = meetingSheriffAbilityData;
        Count = MeetingSheriffAbilityData.KillCount;
    }

    public override bool CheckHasButton(ExPlayerControl player)
    {
        return ExPlayerControl.LocalPlayer.IsAlive() && HasCount && player.IsAlive() && ShotThisMeeting < MeetingSheriffAbilityData.ShotsPerMeeting;
    }

    public override bool CheckIsAvailable(ExPlayerControl player)
    {
        return player.IsAlive();
    }

    public override void OnMeetingStart()
    {
        ShotThisMeeting = 0;
        MeetingCount++;

        // 残り使用回数を表示するテキストを作成
        if (Player.IsDead()) return;
        if (limitText != null)
            GameObject.Destroy(limitText.gameObject);
        limitText = GameObject.Instantiate(FastDestroyableSingleton<HudManager>.Instance.KillButton.cooldownTimerText, MeetingHud.Instance.transform);
        limitText.text = ModTranslation.GetString("MeetingSheriffLimitText", Count, MeetingSheriffAbilityData.ShotsPerMeeting - ShotThisMeeting);
        limitText.enableWordWrapping = false;
        limitText.transform.localScale = Vector3.one * 0.5f;
        limitText.transform.localPosition = new Vector3(-3.58f, 2.27f, -10);
        limitText.gameObject.SetActive(true);
        limitText.alignment = TMPro.TextAlignmentOptions.Left;
    }

    public override void OnMeetingUpdate()
    {
        if (ExPlayerControl.LocalPlayer.IsDead())
        {
            // プレイヤーが死亡した場合、テキストも削除
            if (limitText != null)
                GameObject.Destroy(limitText.gameObject);
            limitText = null;
        }
        base.OnMeetingUpdate();
    }

    public override void OnClick(ExPlayerControl exPlayer, GameObject button)
    {
        // ミーティングHUDの取得と状態チェック
        MeetingHud meetingHud = MeetingHud.Instance;
        if (meetingHud == null) return;
        if (!(meetingHud.state == MeetingHud.VoteStates.Voted ||
              meetingHud.state == MeetingHud.VoteStates.NotVoted ||
              meetingHud.state == MeetingHud.VoteStates.Discussion))
            return;

        if (exPlayer.IsDead()) return;

        this.UseAbilityCount();
        this.ShotThisMeeting++;

        // 残り使用回数を更新
        if (limitText != null)
            limitText.text = ModTranslation.GetString("MeetingSheriffLimitText", Count, MeetingSheriffAbilityData.ShotsPerMeeting - ShotThisMeeting);

        if (MeetingSheriffAbilityData.CanKill(PlayerControl.LocalPlayer, exPlayer))
        {
            // 正当なキル
            RpcShotMeetingSheriff(PlayerControl.LocalPlayer, exPlayer, false, false);
        }
        else
        {
            // 誤射の場合は自分が死ぬ
            RpcShotMeetingSheriff(PlayerControl.LocalPlayer, ExPlayerControl.LocalPlayer, true, true);
        }
    }

    [CustomRPC]
    public static void RpcShotMeetingSheriff(PlayerControl killer, ExPlayerControl dyingTarget, bool isSuicide, bool isMisFire)
    {
        if (killer == null || dyingTarget == null)
            return;

        dyingTarget.Player.Exiled();

        if (isMisFire || isSuicide)
            dyingTarget.FinalStatus = FinalStatus.SheriffSelfDeath;
        else
            dyingTarget.FinalStatus = FinalStatus.SheriffKill;

        if (Constants.ShouldPlaySfx())
            SoundManager.Instance.PlaySound(dyingTarget.Player.KillSfx, false, 0.8f);

        if (MeetingHud.Instance)
        {
            foreach (PlayerVoteArea pva in MeetingHud.Instance.playerStates)
            {
                if (pva.TargetPlayerId == dyingTarget.PlayerId)
                {
                    pva.SetDead(pva.DidReport, true);
                    pva.Overlay.gameObject.SetActive(true);
                }
                pva.UnsetVote();
            }
            MeetingHud.Instance.ClearVote();
            if (AmongUsClient.Instance.AmHost)
                MeetingHud.Instance.CheckForEndVoting();
        }

        if (FastDestroyableSingleton<HudManager>.Instance != null && killer != null)
        {
            if (PlayerControl.LocalPlayer == dyingTarget)
            {
                FastDestroyableSingleton<HudManager>.Instance.KillOverlay.ShowKillAnimation(killer.Data, dyingTarget.Data);
            }
        }
    }
}