using System;
using SuperNewRoles.Events;
using SuperNewRoles.Modules;
using UnityEngine;

namespace SuperNewRoles.Roles.Ability;

public class DisableOtherPlayerInfoAbility : AbilityBase
{
    private Func<bool> _isAvailable;
    public DisableOtherPlayerInfoAbility(Func<bool> isAvailable)
    {
        _isAvailable = isAvailable ?? (() => true);
    }

    public override void AttachToLocalPlayer()
    {
        base.AttachToLocalPlayer();
        SubscribeWithAbility(NameTextUpdateEvent.Instance, OnNameTextUpdate);
    }

    private void OnNameTextUpdate(NameTextUpdateEventData data)
    {
        if (data.Player == ExPlayerControl.LocalPlayer) return;
        if (!_isAvailable()) return;
        if (data.Player.PlayerInfoText != null)
            data.Player.PlayerInfoText.enabled = false;
        if (data.Player.MeetingInfoText != null)
            data.Player.MeetingInfoText.enabled = false;
        data.Player.cosmetics.nameText.color = Color.white;
        data.Player.cosmetics.nameText.text = data.Player.Data.PlayerName;
        if (data.Player.VoteArea != null)
        {
            data.Player.VoteArea.NameText.color = Color.white;
            data.Player.VoteArea.NameText.text = data.Player.Data.PlayerName;
        }
    }
}
