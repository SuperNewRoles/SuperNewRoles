using HarmonyLib;
using SuperNewRoles.Modules.Events.Bases;

namespace SuperNewRoles.Events;

public class MapBehaviourShowPrefixEventData : IEventData
{
    public MapBehaviour __instance;
    public MapOptions opts;
}
public class MapBehaviourShowPrefixEvent : EventTargetBase<MapBehaviourShowPrefixEvent, MapBehaviourShowPrefixEventData>
{
    public static void Invoke(MapBehaviour __instance, MapOptions opts)
    {
        MapBehaviourShowPrefixEventData data = new()
        {
            __instance = __instance,
            opts = opts,
        };
        Instance.Awake(data);
    }
}
public class MapBehaviourShowPostfixEventData : IEventData
{
    public MapBehaviour __instance;
    public MapOptions opts;
}
public class MapBehaviourShowPostfixEvent : EventTargetBase<MapBehaviourShowPostfixEvent, MapBehaviourShowPostfixEventData>
{
    public static void Invoke(MapBehaviour __instance, MapOptions opts)
    {
        MapBehaviourShowPostfixEventData data = new()
        {
            __instance = __instance,
            opts = opts,
        };
        Instance.Awake(data);
    }
}

public class MapBehaviourFixedUpdatePostfixEventData : IEventData
{
    public MapBehaviour __instance;
}
public class MapBehaviourFixedUpdatePostfixEvent : EventTargetBase<MapBehaviourFixedUpdatePostfixEvent, MapBehaviourFixedUpdatePostfixEventData>
{
    public static void Invoke(MapBehaviour __instance)
    {
        MapBehaviourFixedUpdatePostfixEventData data = new() { __instance = __instance };
        Instance.Awake(data);
    }
}

public class MapBehaviourAwakePostfixEventData : IEventData
{
    public MapBehaviour __instance;
}
public class MapBehaviourAwakePostfixEvent : EventTargetBase<MapBehaviourAwakePostfixEvent, MapBehaviourAwakePostfixEventData>
{
    public static void Invoke(MapBehaviour __instance)
    {
        MapBehaviourAwakePostfixEventData data = new() { __instance = __instance };
        Instance.Awake(data);
    }
}

public class MapBehaviourClosePostfixEventData : IEventData
{
    public MapBehaviour __instance;
}
public class MapBehaviourClosePostfixEvent : EventTargetBase<MapBehaviourClosePostfixEvent, MapBehaviourClosePostfixEventData>
{
    public static void Invoke(MapBehaviour __instance)
    {
        MapBehaviourClosePostfixEventData data = new() { __instance = __instance };
        Instance.Awake(data);
    }
}

[HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.Show))]
public static class MapBehaviourShowPatch
{
    public static void Prefix(MapBehaviour __instance, MapOptions opts)
    {
        MapBehaviourShowPrefixEvent.Invoke(__instance, opts);
    }
    public static void Postfix(MapBehaviour __instance, MapOptions opts)
    {
        MapBehaviourShowPostfixEvent.Invoke(__instance, opts);
    }
}
[HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.FixedUpdate))]
public static class MapBehaviourFixedUpdatePatch
{
    public static void Postfix(MapBehaviour __instance)
    {
        MapBehaviourFixedUpdatePostfixEvent.Invoke(__instance);
    }
}
[HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.Awake))]
public static class MapBehaviourAwakePatch
{
    public static void Postfix(MapBehaviour __instance)
    {
        MapBehaviourAwakePostfixEvent.Invoke(__instance);
    }
}
[HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.Close))]
public static class MapBehaviourClosePatch
{
    public static void Postfix(MapBehaviour __instance)
    {
        MapBehaviourClosePostfixEvent.Invoke(__instance);
    }
}