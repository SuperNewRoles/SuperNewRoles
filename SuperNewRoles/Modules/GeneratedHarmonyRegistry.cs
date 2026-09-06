using System;
using System.Runtime.CompilerServices;
using HarmonyLib;

namespace SuperNewRoles.Modules;

internal sealed class StartupPatchEntry
{
    internal readonly Type Type;
    internal readonly HarmonyMethod[] Patches;
    internal readonly HarmonyPatchType[] Kinds;

    internal StartupPatchEntry(Type type, HarmonyMethod[] patches, HarmonyPatchType[] kinds)
    {
        Type = type;
        Patches = patches;
        Kinds = kinds;
    }
}

internal static class GeneratedHarmonyRegistry
{
    // Build-time weaving replaces this body. Unprocessed builds retain the normal discovery path.
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static StartupPatchEntry[] Create() => null;
}
