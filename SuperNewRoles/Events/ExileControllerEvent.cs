using HarmonyLib;
using SuperNewRoles.Modules.Events.Bases;

namespace SuperNewRoles.Events;

public class ExileControllerEvent : EventTargetBase<ExileControllerEvent>
{
    public static void Invoke()
    {
        Instance.Awake();
    }
}

[HarmonyPatch(typeof(ExileController), nameof(ExileController.Begin))]
public static class ExileControllerBeginPatch
{
    public static void Postfix(ExileController __instance)
    {
        ExileControllerEvent.Invoke();
    }
}