using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.IO;
using System.Runtime.Loader;
using HarmonyLib;
using SuperNewRoles.Modules;
using Xunit;

namespace SuperNewRoles.HarmonyTests;

public class GeneratedHarmonyRegistryTests
{
    [Fact]
    public void EveryGeneratedPatchMatchesHarmonyMetadataAndDiscoveryOrder()
    {
        string path = Environment.GetEnvironmentVariable("SNR_CORPUS_ASSEMBLY");
        Assert.False(string.IsNullOrEmpty(path));
        AssemblyLoadContext.Default.Resolving += (_, name) =>
        {
            foreach (var dir in new[] { Path.GetDirectoryName(path), Path.Combine(Environment.GetEnvironmentVariable("AmongUs"), "BepInEx", "interop") })
            {
                var file = Path.Combine(dir, name.Name + ".dll");
                if (File.Exists(file)) return AssemblyLoadContext.Default.LoadFromAssemblyPath(file);
            }
            return null;
        };
        var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(Path.GetFullPath(path));
        var generated = (Array)assembly.GetType("SuperNewRoles.Modules.GeneratedHarmonyRegistry").GetMethod("Create", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, null);
        Assert.NotNull(generated);
        var entries = generated.Cast<object>().Select(entry =>
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            var type = entry.GetType();
            return new StartupPatchEntry((Type)type.GetField("Type", flags).GetValue(entry),
                (HarmonyMethod[])type.GetField("Patches", flags).GetValue(entry),
                (HarmonyPatchType[])type.GetField("Kinds", flags).GetValue(entry));
        }).ToArray();
        var harmony = new Harmony("snr.tests.registry.corpus");
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        var attributes = typeof(PatchClassProcessor).GetField("containerAttributes", flags)!;
        var patches = typeof(PatchClassProcessor).GetField("patchMethods", flags)!;
        var expectedTypes = assembly.GetTypes()
            .Where(t => t.FullName != "SuperNewRoles.Modules.StartupHarmonyPatcher+BatchContainer")
            .Where(t => attributes.GetValue(harmony.CreateClassProcessor(t)) != null).ToArray();
        Assert.Equal(expectedTypes, entries.Select(e => e.Type).ToArray());
        Assert.NotEmpty(entries.Where(e => e.Patches != null));
        foreach (var entry in entries.Where(e => e.Patches != null))
        {
            var expected = (IList)patches.GetValue(harmony.CreateClassProcessor(entry.Type))!;
            Assert.Equal(expected.Count, entry.Patches.Length);
            for (int i = 0; i < expected.Count; i++)
            {
                var item = expected[i]!;
                var info = (HarmonyMethod)item.GetType().GetField("info", flags)!.GetValue(item)!;
                Assert.Equal(item.GetType().GetField("type", flags)!.GetValue(item), entry.Kinds[i]);
                foreach (var field in typeof(HarmonyMethod).GetFields(BindingFlags.Instance | BindingFlags.Public))
                {
                    var original = field.GetValue(info);
                    var generatedValue = field.GetValue(entry.Patches[i]);
                    if (original is Array array)
                        Assert.Equal(array.Cast<object>(), ((Array)generatedValue!).Cast<object>());
                    else
                        Assert.True(Equals(original, generatedValue), $"{entry.Type}.{info.method.Name}/{field.Name}: {original} != {generatedValue}");
                }
            }
        }
    }
}
