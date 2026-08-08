using System;
using System.Collections.Generic;
using SuperNewRoles.Modules;
using SuperNewRoles.Modules.Events.Bases;

namespace SuperNewRoles.Roles.Ability;

public abstract class AbilityBase
{
    private readonly List<IEventListener> _eventListeners = new();

    public ulong AbilityId { get; protected set; }
    public ExPlayerControl Player => Parent.Player;
    public AbilityParentBase Parent { get; private set; }

    public int Count { get; set; }
    public bool HasCount => Count > 0;

    /// <summary>
    /// Ability が所有する引数なしイベントを登録します。登録した Listener は Detach 時に自動解除されます。
    /// </summary>
    protected EventListener SubscribeWithAbility<TEvent>(EventTargetBase<TEvent> eventTarget, Action action)
        where TEvent : InternalEventTargetBase<TEvent, EventListener>, new()
    {
        return TrackEventListener(eventTarget.AddListener(action));
    }

    /// <summary>
    /// Ability が所有するデータ付きイベントを登録します。登録した Listener は Detach 時に自動解除されます。
    /// </summary>
    protected EventListener<TData> SubscribeWithAbility<TEvent, TData>(EventTargetBase<TEvent, TData> eventTarget, Action<TData> action)
        where TEvent : InternalEventTargetBase<TEvent, EventListener<TData>>, new()
        where TData : IEventData
    {
        return TrackEventListener(eventTarget.AddListener(action));
    }

    /// <summary>
    /// 外部で生成した Listener を Ability のライフサイクルに紐付けます。
    /// </summary>
    protected TListener TrackEventListener<TListener>(TListener listener) where TListener : IEventListener
    {
        if (listener != null)
            _eventListeners.Add(listener);
        return listener;
    }

    protected void RemoveEventListeners()
    {
        for (var i = _eventListeners.Count - 1; i >= 0; i--)
            _eventListeners[i]?.RemoveListener();
        _eventListeners.Clear();
    }

    protected virtual bool IsLocalPlayer(PlayerControl player) => player == PlayerControl.LocalPlayer;

    public void Attach(PlayerControl player, ulong abilityId, AbilityParentBase parent)
    {
        Parent = parent;
        AbilityId = abilityId;
        try
        {
            if (IsLocalPlayer(player))
                AttachToLocalPlayer();
            else
                AttachToOthers();
            AttachToAlls();
            OnAttached(player);
        }
        catch (Exception ex)
        {
            LogLifecycleError(nameof(Attach), ex);
        }
    }

    protected virtual void OnAttached(PlayerControl player) { }

    public virtual void AttachToLocalPlayer() { }

    public virtual void AttachToOthers() { }

    public virtual void AttachToAlls() { }

    public void Detach()
    {
        try
        {
            try
            {
                if (IsLocalPlayer(Player))
                    DetachToLocalPlayer();
                else
                    DetachToOthers();
                DetachToAlls();
            }
            finally
            {
                RemoveEventListeners();
            }
        }
        catch (Exception ex)
        {
            LogLifecycleError(nameof(Detach), ex);
        }
    }

    private void LogLifecycleError(string operation, Exception ex)
        => Logger.Error(
            $"Ability lifecycle failure during {operation}: ability={GetType().FullName}, abilityId={AbilityId}\n{ex}",
            "SNR.Ability");

    public virtual void DetachToLocalPlayer() { }
    public virtual void DetachToOthers() { }
    public virtual void DetachToAlls() { }
}

/// <summary>
/// ExPlayerControl が複数の同種 Ability から判断値を選ぶ場合にのみ使用します。
/// </summary>
public interface IPrioritizedAbility
{
    int Priority { get; }
}

internal static class AbilityPriorityGroup<T> where T : AbilityBase, IPrioritizedAbility
{
    // Each resolver pays the runtime type lookup only once per closed generic type.
    internal static readonly int Id = AbilityPriorityGroupRegistry.GetGroupId(typeof(T));
}

internal static class AbilityPriorityGroupRegistry
{
    private static readonly Dictionary<Type, Type> ResolvedGroupTypes = new();
    private static readonly Dictionary<Type, int> GroupIds = new();
    private static readonly object Sync = new();
    private static int _nextGroupId;

    internal static int GetGroupId(Type abilityType)
    {
        lock (Sync)
        {
            if (!ResolvedGroupTypes.TryGetValue(abilityType, out var groupType))
            {
                groupType = FindGroupType(abilityType);
                ResolvedGroupTypes[abilityType] = groupType;
            }

            if (!GroupIds.TryGetValue(groupType, out var groupId))
            {
                groupId = _nextGroupId++;
                GroupIds[groupType] = groupId;
            }
            return groupId;
        }
    }

    private static Type FindGroupType(Type abilityType)
    {
        // A derived ability belongs to the first base type that introduced
        // IPrioritizedAbility (for example, every CustomVentAbility subclass).
        for (var current = abilityType;
             current != null && typeof(AbilityBase).IsAssignableFrom(current);
             current = current.BaseType)
        {
            if (!typeof(IPrioritizedAbility).IsAssignableFrom(current)) continue;
            if (current.BaseType == null || !typeof(IPrioritizedAbility).IsAssignableFrom(current.BaseType))
                return current;
        }

        throw new InvalidOperationException($"{abilityType.FullName} does not implement {nameof(IPrioritizedAbility)}.");
    }
}

public static class AbilityPriority
{
    public const int Default = 0;
    public const int Modifier = 100;
    public const int GhostRole = 200;
    public const int RuntimeOverride = 300;
}
