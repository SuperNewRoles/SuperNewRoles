using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;

namespace SuperNewRoles.Patches;

[HarmonyPatch]
public static class AchievementManagerPatch
{
    private static readonly string[] SafeFinalizeMethodNames =
    [
        "ProcessAuthentication",
        "InitializeAchievementProgressDictionary",
        "UpdateAchievementProgress"
    ];

    public static IEnumerable<MethodBase> TargetMethods()
    {
        Type achievementManagerType = AccessTools.TypeByName("AchievementManager");
        if (achievementManagerType == null)
            yield break;

        foreach (MethodInfo method in AccessTools.GetDeclaredMethods(achievementManagerType)
            .Where(method => SafeFinalizeMethodNames.Contains(method.Name)))
        {
            yield return method;
        }
    }

    public static Exception Finalizer(Exception __exception)
    {
        if (__exception == null)
            return null;

        string exceptionText = __exception.ToString();
        if (exceptionText.Contains("PlayGamesPlatform") &&
            exceptionText.Contains("Local") &&
            exceptionText.Contains("AchievementManager"))
        {
            Logger.Warning($"Suppressed unsupported platform achievement exception: {__exception.GetType().Name}: {__exception.Message}");
            return null;
        }

        return __exception;
    }
}
