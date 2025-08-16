using System;
using FluentAssertions;
using SuperNewRoles;
using Xunit;

namespace SuperNewRoles.Tests;

public class ModTranslationTests
{
    [Fact]
    public void Load_ShouldPopulate_AndReturnEnglishByDefault()
    {
        // Arrange
        ModTranslation.Cleanup();

        // Act
        ModTranslation.Load();
        var close = ModTranslation.GetString("Close");

        // Assert: Fallback language is English when environment language is unknown
        close.Should().Be("Close");
    }

    [Fact]
    public void GetString_WithFormat_And_TryGet_UnknownKey()
    {
        // Arrange
        ModTranslation.Cleanup();
        ModTranslation.Load();

        // Act / Assert: string.Format over translated template
        ModTranslation.GetString("People", 5).Should().Be("5 people");

        // Unknown key returns false and null via TryGet
        ModTranslation.TryGetString("__unknown_key__", out var value).Should().BeFalse();
        value.Should().BeNull();
    }

    [Fact]
    public void EscapedNewlines_AreConvertedToRealNewlines()
    {
        // Arrange
        ModTranslation.Cleanup();
        ModTranslation.Load();

        // Act
        var s = ModTranslation.GetString("AnalyticsPopupText");

        // Assert
        s.Should().Contain('\n');
        s.Should().NotContain("\\n");
    }

    [Fact]
    public void Cleanup_ShouldReleaseTables_AndDisableLookup()
    {
        // Arrange: ensure loaded
        ModTranslation.Cleanup();
        ModTranslation.Load();

        // Sanity: value exists before cleanup
        ModTranslation.TryGetString("Close", out var before).Should().BeTrue();
        before.Should().Be("Close");

        // Act
        ModTranslation.Cleanup();

        // Assert: lookup disabled -> TryGet false, GetString falls back to key
        ModTranslation.TryGetString("Close", out var after).Should().BeFalse();
        after.Should().BeNull();
        ModTranslation.GetString("Close").Should().Be("Close");
    }
}
