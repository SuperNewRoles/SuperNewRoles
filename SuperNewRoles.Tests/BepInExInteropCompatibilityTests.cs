using System;
using System.IO;
using BepInEx.Configuration;
using FluentAssertions;
using SuperNewRoles.Modules;
using Xunit;

namespace SuperNewRoles.Tests;

public sealed class BepInExInteropCompatibilityTests
{
    [Fact]
    public void EnsureCompatible_EnablesScanMethodRefsAndInvalidatesInterop()
    {
        using TemporaryBepInExDirectory temporary = new();
        ConfigEntry<bool> setting = temporary.Config.Bind("IL2CPP", "ScanMethodRefs", false);
        temporary.WriteAssemblyHash();

        BepInExInteropCompatibilityResult result = BepInExInteropCompatibility.EnsureCompatible(
            temporary.Config,
            temporary.InteropDirectory);

        setting.Value.Should().BeTrue();
        result.ScanMethodRefsChanged.Should().BeTrue();
        result.RegenerationScheduled.Should().BeTrue();
        File.Exists(temporary.AssemblyHashPath).Should().BeFalse();
    }

    [Fact]
    public void EnsureCompatible_InvalidatesInteropWithEmptyMethodXrefCache()
    {
        using TemporaryBepInExDirectory temporary = new();
        temporary.Config.Bind("IL2CPP", "ScanMethodRefs", true);
        temporary.WriteAssemblyHash();
        temporary.WriteMethodXrefCache(hasEntries: false);

        BepInExInteropCompatibilityResult result = BepInExInteropCompatibility.EnsureCompatible(
            temporary.Config,
            temporary.InteropDirectory);

        result.ScanMethodRefsChanged.Should().BeFalse();
        result.RegenerationScheduled.Should().BeTrue();
        File.Exists(temporary.AssemblyHashPath).Should().BeFalse();
    }

    [Fact]
    public void EnsureCompatible_KeepsInteropWithPopulatedMethodXrefCache()
    {
        using TemporaryBepInExDirectory temporary = new();
        temporary.Config.Bind("IL2CPP", "ScanMethodRefs", true);
        temporary.WriteAssemblyHash();
        temporary.WriteMethodXrefCache(hasEntries: true);

        BepInExInteropCompatibilityResult result = BepInExInteropCompatibility.EnsureCompatible(
            temporary.Config,
            temporary.InteropDirectory);

        result.ScanMethodRefsChanged.Should().BeFalse();
        result.RegenerationScheduled.Should().BeFalse();
        File.Exists(temporary.AssemblyHashPath).Should().BeTrue();
    }

    [Fact]
    public void EnsureCompatible_DoesNotInvalidateUnknownEmptyFile()
    {
        using TemporaryBepInExDirectory temporary = new();
        temporary.Config.Bind("IL2CPP", "ScanMethodRefs", true);
        temporary.WriteAssemblyHash();
        File.WriteAllBytes(temporary.MethodXrefCachePath, new byte[16]);

        BepInExInteropCompatibilityResult result = BepInExInteropCompatibility.EnsureCompatible(
            temporary.Config,
            temporary.InteropDirectory);

        result.RegenerationScheduled.Should().BeFalse();
        File.Exists(temporary.AssemblyHashPath).Should().BeTrue();
    }

    private sealed class TemporaryBepInExDirectory : IDisposable
    {
        private readonly string _rootDirectory = Path.Combine(
            Path.GetTempPath(),
            $"snr-interop-tests-{Guid.NewGuid():N}");

        public string InteropDirectory { get; }
        public string AssemblyHashPath => Path.Combine(InteropDirectory, "assembly-hash.txt");
        public string MethodXrefCachePath => Path.Combine(InteropDirectory, "MethodXrefScanCache.db");
        public ConfigFile Config { get; }

        public TemporaryBepInExDirectory()
        {
            InteropDirectory = Path.Combine(_rootDirectory, "interop");
            Directory.CreateDirectory(InteropDirectory);
            Config = new ConfigFile(Path.Combine(_rootDirectory, "BepInEx.cfg"), true);
        }

        public void WriteAssemblyHash() => File.WriteAllText(AssemblyHashPath, "hash");

        public void WriteMethodXrefCache(bool hasEntries)
        {
            using BinaryWriter writer = new(File.Create(MethodXrefCachePath));
            writer.Write(0x43584D55u);
            writer.Write(1);
            writer.Write(0L);
            if (hasEntries)
                writer.Write(1L);
        }

        public void Dispose()
        {
            if (Directory.Exists(_rootDirectory))
                Directory.Delete(_rootDirectory, recursive: true);
        }
    }
}
