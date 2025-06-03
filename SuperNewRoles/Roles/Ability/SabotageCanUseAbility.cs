using System;
using System.Linq;
using HarmonyLib;
using SuperNewRoles.Modules;

namespace SuperNewRoles.Roles.Ability;

[Flags]
public enum SabotageType
{
    None = 1 << 0,
    Lights = 1 << 1,
    O2 = 1 << 2,
    Reactor = 1 << 3,
    Comms = 1 << 4,
}
public class SabotageCanUseAbility : AbilityBase
{
    private Func<SabotageType> _sabotageType;
    public SabotageCanUseAbility(Func<SabotageType> cannontUseSabotageType)
    {
        _sabotageType = cannontUseSabotageType;
    }
    public bool TryUse(TaskTypes taskTypes)
    {
        SabotageType type = SabotageType.None;
        switch (taskTypes)
        {
            case TaskTypes.FixLights:
                type = SabotageType.Lights;
                break;
            case TaskTypes.RestoreOxy:
                type = SabotageType.O2;
                break;
            case TaskTypes.ResetReactor:
            case TaskTypes.ResetSeismic:
            case TaskTypes.StopCharles:
                type = SabotageType.Reactor;
                break;
            case TaskTypes.FixComms:
                type = SabotageType.Comms;
                break;
            default:
                return true;
        }
        // Cannontに指定されてる場合はfalseを使って使えなくする
        return !((_sabotageType?.Invoke() ?? SabotageType.None).HasFlag(type));
    }
}

[HarmonyPatch(typeof(Console), nameof(Console.Use))]
public static class ConsolsUsePatch
{
    public static bool Prefix(Console __instance)
    {
        if (!ExPlayerControl.LocalPlayer.TryGetAbility<SabotageCanUseAbility>(out var ability))
            return true;
        __instance.CanUse(PlayerControl.LocalPlayer.Data, out var canUse, out var _);
        if (canUse) return ability.TryUse(__instance.TaskTypes.FirstOrDefault());
        return true;
    }
}
[HarmonyPatch(typeof(Console), nameof(Console.CanUse))]
public static class ConsolsCanUsePatch
{
    public static bool Prefix(Console __instance, out float __result, out bool canUse, out bool couldUse)
    {
        canUse = false;
        couldUse = false;
        __result = 0;
        if (AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay) return true;
        if (!ExPlayerControl.LocalPlayer.TryGetAbility<SabotageCanUseAbility>(out var ability))
            return true;
        return ability.TryUse(__instance.TaskTypes.FirstOrDefault());
    }
}