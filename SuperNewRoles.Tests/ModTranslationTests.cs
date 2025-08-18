using System;
using FluentAssertions;
using SuperNewRoles;
using Xunit;
using AmongUs.Data;

namespace SuperNewRoles.Tests;

// 翻訳 CSV のロード/言語切替/エスケープ処理/クリーンアップ時の動作を検証するテスト。
public class ModTranslationTests
{
    private const string TestCsv =
        "# comment line should be ignored\n" +
        "Test.Close,閉じる,Close\n" +
        "Test.People,{0} 人,{0} people\n" +
        "Test.Newlines,一行目\\n二行目,first\\nsecond\n" +
        "Test.Langs4,日,英,简,繁\n";

    // 目的: 未知の言語では英語列が既定で使用されることを検証
    [Fact]
    public void Load_UsesTestCsv_EnglishByDefault()
    {
        // Arrange: unknown language => English fallback
        ModTranslation.SetTestTranslationCsv(TestCsv);
        ModTranslation.Cleanup();

        // Act
        ModTranslation.Load();

        // Assert
        // 目的: 未知言語では英語列が既定で取得されること
        ModTranslation.GetString("Test.Close").Should().Be("Close");
        // 目的: TryGetString が true を返し、値が取得できること
        ModTranslation.TryGetString("Test.Close", out var v).Should().BeTrue();
        // 目的: 取得された値が英語列であること
        v.Should().Be("Close");
    }

    // 目的: 文字列のフォーマットと未知キーでのフォールバック動作を検証
    [Fact]
    public void GetString_Format_And_TryGet_UnknownKey_WithTestCsv()
    {
        // Arrange
        ModTranslation.SetTestTranslationCsv(TestCsv);
        ModTranslation.Cleanup();
        ModTranslation.Load();

        // Act / Assert
        // 目的: 書式プレースホルダが置換されること
        ModTranslation.GetString("Test.People", 5).Should().Be("5 people");
        // 目的: 未知キーでは false を返すこと
        ModTranslation.TryGetString("__unknown_key__", out var value).Should().BeFalse();
        // 目的: 未知キー時の out 値が null であること
        value.Should().BeNull();
        // 目的: GetString ではキー文字列をそのまま返すこと
        ModTranslation.GetString("__unknown_key__").Should().Be("__unknown_key__");
    }

    // 目的: エスケープされた \n が実際の改行に置換されることを検証
    [Fact]
    public void EscapedNewlines_Replaced_WithTestCsv()
    {
        // Arrange
        ModTranslation.SetTestTranslationCsv(TestCsv);
        ModTranslation.Cleanup();
        ModTranslation.Load();

        // Act
        var s = ModTranslation.GetString("Test.Newlines");

        // Assert
        // 目的: 実際の改行が含まれること
        s.Should().Contain("\n");
        // 目的: エスケープ表記(\\n)は残らないこと
        s.Should().NotContain("\\n");
        // 目的: 期待する2行の文字列になること
        s.Should().Be("first\nsecond");
    }

    // 目的: 言語切替で適切な列が選択され、欠落時は英語へフォールバックすることを検証
    [Fact]
    public void LanguageSwitch_SelectsCorrectColumn_WithTestCsv()
    {
        // Arrange: Japanese (avoid touching IL2CPP DataManager in tests)
        ModTranslation.SetTestTranslationCsv(TestCsv);
        ModTranslation.SetTestLanguage(SupportedLangs.Japanese);
        ModTranslation.Cleanup();
        ModTranslation.Load();
        // 目的: 日本語選択時は日本語列が取得されること
        ModTranslation.GetString("Test.Close").Should().Be("閉じる");

        // SChinese: prefers 4th column if exists, else falls back to English
        ModTranslation.SetTestLanguage(SupportedLangs.SChinese);
        ModTranslation.Cleanup();
        ModTranslation.SetTestTranslationCsv(TestCsv);
        ModTranslation.Load();
        // 目的: 簡体中国語選択時、4列目が取得されること
        ModTranslation.GetString("Test.Langs4").Should().Be("简");
        // 目的: 対応列が無い場合は英語へフォールバックすること
        ModTranslation.GetString("Test.Close").Should().Be("Close");
    }

    // 目的: Cleanup 後はテーブルが解放され、LookUp が無効化されることを検証
    [Fact]
    public void Cleanup_ReleasesTables_DisablesLookup_WithTestCsv()
    {
        // Arrange
        ModTranslation.SetTestTranslationCsv(TestCsv);
        ModTranslation.Cleanup();
        ModTranslation.Load();

        // Sanity
        // 目的: 事前確認として値が取得できること
        ModTranslation.TryGetString("Test.Close", out var before).Should().BeTrue();
        // 目的: 取得された値が英語列であること
        before.Should().Be("Close");

        // Act
        ModTranslation.Cleanup();

        // Assert
        // 目的: Cleanup 後は検索できないこと
        ModTranslation.TryGetString("Test.Close", out var after).Should().BeFalse();
        // 目的: Cleanup 後の out 値は null であること
        after.Should().BeNull();
        // 目的: Cleanup 後はキーをそのまま返すこと
        ModTranslation.GetString("Test.Close").Should().Be("Test.Close");
    }
}
