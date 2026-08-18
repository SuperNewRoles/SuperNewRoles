using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperNewRoles.Modules.Events.Bases;

public class EventListener<T> : IEventListener where T : IEventData
{
    protected Action<T> action;
    protected Action remover;
    private bool _removed;

    public EventListener(Action<T> action, Action remover)
    {
        this.action = action;
        this.remover = remover;
    }

    public void RemoveListener()
    {
        if (_removed) return;
        _removed = true;
        remover.Invoke();
    }

    public virtual void Do(T obj)
    {
        try
        {
            action.Invoke(obj);
        }
        catch (Exception e)
        {
            SuperNewRoles.Logger.Error($"Error in {action.Method.DeclaringType?.Name}.{action.Method.Name}: {e}");
        }
    }
}

public class EventListener : IEventListener
{
    protected Action action;
    protected Action remover;
    private bool _removed;
    public EventListener(Action action, Action remover)
    {
        this.action = action;
        this.remover = remover;
    }

    public void RemoveListener()
    {
        if (_removed) return;
        _removed = true;
        remover.Invoke();
    }

    public virtual void Do()
    {
        try
        {
            action.Invoke();
        }
        catch (Exception e)
        {
            SuperNewRoles.Logger.Error($"Error in {action.Method.DeclaringType?.Name}.{action.Method.Name}: {e}");
        }
    }
}

public interface IEventListener
{
    void RemoveListener();
}
