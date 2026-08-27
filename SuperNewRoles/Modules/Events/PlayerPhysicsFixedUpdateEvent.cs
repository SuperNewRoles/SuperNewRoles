using System.Collections.Generic;
using HarmonyLib;
using SuperNewRoles.Modules.Events.Bases;

namespace SuperNewRoles.Modules.Events;
public class PlayerPhysicsFixedUpdateEventData : IEventData
{
    public PlayerPhysics Instance { get; private set; }

    public PlayerPhysicsFixedUpdateEventData(PlayerPhysics instance)
    {
        Instance = instance;
    }

    public void SetInstance(PlayerPhysics instance)
    {
        Instance = instance;
    }
}

public class PlayerPhysicsFixedUpdateEvent : EventTargetBase<PlayerPhysicsFixedUpdateEvent, PlayerPhysicsFixedUpdateEventData>
{
}

[HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.FixedUpdate))]
public static class PlayerPhysicsFixedUpdateEventPatch
{
    private static readonly Stack<PlayerPhysicsFixedUpdateEventData> DataPool = new();

    private static PlayerPhysicsFixedUpdateEventData RentData(PlayerPhysics instance)
    {
        if (!DataPool.TryPop(out var data))
            return new PlayerPhysicsFixedUpdateEventData(instance);

        data.SetInstance(instance);
        return data;
    }

    private static void ReturnData(PlayerPhysicsFixedUpdateEventData data)
    {
        data.SetInstance(null);
        DataPool.Push(data);
    }

    public static void Postfix(PlayerPhysics __instance)
    {
        var evt = PlayerPhysicsFixedUpdateEvent.Instance;
        if (!evt.HasListeners)
            return;

        var data = RentData(__instance);
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
