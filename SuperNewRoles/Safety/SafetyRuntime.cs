using SuperNewRoles.Modules;
using UnityEngine;

namespace SuperNewRoles.Safety;

internal static class SafetyRuntime
{
    public static MonoBehaviour FindCoroutineRunner(MonoBehaviour fallback)
    {
        if (IsUsable(AmongUsClient.Instance))
            return AmongUsClient.Instance;

        if (DestroyableSingleton<HudManager>.InstanceExists)
        {
            HudManager hud = FastDestroyableSingleton<HudManager>.Instance;
            if (IsUsable(hud))
                return hud;
        }

        MainMenuManager menu = Object.FindObjectOfType<MainMenuManager>();
        if (IsUsable(menu))
            return menu;

        if (IsUsable(fallback))
            return fallback;

        Logger.Error(
            "Safety coroutine host missing (fallback="
            + (fallback == null ? "null" : fallback.name + " inactive")
            + ")");
        return null;
    }

    public static bool IsUsable(MonoBehaviour behaviour)
    {
        return behaviour != null && behaviour.isActiveAndEnabled;
    }
}
