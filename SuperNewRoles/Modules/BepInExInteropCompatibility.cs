using System;
using System.IO;
using BepInEx.Configuration;

namespace SuperNewRoles.Modules;

public readonly record struct BepInExInteropCompatibilityResult(
    bool ScanMethodRefsChanged,
    bool RegenerationScheduled);

public static class BepInExInteropCompatibility
{
    private const uint MethodXrefCacheMagic = 0x43584D55; // "UMXC"
    private const int MethodXrefCacheVersion = 1;
    private const long EmptyMethodXrefCacheLength = 16;

    public static BepInExInteropCompatibilityResult EnsureCompatible(
        ConfigFile coreConfig,
        string interopDirectory)
    {
        ArgumentNullException.ThrowIfNull(coreConfig);
        if (string.IsNullOrWhiteSpace(interopDirectory))
            throw new ArgumentException("Interop directory must not be empty.", nameof(interopDirectory));

        ConfigEntry<bool> scanMethodRefs = coreConfig.Bind(
            "IL2CPP",
            "ScanMethodRefs",
            true,
            "If enabled, Il2CppInterop will use xref to find dead methods and generate CallerCount attributes.");

        bool scanMethodRefsChanged = !scanMethodRefs.Value;
        if (scanMethodRefsChanged)
        {
            scanMethodRefs.Value = true;
            coreConfig.Save();
        }

        string methodXrefCachePath = Path.Combine(interopDirectory, "MethodXrefScanCache.db");
        bool generatedWithoutMethodRefs = IsEmptyMethodXrefCache(methodXrefCachePath);
        bool regenerationScheduled = scanMethodRefsChanged || generatedWithoutMethodRefs;

        if (regenerationScheduled)
        {
            string assemblyHashPath = Path.Combine(interopDirectory, "assembly-hash.txt");
            if (File.Exists(assemblyHashPath))
                File.Delete(assemblyHashPath);
        }

        return new BepInExInteropCompatibilityResult(scanMethodRefsChanged, regenerationScheduled);
    }

    private static bool IsEmptyMethodXrefCache(string path)
    {
        if (!File.Exists(path))
            return false;

        try
        {
            using FileStream stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            if (stream.Length != EmptyMethodXrefCacheLength)
                return false;

            using BinaryReader reader = new(stream);
            return reader.ReadUInt32() == MethodXrefCacheMagic
                && reader.ReadInt32() == MethodXrefCacheVersion;
        }
        catch (IOException)
        {
            return false;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
    }
}
