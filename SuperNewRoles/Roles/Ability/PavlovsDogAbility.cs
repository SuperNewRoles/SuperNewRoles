using System;
using SuperNewRoles.Events;
using SuperNewRoles.Events.PCEvents;
using SuperNewRoles.Modules;
using SuperNewRoles.Modules.Events.Bases;
using SuperNewRoles.Roles.Neutral;
using UnityEngine;

namespace SuperNewRoles.Roles.Ability;

public record PavlovsDogData(
    bool canUseVent,
    bool isImpostorVision,
    float killCooldown,
    float rampageKillCooldown,
    float rampageSuicideTime,
    bool resetSuicideTimeOnMeeting
);

public class PavlovsDogAbility : AbilityBase
{
    private readonly PavlovsDogData _data;
    private bool _isRampage;
    private float _lastKillTime;
    private float _timer;
    private CustomVentAbility _ventAbility;
    private CustomKillButtonAbility _killAbility;
    private ImpostorVisionAbility _visionAbility;
    private KnowOtherAbility _knowOtherAbility;
    private EventListener<DieEventData> _dieEventListener;
    private EventListener _meetingCloseListener;
    public PavlovsOwnerAbility ownerAbility;
    public PavlovsDogAbility(PavlovsDogData data)
    {
        _data = data;
        _isRampage = false;
        _lastKillTime = 0f;
        _timer = 0f;
    }

    public override void Attach(PlayerControl player, ulong abilityId, AbilityParentBase parent)
    {
        base.Attach(player, abilityId, parent);

        _ventAbility = new CustomVentAbility(() => _data.canUseVent);
        _visionAbility = new ImpostorVisionAbility(() => _data.isImpostorVision);
        _killAbility = new CustomKillButtonAbility(
            () => true,
            () => _isRampage ? _data.rampageKillCooldown : _data.killCooldown,
            () => false,
            isTargetable: (x) => !x.IsPavlovsTeam(),
            killedCallback: (target) =>
            {
                _lastKillTime = Time.time;
            }
        );
        _knowOtherAbility = new KnowOtherAbility(
            p => p.IsPavlovsTeam(),
            () => true
        );

        ExPlayerControl exPlayer = (ExPlayerControl)player;
        AbilityParentAbility parentAbility = new(this);
        exPlayer.AttachAbility(_ventAbility, parentAbility);
        exPlayer.AttachAbility(_visionAbility, parentAbility);
        exPlayer.AttachAbility(_killAbility, parentAbility);
        exPlayer.AttachAbility(_knowOtherAbility, parentAbility);
    }

    public override void AttachToLocalPlayer()
    {
        _dieEventListener = DieEvent.Instance.AddListener(OnPlayerDead);
        _meetingCloseListener = MeetingCloseEvent.Instance.AddListener(OnMeetingEnd);
    }

    private void OnPlayerDead(DieEventData data)
    {
        ExPlayerControl exPlayer = (ExPlayerControl)data.player;
        if (ownerAbility?.Player != null && exPlayer.PlayerId == ownerAbility.Player.PlayerId)
        {
            _isRampage = true;
            _timer = _data.rampageSuicideTime;
        }

        if (_isRampage && MeetingHud.Instance == null && ExileController.Instance == null)
        {
            _timer -= Time.fixedDeltaTime;
            if (_timer <= 0f)
            {
                ExPlayerControl exLocalPlayer = (ExPlayerControl)Player;
                exLocalPlayer.RpcCustomDeath(CustomDeathType.Suicide);
            }
        }
    }

    private void OnMeetingEnd()
    {
        if (_data.resetSuicideTimeOnMeeting && _isRampage)
        {
            _timer = _data.rampageSuicideTime;
        }
    }

    public override void DetachToLocalPlayer()
    {
        base.DetachToLocalPlayer();
        DieEvent.Instance.RemoveListener(_dieEventListener);
        MeetingCloseEvent.Instance.RemoveListener(_meetingCloseListener);
    }
}