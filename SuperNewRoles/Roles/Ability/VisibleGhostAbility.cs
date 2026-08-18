using System;
using SuperNewRoles.Events;
using SuperNewRoles.Modules;

namespace SuperNewRoles.Roles.Ability;

class VisibleGhostAbility : AbilityBase
{
    public Func<bool> _isVisible;
    public VisibleGhostAbility(Func<bool> isVisible)
    {
        _isVisible = isVisible;
    }
    public override void AttachToLocalPlayer()
    {
        base.AttachToLocalPlayer();
        SubscribeWithAbility(FixedUpdateWithInstanceEvent.Instance, OnFixedUpdate);
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
