using HarmonyLib;
using SuperNewRoles.Modules.Events.Bases;

namespace SuperNewRoles.Modules.Events;
public class PlayerPhysicsFixedUpdateEventData : IEventData
{
    public PlayerPhysics Instance { get; }

    public PlayerPhysicsFixedUpdateEventData(PlayerPhysics instance)
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
    public static void Postfix(PlayerPhysics __instance)
    {
        PlayerPhysicsFixedUpdateEvent.Instance.Awake(new PlayerPhysicsFixedUpdateEventData(__instance));
    }
}