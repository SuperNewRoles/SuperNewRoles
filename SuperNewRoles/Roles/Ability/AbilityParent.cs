using System;
using SuperNewRoles.Modules;

namespace SuperNewRoles.Roles.Ability;

public abstract class AbilityParentBase
{
    public abstract ExPlayerControl Player { get; set; }
}

public class AbilityParentPlayer : AbilityParentBase
{
    public override ExPlayerControl Player { get; set; }
    public AbilityParentPlayer(ExPlayerControl player)
    {
        Player = player;
    }
}

public class AbilityParentRole : AbilityParentBase
{
    public override ExPlayerControl Player { get; set; }
    public IRoleBase ParentRole { get; }
    public AbilityParentRole(ExPlayerControl player, IRoleBase role)
    {
        Player = player;
        ParentRole = role;
    }
}
public class AbilityParentModifier : AbilityParentBase
{
    public override ExPlayerControl Player { get; set; }
    public IModifierBase ParentModifier { get; }
    public AbilityParentModifier(ExPlayerControl player, IModifierBase modifier)
    {
        Player = player;
        ParentModifier = modifier;
    }
}
public class AbilityParentGhostRole : AbilityParentBase
{
    public override ExPlayerControl Player { get; set; }
    public IGhostRoleBase ParentGhostRole { get; }
    public AbilityParentGhostRole(ExPlayerControl player, IGhostRoleBase ghostRole)
    {
        Player = player;
        ParentGhostRole = ghostRole;
    }
}

public class AbilityParentAbility : AbilityParentBase
{
    public override ExPlayerControl Player { get => ParentAbility.Player; set => throw new Exception("AbilityParentAbilityはPlayerを設定できません"); }
    public AbilityBase ParentAbility { get; }
    public AbilityParentAbility(AbilityBase ability)
    {
        ParentAbility = ability;
    }
}
