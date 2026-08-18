using FluentAssertions;
using SuperNewRoles.Roles.Ability;
using Xunit;

namespace SuperNewRoles.Tests;

public class BodyBuilderAbilityTests
{
    [Theory]
    [InlineData(0f)]
    [InlineData(0.01f)]
    [InlineData(-1f)]
    public void CalculatePosingVolume_ReturnsFullVolume_WhenDistanceIsNearZero(float distance)
    {
        float volume = BodyBuilderAbility.CalculatePosingVolume(distance);

        float.IsFinite(volume).Should().BeTrue();
        volume.Should().Be(1f);
    }

    [Theory]
    [InlineData(1f, 1f)]
    [InlineData(2f, 0.5f)]
    [InlineData(4f, 0f)]
    [InlineData(5f, 0f)]
    public void CalculatePosingVolume_UsesInverseDistanceWithCutoff(float distance, float expected)
    {
        BodyBuilderAbility.CalculatePosingVolume(distance).Should().Be(expected);
    }

    [Theory]
    [InlineData(true, 0.5f)]
    [InlineData(false, 1f)]
    public void GetRestoredSpriteAlpha_UsesHalfAlphaWhenDead(bool isDead, float expected)
    {
        BodyBuilderAbility.GetRestoredSpriteAlpha(isDead).Should().Be(expected);
    }
}
