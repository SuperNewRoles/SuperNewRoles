using System;
using SuperNewRoles.Events;
using SuperNewRoles.Modules;
using SuperNewRoles.Modules.Events.Bases;

namespace SuperNewRoles.Roles.Ability;

public class HideRoleOnGhostAbility : AbilityBase
{
    private Func<ExPlayerControl, bool> _isHideRole;

    public HideRoleOnGhostAbility(Func<ExPlayerControl, bool> isHideRole)
    {
        _isHideRole = isHideRole;
    }

    public bool IsHideRole(ExPlayerControl player)
    {
        return _isHideRole?.Invoke(player) ?? false;
    }
}