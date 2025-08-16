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

        // 準備: 事前にプリセット名を設定しておく（ファイルが無い想定）
        CustomOptionSaver.presetNames.Clear();
        CustomOptionSaver.presetNames[1] = "X";
        File.Exists(Path.Combine(tempDir.FullName, "Options.data")).Should().BeFalse();

        // 実行: 存在しないファイルを読み込む
        var (ok, _, _) = storage.LoadOptionData();
        ok.Should().BeFalse(); // 失敗であること
        CustomOptionSaver.presetNames.Should().BeEmpty(); // 内部のプリセット名もクリアされること
    }

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
        ok.Should().BeTrue();

        // 検証: 既定値のAは保存されず、変更したBのみ保存されていること
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
            bw.Write((byte)1); // バージョン
            bw.Write((byte)5); // チェックサム(乱数)
            bw.Write((byte)6); // チェックサム不一致(random^2 と異なる) => 失敗になる
        }

        var (ok, _, _) = storage.LoadOptionData();
        ok.Should().BeFalse(); // チェックサム不一致で失敗
        CustomOptionSaver.presetNames.Should().BeEmpty(); // 読込失敗時はプリセット名がクリアされる
    }

    private static bool _dummyBool;
    private static int _dummyInt;
}
