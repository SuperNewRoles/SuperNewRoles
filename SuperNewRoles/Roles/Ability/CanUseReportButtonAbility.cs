using System;
using SuperNewRoles.Events;
using SuperNewRoles.Events.PCEvents;
using UnityEngine;

namespace SuperNewRoles.Roles.Ability;

public class CanUseReportButtonAbility : AbilityBase
{
    private Func<bool> _canUseReportButton;
    private bool _lastCanUseReportButton;
    public CanUseReportButtonAbility(Func<bool> canUseReportButton)
    {
        _canUseReportButton = canUseReportButton;
    }

    public override void AttachToLocalPlayer()
    {
        base.AttachToLocalPlayer();
        SubscribeWithAbility(FixedUpdateEvent.Instance, OnUpdate);
        SubscribeWithAbility(HudUpdateEvent.Instance, OnUpdate);
        SubscribeWithAbility(DieEvent.Instance, OnDie);
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
