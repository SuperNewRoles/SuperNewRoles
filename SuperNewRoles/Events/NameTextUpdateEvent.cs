using SuperNewRoles.Modules;
using SuperNewRoles.Modules.Events.Bases;

namespace SuperNewRoles.Events;

public class NameTextUpdateEventData : IEventData
{
    public ExPlayerControl Player { get; }
    public bool Visiable { get; }
    public NameTextUpdateEventData(ExPlayerControl player, bool visiable)
    {
        Player = player;
        Visiable = visiable;
    }
}
public class NameTextUpdateEvent : EventTargetBase<NameTextUpdateEvent, NameTextUpdateEventData>
{
    public static void Invoke(ExPlayerControl player, bool visiable)
    {
        var data = new NameTextUpdateEventData(player, visiable);
        Instance.Awake(data);
    }
}

