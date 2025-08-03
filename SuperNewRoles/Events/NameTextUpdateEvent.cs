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
    public ExPlayerControl Player { get; }
    public bool Visiable { get; }
    public NameTextUpdateVisiableEventData(ExPlayerControl player, bool visiable)
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
    public static void Invoke(ExPlayerControl player, bool visiable)
    {
        var data = new NameTextUpdateVisiableEventData(player, visiable);
        Instance.Awake(data);
    }
}

