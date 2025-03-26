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

    public KnowOtherAbility(Func<ExPlayerControl, bool> canKnowOther, Func<bool> isShowRole, Func<ExPlayerControl, Color32>? color = null)
    {
        CanKnowOther = canKnowOther;
        IsShowRole = isShowRole;
        Color = color;
    }

    public override void AttachToLocalPlayer()
    {
        _nameTextUpdateEvent = NameTextUpdateEvent.Instance.AddListener(OnNameTextUpdate);
    }

    private void OnNameTextUpdate(NameTextUpdateEventData data)
    {
        if (!CanKnowOther(data.Player)) return;
        if (IsShowRole())
            NameText.UpdateVisiable(data.Player, true);
        Color32 color = Color?.Invoke(data.Player) ?? data.Player.roleBase.RoleColor;
        UpdatePlayerNameColor(data.Player, color);
    }

    private void UpdatePlayerNameColor(ExPlayerControl player, Color color)
    {
        player.Data.Role.NameColor = color;
        player.Player.cosmetics.nameText.color = color;
    }

    public override void DetachToLocalPlayer()
    {
        base.DetachToLocalPlayer();
        NameTextUpdateEvent.Instance.RemoveListener(_nameTextUpdateEvent);
    }
}
