using System;
using System.Collections.Generic;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Events.PCEvents;
using SuperNewRoles.Modules;
using SuperNewRoles.Modules.Events.Bases;
using SuperNewRoles.Roles.Ability;
using SuperNewRoles.Roles.Ability.CustomButton;
using UnityEngine;

namespace SuperNewRoles.Roles.Crewmate;

class MeetingSheriff : RoleBase<MeetingSheriff>
{
    public override RoleId Role { get; } = RoleId.MeetingSheriff;
    public override Color32 RoleColor { get; } = Sheriff.Instance.RoleColor;
    public override List<Func<AbilityBase>> Abilities { get; } = [() => new MeetingSheriffAbility(new MeetingSheriffAbilityData(
        killCount: MeetingSheriffMaxKillCount,
        shotsPerMeeting: MeetingSheriffShotsPerMeeting,
        mode: MeetingSheriffSuicideMode,
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

    [CustomOptionSelect("Sheriff.SuicideMode", typeof(SheriffSuicideMode), "Sheriff.SuicideMode.")]
    public static SheriffSuicideMode MeetingSheriffSuicideMode = SheriffSuicideMode.Default;

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
    public SheriffSuicideMode Mode { get; set; }
    public bool CanKillImpostor { get; set; }
    public bool CanKillMadRoles { get; set; }
    public bool CanKillFriendRoles { get; set; }
    public bool CanKillLovers { get; set; }

    public MeetingSheriffAbilityData(int killCount, int shotsPerMeeting, SheriffSuicideMode mode, bool canKillNeutral, bool canKillImpostor, bool canKillMadRoles, bool canKillFriendRoles, bool canKillLovers)
    {
        KillCount = killCount;
        ShotsPerMeeting = shotsPerMeeting;
        Mode = mode;

        CanKillNeutral = canKillNeutral;
        CanKillImpostor = canKillImpostor;
        CanKillMadRoles = canKillMadRoles;
        CanKillFriendRoles = canKillFriendRoles;
        CanKillLovers = canKillLovers;
    }

    public bool CanKill(PlayerControl killer, ExPlayerControl target)
    {
        bool canKill = false;

        if (target.IsImpostor())
            canKill = CanKillImpostor;
        else if (target.IsNeutral())
            canKill = CanKillNeutral;
        else if (target.Role == RoleId.HauntedWolf || target.ModifierRole.HasFlag(ModifierRoleId.ModifierHauntedWolf)) // 第三陣営に付与される事はEvilSeerのAbilityによる付与時のみの為、第三陣営としての判定を優先
            canKill = CanKillImpostor;
        else if (target.IsMadRoles())
            canKill = CanKillMadRoles;
        else if (target.IsFriendRoles())
            canKill = CanKillFriendRoles;
        else if (target.IsLovers())
            canKill = CanKillLovers;

        // 狼憑きシェリフの判定反転
        if (Modifiers.ModifierHauntedWolf.ModifierHauntedWolfIsReverseSheriffDecision)
        {
            var killExP = ExPlayerControl.ById(killer.PlayerId);
            if (killExP.ModifierRole.HasFlag(ModifierRoleId.ModifierHauntedWolf))
                canKill = !canKill;
        }

        return canKill;
    }

    /// <summary>ミーティングシェリフのキルと自害の状態を取得する</summary>
    /// <param name="killer">ミーティングシェリフ</param>
    /// <param name="target">キル対象</param>
    /// <param name="isMurdering">対象を殺害するか</param>
    /// <param name="isSuicide">シェリフは自害するか</param>
    public void JudgmentData(PlayerControl killer, ExPlayerControl target, out bool isMurdering, out bool isSuicide)
    {
        var canKill = CanKill(killer, target);

        // 常に自決する場合
        if (IsAlwaysSuicide)
        {
            isMurdering = canKill;
            isSuicide = true;
            return;
        }

        // 自決判定は通常の場合 ("通常" & "誤射時も対象をキルする")
        isMurdering = canKill || Mode == SheriffSuicideMode.AlwaysKill;
        isSuicide = !canKill;

        return;
    }

    /// <summary>対象のキルは誤射によるものか</summary>
    /// <param name="isSuicide">シェリフは自殺しているか</param>
    /// <returns>true : 誤射によるキル, false : 正当なキル(正義執行)</returns>
    public bool IsWrongMurder(bool isSuicide) => isSuicide && Mode == SheriffSuicideMode.AlwaysKill;

    /// <summary>常に自決する設定が有効か</summary>
    public bool IsAlwaysSuicide => Mode == SheriffSuicideMode.AlwaysSuicide;
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

        // キルと自害の状態を取得
        MeetingSheriffAbilityData.JudgmentData(PlayerControl.LocalPlayer, exPlayer, out bool isMurdering, out bool isSuicide);

        // キル&自害の実行
        RpcShotMeetingSheriff(PlayerControl.LocalPlayer, exPlayer, isMurdering, isSuicide);
    }

    [CustomRPC]
    /// <summary>ミーティングシェリフのキル及び自害の実行</summary>
    /// <param name="killer">ミーティングシェリフ</param>
    /// <param name="target">キル対象</param>
    /// <param name="isMurdering">対象をキルするか</param>
    /// <param name="isSuicide">自害するか</param>
    public static void RpcShotMeetingSheriff(PlayerControl killer, ExPlayerControl target, bool isMurdering, bool isSuicide)
    {
        if (killer == null || target == null)
            return;

        ExPlayerControl expKiller = killer;
        var abilityData = expKiller.GetAbility<MeetingSheriffAbility>().MeetingSheriffAbilityData;

        if (isMurdering) // 殺害処理
        {
            target.Player.Exiled();
            target.FinalStatus = abilityData.IsWrongMurder(isSuicide) ? FinalStatus.SheriffWrongfulMurder : FinalStatus.SheriffKill;
            MurderDataManager.AddMurderData(expKiller, target);

            if (Constants.ShouldPlaySfx())
                SoundManager.Instance.PlaySound(target.Player.KillSfx, false, 0.8f);

            if (expKiller != null && PlayerControl.LocalPlayer == target && FastDestroyableSingleton<HudManager>.Instance != null)
                FastDestroyableSingleton<HudManager>.Instance.KillOverlay.ShowKillAnimation(expKiller.Data, target.Data);
        }

        if (isSuicide) // 自害処理
        {
            expKiller.Player.Exiled();
            expKiller.FinalStatus = abilityData.IsAlwaysSuicide ? FinalStatus.SheriffSuicide : FinalStatus.SheriffMisFire;

            if (Constants.ShouldPlaySfx())
                SoundManager.Instance.PlaySound(expKiller.Player.KillSfx, false, 0.8f);

            if (expKiller != null && PlayerControl.LocalPlayer == killer && FastDestroyableSingleton<HudManager>.Instance != null)
                FastDestroyableSingleton<HudManager>.Instance.KillOverlay.ShowKillAnimation(expKiller.Data, expKiller.Data);
        }

        if (MeetingHud.Instance)
        {
            foreach (PlayerVoteArea pva in MeetingHud.Instance.playerStates)
            {
                var isTargetPlayer = isMurdering && pva.TargetPlayerId == target.PlayerId;
                var isSuicidePlayer = isSuicide && pva.TargetPlayerId == expKiller.PlayerId;

                if (isTargetPlayer || isSuicidePlayer)
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
    }
}