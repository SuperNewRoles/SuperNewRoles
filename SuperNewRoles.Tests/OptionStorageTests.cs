using System;
using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using SuperNewRoles.Modules;
using Xunit;

namespace SuperNewRoles.Tests;

public class OptionStorageTests
{
    [Fact]
    public void FileOptionStorage_SaveAndLoad_OptionDataAndPresetNames()
    {
        // Arrange
        var tempDir = new DirectoryInfo(Path.Combine(Path.GetTempPath(), "snr-tests-" + Guid.NewGuid().ToString("N")));
        var storage = new FileOptionStorage(tempDir, "Options.data", "PresetOptions_");
        storage.EnsureStorageExists();

        // Seed preset names
        CustomOptionSaver.presetNames.Clear();
        CustomOptionSaver.presetNames[0] = "Default";
        CustomOptionSaver.presetNames[3] = "Tournament";

        // Act: save and then load
        storage.SaveOptionData(version: 1, preset: 3);
        var (success, version, preset) = storage.LoadOptionData();
        var (namesOk, names) = storage.LoadPresetNames();

        // Assert
        success.Should().BeTrue();
        namesOk.Should().BeTrue();
        version.Should().Be(1);
        preset.Should().Be(3);
        names.Should().ContainKey(0).WhoseValue.Should().Be("Default");
        names.Should().ContainKey(3).WhoseValue.Should().Be("Tournament");
    }

    [Fact]
    public void FileOptionStorage_LoadOptionData_MissingFile_ClearsPresetNames()
    {
        var tempDir = new DirectoryInfo(Path.Combine(Path.GetTempPath(), "snr-tests-" + Guid.NewGuid().ToString("N")));
        var storage = new FileOptionStorage(tempDir, "Options.data", "PresetOptions_");
        storage.EnsureStorageExists();

        // Seed and verify before
        CustomOptionSaver.presetNames.Clear();
        CustomOptionSaver.presetNames[1] = "X";
        File.Exists(Path.Combine(tempDir.FullName, "Options.data")).Should().BeFalse();

        var (ok, _, _) = storage.LoadOptionData();
        ok.Should().BeFalse();
        CustomOptionSaver.presetNames.Should().BeEmpty();
    }

    [Fact]
    public void FileOptionStorage_SaveAndLoadPresetData_PersistsOnlyNonDefault()
    {
        var tempDir = new DirectoryInfo(Path.Combine(Path.GetTempPath(), "snr-tests-" + Guid.NewGuid().ToString("N")));
        var storage = new FileOptionStorage(tempDir, "Options.data", "PresetOptions_");
        storage.EnsureStorageExists();

        // Prepare two options: A at default, B changed
        var attrA = new CustomOptionBoolAttribute("A", true, translationName: "A");
        var optA = new CustomOption(attrA, typeof(OptionStorageTests).GetField("_dummyBool", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)!);

        var attrB = new CustomOptionIntAttribute("B", 0, 10, 2, 0, translationName: "B");
        var optB = new CustomOption(attrB, typeof(OptionStorageTests).GetField("_dummyInt", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)!);
        optB.UpdateSelection(3); // value 6, non-default

        storage.SavePresetData(7, new[] { optA, optB });
        var (ok, dict) = storage.LoadPresetData(7);
        ok.Should().BeTrue();

        // Only B is present
        dict.Should().NotContainKey(optA.Id);
        dict.Should().ContainKey(optB.Id).WhoseValue.Should().Be((byte)optB.Selection);
    }

    [Fact]
    public void FileOptionStorage_InvalidChecksum_ShouldFail()
    {
        var tempDir = new DirectoryInfo(Path.Combine(Path.GetTempPath(), "snr-tests-" + Guid.NewGuid().ToString("N")));
        var storage = new FileOptionStorage(tempDir, "Options.data", "PresetOptions_");
        storage.EnsureStorageExists();

        var optionPath = Path.Combine(tempDir.FullName, "Options.data");
        using (var fs = new FileStream(optionPath, FileMode.Create))
        using (var bw = new BinaryWriter(fs))
        {
            bw.Write((byte)1); // version
            bw.Write((byte)5); // random
            bw.Write((byte)6); // random^2 mismatched -> invalid
        }

        var (ok, _, _) = storage.LoadOptionData();
        ok.Should().BeFalse();
        CustomOptionSaver.presetNames.Should().BeEmpty();
    }

    private static bool _dummyBool;
    private static int _dummyInt;
}
