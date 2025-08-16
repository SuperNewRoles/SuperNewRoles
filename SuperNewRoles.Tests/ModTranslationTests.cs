using System;
using FluentAssertions;
using SuperNewRoles;
using Xunit;
using AmongUs.Data;

namespace SuperNewRoles.Tests;

public class ModTranslationTests
{
    private const string TestCsv =
        "# comment line should be ignored\n" +
        "Test.Close,閉じる,Close\n" +
        "Test.People,{0} 人,{0} people\n" +
        "Test.Newlines,一行目\\n二行目,first\\nsecond\n" +
        "Test.Langs4,日,英,简,繁\n";

    [Fact]
    public void Load_UsesTestCsv_EnglishByDefault()
    {
        // Arrange: unknown language => English fallback
        ModTranslation.SetTestTranslationCsv(TestCsv);
        ModTranslation.Cleanup();

        // Act
        ModTranslation.Load();

        // Assert
        ModTranslation.GetString("Test.Close").Should().Be("Close");
        ModTranslation.TryGetString("Test.Close", out var v).Should().BeTrue();
        v.Should().Be("Close");
    }

    [Fact]
    public void GetString_Format_And_TryGet_UnknownKey_WithTestCsv()
    {
        // Arrange
        ModTranslation.SetTestTranslationCsv(TestCsv);
        ModTranslation.Cleanup();
        ModTranslation.Load();

        // Act / Assert
        ModTranslation.GetString("Test.People", 5).Should().Be("5 people");
        ModTranslation.TryGetString("__unknown_key__", out var value).Should().BeFalse();
        value.Should().BeNull();
        ModTranslation.GetString("__unknown_key__").Should().Be("__unknown_key__");
    }

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
        s.Should().Contain('\n');
        s.Should().NotContain("\\n");
        s.Should().Be("first\nsecond");
    }

    [Fact]
    public void LanguageSwitch_SelectsCorrectColumn_WithTestCsv()
    {
        // Arrange: Japanese
        ModTranslation.SetTestTranslationCsv(TestCsv);
        DataManager.Settings.Language.CurrentLanguage = SupportedLangs.Japanese;
        ModTranslation.Cleanup();
        ModTranslation.Load();
        ModTranslation.GetString("Test.Close").Should().Be("閉じる");

        // SChinese: prefers 4th column if exists, else falls back to English
        DataManager.Settings.Language.CurrentLanguage = SupportedLangs.SChinese;
        ModTranslation.Cleanup();
        ModTranslation.SetTestTranslationCsv(TestCsv);
        ModTranslation.Load();
        ModTranslation.GetString("Test.Langs4").Should().Be("简");
        // Missing SChinese column -> fallback to English
        ModTranslation.GetString("Test.Close").Should().Be("Close");
    }

    [Fact]
    public void Cleanup_ReleasesTables_DisablesLookup_WithTestCsv()
    {
        // Arrange
        ModTranslation.SetTestTranslationCsv(TestCsv);
        ModTranslation.Cleanup();
        ModTranslation.Load();

        // Sanity
        ModTranslation.TryGetString("Test.Close", out var before).Should().BeTrue();
        before.Should().Be("Close");

        // Act
        ModTranslation.Cleanup();

        // Assert
        ModTranslation.TryGetString("Test.Close", out var after).Should().BeFalse();
        after.Should().BeNull();
        ModTranslation.GetString("Test.Close").Should().Be("Test.Close");
    }
}
