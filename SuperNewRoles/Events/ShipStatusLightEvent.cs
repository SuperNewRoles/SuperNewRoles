using SuperNewRoles.Modules.Events.Bases;

namespace SuperNewRoles.Events;

public class ShipStatusLightEventData : IEventData
{
    public NetworkedPlayerInfo player { get; }
    public float lightRadius { get; set; }
    public ShipStatusLightEventData(NetworkedPlayerInfo player, float lightRadius)
    {
        this.player = player;
        this.lightRadius = lightRadius;
    }
}

public class ShipStatusLightEvent : EventTargetBase<ShipStatusLightEvent, ShipStatusLightEventData>
{
    public static float Invoke(NetworkedPlayerInfo player, float lightRadius)
    {
        var data = new ShipStatusLightEventData(player, lightRadius);
        Instance.Awake(data);
        return data.lightRadius;
    }
}
