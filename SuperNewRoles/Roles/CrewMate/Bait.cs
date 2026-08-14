using System;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using Hazel;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Events;
using SuperNewRoles.Events.PCEvents;
using SuperNewRoles.Modules;
using SuperNewRoles.Modules.Events.Bases;
using SuperNewRoles.Patches;
using SuperNewRoles.Roles.Ability;
using SuperNewRoles.SuperTrophies;
using UnityEngine;

namespace SuperNewRoles.Roles.CrewMate;

class Bait : RoleBase<Bait>
{
    public override RoleId Role { get; } = RoleId.Bait;
    //使いまわすことがないならNameKeyは必要なさそう
    //public override string NameKey { get; } = RoleId.Bait.ToString();

    public override Color32 RoleColor { get; } = new(222, 184, 135, byte.MaxValue);
    public override List<Func<AbilityBase>> Abilities { get; } = [() => new BaitAbility()];

    public override QuoteMod QuoteMod { get; } = QuoteMod.TheOtherRoles;
    public override RoleTypes IntroSoundType { get; } = RoleTypes.Crewmate;
    //TODO:Intro文章の指定だけどこれなんで複数あるの？
    public override short IntroNum { get; } = 1;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Crewmate;
    //これはむしろPlayer毎に設定するべきでは？
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Crewmate;

    public override TeamTag TeamTag { get; } = TeamTag.Crewmate;

    //TODO:要検討
    public override RoleTag[] RoleTags { get; } = [RoleTag.PowerPlayResistance];

    // 通報を遅延させる
    [CustomOptionFloat("BaitReportTime", 0.5f, 10f, 0.1f, 0.5f)]
    public static float BaitReportTime;
    // キラーに警告する
    [CustomOptionBool("BaitWarnKiller", true)]
    public static bool BaitWarnKiller;

    // 通報をランダムに遅延させる
    [CustomOptionBool("BaitRandomDelay", false)]
    public static bool BaitRandomDelay;

    // 遅延のブレ幅
    [CustomOptionInt("BaitDelayVariation", 0, 30, 1, 2, parentFieldName: nameof(BaitRandomDelay))]
    public static int BaitDelayVariation;

    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Crewmate;
}

class BaitAbility : AbilityBase
{
    //何らかの要因で能力を失う時に使うのでListenerは保持しておく
    public EventListener<MurderEventData> killedEventListener;

    public override void AttachToLocalPlayer()
    {
        //ここでEventListenerと紐付ける
        killedEventListener = MurderEvent.Instance.AddListener(OnKilled);
    }

    public override void DetachToLocalPlayer()
    {
        base.DetachToLocalPlayer();
        if (killedEventListener != null)
        {
            MurderEvent.Instance.RemoveListener(killedEventListener);
            killedEventListener = null;
        }
    }

    public void OnKilled(MurderEventData data)
    {
        if (!ShouldAutoReport(data.target, data.killer, data.resultFlags, Player))
            return;

        // キラーに警告する（画面を青く光らせる）
        if (Bait.BaitWarnKiller)
        {
            FlashHandler.RpcShowFlash(data.killer, Color.cyan, 0.2f);
        }

        ExPlayerControl killer = data.killer;
        ExPlayerControl target = data.target;
        float delay = CalculateReportDelay(Bait.BaitReportTime, Bait.BaitRandomDelay, Bait.BaitDelayVariation);
        Logger.Info($"Bait auto-report scheduled in {delay}s (killer={killer.PlayerId}, target={target.PlayerId})", "Bait");
        new LateTask(() => TryReport(killer, target), delay, "BaitReport");
    }

    internal static bool ShouldAutoReport(ExPlayerControl target, ExPlayerControl killer, MurderResultFlags resultFlags, ExPlayerControl self)
    {
        if (target == null || killer == null || self == null)
            return false;
        if (target.PlayerId != self.PlayerId)
            return false;
        if (!resultFlags.HasFlag(MurderResultFlags.Succeeded))
            return false;
        // 自殺扱いの MurderEvent（波動砲など）では reporter が死者になり、
        // PlayerControl.ReportDeadBody が Data.IsDead で即 return する
        if (killer.PlayerId == target.PlayerId)
            return false;
        return true;
    }

    internal static float CalculateReportDelay(float reportTime, bool randomDelay, int delayVariation, Func<int, int, int> randomRange = null)
    {
        // 最低限の遅延（キラーへの警告が見えるように）
        const float minimumFlashDelay = 0.5f;
        float extraDelay = 0f;

        if (randomDelay)
        {
            int minDelay = Math.Max(0, (int)reportTime - delayVariation);
            int maxDelay = (int)reportTime + delayVariation;
            extraDelay = randomRange != null
                ? randomRange(minDelay, maxDelay + 1)
                : UnityEngine.Random.Range(minDelay, maxDelay + 1);
        }
        else if (reportTime > 0)
        {
            extraDelay = reportTime;
        }

        return minimumFlashDelay + extraDelay;
    }

    private static void TryReport(ExPlayerControl killer, ExPlayerControl target)
    {
        if (AmongUsClient.Instance == null || AmongUsClient.Instance.IsGameOver)
            return;
        if (MeetingHud.Instance != null)
            return;
        if (killer?.Player == null || target?.Data == null)
            return;

        Logger.Info($"Bait auto-report sending RpcCustomReportDeadBody (killer={killer.PlayerId}, target={target.PlayerId})", "Bait");
        killer.RpcCustomReportDeadBody(target.Data);
    }
}

/// <summary>
/// ベイトが自動通報されるとトロフィーを獲得するクラス
/// </summary>
public class BaitAutoReportTrophy : SuperTrophyAbility<BaitAutoReportTrophy>
{
    public override TrophiesEnum TrophyId => TrophiesEnum.BaitAutoReport;
    public override TrophyRank TrophyRank => TrophyRank.Bronze;

    public override Type[] TargetAbilities => [typeof(BaitAbility)];
    private bool _killedMe = false;
    private EventListener<MurderEventData> _onMurderEvent;
    private EventListener<CalledMeetingEventData> _onCalledMeetingEvent;

    public override void OnRegister()
    {
        _onMurderEvent = MurderEvent.Instance.AddListener(HandleMurderEvent);
        _onCalledMeetingEvent = CalledMeetingEvent.Instance.AddListener(HandleCalledMeetingEvent);
        _killedMe = false;
        Logger.Info("BaitAutoReportTrophy OnRegister");
    }

    private void HandleMurderEvent(MurderEventData data)
    {
        Logger.Info("BaitAutoReportTrophy HandleMurderEvent: " + data.target.Player.name + " " + data.killer.Player.name);
        if (data.target == null || data.target.PlayerId != PlayerControl.LocalPlayer.PlayerId)
        {
            return;
        }
        _killedMe = true;
    }

    private void HandleCalledMeetingEvent(CalledMeetingEventData data)
    {
        Logger.Info("BaitAutoReportTrophy HandleCalledMeetingEvent: " + data.target.name + " " + data.reporter.name + " " + _killedMe);
        if (data.target == null || data.target.PlayerId != PlayerControl.LocalPlayer.PlayerId)
            return;
        if (_killedMe)
            Complete();
    }
    public override void OnDetached()
    {
        if (_onMurderEvent != null)
        {
            MurderEvent.Instance.RemoveListener(_onMurderEvent);
            _onMurderEvent = null;
        }
        if (_onCalledMeetingEvent != null)
        {
            CalledMeetingEvent.Instance.RemoveListener(_onCalledMeetingEvent);
            _onCalledMeetingEvent = null;
        }
    }
}

/// <summary>
/// ベイトが自動通報した後、キラーが2ターン以内に追放されるとトロフィーを獲得するクラス
/// </summary>
public class BaitKillerExiledTrophy : SuperTrophyAbility<BaitKillerExiledTrophy>
{
    public override TrophiesEnum TrophyId => TrophiesEnum.BaitKillerExiled;
    public override TrophyRank TrophyRank => TrophyRank.Silver;

    public override Type[] TargetAbilities => [typeof(BaitAbility)];

    private BaitAbility _baitAbility;
    private EventListener<MurderEventData> _onMurderEvent;
    private EventListener<WrapUpEventData> _onWrapUpEvent;

    private byte _killerPlayerId;
    private int _meetingsCount;
    private const int RequiredMeetings = 2; // 2ターン以内

    public override void OnRegister()
    {
        _baitAbility = ExPlayerControl.LocalPlayer.PlayerAbilities
            .FirstOrDefault(x => x is BaitAbility) as BaitAbility;
        _onMurderEvent = MurderEvent.Instance.AddListener(HandleMurderEvent);
        _onWrapUpEvent = WrapUpEvent.Instance.AddListener(HandleWrapUpEvent);
        _killerPlayerId = byte.MaxValue;
        _meetingsCount = 0;
    }

    private void HandleMurderEvent(MurderEventData data)
    {
        if (data.target != ExPlayerControl.LocalPlayer)
        {
            return;
        }

        // ベイトがキルされたとき、キラーのIDを記録
        _killerPlayerId = data.killer.PlayerId;
        _meetingsCount = 0;
    }

    private void HandleWrapUpEvent(WrapUpEventData data)
    {
        // キラーが設定されていない場合は無視
        if (_killerPlayerId == byte.MaxValue)
        {
            return;
        }

        _meetingsCount++;

        // 会議の回数が指定された回数以内で、追放されたプレイヤーがキラーである場合
        if (_meetingsCount <= RequiredMeetings && data.exiled?.Object?.PlayerId == _killerPlayerId)
        {
            Complete();
            // 達成したのでリセット
            _killerPlayerId = byte.MaxValue;
            _meetingsCount = 0;
        }
        // 指定された会議回数を超えた場合はリセット
        else if (_meetingsCount > RequiredMeetings)
        {
            _killerPlayerId = byte.MaxValue;
            _meetingsCount = 0;
        }
    }

    public override void OnDetached()
    {
        if (_onMurderEvent != null)
        {
            MurderEvent.Instance.RemoveListener(_onMurderEvent);
            _onMurderEvent = null;
        }

        if (_onWrapUpEvent != null)
        {
            WrapUpEvent.Instance.RemoveListener(_onWrapUpEvent);
            _onWrapUpEvent = null;
        }
    }
}