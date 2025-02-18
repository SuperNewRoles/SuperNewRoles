using HarmonyLib;
using SuperNewRoles.Modules.Events.Bases;

namespace SuperNewRoles.Events;

public class FixedUpdateEvent : EventTargetBase<FixedUpdateEvent>
{
    public static void Invoke()
    {
        Instance.Awake();
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
public static class FixedUpdatePatch
{
    public static void Postfix(PlayerControl __instance)
    {
        if (PlayerControl.LocalPlayer == __instance)
            FixedUpdateEvent.Invoke();
    }
}

