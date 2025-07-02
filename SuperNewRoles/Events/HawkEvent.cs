using SuperNewRoles.Modules.Events.Bases;

namespace SuperNewRoles.Events;

public class HawkEventData : IEventData
{
    public bool RefCancelZoom { get; set; } = false;
    public int RefZoomSize { get; set; } = 0;
    public bool RefAcceleration { get; set; } = false;
    public HawkEventData(bool refCancelZoom, int refZoomSize, bool refAcceleration)
    {
        RefCancelZoom = refCancelZoom;
        RefZoomSize = refZoomSize;
        RefAcceleration = refAcceleration;
    }
}
public class HawkEvent : EventTargetBase<HawkEvent, HawkEventData>
{
    public static HawkEventData Invoke(bool refCancelZoom, int refZoomSize, bool refAcceleration)
    {
        HawkEventData data = new(refCancelZoom, refZoomSize, refAcceleration);
        Instance.Awake(data);
        return data;
    }
}