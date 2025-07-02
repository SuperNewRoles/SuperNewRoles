using System;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles;

namespace SuperNewRoles.SuperTrophies;

public interface ISuperTrophy
{
    public TrophiesEnum TrophyId { get; }
    public TrophyRank TrophyRank { get; }
    public bool Completed { get; set; }
    public long TrophyData { get; set; }
    public void Complete();
}
public interface ISuperTrophyAbility : ISuperTrophy
{
    public Type[] TargetAbilities { get; }
    public abstract void OnRegister();
    public abstract void OnDetached();
}
public interface ISuperTrophyRole : ISuperTrophy
{
    public RoleId[] TargetRoles { get; }
    public abstract void OnRegister();
    public abstract void OnDetached();
}
public interface ISuperTrophyModifier : ISuperTrophy
{
    public ModifierRoleId[] TargetModifiers { get; }
    public abstract void OnRegister();
    public abstract void OnDetached();
}
public interface ISuperTrophyGhostRole : ISuperTrophy
{
    public GhostRoleId[] TargetGhostRoles { get; }
    public abstract void OnRegister();
    public abstract void OnDetached();
}

public abstract class SuperTrophyBase<T> : BaseSingleton<T>, ISuperTrophy where T : SuperTrophyBase<T>, new()
{
    public abstract TrophiesEnum TrophyId { get; }
    public abstract TrophyRank TrophyRank { get; }
    public bool Completed { get; set; }
    public long TrophyData { get; set; }
    public void Complete()
    {
        SuperTrophyManager.CompleteTrophy(this);
    }
    public void InComplete()
    {
        SuperTrophyManager.InCompleteTrophy(this);
    }
}

public abstract class SuperTrophyAbility<T> : SuperTrophyBase<T>, ISuperTrophyAbility where T : SuperTrophyAbility<T>, new()
{
    public abstract Type[] TargetAbilities { get; }
    public abstract void OnRegister();
    public abstract void OnDetached();
}

public abstract class SuperTrophyRole<T> : SuperTrophyBase<T>, ISuperTrophyRole where T : SuperTrophyRole<T>, new()
{
    public abstract RoleId[] TargetRoles { get; }
    public abstract void OnRegister();
    public abstract void OnDetached();
}

public abstract class SuperTrophyModifier<T> : SuperTrophyBase<T>, ISuperTrophyModifier where T : SuperTrophyModifier<T>, new()
{
    public abstract ModifierRoleId[] TargetModifiers { get; }
    public abstract void OnRegister();
    public abstract void OnDetached();
}

public abstract class SuperTrophyGhostRole<T> : SuperTrophyBase<T>, ISuperTrophyGhostRole where T : SuperTrophyGhostRole<T>, new()
{
    public abstract GhostRoleId[] TargetGhostRoles { get; }
    public abstract void OnRegister();
    public abstract void OnDetached();
}
