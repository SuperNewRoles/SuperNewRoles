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
}
