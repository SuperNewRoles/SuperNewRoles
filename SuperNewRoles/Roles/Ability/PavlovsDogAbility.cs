using System;
using SuperNewRoles.Events;
using SuperNewRoles.Events.PCEvents;
using SuperNewRoles.Modules;
using SuperNewRoles.Modules.Events.Bases;
using SuperNewRoles.Roles.Neutral;
using TMPro;
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
    private bool _isRampage => ownerAbility?.Player == null || ownerAbility.Player.IsDead();
    private float _lastKillTime;
    private float _timer;
    private CustomVentAbility _ventAbility;
    private CustomKillButtonAbility _killAbility;
    private ImpostorVisionAbility _visionAbility;
    private KnowOtherAbility _knowOtherAbility;
    private EventListener<DieEventData> _dieEventListener;
    private EventListener<MeetingCloseEventData> _meetingCloseListener;
    private EventListener _fixedUpdateListener;
    public PavlovsOwnerAbility ownerAbility;
    private TextMeshPro timerText;
    public PavlovsDogAbility(PavlovsDogData data)
    {
        _data = data;
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
                _timer = _data.rampageSuicideTime;
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
        _timer = _data.rampageSuicideTime;

        if (player.AmOwner)
        {
            timerText = GameObject.Instantiate(FastDestroyableSingleton<HudManager>.Instance.KillButton.cooldownTimerText, _killAbility.actionButton.transform);
            timerText.text = "";
            timerText.enableWordWrapping = false;
            timerText.transform.localScale = Vector3.one * 0.5f;
            timerText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);
            timerText.gameObject.SetActive(true);
            timerText.text = "";
        }
    }

    public override void AttachToLocalPlayer()
    {
        _dieEventListener = DieEvent.Instance.AddListener(OnPlayerDead);
        _meetingCloseListener = MeetingCloseEvent.Instance.AddListener(x => OnMeetingEnd());
        _fixedUpdateListener = FixedUpdateEvent.Instance.AddListener(OnFixedUpdate);
    }

    private void OnPlayerDead(DieEventData data)
    {
        ExPlayerControl exPlayer = (ExPlayerControl)data.player;
        if (ownerAbility?.Player != null && exPlayer.PlayerId == ownerAbility.Player.PlayerId)
        {
            _timer = _data.rampageSuicideTime;
        }
    }

    private void OnFixedUpdate()
    {
        if (Player.IsDead())
        {
            timerText.text = "";
            return;
        }
        if (_isRampage && MeetingHud.Instance == null && ExileController.Instance == null)
        {
            _timer -= Time.fixedDeltaTime;
            timerText.text = ModTranslation.GetString("PavlovsDogTimerText", _timer.ToString("F0"));
            if (_timer <= 0f)
            {
                ExPlayerControl exLocalPlayer = (ExPlayerControl)Player;
                exLocalPlayer.RpcCustomDeath(CustomDeathType.Suicide);
            }
        }
        else
            timerText.text = "";
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
        FixedUpdateEvent.Instance.RemoveListener(_fixedUpdateListener);
    }
}