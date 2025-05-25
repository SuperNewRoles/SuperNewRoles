using System;
using SuperNewRoles.Events;
using SuperNewRoles.Modules.Events.Bases;

namespace SuperNewRoles.Roles.Ability;

public class CanUseEmergencyButtonAbility : AbilityBase
{
    private Func<bool> _canUseMeetingButton;
    private EventListener<EmergencyCheckEventData> _meetingMinigameEvent;
    private Func<string> _cannotUseReason;
    public CanUseEmergencyButtonAbility(Func<bool> canUseMeetingButton, Func<string> cannotUseReason)
    {
        _canUseMeetingButton = canUseMeetingButton;
        _cannotUseReason = cannotUseReason;
    }

    public override void AttachToLocalPlayer()
    {
        base.AttachToLocalPlayer();
        _meetingMinigameEvent = EmergencyCheckEvent.Instance.AddListener(OnEmergencyCheck);
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