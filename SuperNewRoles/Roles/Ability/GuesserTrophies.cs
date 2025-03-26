using System;
using System.Collections.Generic;
using System.Linq;
using SuperNewRoles.Events;
using SuperNewRoles.Events.PCEvents;
using SuperNewRoles.Modules;
using SuperNewRoles.Modules.Events.Bases;
using SuperNewRoles.SuperTrophies;
using UnityEngine;

namespace SuperNewRoles.Roles.Ability;

/// <summary>
/// 1回のミーティングで完璧なショット（外さずに）を決めるトロフィー
/// </summary>
public class GuesserPerfectShotTrophy : SuperTrophyAbility<GuesserPerfectShotTrophy>
{
    public override TrophiesEnum TrophyId => TrophiesEnum.GuesserPerfectShot;
    public override TrophyRank TrophyRank => TrophyRank.Bronze;

    public override Type[] TargetAbilities => [typeof(GuesserAbility)];

    private bool _hasMisfired = false;
    private int _shotsThisMeeting = 0;

    private EventListener<MeetingStartEventData> _onMeetingStartEvent;
    private EventListener<MeetingCloseEventData> _onMeetingCloseEvent;
    private EventListener<GuesserShotEventData> _onGuesserShotEvent;
    private EventListener<EndGameEventData> _onEndGameEvent;

    public override void OnRegister()
    {
        _onMeetingStartEvent = MeetingStartEvent.Instance.AddListener(HandleMeetingStartEvent);
        _onMeetingCloseEvent = MeetingCloseEvent.Instance.AddListener(HandleMeetingCloseEvent);
        _onGuesserShotEvent = GuesserShotEvent.Instance.AddListener(HandleGuesserShotEvent);
        _onEndGameEvent = EndGameEvent.Instance.AddListener(HandleEndGameEvent);
    }

    private void HandleMeetingStartEvent(MeetingStartEventData data)
    {
        _hasMisfired = false;
        _shotsThisMeeting = 0;
    }

    private void HandleGuesserShotEvent(GuesserShotEventData data)
    {
        if (data.killer != PlayerControl.LocalPlayer)
            return;

        _shotsThisMeeting++;

        if (data.isMisFire)
        {
            _hasMisfired = true;
        }
    }

    private void HandleMeetingCloseEvent(MeetingCloseEventData data)
    {
        // ミーティングが終了したとき、ミスなしで少なくとも1回以上撃っていればトロフィー獲得
        if (!_hasMisfired && _shotsThisMeeting > 0)
            Complete();
    }

    private void HandleEndGameEvent(EndGameEventData data)
    {
        if (!_hasMisfired && _shotsThisMeeting > 0)
            Complete();
    }

    public override void OnDetached()
    {
        if (_onMeetingStartEvent != null)
        {
            MeetingStartEvent.Instance.RemoveListener(_onMeetingStartEvent);
            _onMeetingStartEvent = null;
        }

        if (_onMeetingCloseEvent != null)
        {
            MeetingCloseEvent.Instance.RemoveListener(_onMeetingCloseEvent);
            _onMeetingCloseEvent = null;
        }

        if (_onGuesserShotEvent != null)
        {
            GuesserShotEvent.Instance.RemoveListener(_onGuesserShotEvent);
            _onGuesserShotEvent = null;
        }

        if (_onEndGameEvent != null)
        {
            EndGameEvent.Instance.RemoveListener(_onEndGameEvent);
            _onEndGameEvent = null;
        }
    }
}
/// <summary>
/// 1回のミーティングで3人以上キルするトロフィー
/// </summary>
public class GuesserTripleKillTrophy : SuperTrophyAbility<GuesserTripleKillTrophy>
{
    public override TrophiesEnum TrophyId => TrophiesEnum.GuesserTripleKill;
    public override TrophyRank TrophyRank => TrophyRank.Silver;

    public override Type[] TargetAbilities => [typeof(GuesserAbility)];

    private int _killsThisMeeting = 0;
    private const int RequiredKills = 3;

    private EventListener<MeetingStartEventData> _onMeetingStartEvent;
    private EventListener<GuesserShotEventData> _onGuesserShotEvent;

    public override void OnRegister()
    {
        _onMeetingStartEvent = MeetingStartEvent.Instance.AddListener(HandleMeetingStartEvent);
        _onGuesserShotEvent = GuesserShotEvent.Instance.AddListener(HandleGuesserShotEvent);
    }

    private void HandleMeetingStartEvent(MeetingStartEventData data)
    {
        _killsThisMeeting = 0;
    }

    private void HandleGuesserShotEvent(GuesserShotEventData data)
    {
        if (data.killer != PlayerControl.LocalPlayer || data.isMisFire)
            return;

        _killsThisMeeting++;

        if (_killsThisMeeting >= RequiredKills)
        {
            Complete();
        }
    }

    public override void OnDetached()
    {
        if (_onMeetingStartEvent != null)
        {
            MeetingStartEvent.Instance.RemoveListener(_onMeetingStartEvent);
            _onMeetingStartEvent = null;
        }

        if (_onGuesserShotEvent != null)
        {
            GuesserShotEvent.Instance.RemoveListener(_onGuesserShotEvent);
            _onGuesserShotEvent = null;
        }
    }
}

/// <summary>
/// インポスターを100人以上当てるトロフィー
/// </summary>
public class GuesserImpostorHunterTrophy : SuperTrophyAbility<GuesserImpostorHunterTrophy>
{
    public override TrophiesEnum TrophyId => TrophiesEnum.GuesserImpostorHunter;
    public override TrophyRank TrophyRank => TrophyRank.Gold;

    public override Type[] TargetAbilities => [typeof(GuesserAbility)];

    private EventListener<GuesserShotEventData> _onGuesserShotEvent;
    private const int RequiredImpostorKills = 100;

    public override void OnRegister()
    {
        _onGuesserShotEvent = GuesserShotEvent.Instance.AddListener(HandleGuesserShotEvent);
    }

    private void HandleGuesserShotEvent(GuesserShotEventData data)
    {
        if (data.killer != PlayerControl.LocalPlayer || data.isMisFire)
            return;

        if (((ExPlayerControl)data.target).IsImpostor())
        {
            TrophyData++;

            if (TrophyData >= RequiredImpostorKills)
            {
                Complete();
            }
        }
    }

    public override void OnDetached()
    {
        if (_onGuesserShotEvent != null)
        {
            GuesserShotEvent.Instance.RemoveListener(_onGuesserShotEvent);
            _onGuesserShotEvent = null;
        }
    }
}

/// <summary>
/// 最初の会議で正確な推測をして誰かをキルするトロフィー
/// </summary>
public class GuesserFirstMeetingKillTrophy : SuperTrophyAbility<GuesserFirstMeetingKillTrophy>
{
    public override TrophiesEnum TrophyId => TrophiesEnum.GuesserFirstMeetingKill;
    public override TrophyRank TrophyRank => TrophyRank.Bronze;

    public override Type[] TargetAbilities => [typeof(GuesserAbility)];

    private EventListener<MeetingStartEventData> _onMeetingStartEvent;
    private EventListener<GuesserShotEventData> _onGuesserShotEvent;
    private bool _isFirstMeeting = false;

    public override void OnRegister()
    {
        _onMeetingStartEvent = MeetingStartEvent.Instance.AddListener(HandleMeetingStartEvent);
        _onGuesserShotEvent = GuesserShotEvent.Instance.AddListener(HandleGuesserShotEvent);
    }

    private void HandleMeetingStartEvent(MeetingStartEventData data)
    {
        _isFirstMeeting = data.meetingCount == 0;
    }

    private void HandleGuesserShotEvent(GuesserShotEventData data)
    {
        if (!_isFirstMeeting || data.killer != PlayerControl.LocalPlayer || data.isMisFire)
            return;

        Complete();
    }

    public override void OnDetached()
    {
        if (_onMeetingStartEvent != null)
        {
            MeetingStartEvent.Instance.RemoveListener(_onMeetingStartEvent);
            _onMeetingStartEvent = null;
        }

        if (_onGuesserShotEvent != null)
        {
            GuesserShotEvent.Instance.RemoveListener(_onGuesserShotEvent);
            _onGuesserShotEvent = null;
        }
    }
}

/// <summary>
/// 連続5回正確な推測をするトロフィー
/// </summary>
public class GuesserSharpshooterTrophy : SuperTrophyAbility<GuesserSharpshooterTrophy>
{
    public override TrophiesEnum TrophyId => TrophiesEnum.GuesserSharpshooter;
    public override TrophyRank TrophyRank => TrophyRank.Platinum;

    public override Type[] TargetAbilities => [typeof(GuesserAbility)];

    private EventListener<GuesserShotEventData> _onGuesserShotEvent;
    private const int RequiredConsecutiveShots = 5;

    public override void OnRegister()
    {
        _onGuesserShotEvent = GuesserShotEvent.Instance.AddListener(HandleGuesserShotEvent);
    }

    private void HandleGuesserShotEvent(GuesserShotEventData data)
    {
        if (data.killer != PlayerControl.LocalPlayer)
            return;

        if (data.isMisFire)
        {
            // 連続成功が途切れたらリセット
            TrophyData = 0;
        }
        else
        {
            // 成功したら連続カウントを増やす
            TrophyData++;

            if (TrophyData >= RequiredConsecutiveShots)
            {
                Complete();
            }
        }
    }

    public override void OnDetached()
    {
        if (_onGuesserShotEvent != null)
        {
            GuesserShotEvent.Instance.RemoveListener(_onGuesserShotEvent);
            _onGuesserShotEvent = null;
        }
    }
}