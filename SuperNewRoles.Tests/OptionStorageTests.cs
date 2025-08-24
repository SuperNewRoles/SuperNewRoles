using System;
using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using SuperNewRoles.Modules;
using Xunit;

namespace SuperNewRoles.Tests;

// オプション保存/読込・プリセット名の永続化・チェックサム検証など、
// ファイルベースのストレージ挙動を確認するテスト。
public class OptionStorageTests
{
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

    private static bool _dummyBool;
    private static int _dummyInt;
}
