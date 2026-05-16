using FluentAssertions;
using SuperNewRoles.Modules;
using Xunit;

namespace SuperNewRoles.Tests;

public class DeadBodyVisibilityTests
{
    [Fact]
    public void IsHiddenPosition_ReturnsTrue_ForHiddenDeadBodyPosition()
    {
        DeadBodyVisibility.IsHiddenPosition(9999f, 9999f)
            .Should().BeTrue();
    }

    [Theory]
    [InlineData(9998.9f, 9999f)]
    [InlineData(9999f, 9998.9f)]
    [InlineData(0f, 0f)]
    public void IsHiddenPosition_ReturnsFalse_ForNormalPositions(float x, float y)
    {
        DeadBodyVisibility.IsHiddenPosition(x, y)
            .Should().BeFalse();
    }

    [Fact]
    public void IsTrackableDeadBodyPosition_ExcludesHiddenDeadBodiesFromArrows()
    {
        DeadBodyArrowsAbility.IsTrackableDeadBodyPosition(9999f, 9999f)
            .Should().BeFalse();
    }

    [Theory]
    [InlineData(9998.9f, 9999f)]
    [InlineData(9999f, 9998.9f)]
    [InlineData(0f, 0f)]
    public void IsTrackableDeadBodyPosition_KeepsNormalDeadBodiesTrackable(float x, float y)
    {
        DeadBodyArrowsAbility.IsTrackableDeadBodyPosition(x, y)
            .Should().BeTrue();
    }
}
