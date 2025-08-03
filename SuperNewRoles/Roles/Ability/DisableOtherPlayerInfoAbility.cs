using System;
using SuperNewRoles.Events;
using SuperNewRoles.Modules;
using SuperNewRoles.Modules.Events.Bases;
using UnityEngine;

namespace SuperNewRoles.Roles.Ability;

public class DisableOtherPlayerInfoAbility : AbilityBase
{
    private EventListener<NameTextUpdateEventData> _nameTextUpdateEvent;
    private Func<bool> _isAvailable;
    public DisableOtherPlayerInfoAbility(Func<bool> isAvailable)
    {
        _isAvailable = isAvailable ?? (() => true);
    }

    public override void AttachToLocalPlayer()
    {
        base.AttachToLocalPlayer();
        _nameTextUpdateEvent = NameTextUpdateEvent.Instance.AddListener(OnNameTextUpdate);
    }

    public override void DetachToLocalPlayer()
    {
        base.DetachToLocalPlayer();
        _nameTextUpdateEvent?.RemoveListener();
    }

    private void OnNameTextUpdate(NameTextUpdateEventData data)
    {
        if (data.Player == ExPlayerControl.LocalPlayer) return;
        if (!_isAvailable()) return;
        if (data.Player.PlayerInfoText != null)
            data.Player.PlayerInfoText.enabled = false;
        if (data.Player.PlayerInfoText != null)
            data.Player.PlayerInfoText.enabled = false;
        data.Player.cosmetics.nameText.color = Color.white;
        data.Player.cosmetics.nameText.text = data.Player.Data.PlayerName;
    }
}
