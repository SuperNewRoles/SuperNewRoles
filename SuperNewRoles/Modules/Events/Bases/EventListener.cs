using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperNewRoles.Modules.Events.Bases;

public class EventListener<T> : IEventListener where T : IEventData
{
    protected Action<T> action;

    public EventListener(Action<T> action)
    {
        this.action = action;
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

    public EventListener(Action action)
    {
        this.action = action;
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