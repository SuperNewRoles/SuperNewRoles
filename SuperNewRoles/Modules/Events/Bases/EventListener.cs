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

    public EventListener(Action<T> action, Action remover)
    {
        this.action = action;
        this.remover = remover;
    }

    public void RemoveListener()
    {
        remover.Invoke();
    }

    public virtual void Do(T obj)
    {
        action.Invoke(obj);
        return;
    }
}

public class EventListener : IEventListener
{
    protected Action action;
    protected Action remover;
    public EventListener(Action action, Action remover)
    {
        this.action = action;
        this.remover = remover;
    }

    public void RemoveListener()
    {
        remover.Invoke();
    }

    public virtual void Do()
    {
        action.Invoke();
        return;
    }
}

public interface IEventListener
{
}