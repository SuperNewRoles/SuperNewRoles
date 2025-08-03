using SuperNewRoles.Modules.Events.Bases;

namespace SuperNewRoles.Events;

public class ExileEventData : IEventData
{
    public PlayerControl exiled { get; }
    public ExileEventData(PlayerControl exiled)
    {
        this.exiled = exiled;
    }
}

public class ExileEvent : EventTargetBase<ExileEvent, ExileEventData>
{
    public static void Invoke(PlayerControl exiled)
    {
        var data = new ExileEventData(exiled);
        Instance.Awake(data);
    }
}

