using System.Collections.Generic;
using SuperNewRoles.Modules;
using SuperNewRoles.Modules.Events.Bases;

namespace SuperNewRoles.Events;

public class NameTextUpdateEventData : IEventData
{
    public ExPlayerControl Player { get; }
    public bool Visible { get; }
    public NameTextUpdateEventData(ExPlayerControl player, bool visible)
    {
        Player = player;
        Visible = visible;
    }
}
public class NameTextUpdateVisiableEventData : IEventData
{
    public ExPlayerControl Player { get; private set; }
    public bool Visiable { get; private set; }
    public NameTextUpdateVisiableEventData(ExPlayerControl player, bool visiable)
    {
        Set(player, visiable);
    }

    public void Set(ExPlayerControl player, bool visiable)
    {
        Player = player;
        Visiable = visiable;
    }
}
public class NameTextUpdateEvent : EventTargetBase<NameTextUpdateEvent, NameTextUpdateEventData>
{
    public static void Invoke(ExPlayerControl player, bool visible)
    {
        var data = new NameTextUpdateEventData(player, visible);
        Instance.Awake(data);
    }
}

public class NameTextUpdateVisiableEvent : EventTargetBase<NameTextUpdateVisiableEvent, NameTextUpdateVisiableEventData>
{
    private static readonly Stack<NameTextUpdateVisiableEventData> DataPool = new();

    private static NameTextUpdateVisiableEventData RentData(ExPlayerControl player, bool visiable)
    {
        if (!DataPool.TryPop(out var data))
            return new NameTextUpdateVisiableEventData(player, visiable);

        data.Set(player, visiable);
        return data;
    }

    private static void ReturnData(NameTextUpdateVisiableEventData data)
    {
        data.Set(null, false);
        DataPool.Push(data);
    }

    public static void Invoke(ExPlayerControl player, bool visiable)
    {
        var evt = Instance;
        if (!evt.HasListeners)
            return;

        var data = RentData(player, visiable);
        try
        {
            evt.Awake(data);
        }
        finally
        {
            ReturnData(data);
        }
    }
}

