using System;
using UnityEngine;
using SuperNewRoles.Modules;
using SuperNewRoles.Modules.Events.Bases;
using SuperNewRoles.Events;

namespace SuperNewRoles.Roles.Ability;

public class KnowOtherAbility : AbilityBase
{
    public Func<ExPlayerControl, bool> CanKnowOther { get; }
    public Func<bool> IsShowRole { get; }
    public Func<ExPlayerControl, Color32>? Color { get; }
    private EventListener<NameTextUpdateEventData> _nameTextUpdateEvent;
    private EventListener<NameTextUpdateVisiableEventData> _nameTextUpdateVisiableEvent;

    public KnowOtherAbility(Func<ExPlayerControl, bool> canKnowOther, Func<bool> isShowRole, Func<ExPlayerControl, Color32>? color = null)
    {
        CanKnowOther = canKnowOther;
        IsShowRole = isShowRole;
        Color = color;
    }

    public override void AttachToLocalPlayer()
    {
        _nameTextUpdateEvent = NameTextUpdateEvent.Instance.AddListener(OnNameTextUpdate);
        _nameTextUpdateVisiableEvent = NameTextUpdateVisiableEvent.Instance.AddListener(OnNameTextUpdateVisiable);
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
            NameText.UpdateVisiable(data.Player, data.Visiable);
    }

    public override void DetachToLocalPlayer()
    {
        base.DetachToLocalPlayer();
        _nameTextUpdateEvent?.RemoveListener();
        _nameTextUpdateVisiableEvent?.RemoveListener();
    }
}
