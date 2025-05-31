using HarmonyLib;
using SuperNewRoles.Modules.Events.Bases;

namespace SuperNewRoles.Events;

public class HudUpdateEvent : EventTargetBase<HudUpdateEvent>
{
    public static void Invoke()
    {
        Instance.Awake();
    }
}

[HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
public static class HudUpdatePatch
{
    public static void Postfix(HudManager __instance)
    {
        HudUpdateEvent.Invoke();
    }
}
