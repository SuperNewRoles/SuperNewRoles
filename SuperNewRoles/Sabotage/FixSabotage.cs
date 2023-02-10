using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;

namespace SuperNewRoles.Sabotage;

public class FixSabotage
{
    /// <summary>
    /// (リアクター, 停電, 通信, 酸素)
    /// </summary>
    public static Dictionary<RoleId, (bool, bool, bool, bool)> SetFixSabotageDictionary = new()
    {
        { RoleId.Fox, (false, false, false, false) },
        { RoleId.FireFox, (false, false, false, false) },
    };
    [HarmonyPatch(typeof(Console), nameof(Console.Use))]
    public static class ConsolsUsePatch
    {
        public static bool Prefix(Console __instance)
        {
            if (!SetFixSabotageDictionary.ContainsKey(PlayerControl.LocalPlayer.GetRole())) return true;
            __instance.CanUse(PlayerControl.LocalPlayer.Data, out var canUse, out var _);
            if (canUse) return IsBlocked(__instance.FindTask(CachedPlayer.LocalPlayer).TaskType);
            return true;
        }
        public static bool IsBlocked(TaskTypes type)
        {
            (bool, bool, bool, bool) fixSabotage = SetFixSabotageDictionary[PlayerControl.LocalPlayer.GetRole()];
            if (type is TaskTypes.StopCharles or TaskTypes.ResetSeismic or TaskTypes.ResetReactor && fixSabotage.Item1) return true;
            if (type is TaskTypes.FixLights && fixSabotage.Item2) return true;
            if (type is TaskTypes.FixComms && fixSabotage.Item3) return true;
            if (type is TaskTypes.RestoreOxy && fixSabotage.Item4) return true;
            return false;
        }
    }
}
