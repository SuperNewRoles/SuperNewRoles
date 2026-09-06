using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Collections;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using SuperNewRoles.Modules;
using Xunit;

namespace SuperNewRoles.HarmonyTests;

public class StartupHarmonyPatcherTests
{
    private static readonly List<string> Sequence = new();
    private static readonly Type[] Patches = { typeof(First), typeof(Second), typeof(Third), typeof(Barrier), typeof(Fourth), typeof(Fifth) };
    private static int originalCalls;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static int Target(int value) => value * 2;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static int SkippedTarget(int value) { originalCalls++; return value; }

    private static void OtherPrefix() => Sequence.Add("other-prefix");
    private static void OtherPostfix(ref int __result) => __result += 100;

    [Fact]
    public void SkippedOriginalAndPatchesFromOtherOwnersRemainComposable()
    {
        var outside = new HarmonyLib.Harmony("snr.tests.outside");
        var own = new HarmonyLib.Harmony("snr.tests.own");
        var target = AccessTools.Method(typeof(StartupHarmonyPatcherTests), nameof(SkippedTarget));
        try
        {
            outside.Patch(target, prefix: new HarmonyMethod(AccessTools.Method(typeof(StartupHarmonyPatcherTests), nameof(OtherPrefix))) { priority = Priority.First });
            var types = new[] { typeof(SkipPrefix), typeof(SkipPostfix) };
            var generated = GeneratedHarmonyRegistry.Create();
            if (generated == null) StartupHarmonyPatcher.PatchTypes(own, types);
            else StartupHarmonyPatcher.PatchEntries(own, types.Select(t => generated.Single(e => e.Type == t)));
            originalCalls = 0;
            Sequence.Clear();
            Assert.Equal(43, SkippedTarget(5));
            Assert.Equal(0, originalCalls);
            Assert.Contains("other-prefix", Sequence);
            // A mod patching the same method later still sees the original Harmony metadata.
            outside.Patch(target, postfix: new HarmonyMethod(AccessTools.Method(typeof(StartupHarmonyPatcherTests), nameof(OtherPostfix))));
            Assert.Equal(143, SkippedTarget(5));
            Assert.Contains(HarmonyLib.Harmony.GetPatchInfo(target).Prefixes, p => p.PatchMethod == AccessTools.Method(typeof(SkipPrefix), "Prefix"));
        }
        finally { own.UnpatchSelf(); outside.UnpatchSelf(); }
    }

    [Fact]
    public void GeneratedMetadataMatchesTheBundledHarmonyInterpretation()
    {
        var entries = GeneratedHarmonyRegistry.Create();
#if !GENERATED_FIXTURE
            Assert.Null(entries); // The unprocessed build intentionally uses ordinary discovery.
            return;
#else
        Assert.NotNull(entries);
        var harmony = new HarmonyLib.Harmony("snr.tests.registry.metadata");
        var field = typeof(PatchClassProcessor).GetField("patchMethods", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.Contains(entries, e => e.Type == typeof(Barrier) && e.Patches == null);
        Assert.Contains(entries, e => e.Type == typeof(First) && e.Patches != null);
        Assert.Contains(entries, e => e.Type == typeof(ClassPriority) && e.Patches == null);
        foreach (var entry in entries.Where(e => e.Patches != null))
        {
            var original = (IList)field.GetValue(harmony.CreateClassProcessor(entry.Type));
            Assert.Equal(original.Count, entry.Patches.Length);
            for (int i = 0; i < original.Count; i++)
            {
                var item = original[i];
                var info = (HarmonyMethod)item.GetType().GetField("info", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(item);
                foreach (var member in typeof(HarmonyMethod).GetFields(BindingFlags.Instance | BindingFlags.Public))
                {
                    var expected = member.GetValue(info);
                    var actual = member.GetValue(entry.Patches[i]);
                    if (expected is Array array)
                        Assert.Equal(array.Cast<object>(), ((Array)actual).Cast<object>());
                    else
                        Assert.True(Equals(expected, actual), $"{entry.Type}.{member.Name}: expected={expected}, actual={actual}");
                }
            }
        }
#endif
    }

    [Fact]
    public void BatchingPreservesPrioritiesStateAndPrepareBoundaries()
    {
        var old = new Harmony("snr.tests.batch.baseline");
        int expected;
        string[] expectedSequence;
        try
        {
            Sequence.Clear();
            foreach (var patch in Patches) old.CreateClassProcessor(patch).Patch();
            expected = Target(5);
            expectedSequence = Sequence.ToArray();
        }
        finally { old.UnpatchSelf(); }

        var candidate = new Harmony("snr.tests.batch.candidate");
        try
        {
            Sequence.Clear();
            var generated = GeneratedHarmonyRegistry.Create();
            int wrappers = generated == null
                ? StartupHarmonyPatcher.PatchTypes(candidate, Patches)
                : StartupHarmonyPatcher.PatchEntries(candidate, Patches.Select(t => generated.Single(e => e.Type == t)));
            Assert.Equal(2, wrappers); // Two consecutive groups, separated by Prepare.
            Assert.Equal(expected, Target(5));
            Assert.Equal(expectedSequence, Sequence.ToArray());
            var info = Harmony.GetPatchInfo(AccessTools.Method(typeof(StartupHarmonyPatcherTests), nameof(Target)));
            Assert.Equal(4, info.Prefixes.Count);
            Assert.Equal(3, info.Postfixes.Count);
        }
        finally { candidate.UnpatchSelf(); }
    }

    [HarmonyPatch(typeof(StartupHarmonyPatcherTests), nameof(Target))]
    private static class First
    {
        [HarmonyPriority(Priority.High)]
        public static void Prefix(ref int value, out int __state) { Sequence.Add("first"); __state = value; value++; }
        public static void Postfix(int __state, ref int __result) { Sequence.Add("state:" + __state); __result += __state; }
    }
    [HarmonyPatch(typeof(StartupHarmonyPatcherTests), nameof(Target))]
    private static class Second
    {
        [HarmonyPriority(Priority.Low)]
        public static void Prefix(ref int value) { Sequence.Add("second"); value += 2; }
    }
    [HarmonyPatch(typeof(StartupHarmonyPatcherTests), nameof(Target))]
    private static class Third
    {
        public static void Postfix(ref int __result) { Sequence.Add("third"); __result++; }
    }
    [HarmonyPatch(typeof(StartupHarmonyPatcherTests), nameof(Target))]
    private static class Barrier
    {
        public static bool Prepare() { Sequence.Add("prepare"); return true; }
        public static void Prefix() => Sequence.Add("barrier");
    }
    [HarmonyPatch(typeof(StartupHarmonyPatcherTests), nameof(Target))]
    private static class Fourth
    {
        public static void Prefix() => Sequence.Add("fourth");
    }
    [HarmonyPatch(typeof(StartupHarmonyPatcherTests), nameof(Target))]
    private static class Fifth
    {
        public static void Postfix() => Sequence.Add("fifth");
    }

    [HarmonyPatch(typeof(StartupHarmonyPatcherTests), nameof(Target))]
    [HarmonyPriority(Priority.First)]
    private static class ClassPriority
    {
        public static void Prefix() { }
    }

    [HarmonyPatch(typeof(StartupHarmonyPatcherTests), nameof(SkippedTarget))]
    private static class SkipPrefix
    {
        public static bool Prefix(ref int __result) { __result = 42; return false; }
    }

    [HarmonyPatch(typeof(StartupHarmonyPatcherTests), nameof(SkippedTarget))]
    private static class SkipPostfix
    {
        public static void Postfix(ref int __result) => __result++;
    }
}
