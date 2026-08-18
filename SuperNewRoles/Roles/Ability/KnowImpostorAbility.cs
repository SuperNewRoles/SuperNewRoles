using System;
using UnityEngine;
using SuperNewRoles.Modules;
using SuperNewRoles.Events;

namespace SuperNewRoles.Roles.Ability;

public class KnowImpostorAbility : AbilityBase
{
    public Func<bool> CanKnowImpostors { get; }

    public KnowImpostorAbility(Func<bool> canKnowImpostors)
    {
        CanKnowImpostors = canKnowImpostors;
    }

    public override void AttachToLocalPlayer()
    {
        SubscribeWithAbility(NameTextUpdateEvent.Instance, OnNameTextUpdate);
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

}
