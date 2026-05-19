using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using FluentAssertions;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles;
using Xunit;

namespace SuperNewRoles.Tests;

// オプション保存/読込・プリセット名の永続化・チェックサム検証など、
// ファイルベースのストレージ挙動を確認するテスト。
public class OptionStorageTests
{
    [Fact]
    public void RoleOptionManager_ExclusivitySettings_SeparatesLocalAndHostStorage()
    {
        RoleOptionManager.ClearLocalExclusivitySettings();
        RoleOptionManager.ClearHostExclusivitySettings();

        RoleOptionManager.AddLocalExclusivitySetting(1, new[] { RoleId.Crewmate.ToString() });
        RoleOptionManager.AddHostExclusivitySetting(2, new[] { RoleId.Impostor.ToString() });

        RoleOptionManager.LocalExclusivitySettings.Should().HaveCount(1);
        RoleOptionManager.LocalExclusivitySettings[0].MaxAssign.Should().Be(1);
        RoleOptionManager.LocalExclusivitySettings[0].Roles.Should().ContainSingle().Which.Should().Be(RoleId.Crewmate);

        RoleOptionManager.HostExclusivitySettings.Should().HaveCount(1);
        RoleOptionManager.HostExclusivitySettings[0].MaxAssign.Should().Be(2);
        RoleOptionManager.HostExclusivitySettings[0].Roles.Should().ContainSingle().Which.Should().Be(RoleId.Impostor);

        RoleOptionManager.ClearLocalExclusivitySettings();
        RoleOptionManager.ClearHostExclusivitySettings();
    }

    // 目的: SaveOptionData/LoadOptionData とプリセット名の保存/復元を一括で検証
    [Fact]
    public void FileOptionStorage_SaveAndLoad_OptionDataAndPresetNames()
    {
        // 準備: 一時ディレクトリにストレージを作成
        var tempDir = new DirectoryInfo(Path.Combine(Path.GetTempPath(), "snr-tests-" + Guid.NewGuid().ToString("N")));
        var storage = new FileOptionStorage(tempDir, "Options.data", "PresetOptions_");
        storage.EnsureStorageExists();

        // プリセット名を事前に設定（保存対象）
        CustomOptionSaver.presetNames.Clear();
        CustomOptionSaver.presetNames[0] = "Default";
        CustomOptionSaver.presetNames[3] = "Tournament";

        // 実行: 保存してから再読込
        storage.SaveOptionData(version: 1, preset: 3);
        var (success, version, preset) = storage.LoadOptionData();
        var (namesOk, names) = storage.LoadPresetNames();

        // 検証: バージョン/現在プリセット/プリセット名が正しく復元されること
        // 目的: オプションデータの読み込みが成功すること
        success.Should().BeTrue();
        // 目的: プリセット名の読み込みが成功すること
        namesOk.Should().BeTrue();
        // 目的: バージョンが保存値と一致すること
        version.Should().Be(1);
        // 目的: 現在プリセットが保存値と一致すること
        preset.Should().Be(3);
        // 目的: プリセット名(0)が復元されること
        names.Should().ContainKey(0).WhoseValue.Should().Be("Default");
        // 目的: プリセット名(3)が復元されること
        names.Should().ContainKey(3).WhoseValue.Should().Be("Tournament");
    }

    // 目的: オプションファイルが存在しない場合に false を返し、プリセット名がクリアされることを検証
    [Fact]
    public void FileOptionStorage_LoadOptionData_MissingFile_ClearsPresetNames()
    {
        var tempDir = new DirectoryInfo(Path.Combine(Path.GetTempPath(), "snr-tests-" + Guid.NewGuid().ToString("N")));
        var storage = new FileOptionStorage(tempDir, "Options.data", "PresetOptions_");
        storage.EnsureStorageExists();

        // 準備: 事前にプリセット名を設定しておく（ファイルが無い想定）
        CustomOptionSaver.presetNames.Clear();
        CustomOptionSaver.presetNames[1] = "X";
        File.Exists(Path.Combine(tempDir.FullName, "Options.data")).Should().BeFalse();

        // 実行: 存在しないファイルを読み込む
        var (ok, _, _) = storage.LoadOptionData();
        // 目的: ファイル不在時は読み込みが失敗すること
        ok.Should().BeFalse(); // 失敗であること
        // 目的: 読込失敗時にプリセット名がクリアされること
        CustomOptionSaver.presetNames.Should().BeEmpty(); // 内部のプリセット名もクリアされること
    }

    // 目的: プリセット保存時、既定値の項目は省かれ変更された値のみが保存されることを検証
    [Fact]
    public void FileOptionStorage_SaveAndLoadPresetData_PersistsOnlyNonDefault()
    {
        var tempDir = new DirectoryInfo(Path.Combine(Path.GetTempPath(), "snr-tests-" + Guid.NewGuid().ToString("N")));
        var storage = new FileOptionStorage(tempDir, "Options.data", "PresetOptions_");
        storage.EnsureStorageExists();

        // 準備: 2つのオプションを用意。Aは既定値のまま、Bは既定値から変更
        var attrA = new CustomOptionBoolAttribute("A", true, translationName: "A");
        var optA = new CustomOption(attrA, typeof(OptionStorageTests).GetField("_dummyBool", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)!);

        var attrB = new CustomOptionIntAttribute("B", 0, 10, 2, 0, translationName: "B");
        var optB = new CustomOption(attrB, typeof(OptionStorageTests).GetField("_dummyInt", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)!);
        optB.UpdateSelection(3); // 値6(非既定)に変更

        storage.SavePresetData(7, new[] { optA, optB });
        var (ok, dict) = storage.LoadPresetData(7);
        // 目的: プリセットデータ読み込みが成功すること
        ok.Should().BeTrue();

        // 検証: 既定値のAは保存されず、変更したBのみ保存されていること
        // 目的: 既定値のオプションは保存されないこと
        dict.Should().NotContainKey(optA.Id);
        // 目的: 変更されたオプションのみ保存されること
        dict.Should().ContainKey(optB.Id).WhoseValue.Should().Be((byte)optB.Selection);
    }

    [Fact]
    public void FileOptionStorage_LoadPresetData_RestoresOnlyLocalExclusivitySettings()
    {
        var tempDir = new DirectoryInfo(Path.Combine(Path.GetTempPath(), "snr-tests-" + Guid.NewGuid().ToString("N")));
        var storage = new FileOptionStorage(tempDir, "Options.data", "PresetOptions_");
        storage.EnsureStorageExists();

        RoleOptionManager.ClearLocalExclusivitySettings();
        RoleOptionManager.ClearHostExclusivitySettings();
        RoleOptionManager.AddLocalExclusivitySetting(1, new[] { RoleId.Crewmate.ToString() });
        RoleOptionManager.AddHostExclusivitySetting(2, new[] { RoleId.Impostor.ToString() });

        storage.SavePresetData(5, Array.Empty<CustomOption>());

        RoleOptionManager.ClearLocalExclusivitySettings();

        var (ok, _) = storage.LoadPresetData(5);

        ok.Should().BeTrue();
        RoleOptionManager.LocalExclusivitySettings.Should().HaveCount(1);
        RoleOptionManager.LocalExclusivitySettings[0].MaxAssign.Should().Be(1);
        RoleOptionManager.LocalExclusivitySettings[0].Roles.Should().ContainSingle().Which.Should().Be(RoleId.Crewmate);
        RoleOptionManager.HostExclusivitySettings.Should().HaveCount(1);
        RoleOptionManager.HostExclusivitySettings[0].MaxAssign.Should().Be(2);
        RoleOptionManager.HostExclusivitySettings[0].Roles.Should().ContainSingle().Which.Should().Be(RoleId.Impostor);

        RoleOptionManager.ClearLocalExclusivitySettings();
        RoleOptionManager.ClearHostExclusivitySettings();
    }

    [Fact]
    public void FileOptionStorage_LoadPresetSnapshot_DoesNotApplyRuntimeSettings()
    {
        var tempDir = new DirectoryInfo(Path.Combine(Path.GetTempPath(), "snr-tests-" + Guid.NewGuid().ToString("N")));
        var storage = new FileOptionStorage(tempDir, "Options.data", "PresetOptions_");
        storage.EnsureStorageExists();

        RoleOptionManager.ClearLocalExclusivitySettings();
        RoleOptionManager.ClearHostExclusivitySettings();
        RoleOptionManager.AddLocalExclusivitySetting(9, new[] { RoleId.Crewmate.ToString() });

        storage.SavePresetSnapshot(4, new PresetSnapshot
        {
            Options = new Dictionary<string, byte> { ["option-a"] = 1 },
            ExclusivitySettings = new List<ExclusivitySettingSnapshot>
            {
                new()
                {
                    MaxAssign = 2,
                    Roles = new List<string> { RoleId.Impostor.ToString() }
                }
            }
        });

        var (ok, snapshot) = storage.LoadPresetSnapshot(4);

        ok.Should().BeTrue();
        snapshot.Options.Should().ContainKey("option-a").WhoseValue.Should().Be(1);
        RoleOptionManager.LocalExclusivitySettings.Should().ContainSingle();
        RoleOptionManager.LocalExclusivitySettings[0].MaxAssign.Should().Be(9);
        RoleOptionManager.LocalExclusivitySettings[0].Roles.Should().ContainSingle().Which.Should().Be(RoleId.Crewmate);

        var (loadOk, _) = storage.LoadPresetData(4);

        loadOk.Should().BeTrue();
        RoleOptionManager.LocalExclusivitySettings.Should().ContainSingle();
        RoleOptionManager.LocalExclusivitySettings[0].MaxAssign.Should().Be(2);
        RoleOptionManager.LocalExclusivitySettings[0].Roles.Should().ContainSingle().Which.Should().Be(RoleId.Impostor);

        RoleOptionManager.ClearLocalExclusivitySettings();
        RoleOptionManager.ClearHostExclusivitySettings();
    }

    [Fact]
    public void PresetImportExportService_ExportCurrentPresetArchive_WritesLauncherCompatibleEntries()
    {
        var tempDir = new DirectoryInfo(Path.Combine(Path.GetTempPath(), "snr-tests-" + Guid.NewGuid().ToString("N")));
        var storage = new FileOptionStorage(tempDir, "Options.data", "PresetOptions_");
        storage.EnsureStorageExists();
        var names = new Dictionary<int, string> { [3] = "Tournament" };

        storage.SavePresetSnapshot(3, new PresetSnapshot
        {
            Options = new Dictionary<string, byte> { ["abc"] = 2 },
            RoleOptions = new List<RoleOptionSnapshot>
            {
                new()
                {
                    RoleId = RoleId.Crewmate.ToString(),
                    NumberOfCrews = 1,
                    Percentage = 50
                }
            }
        });

        byte[] archiveBytes = PresetImportExportService.ExportPresetArchive(
            storage,
            new[] { 3 },
            id => names[id],
            currentPreset: 3,
            currentVersion: 1);
        var entries = ReadArchiveEntries(archiveBytes);
        var optionsData = ReadOptionsDataForTest(entries[PresetImportExportService.OptionsArchivePath]);

        entries.Should().ContainKey(PresetImportExportService.OptionsArchivePath);
        entries.Should().ContainKey("SuperNewRolesNext/SaveData/PresetOptions_3.data");
        optionsData.CurrentPreset.Should().Be(3);
        optionsData.Names.Should().ContainSingle().Which.Should().Be(new KeyValuePair<int, string>(3, "Tournament"));
        entries["SuperNewRolesNext/SaveData/PresetOptions_3.data"].Should().Equal(storage.LoadPresetRawData(3).data);
    }

    [Fact]
    public void PresetImportExportService_ExportAllPresetArchive_WritesMultiplePresets()
    {
        var tempDir = new DirectoryInfo(Path.Combine(Path.GetTempPath(), "snr-tests-" + Guid.NewGuid().ToString("N")));
        var storage = new FileOptionStorage(tempDir, "Options.data", "PresetOptions_");
        storage.EnsureStorageExists();
        var names = new Dictionary<int, string> { [0] = "Default", [2] = "Tournament" };

        storage.SavePresetSnapshot(0, new PresetSnapshot { Options = new Dictionary<string, byte> { ["a"] = 1 } });
        storage.SavePresetSnapshot(2, new PresetSnapshot { Options = new Dictionary<string, byte> { ["b"] = 3 } });

        byte[] archiveBytes = PresetImportExportService.ExportPresetArchive(
            storage,
            names.Keys,
            id => names[id],
            currentPreset: 2,
            currentVersion: 1);
        var entries = ReadArchiveEntries(archiveBytes);
        var optionsData = ReadOptionsDataForTest(entries[PresetImportExportService.OptionsArchivePath]);

        entries.Keys.Should().Contain(new[]
        {
            "SuperNewRolesNext/SaveData/PresetOptions_0.data",
            "SuperNewRolesNext/SaveData/PresetOptions_2.data"
        });
        optionsData.CurrentPreset.Should().Be(2);
        optionsData.Names.Should().BeEquivalentTo(names);
    }

    [Fact]
    public void PresetImportExportService_ExportPresetArchive_SkipsMissingPresetIds()
    {
        var tempDir = new DirectoryInfo(Path.Combine(Path.GetTempPath(), "snr-tests-" + Guid.NewGuid().ToString("N")));
        var storage = new FileOptionStorage(tempDir, "Options.data", "PresetOptions_");
        storage.EnsureStorageExists();
        var names = new Dictionary<int, string> { [0] = "Default", [5] = "Missing" };

        storage.SavePresetSnapshot(0, new PresetSnapshot { Options = new Dictionary<string, byte> { ["a"] = 1 } });

        byte[] archiveBytes = PresetImportExportService.ExportPresetArchive(
            storage,
            new[] { 0, 5 },
            id => names[id],
            currentPreset: 5,
            currentVersion: 1);
        var entries = ReadArchiveEntries(archiveBytes);
        var optionsData = ReadOptionsDataForTest(entries[PresetImportExportService.OptionsArchivePath]);

        entries.Should().ContainKey("SuperNewRolesNext/SaveData/PresetOptions_0.data");
        entries.Should().NotContainKey("SuperNewRolesNext/SaveData/PresetOptions_5.data");
        optionsData.CurrentPreset.Should().Be(0);
        optionsData.Names.Should().ContainSingle().Which.Should().Be(new KeyValuePair<int, string>(0, "Default"));
    }

    [Fact]
    public void PresetImportExportService_ImportPresetArchive_AddsNewIdsWithoutOverwritingExistingFiles()
    {
        var tempDir = new DirectoryInfo(Path.Combine(Path.GetTempPath(), "snr-tests-" + Guid.NewGuid().ToString("N")));
        var storage = new FileOptionStorage(tempDir, "Options.data", "PresetOptions_");
        storage.EnsureStorageExists();
        var names = new Dictionary<int, string> { [0] = "Existing", [9] = "High" };

        storage.SavePresetSnapshot(0, new PresetSnapshot { Options = new Dictionary<string, byte> { ["existing"] = 1 } });
        storage.SavePresetSnapshot(9, new PresetSnapshot { Options = new Dictionary<string, byte> { ["high"] = 1 } });
        storage.SaveOptionData(1, 0, names);
        byte[] existingRaw = storage.LoadPresetRawData(0).data;
        byte[] importedRaw = CreatePresetRawData(new Dictionary<string, byte> { ["imported"] = 2 });
        byte[] archiveBytes = CreatePresetArchive(
            storage.BuildOptionDataBytes(1, 2, new Dictionary<int, string> { [2] = "Imported" }),
            new Dictionary<int, byte[]> { [2] = importedRaw });

        var result = PresetImportExportService.ImportPresetsArchive(archiveBytes, storage, names, currentPreset: 0, currentVersion: 1);

        result.ImportedPresetIds.Should().ContainSingle().Which.Should().Be(10);
        names.Should().ContainKey(0).WhoseValue.Should().Be("Existing");
        names.Should().ContainKey(9).WhoseValue.Should().Be("High");
        names.Should().ContainKey(10).WhoseValue.Should().Be("Imported");
        storage.LoadPresetRawData(0).data.Should().Equal(existingRaw);
        storage.LoadPresetSnapshot(10).snapshot.Options.Should().ContainKey("imported").WhoseValue.Should().Be(2);
    }

    [Fact]
    public void PresetImportExportService_ImportPresetArchive_AvoidsDuplicateNames()
    {
        var tempDir = new DirectoryInfo(Path.Combine(Path.GetTempPath(), "snr-tests-" + Guid.NewGuid().ToString("N")));
        var storage = new FileOptionStorage(tempDir, "Options.data", "PresetOptions_");
        storage.EnsureStorageExists();
        var names = new Dictionary<int, string> { [0] = "Imported" };
        storage.SavePresetSnapshot(0, new PresetSnapshot { Options = new Dictionary<string, byte> { ["existing"] = 1 } });
        byte[] archiveBytes = CreatePresetArchive(
            storage.BuildOptionDataBytes(1, 4, new Dictionary<int, string> { [4] = "Imported" }),
            new Dictionary<int, byte[]> { [4] = CreatePresetRawData(new Dictionary<string, byte> { ["new"] = 2 }) });

        var result = PresetImportExportService.ImportPresetsArchive(archiveBytes, storage, names, currentPreset: 0, currentVersion: 1);

        int importedId = result.ImportedPresetIds.Single();
        importedId.Should().Be(1);
        names.Should().ContainKey(importedId).WhoseValue.Should().Be("Imported (2)");
    }

    [Fact]
    public void PresetImportExportService_ImportPresetArchive_DoesNotUseInvalidMaxPresetId()
    {
        var tempDir = new DirectoryInfo(Path.Combine(Path.GetTempPath(), "snr-tests-" + Guid.NewGuid().ToString("N")));
        var storage = new FileOptionStorage(tempDir, "Options.data", "PresetOptions_");
        storage.EnsureStorageExists();
        var names = new Dictionary<int, string> { [CustomOptionSaver.MaxPresetCount - 1] = "Last" };
        storage.SavePresetSnapshot(CustomOptionSaver.MaxPresetCount - 1, new PresetSnapshot { Options = new Dictionary<string, byte> { ["last"] = 1 } });
        byte[] archiveBytes = CreatePresetArchive(
            storage.BuildOptionDataBytes(1, 3, new Dictionary<int, string> { [3] = "Imported" }),
            new Dictionary<int, byte[]> { [3] = CreatePresetRawData(new Dictionary<string, byte> { ["new"] = 2 }) });

        var result = PresetImportExportService.ImportPresetsArchive(archiveBytes, storage, names, currentPreset: 0, currentVersion: 1);

        int importedId = result.ImportedPresetIds.Single();
        importedId.Should().Be(0);
        importedId.Should().BeLessThan(CustomOptionSaver.MaxPresetCount);
        names.Should().NotContainKey(CustomOptionSaver.MaxPresetCount);
        names.Should().ContainKey(importedId).WhoseValue.Should().Be("Imported");
    }

    [Fact]
    public void PresetImportExportService_ImportPresetArchive_ImportsPresetDataWithoutNameEntry()
    {
        var tempDir = new DirectoryInfo(Path.Combine(Path.GetTempPath(), "snr-tests-" + Guid.NewGuid().ToString("N")));
        var storage = new FileOptionStorage(tempDir, "Options.data", "PresetOptions_");
        storage.EnsureStorageExists();
        var names = new Dictionary<int, string>();
        byte[] archiveBytes = CreatePresetArchive(
            storage.BuildOptionDataBytes(1, 0, new Dictionary<int, string>()),
            new Dictionary<int, byte[]>
            {
                [0] = CreatePresetRawData(new Dictionary<string, byte> { ["unnamed"] = 4 })
            });

        var result = PresetImportExportService.ImportPresetsArchive(archiveBytes, storage, names, currentPreset: 0, currentVersion: 1);

        int importedId = result.ImportedPresetIds.Single();
        importedId.Should().Be(0);
        names.Should().ContainKey(importedId).WhoseValue.Should().Be("Preset 1");
        storage.LoadPresetSnapshot(importedId).snapshot.Options.Should().ContainKey("unnamed").WhoseValue.Should().Be(4);
    }

    [Fact]
    public void PresetImportExportService_ImportPresetArchive_KeepsModifierAssignFilterPayloadWhenRuntimeFlagDiffers()
    {
        var tempDir = new DirectoryInfo(Path.Combine(Path.GetTempPath(), "snr-tests-" + Guid.NewGuid().ToString("N")));
        var storage = new FileOptionStorage(tempDir, "Options.data", "PresetOptions_");
        storage.EnsureStorageExists();
        var names = new Dictionary<int, string>();
        byte[] archiveBytes = CreatePresetArchive(
            storage.BuildOptionDataBytes(1, 6, new Dictionary<int, string> { [6] = "Modifier" }),
            new Dictionary<int, byte[]> { [6] = CreatePresetRawDataWithUnknownModifierAssignFilter() });

        var result = PresetImportExportService.ImportPresetsArchive(archiveBytes, storage, names, currentPreset: 0, currentVersion: 1);

        int importedId = result.ImportedPresetIds.Single();
        var snapshot = storage.LoadPresetSnapshot(importedId).snapshot;
        snapshot.ModifierRoleOptions.Should().ContainSingle();
        snapshot.ModifierRoleOptions[0].ModifierRoleId.Should().Be("UnknownModifier");
        snapshot.ModifierRoleOptions[0].AssignFilterRoles.Should().ContainSingle().Which.Should().Be(RoleId.Crewmate.ToString());
    }

    [Fact]
    public void PresetFilePickerWorkflow_Import_HandlesSuccessCancelAndError()
    {
        byte[] importBytes = { 1, 2, 3 };
        var successPicker = new FakePresetFilePicker { ImportResult = PresetFilePickerResult.Success(importBytes) };
        byte[] importedContent = Array.Empty<byte>();
        PresetImportResult? importResult = null;
        string warning = string.Empty;
        Exception? failure = null;

        PresetFilePickerWorkflow.Import(
            successPicker,
            content =>
            {
                importedContent = content;
                return new PresetImportResult(new List<int> { 3 });
            },
            result => importResult = result,
            message => warning = message,
            ex => failure = ex);

        successPicker.ImportCalls.Should().Be(1);
        importedContent.Should().Equal(importBytes);
        importResult!.ImportedPresetIds.Should().ContainSingle().Which.Should().Be(3);
        warning.Should().BeEmpty();
        failure.Should().BeNull();

        var cancelPicker = new FakePresetFilePicker { ImportResult = PresetFilePickerResult.Cancelled() };
        bool cancelImported = false;
        PresetFilePickerWorkflow.Import(
            cancelPicker,
            _ =>
            {
                cancelImported = true;
                return new PresetImportResult(new List<int>());
            },
            _ => cancelImported = true,
            message => warning = message,
            ex => failure = ex);

        cancelPicker.ImportCalls.Should().Be(1);
        cancelImported.Should().BeFalse();

        var errorPicker = new FakePresetFilePicker { ImportResult = PresetFilePickerResult.Error("open failed") };
        warning = string.Empty;
        PresetFilePickerWorkflow.Import(
            errorPicker,
            _ => new PresetImportResult(new List<int> { 1 }),
            _ => { },
            message => warning = message,
            ex => failure = ex);

        errorPicker.ImportCalls.Should().Be(1);
        warning.Should().Be("open failed");
    }

    [Fact]
    public void PresetFilePickerWorkflow_Export_HandlesSuccessCancelAndError()
    {
        var successPicker = new FakePresetFilePicker { ExportResult = PresetFilePickerResult.Success() };
        byte[] exportBytes = { 4, 5, 6 };
        bool exported = false;
        string warning = string.Empty;
        Exception? failure = null;

        PresetFilePickerWorkflow.Export(
            successPicker,
            "Preset.snrpresets",
            () => exportBytes,
            () => exported = true,
            message => warning = message,
            ex => failure = ex);

        successPicker.ExportCalls.Should().Be(1);
        successPicker.ExportDefaultFileName.Should().Be("Preset.snrpresets");
        successPicker.ExportContents.Should().Equal(exportBytes);
        exported.Should().BeTrue();
        warning.Should().BeEmpty();
        failure.Should().BeNull();

        var cancelPicker = new FakePresetFilePicker { ExportResult = PresetFilePickerResult.Cancelled() };
        exported = false;
        PresetFilePickerWorkflow.Export(
            cancelPicker,
            "Preset.snrpresets",
            () => exportBytes,
            () => exported = true,
            message => warning = message,
            ex => failure = ex);

        cancelPicker.ExportCalls.Should().Be(1);
        exported.Should().BeFalse();

        var errorPicker = new FakePresetFilePicker { ExportResult = PresetFilePickerResult.Error("save failed") };
        warning = string.Empty;
        PresetFilePickerWorkflow.Export(
            errorPicker,
            "Preset.snrpresets",
            () => exportBytes,
            () => exported = true,
            message => warning = message,
            ex => failure = ex);

        errorPicker.ExportCalls.Should().Be(1);
        warning.Should().Be("save failed");
    }

    [Fact]
    public void AndroidSafSuspensionGuard_BeginEnd_RestoresOriginalValues()
    {
        int suspendedSeconds = 30;
        bool runInBackground = false;
        var guard = new AndroidSafSuspensionGuard(
            () => suspendedSeconds,
            value => suspendedSeconds = value,
            () => runInBackground,
            value => runInBackground = value);

        var scope = guard.Begin();

        guard.ActiveCount.Should().Be(1);
        suspendedSeconds.Should().Be(AndroidSafSuspensionGuard.MinimumSuspendedSeconds);
        runInBackground.Should().BeTrue();

        scope.Dispose();

        guard.ActiveCount.Should().Be(0);
        suspendedSeconds.Should().Be(30);
        runInBackground.Should().BeFalse();
    }

    [Fact]
    public void AndroidSafSuspensionGuard_NestedScopes_RestoreOnlyAfterLastDispose()
    {
        int suspendedSeconds = 45;
        bool runInBackground = false;
        var guard = new AndroidSafSuspensionGuard(
            () => suspendedSeconds,
            value => suspendedSeconds = value,
            () => runInBackground,
            value => runInBackground = value);

        var first = guard.Begin();
        var second = guard.Begin();

        guard.ActiveCount.Should().Be(2);
        suspendedSeconds.Should().Be(AndroidSafSuspensionGuard.MinimumSuspendedSeconds);
        runInBackground.Should().BeTrue();

        first.Dispose();

        guard.ActiveCount.Should().Be(1);
        suspendedSeconds.Should().Be(AndroidSafSuspensionGuard.MinimumSuspendedSeconds);
        runInBackground.Should().BeTrue();

        second.Dispose();

        guard.ActiveCount.Should().Be(0);
        suspendedSeconds.Should().Be(45);
        runInBackground.Should().BeFalse();
    }

    [Fact]
    public void AndroidSafSuspensionGuard_DisposeIsIdempotent()
    {
        int suspendedSeconds = 60;
        bool runInBackground = false;
        var guard = new AndroidSafSuspensionGuard(
            () => suspendedSeconds,
            value => suspendedSeconds = value,
            () => runInBackground,
            value => runInBackground = value);
        var scope = guard.Begin();

        scope.Dispose();
        scope.Dispose();

        guard.ActiveCount.Should().Be(0);
        suspendedSeconds.Should().Be(60);
        runInBackground.Should().BeFalse();
    }

    [Fact]
    public void PresetImportExportService_InvalidArchive_ShouldFail()
    {
        var tempDir = new DirectoryInfo(Path.Combine(Path.GetTempPath(), "snr-tests-" + Guid.NewGuid().ToString("N")));
        var storage = new FileOptionStorage(tempDir, "Options.data", "PresetOptions_");
        storage.EnsureStorageExists();
        var names = new Dictionary<int, string>();

        Action corruptZip = () => PresetImportExportService.ImportPresetsArchive(new byte[] { 1, 2, 3 }, storage, names, currentPreset: 0, currentVersion: 1);
        Action missingOptions = () => PresetImportExportService.ImportPresetsArchive(
            CreateArchiveWithEntries(new Dictionary<string, byte[]>
            {
                ["SuperNewRolesNext/SaveData/PresetOptions_1.data"] = CreatePresetRawData(new Dictionary<string, byte> { ["x"] = 1 })
            }),
            storage,
            names,
            currentPreset: 0,
            currentVersion: 1);
        Action missingPresetData = () => PresetImportExportService.ImportPresetsArchive(
            CreatePresetArchive(
                storage.BuildOptionDataBytes(1, 7, new Dictionary<int, string> { [7] = "Missing" }),
                new Dictionary<int, byte[]>()),
            storage,
            names,
            currentPreset: 0,
            currentVersion: 1);
        Action corruptPresetData = () => PresetImportExportService.ImportPresetsArchive(
            CreatePresetArchive(
                storage.BuildOptionDataBytes(1, 8, new Dictionary<int, string> { [8] = "Corrupt" }),
                new Dictionary<int, byte[]> { [8] = new byte[] { 1, 2, 3, 4, 5, 6 } }),
            storage,
            names,
            currentPreset: 0,
            currentVersion: 1);
        Action oversizedOptionsData = () => PresetImportExportService.ImportPresetsArchive(
            CreateArchiveWithEntries(new Dictionary<string, byte[]>
            {
                [PresetImportExportService.OptionsArchivePath] = new byte[(1024 * 1024) + 1]
            }),
            storage,
            names,
            currentPreset: 0,
            currentVersion: 1);
        Action oversizedPresetData = () => PresetImportExportService.ImportPresetsArchive(
            CreatePresetArchive(
                storage.BuildOptionDataBytes(1, 9, new Dictionary<int, string> { [9] = "Oversized" }),
                new Dictionary<int, byte[]> { [9] = new byte[(4 * 1024 * 1024) + 1] }),
            storage,
            names,
            currentPreset: 0,
            currentVersion: 1);
        Action tooManyModifiers = () => PresetImportExportService.ImportPresetsArchive(
            CreatePresetArchive(
                storage.BuildOptionDataBytes(1, 10, new Dictionary<int, string> { [10] = "TooManyModifiers" }),
                new Dictionary<int, byte[]> { [10] = CreatePresetRawDataWithTooManyModifiers() }),
            storage,
            names,
            currentPreset: 0,
            currentVersion: 1);

        corruptZip.Should().Throw<PresetImportExportException>();
        missingOptions.Should().Throw<PresetImportExportException>();
        missingPresetData.Should().Throw<PresetImportExportException>();
        corruptPresetData.Should().Throw<PresetImportExportException>();
        oversizedOptionsData.Should().Throw<PresetImportExportException>();
        oversizedPresetData.Should().Throw<PresetImportExportException>();
        tooManyModifiers.Should().Throw<PresetImportExportException>();
    }

    // 目的: チェックサム不一致時に読み込みが失敗し、プリセット名がクリアされることを検証
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
            bw.Write((byte)1); // バージョン
            bw.Write((byte)5); // チェックサム(乱数)
            bw.Write((byte)6); // チェックサム不一致(random^2 と異なる) => 失敗になる
        }

        var (ok, _, _) = storage.LoadOptionData();
        // 目的: チェックサム不一致で読み込みが失敗すること
        ok.Should().BeFalse(); // チェックサム不一致で失敗
        // 目的: 読込失敗時にプリセット名がクリアされること
        CustomOptionSaver.presetNames.Should().BeEmpty(); // 読込失敗時はプリセット名がクリアされる
    }

    private static byte[] CreatePresetRawData(Dictionary<string, byte> options)
    {
        var tempDir = new DirectoryInfo(Path.Combine(Path.GetTempPath(), "snr-tests-" + Guid.NewGuid().ToString("N")));
        var storage = new FileOptionStorage(tempDir, "Options.data", "PresetOptions_");
        storage.EnsureStorageExists();
        storage.SavePresetSnapshot(1, new PresetSnapshot { Options = options });
        return storage.LoadPresetRawData(1).data;
    }

    private static byte[] CreatePresetRawDataWithUnknownModifierAssignFilter()
    {
        using var memoryStream = new MemoryStream();
        using (var writer = new BinaryWriter(memoryStream))
        {
            writer.Write((byte)2);
            writer.Write((byte)4);
            writer.Write(0);
            writer.Write(0);
            writer.Write(0);
            writer.Write(1);
            writer.Write("UnknownModifier");
            writer.Write((byte)1);
            writer.Write(100);
            writer.Write(0);
            writer.Write(0);
            writer.Write(0);
            writer.Write(0);
            writer.Write(0);
            writer.Write(0);
            writer.Write(1);
            writer.Write(RoleId.Crewmate.ToString());
            writer.Write(0);
            writer.Write(0);
        }

        return memoryStream.ToArray();
    }

    private static byte[] CreatePresetRawDataWithTooManyModifiers()
    {
        using var memoryStream = new MemoryStream();
        using (var writer = new BinaryWriter(memoryStream))
        {
            writer.Write((byte)2);
            writer.Write((byte)4);
            writer.Write(0);
            writer.Write(0);
            writer.Write(0);
            writer.Write(257);
        }

        return memoryStream.ToArray();
    }

    private static byte[] CreatePresetArchive(byte[] optionsData, IReadOnlyDictionary<int, byte[]> presetDataById)
    {
        var entries = new Dictionary<string, byte[]>
        {
            [PresetImportExportService.OptionsArchivePath] = optionsData
        };
        foreach (var pair in presetDataById)
            entries[$"SuperNewRolesNext/SaveData/PresetOptions_{pair.Key}.data"] = pair.Value;
        return CreateArchiveWithEntries(entries);
    }

    private static byte[] CreateArchiveWithEntries(IReadOnlyDictionary<string, byte[]> entries)
    {
        using var memoryStream = new MemoryStream();
        using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, leaveOpen: true))
        {
            foreach (var pair in entries)
            {
                var entry = archive.CreateEntry(pair.Key);
                using var stream = entry.Open();
                byte[] bytes = pair.Value;
                stream.Write(bytes, 0, bytes.Length);
            }
        }

        return memoryStream.ToArray();
    }

    private static Dictionary<string, byte[]> ReadArchiveEntries(byte[] archiveBytes)
    {
        using var memoryStream = new MemoryStream(archiveBytes, writable: false);
        using var archive = new ZipArchive(memoryStream, ZipArchiveMode.Read);
        return archive.Entries
            .Where(entry => !string.IsNullOrEmpty(entry.Name))
            .ToDictionary(entry => entry.FullName, ReadEntryBytes);
    }

    private static byte[] ReadEntryBytes(ZipArchiveEntry entry)
    {
        using var stream = entry.Open();
        using var memoryStream = new MemoryStream();
        stream.CopyTo(memoryStream);
        return memoryStream.ToArray();
    }

    private static OptionsDataForTest ReadOptionsDataForTest(byte[] optionsData)
    {
        using var memoryStream = new MemoryStream(optionsData, writable: false);
        using var reader = new BinaryReader(memoryStream);
        _ = reader.ReadByte();
        int checksumSeed = reader.ReadByte();
        int checksum = reader.ReadByte();
        (checksumSeed * checksumSeed).Should().Be(checksum);
        int currentPreset = reader.ReadInt32();
        int nameCount = reader.ReadInt32();
        var names = new Dictionary<int, string>();
        for (int i = 0; i < nameCount; i++)
            names[reader.ReadInt32()] = reader.ReadString();
        return new OptionsDataForTest(currentPreset, names);
    }

    private sealed class OptionsDataForTest
    {
        public int CurrentPreset { get; }
        public Dictionary<int, string> Names { get; }

        public OptionsDataForTest(int currentPreset, Dictionary<int, string> names)
        {
            CurrentPreset = currentPreset;
            Names = names;
        }
    }

    private sealed class FakePresetFilePicker : IPresetFilePicker
    {
        public PresetFilePickerResult ExportResult { get; init; } = PresetFilePickerResult.Success();
        public PresetFilePickerResult ImportResult { get; init; } = PresetFilePickerResult.Success();
        public int ExportCalls { get; private set; }
        public int ImportCalls { get; private set; }
        public string ExportDefaultFileName { get; private set; } = string.Empty;
        public byte[] ExportContents { get; private set; } = Array.Empty<byte>();

        public void Export(string defaultFileName, byte[] contents, Action<PresetFilePickerResult> onComplete)
        {
            ExportCalls++;
            ExportDefaultFileName = defaultFileName;
            ExportContents = contents ?? Array.Empty<byte>();
            onComplete(ExportResult);
        }

        public void Import(Action<PresetFilePickerResult> onComplete)
        {
            ImportCalls++;
            onComplete(ImportResult);
        }
    }

    private static bool _dummyBool;
    private static int _dummyInt;
}
