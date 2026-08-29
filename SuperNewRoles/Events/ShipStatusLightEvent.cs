using System.Collections.Generic;
using SuperNewRoles.Modules.Events.Bases;

namespace SuperNewRoles.Events;

public class ShipStatusLightEventData : IEventData
{
    public NetworkedPlayerInfo player { get; private set; }
    public float lightRadius { get; set; }
    public ShipStatusLightEventData(NetworkedPlayerInfo player, float lightRadius)
    {
        this.player = player;
        this.lightRadius = lightRadius;
    }

    public void Set(NetworkedPlayerInfo player, float lightRadius)
    {
        this.player = player;
        this.lightRadius = lightRadius;
    }
}

public class ShipStatusLightEvent : EventTargetBase<ShipStatusLightEvent, ShipStatusLightEventData>
{
    private static readonly Stack<ShipStatusLightEventData> DataPool = new();

    private static ShipStatusLightEventData RentData(NetworkedPlayerInfo player, float lightRadius)
    {
        if (!DataPool.TryPop(out var data))
            return new ShipStatusLightEventData(player, lightRadius);

        data.Set(player, lightRadius);
        return data;
    }

    private static void ReturnData(ShipStatusLightEventData data)
    {
        data.Set(null, 0f);
        DataPool.Push(data);
    }

    public static float Invoke(NetworkedPlayerInfo player, float lightRadius)
    {
        var evt = Instance;
        if (!evt.HasListeners)
            return lightRadius;

        var data = RentData(player, lightRadius);
        try
        {
            evt.Awake(data);
            return data.lightRadius;
        }
        finally
        {
            ReturnData(data);
        }
    }
}
