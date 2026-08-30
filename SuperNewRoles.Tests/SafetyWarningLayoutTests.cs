using FluentAssertions;
using SuperNewRoles.Modules;
using SuperNewRoles.Safety;
using Xunit;

namespace SuperNewRoles.Tests;

public class SafetyWarningLayoutTests
{
    [Fact]
    public void PrefersHudOverlayDuringMatch()
    {
        SafetyWarningLayout.ChooseParent(hudAvailable: true, cameraAvailable: true)
            .Should().Be(SafetyWarningLayout.ParentKind.Hud);
    }

    [Fact]
    public void UsesCameraChildOnMainMenu()
    {
        SafetyWarningLayout.ChooseParent(hudAvailable: false, cameraAvailable: true)
            .Should().Be(SafetyWarningLayout.ParentKind.Camera);
    }

    [Fact]
    public void FallsBackToHostWhenNoHudOrCamera()
    {
        SafetyWarningLayout.ChooseParent(hudAvailable: false, cameraAvailable: false)
            .Should().Be(SafetyWarningLayout.ParentKind.Host);
    }

    [Fact]
    public void HudWinsEvenWithoutCamera()
    {
        SafetyWarningLayout.ChooseParent(hudAvailable: true, cameraAvailable: false)
            .Should().Be(SafetyWarningLayout.ParentKind.Hud);
    }
}

public class SafetyWarningMarkdownTests
{
    [Fact]
    public void ConvertsBoldMarkdown()
    {
        MarkdownToUnityTag.ConvertForSafetyPopup("**hello**")
            .Should().Be("<b>hello</b>");
    }

    [Fact]
    public void StripsSpriteTagsThatCrashTmpInGame()
    {
        MarkdownToUnityTag.ConvertForSafetyPopup("<sprite=0>hello")
            .Should().Be("hello");
        MarkdownToUnityTag.ConvertForSafetyPopup("<sprite index=1>x</sprite>")
            .Should().Be("x");
    }

    [Fact]
    public void StripsMaterialAndQuadTags()
    {
        MarkdownToUnityTag.StripUnsafeTmpTags("<material=1>a</material><quad=0>b")
            .Should().Be("ab");
    }

    [Fact]
    public void EmptyBodyStaysEmpty()
    {
        MarkdownToUnityTag.ConvertForSafetyPopup(null).Should().BeEmpty();
        MarkdownToUnityTag.ConvertForSafetyPopup("").Should().BeEmpty();
    }
}

public class WarningAckCountdownTests
{
    [Theory]
    [InlineData(0f, 5)]
    [InlineData(0.99f, 5)]
    [InlineData(1f, 4)]
    [InlineData(2f, 3)]
    [InlineData(3f, 2)]
    [InlineData(4f, 1)]
    [InlineData(4.99f, 1)]
    public void ShowsIntegerCountdownUntilReady(float elapsed, int remaining)
    {
        WarningAckCountdown.RemainingDisplay(elapsed).Should().Be(remaining);
        WarningAckCountdown.IsReady(elapsed).Should().BeFalse();
        WarningAckCountdown.ButtonLabel(elapsed, "確認しました").Should().Be(remaining.ToString());
    }

    [Theory]
    [InlineData(5f)]
    [InlineData(5.01f)]
    [InlineData(8f)]
    public void RestoresAckLabelWhenReady(float elapsed)
    {
        WarningAckCountdown.RemainingDisplay(elapsed).Should().Be(0);
        WarningAckCountdown.IsReady(elapsed).Should().BeTrue();
        WarningAckCountdown.ButtonLabel(elapsed, "確認しました").Should().Be("確認しました");
    }

    [Fact]
    public void DimDarkensOriginalColorAndKeepsAlpha()
    {
        var original = new UnityEngine.Color(0.2f, 0.6f, 0.9f, 0.8f);
        var dimmed = WarningAckCountdown.Dim(original);
        dimmed.r.Should().BeApproximately(original.r * WarningAckCountdown.DimMultiplier, 0.0001f);
        dimmed.g.Should().BeApproximately(original.g * WarningAckCountdown.DimMultiplier, 0.0001f);
        dimmed.b.Should().BeApproximately(original.b * WarningAckCountdown.DimMultiplier, 0.0001f);
        dimmed.a.Should().BeApproximately(original.a, 0.0001f);
    }
}
