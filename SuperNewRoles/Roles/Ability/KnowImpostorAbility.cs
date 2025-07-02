using System;
using UnityEngine;
using SuperNewRoles.Modules;
using SuperNewRoles.Modules.Events.Bases;
using SuperNewRoles.Events;

namespace SuperNewRoles.Roles.Ability;

public class KnowImpostorAbility : AbilityBase
{
    public Func<bool> CanKnowImpostors { get; }
    private EventListener<NameTextUpdateEventData> _nameTextUpdateEvent;

    public KnowImpostorAbility(Func<bool> canKnowImpostors)
    {
        CanKnowImpostors = canKnowImpostors;
    }

    public override void AttachToLocalPlayer()
    {
        _nameTextUpdateEvent = NameTextUpdateEvent.Instance.AddListener(OnNameTextUpdate);
    }

    private void OnNameTextUpdate(NameTextUpdateEventData data)
    {
        if (CanKnowImpostors())
            UpdateImpostorNameColors(Palette.ImpostorRed);
    }

    private void UpdateImpostorNameColors(Color color)
    {
        foreach (var player in ExPlayerControl.ExPlayerControls)
        {
            if (player.IsImpostor())
                NameText.SetNameTextColor(player, color);
        }
    }

    public override void DetachToLocalPlayer()
    {
        base.DetachToLocalPlayer();
        NameTextUpdateEvent.Instance.RemoveListener(_nameTextUpdateEvent);
    }
}
