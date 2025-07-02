using System;
using SuperNewRoles.Events;
using SuperNewRoles.Modules;
using SuperNewRoles.Modules.Events.Bases;

namespace SuperNewRoles.Roles.Ability;

class VisibleGhostAbility : AbilityBase
{
    public EventListener<FixedUpdateWithInstanceEventData> _fixedUpdateListener;
    public Func<bool> _isVisible;
    public VisibleGhostAbility(Func<bool> isVisible)
    {
        _isVisible = isVisible;
    }
    public override void AttachToLocalPlayer()
    {
        base.AttachToLocalPlayer();
        _fixedUpdateListener = FixedUpdateWithInstanceEvent.Instance.AddListener(OnFixedUpdate);
    }

    public override void DetachToLocalPlayer()
    {
        base.DetachToLocalPlayer();
        _fixedUpdateListener?.RemoveListener();
    }

    public void OnFixedUpdate(FixedUpdateWithInstanceEventData data)
    {
        if (!_isVisible()) return;
        if (data.CurrentPlayer.IsDead())
        {
            data.CurrentPlayer.Player.Visible = true;
            data.CurrentPlayer.cosmetics.Visible = true;
            data.CurrentPlayer.cosmetics.currentBodySprite.Visible = true;
        }
    }
}
