using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperNewRoles.Modules.Events.Bases;

abstract class EventListener<T> : IEventListener where T : IEventData
{
    protected Action<T> action;

    protected EventListener(Action<T> action)
    {
        this.action = action;
    }

    public virtual void Do(T obj)
    {
        action.Invoke(obj);
        return;
    }
}

abstract class EventListener : IEventListener
{
    protected Action action;

    protected EventListener(Action action)
    {
        this.action = action;
    }

    public virtual void Do()
    {
        action.Invoke();
        return;
    }
}

interface IEventListener
{
}