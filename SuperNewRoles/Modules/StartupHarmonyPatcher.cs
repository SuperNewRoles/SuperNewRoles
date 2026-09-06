using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Diagnostics;
using HarmonyLib;

namespace SuperNewRoles.Modules;

internal static class StartupHarmonyPatcher
{
    private const BindingFlags Fields = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

    private static bool HasKnownHarmonyVersion()
    {
        var version = typeof(Harmony).Assembly.GetName().Version;
        return version == new Version(2, 10, 2, 0) || version == new Version(2, 16, 0, 0);
    }

    internal static void PatchAll(Harmony harmony, Assembly assembly)
    {
        // Check before entering the generated method, including before its field references are JIT-compiled.
        var generated = HasKnownHarmonyVersion() ? GeneratedHarmonyRegistry.Create() : null;
        if (generated == null)
            PatchTypes(harmony, AccessTools.GetTypesFromAssembly(assembly));
        else
            PatchEntries(harmony, generated);
    }

    internal static int PatchTypes(Harmony harmony, IEnumerable<Type> types)
        => PatchEntries(harmony, Entries(types));

    private static IEnumerable<StartupPatchEntry> Entries(IEnumerable<Type> types)
    {
        foreach (var type in types)
            yield return new StartupPatchEntry(type, null, null);
    }

    // Only used as an empty Harmony-owned processor, never as a patch target.
    [HarmonyPatch]
    private static class BatchContainer { }

    internal static int PatchEntries(Harmony harmony, IEnumerable<StartupPatchEntry> entries)
    {
        // HarmonyXの処理済みメタデータをそのまま使う。未知のバージョンでは通常処理へ戻す。
        var processorType = typeof(PatchClassProcessor);
        var attributesField = processorType.GetField("containerAttributes", Fields);
        var auxiliaryField = processorType.GetField("auxilaryMethods", Fields);
        var patchesField = processorType.GetField("patchMethods", Fields);
        var patchType = patchesField?.FieldType.IsGenericType == true ? patchesField.FieldType.GetGenericArguments()[0] : null;
        var kindField = patchType?.GetField("type", Fields);
        var infoField = patchType?.GetField("info", Fields);
        bool knownVersion = HasKnownHarmonyVersion();
        if (!knownVersion || attributesField == null || auxiliaryField == null || patchesField == null || kindField == null || infoField == null ||
            !typeof(IList).IsAssignableFrom(patchesField.FieldType) ||
            !typeof(IDictionary).IsAssignableFrom(auxiliaryField.FieldType))
        {
            Logger.Info("[HarmonyBatch] Unsupported processor layout; using standard patching.");
            foreach (var entry in entries)
                harmony.CreateClassProcessor(entry.Type).Patch();
            return -1;
        }

        PatchClassProcessor pending = null;
        IList pendingPatches = null;
        int eligibleClasses = 0, batches = 0, wrappers = 0, standardClasses = 0, generatedClasses = 0;
        var applyTimer = new Stopwatch();
        void Flush()
        {
            if (pending == null)
                return;
            applyTimer.Start();
            try { wrappers += pending.Patch()?.Count ?? 0; }
            finally { applyTimer.Stop(); }
            batches++;
            pending = null;
            pendingPatches = null;
        }

        foreach (var entry in entries)
        {
            var type = entry.Type;
            if (entry.Patches != null)
            {
                if (pending == null)
                {
                    pending = harmony.CreateClassProcessor(typeof(BatchContainer));
                    pendingPatches = (IList)patchesField.GetValue(pending);
                }
                for (int i = 0; i < entry.Patches.Length; i++)
                {
                    var patch = Activator.CreateInstance(patchType, nonPublic: true);
                    infoField.SetValue(patch, entry.Patches[i]);
                    kindField.SetValue(patch, entry.Kinds[i]);
                    pendingPatches.Add(patch);
                }
                eligibleClasses++;
                generatedClasses++;
                continue;
            }
            // 属性マージや対象解決はHarmony自身に任せる。
            var processor = harmony.CreateClassProcessor(type);
            if (attributesField.GetValue(processor) is not HarmonyMethod attributes)
                continue; // 元のPatch()も、この場合は何もしない。
            var auxiliary = (IDictionary)auxiliaryField.GetValue(processor);
            var patches = (IList)patchesField.GetValue(processor);
            bool eligible = auxiliary?.Count == 0 && patches?.Count > 0 && attributes.debug != true &&
                !type.IsDefined(typeof(HarmonyPatchAll), inherit: true);
            if (eligible)
                foreach (var patch in patches)
                {
                    var kind = (HarmonyPatchType)kindField.GetValue(patch);
                    if (kind != HarmonyPatchType.Prefix && kind != HarmonyPatchType.Postfix)
                    {
                        eligible = false;
                        break;
                    }
                }
            if (!eligible)
            {
                // Prepare/Cleanup/TargetMethods/ReversePatch/Transpiler等の前後をまたがない。
                Flush();
                processor.Patch();
                standardClasses++;
                continue;
            }

            eligibleClasses++;
            if (pending == null)
            {
                pending = processor;
                pendingPatches = patches;
            }
            else
                foreach (var patch in patches)
                    pendingPatches.Add(patch);
        }
        Flush();
        Logger.Info($"[HarmonyBatch] classes={eligibleClasses}, batches={batches}, wrappers={wrappers}, standardClasses={standardClasses}, generatedClasses={generatedClasses}, batchApplyMs={applyTimer.ElapsedMilliseconds}");
        return wrappers;
    }
}
