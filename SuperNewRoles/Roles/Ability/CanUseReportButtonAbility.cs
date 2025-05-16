using System;
using SuperNewRoles.Events;
using SuperNewRoles.Modules.Events.Bases;
using UnityEngine;

namespace SuperNewRoles.Roles.Ability;

public class CanUseReportButtonAbility : AbilityBase
{
    private Func<bool> _canUseReportButton;
    private EventListener _fixedUpdateListener;
    private EventListener _hudUpdateListener;
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
    }

    public override void DetachToLocalPlayer()
    {
        base.DetachToLocalPlayer();
        _fixedUpdateListener?.RemoveListener();
        _hudUpdateListener?.RemoveListener();
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
}
