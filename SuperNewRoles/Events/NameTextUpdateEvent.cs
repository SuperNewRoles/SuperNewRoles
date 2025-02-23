using SuperNewRoles.Modules;
using SuperNewRoles.Modules.Events.Bases;

namespace SuperNewRoles.Events;

public class NameTextUpdateEventData : IEventData
{
    public ExPlayerControl Player { get; }
    public NameTextUpdateEventData(ExPlayerControl player)
    {
        Player = player;
    }
}
public class NameTextUpdateEvent : EventTargetBase<NameTextUpdateEvent, NameTextUpdateEventData>
{
    public static void Invoke(ExPlayerControl player)
    {
        var data = new NameTextUpdateEventData(player);
        Instance.Awake(data);
    }
}

