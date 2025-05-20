using System;
using SuperNewRoles.Events;
using SuperNewRoles.Events.PCEvents;
using SuperNewRoles.Modules.Events.Bases;
using UnityEngine;

namespace SuperNewRoles.Roles.Ability;

public class CanUseReportButtonAbility : AbilityBase
{
    private Func<bool> _canUseReportButton;
    private EventListener _fixedUpdateListener;
    private EventListener _hudUpdateListener;
    private EventListener<DieEventData> _dieListener;
    private bool _lastCanUseReportButton;
    public CanUseReportButtonAbility(Func<bool> canUseReportButton)
    {
        _canUseReportButton = canUseReportButton;
    }

    public override void AttachToLocalPlayer()
    {
        base.AttachToLocalPlayer();
        _fixedUpdateListener = FixedUpdateEvent.Instance.AddListener(OnUpdate);
        _hudUpdateListener = HudUpdateEvent.Instance.AddListener(OnUpdate);
        _dieListener = DieEvent.Instance.AddListener(OnDie);
    }

    public override void DetachToLocalPlayer()
    {
        base.DetachToLocalPlayer();
        _fixedUpdateListener?.RemoveListener();
        _hudUpdateListener?.RemoveListener();
        _dieListener?.RemoveListener();
    }

    private void OnUpdate()
    {
        bool canUseReportButton = _canUseReportButton();
        if (canUseReportButton != _lastCanUseReportButton)
        {
            foreach (DeadBody deadBody in GameObject.FindObjectsOfType<DeadBody>())
            {
                deadBody.Reported = !canUseReportButton;
            }
        }
        if (!canUseReportButton)
        {
            HudManager.Instance.ReportButton.gameObject.SetActive(canUseReportButton);
            _lastCanUseReportButton = canUseReportButton;
        }
    }

    private void OnDie(DieEventData data)
    {
        if (_canUseReportButton()) return;
        foreach (DeadBody deadBody in GameObject.FindObjectsOfType<DeadBody>())
        {
            deadBody.Reported = true;
        }
    }
}
