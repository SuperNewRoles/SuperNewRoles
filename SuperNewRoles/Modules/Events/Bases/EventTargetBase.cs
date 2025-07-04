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
    private EventListener<U>[] _listenersCache = null;
    private bool _listenersCacheDirty = true;

    public void Awake(U obj)
    {
        _isAwaking = true;

        // キャッシュが無効な場合は再生成
        if (_listenersCacheDirty || _listenersCache == null)
        {
            _listenersCache = listeners.ToArray();
            _listenersCacheDirty = false;
        }

        // 配列でイテレート（高速）
        foreach (EventListener<U> listener in _listenersCache)
        {
            if (!_pendingRemoval.Contains(listener))
            {
                try
                {
                    listener.Do(obj);
                }
                catch (Exception e)
                {
                    SuperNewRolesPlugin.Logger.LogError($"Error in {listener.GetType().Name}: {e}");
                }
            }
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
            _listenersCacheDirty = true;
        }
    }

    public EventListener<U> AddListener(Action<U> action)
    {
        EventListener<U> listener = null;
        listener = new EventListener<U>(action, () => RemoveListener(listener));
        listeners.Add(listener);
        _listenersCacheDirty = true;
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
            _listenersCacheDirty = true;
        }
    }

    public override void RemoveListenerAll()
    {
        var listenerCount = listeners.Count;
        var pendingCount = _pendingRemoval.Count;
        SuperNewRolesPlugin.Logger.LogInfo($"[{GetType().Name}] RemoveListenerAll: {listenerCount} listeners, {pendingCount} pending removal");

        base.RemoveListenerAll();
        _pendingRemoval.Clear();
        _listenersCacheDirty = true;
        _listenersCache = null;

        SuperNewRolesPlugin.Logger.LogInfo($"[{GetType().Name}] RemoveListenerAll completed");
    }
}
/// <summary>
/// <para><typeparamref name="T" />: Singletonとする自分自身を指定</para>
/// </summary>
public abstract class EventTargetBase<T> : InternalEventTargetBase<T, EventListener> where T : InternalEventTargetBase<T, EventListener>, new()
{
    private List<EventListener> _pendingRemoval = new();
    private bool _isAwaking = false;
    private EventListener[] _listenersCache = null;
    private bool _listenersCacheDirty = true;

    public void Awake()
    {
        _isAwaking = true;

        // キャッシュが無効な場合は再生成
        if (_listenersCacheDirty || _listenersCache == null)
        {
            _listenersCache = listeners.ToArray();
            _listenersCacheDirty = false;
        }

        // 配列でイテレート（高速）
        foreach (EventListener listener in _listenersCache)
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
            _listenersCacheDirty = true;
        }
    }

    public EventListener AddListener(Action action)
    {
        EventListener listener = null;
        listener = new EventListener(action, () => RemoveListener(listener));
        listeners.Add(listener);
        _listenersCacheDirty = true;
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
            _listenersCacheDirty = true;
        }
    }

    public override void RemoveListenerAll()
    {
        var listenerCount = listeners.Count;
        var pendingCount = _pendingRemoval.Count;
        SuperNewRolesPlugin.Logger.LogInfo($"[{GetType().Name}] RemoveListenerAll: {listenerCount} listeners, {pendingCount} pending removal");

        base.RemoveListenerAll();
        _pendingRemoval.Clear();
        _listenersCacheDirty = true;
        _listenersCache = null;

        SuperNewRolesPlugin.Logger.LogInfo($"[{GetType().Name}] RemoveListenerAll completed");
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
    public virtual void RemoveListenerAll()
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
