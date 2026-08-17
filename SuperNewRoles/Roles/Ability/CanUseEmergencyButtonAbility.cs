using System;
using SuperNewRoles.Events;

namespace SuperNewRoles.Roles.Ability;

public class CanUseEmergencyButtonAbility : AbilityBase
{
    private Func<bool> _canUseMeetingButton;
    private Func<string> _cannotUseReason;
    public CanUseEmergencyButtonAbility(Func<bool> canUseMeetingButton, Func<string> cannotUseReason)
    {
        _canUseMeetingButton = canUseMeetingButton;
        _cannotUseReason = cannotUseReason;
    }

    public override void AttachToLocalPlayer()
    {
        base.AttachToLocalPlayer();
        SubscribeWithAbility(EmergencyCheckEvent.Instance, OnEmergencyCheck);
    }

    private void OnEmergencyCheck(EmergencyCheckEventData data)
    {
        if (!_canUseMeetingButton())
        {
            data.RefEnabledEmergency = false;
            data.RefEmergencyTexts.Add(_cannotUseReason());
        }
    }
}
