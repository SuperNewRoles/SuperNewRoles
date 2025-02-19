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
    public void Awake(U obj)
    {
        foreach (EventListener<U> listener in listeners) listener.Do(obj);
    }

    public EventListener<U> AddListener(Action<U> action)
    {
        var listener = new EventListener<U>(action);
        listeners.Add(listener);
        return listener;
    }
    public void RemoveListener(EventListener<U> listener) => listeners.Remove(listener);
}
/// <summary>
/// <para><typeparamref name="T" />: Singletonとする自分自身を指定</para>
/// </summary>
public abstract class EventTargetBase<T> : InternalEventTargetBase<T, EventListener> where T : InternalEventTargetBase<T, EventListener>, new()
{
    public void Awake()
    {
        foreach (EventListener listener in listeners) listener.Do();
    }

    public EventListener AddListener(Action action)
    {
        var listener = new EventListener(action);
        listeners.Add(listener);
        return listener;
    }
    public void RemoveListener(EventListener listener) => listeners.Remove(listener);
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
