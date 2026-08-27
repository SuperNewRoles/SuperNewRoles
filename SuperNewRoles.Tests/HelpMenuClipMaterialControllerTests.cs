using FluentAssertions;
using SuperNewRoles.HelpMenus;
using Xunit;

namespace SuperNewRoles.Tests;

public class HelpMenuClipMaterialControllerTests
{
    [Fact]
    public void CalculateClipRectFromPoints_UsesAxisAlignedBounds()
    {
        var points = new[]
        {
            (2f, 1f),
            (-3f, 4f),
            (0.5f, -2f),
            (1f, 0f),
        };

        var clipRect = HelpMenuClipMaterialController.CalculateClipRectFromPoints(
            points,
            static point => point);

        clipRect.Left.Should().Be(-3f);
        clipRect.Bottom.Should().Be(-2f);
        clipRect.Right.Should().Be(2f);
        clipRect.Top.Should().Be(4f);
    }

    [Fact]
    public void CalculateClipRectFromPoints_EmptyOrNull_ReturnsZero()
    {
        HelpMenuClipMaterialController.CalculateClipRectFromPoints<(float x, float y)>(
                null,
                static point => point)
            .Should().Be((0f, 0f, 0f, 0f));
        HelpMenuClipMaterialController.CalculateClipRectFromPoints(
                System.Array.Empty<(float x, float y)>(),
                static point => point)
            .Should().Be((0f, 0f, 0f, 0f));
    }

    [Fact]
    public void ShouldUpdateClipForCamera_NullCamera_IsFalse()
    {
        HelpMenuClipMaterialController.ShouldUpdateClipForCamera(null).Should().BeFalse();
    }

    [Fact]
    public void DisableAllSpriteMasks_NullArray_DoesNotThrow()
    {
        HelpMenuClipMaterialController.DisableAllSpriteMasks(null);
    }
}
