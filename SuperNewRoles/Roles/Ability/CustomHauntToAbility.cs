using System;
using SuperNewRoles.Events;
using SuperNewRoles.Modules;
using SuperNewRoles.Modules.Events.Bases;
using UnityEngine;

namespace SuperNewRoles.Roles.Ability;

public class CustomHauntToAbility : AbilityBase
{
    private EventListener _fixedUpdateEvent;
    private EventListener<MeetingStartEventData> _meetingStartEvent;
    private Func<ExPlayerControl> _target;
    private Vector2 Offset;
    private bool lastEnabled = false;
    public CustomHauntToAbility(Func<ExPlayerControl> target)
    {
        _target = target;

        Offset = UnityEngine.Random.insideUnitCircle;
        if (Offset.magnitude < 0.2f)
        {
            Offset = Offset.normalized * 0.2f;
        }
        else if (Offset.magnitude > 0.5f)
        {
            Offset = Offset.normalized * 0.5f;
        }
    }

    public override void AttachToLocalPlayer()
    {
        base.AttachToLocalPlayer();
        _fixedUpdateEvent = FixedUpdateEvent.Instance.AddListener(OnFixedUpdate);
        _meetingStartEvent = MeetingStartEvent.Instance.AddListener(OnMeetingStart);
    }

    public override void DetachToLocalPlayer()
    {
        base.DetachToLocalPlayer();
        _fixedUpdateEvent?.RemoveListener();
        _meetingStartEvent?.RemoveListener();
        if (lastEnabled)
            ExPlayerControl.LocalPlayer.Player.moveable = true;
    }

    private void OnFixedUpdate()
    {
        if (MeetingHud.Instance != null || ExileController.Instance != null) return;
        ExPlayerControl target = _target?.Invoke();
        if (target == null)
        {
            if (lastEnabled)
                ExPlayerControl.LocalPlayer.Player.moveable = true;
            lastEnabled = false;
            return;
        }
        lastEnabled = true;
        ExPlayerControl.LocalPlayer.Player.moveable = false;
        Vector2 val = Vector2.zero;
        Vector2 val2 = target.GetTruePosition() + Offset;
        Vector2 truePosition = PlayerControl.LocalPlayer.GetTruePosition();
        Vector2 val3 = PlayerControl.LocalPlayer.MyPhysics.GetVelocity() / PlayerControl.LocalPlayer.MyPhysics.TrueSpeed;
        Vector2 val4 = val2 - truePosition;
        float magnitude = val4.magnitude;
        if (magnitude > 0.05f)
        {
            val4 = val4.normalized * Mathf.Clamp(magnitude, 0.75f, 4f);
            val = val3 * 0.8f + val4 * 0.2f;
        }
        else
        {
            val *= 0.7f;
        }
        PlayerControl.LocalPlayer.MyPhysics.SetNormalizedVelocity(val);
    }
    private void OnMeetingStart(MeetingStartEventData _)
    {
        lastEnabled = false;
    }
}
