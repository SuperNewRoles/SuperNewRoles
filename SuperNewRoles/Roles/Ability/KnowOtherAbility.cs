using System;
using UnityEngine;
using SuperNewRoles.Modules;
using SuperNewRoles.Events;

namespace SuperNewRoles.Roles.Ability;

public class KnowOtherAbility : AbilityBase
{
    public Func<ExPlayerControl, bool> CanKnowOther { get; }
    public Func<bool> IsShowRole { get; }
    public Func<ExPlayerControl, Color32>? Color { get; }

    public KnowOtherAbility(Func<ExPlayerControl, bool> canKnowOther, Func<bool> isShowRole, Func<ExPlayerControl, Color32>? color = null)
    {
        CanKnowOther = canKnowOther;
        IsShowRole = isShowRole;
        Color = color;
    }

    public override void AttachToLocalPlayer()
    {
        SubscribeWithAbility(NameTextUpdateEvent.Instance, OnNameTextUpdate);
        SubscribeWithAbility(NameTextUpdateVisiableEvent.Instance, OnNameTextUpdateVisiable);
    }

    private void OnNameTextUpdate(NameTextUpdateEventData data)
    {
        if (!CanKnowOther(data.Player)) return;
        Color32 color = Color?.Invoke(data.Player) ?? data.Player.roleBase.RoleColor;
        NameText.SetNameTextColor(data.Player, color);
    }

    private void OnNameTextUpdateVisiable(NameTextUpdateVisiableEventData data)
    {
        if (!CanKnowOther(data.Player)) return;
        if (IsShowRole())
            NameText.UpdateVisible(data.Player, data.Visiable);
    }

}
