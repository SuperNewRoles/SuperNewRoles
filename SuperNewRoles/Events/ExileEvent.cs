using SuperNewRoles.Modules.Events.Bases;

namespace SuperNewRoles.Events;

public class ExileEventData : IEventData
{
    public PlayerControl exiled { get; }
    public bool RefCanceled { get; set; }
    public ExileEventData(PlayerControl exiled)
    {
        this.exiled = exiled;
        RefCanceled = false;
    }
}

public class ExileEvent : EventTargetBase<ExileEvent, ExileEventData>
{
    public static ExileEventData Invoke(PlayerControl exiled)
    {
        var data = new ExileEventData(exiled);
        Instance.Awake(data);
        return data;
    }
}

