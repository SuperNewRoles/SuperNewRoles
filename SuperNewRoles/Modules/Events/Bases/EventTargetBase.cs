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
abstract class EventTargetBase<T, U> : InternalEventTargetBase<T, EventListener<U>> where T : InternalEventTargetBase<T, EventListener<U>>, new() where U : IEventData
{
    public void Awake(U obj)
    {
        foreach (EventListener<U> listener in listeners) listener.Do(obj);
    }

    public void AddEventListener(EventListener<U> listener) => listeners.Add(listener);
    public void RemoveEventListener(EventListener<U> listener) => listeners.Remove(listener);
}
/// <summary>
/// <para><typeparamref name="T" />: Singletonとする自分自身を指定</para>
/// </summary>
abstract class EventTargetBase<T> : InternalEventTargetBase<T, EventListener> where T : InternalEventTargetBase<T, EventListener>, new()
{
    public void Awake()
    {
        foreach (EventListener listener in listeners) listener.Do();
    }

    public void AddListener(EventListener listener) => listeners.Add(listener);
    public void RemoveListener(EventListener listener) => listeners.Remove(listener);
}

/// <summary>
/// <para><typeparamref name="T" />: Singletonとする自分自身を指定</para>
/// </summary>
abstract class InternalEventTargetBase<T, U> : BaseSingleton<T> where T : InternalEventTargetBase<T, U>, new() where U : IEventListener
{
    protected List<U> listeners { get; set; }
    protected override void Init()
    {
        listeners = new();
    }
}

interface IEventData
{
}
