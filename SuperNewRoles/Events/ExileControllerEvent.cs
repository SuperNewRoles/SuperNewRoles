using HarmonyLib;
using SuperNewRoles.Modules.Events.Bases;

namespace SuperNewRoles.Events;

public class ExileControllerEventData : IEventData
{
    public ExileController instance { get; }
    public ExileControllerEventData(ExileController instance)
    {
        this.instance = instance;
    }
}
public class ExileControllerEvent : EventTargetBase<ExileControllerEvent, ExileControllerEventData>
{
    public static void Invoke(ExileController instance)
    {
        Instance.Awake(new ExileControllerEventData(instance));
    }
}

[HarmonyPatch(typeof(ExileController), nameof(ExileController.Begin))]
public static class ExileControllerBeginPatch
{
    public static void Postfix(ExileController __instance)
    {
        ExileControllerEvent.Invoke(__instance);
    }
}