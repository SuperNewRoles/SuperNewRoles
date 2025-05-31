using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperNewRoles.Modules.Events.Bases;

/// <summary>
/// 使用例：class ExampleEvent : EventTargetBase<ExampleEvent, ExampleEventData> { }
/// <para><typeparamref name="T" />: Singletonとする自分自身を指定</para>
/// <para><typeparamref name="U" />: Listenerに登録するActionの引数型(IEventDataを継承したもの)</para>
/// </summary>
public abstract class EventTargetBase<T, U> : InternalEventTargetBase<T, EventListener<U>> where T : InternalEventTargetBase<T, EventListener<U>>, new() where U : IEventData
{
    private List<EventListener<U>> _pendingRemoval = new();
    private bool _isAwaking = false;

    public void Awake(U obj)
    {
        _isAwaking = true;
        foreach (EventListener<U> listener in listeners)
        {
            if (!_pendingRemoval.Contains(listener))
                listener.Do(obj);
        }
        _isAwaking = false;

        // Awake完了後に保留中の削除を処理
        if (_pendingRemoval.Count > 0)
        {
            foreach (var listener in _pendingRemoval)
            {
                listeners.Remove(listener);
            }
            _pendingRemoval.Clear();
        }
    }

    public EventListener<U> AddListener(Action<U> action)
    {
        EventListener<U> listener = null;
        listener = new EventListener<U>(action, () => RemoveListener(listener));
        listeners.Add(listener);
        return listener;
    }

    public void RemoveListener(EventListener<U> listener)
    {
        if (_isAwaking)
        {
            // Awake中は削除をキューに入れる
            if (!_pendingRemoval.Contains(listener))
                _pendingRemoval.Add(listener);
        }
        else
        {
            listeners.Remove(listener);
        }
    }
}
/// <summary>
/// <para><typeparamref name="T" />: Singletonとする自分自身を指定</para>
/// </summary>
public abstract class EventTargetBase<T> : InternalEventTargetBase<T, EventListener> where T : InternalEventTargetBase<T, EventListener>, new()
{
    private List<EventListener> _pendingRemoval = new();
    private bool _isAwaking = false;

    public void Awake()
    {
        _isAwaking = true;
        foreach (EventListener listener in listeners)
        {
            if (!_pendingRemoval.Contains(listener))
                listener.Do();
        }
        _isAwaking = false;

        // Awake完了後に保留中の削除を処理
        if (_pendingRemoval.Count > 0)
        {
            foreach (var listener in _pendingRemoval)
            {
                listeners.Remove(listener);
            }
            _pendingRemoval.Clear();
        }
    }

    public EventListener AddListener(Action action)
    {
        EventListener listener = null;
        listener = new EventListener(action, () => RemoveListener(listener));
        listeners.Add(listener);
        return listener;
    }

    public void RemoveListener(EventListener listener)
    {
        if (_isAwaking)
        {
            // Awake中は削除をキューに入れる
            if (!_pendingRemoval.Contains(listener))
                _pendingRemoval.Add(listener);
        }
        else
        {
            listeners.Remove(listener);
        }
    }
}

/// <summary>
/// <para><typeparamref name="T" />: Singletonとする自分自身を指定</para>
/// </summary>
public abstract class InternalEventTargetBase<T, U> : BaseSingleton<T>, IEventTargetBase where T : InternalEventTargetBase<T, U>, new() where U : IEventListener
{
    protected List<U> listeners { get; set; }
    protected override void Init()
    {
        listeners = new();
    }
    public void RemoveListenerAll()
    {
        listeners.Clear();
    }
}
public interface IEventTargetBase
{
    public abstract void RemoveListenerAll();
}
public interface IEventData
{

}
