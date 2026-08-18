using FluentAssertions;
using SuperNewRoles.Roles.Ability;
using Xunit;

namespace SuperNewRoles.Tests;

public class SchrodingersCatAbilityTests
{
    [Theory]
    [InlineData(0f, 0f, true)]
    [InlineData(1.5f, 0f, true)]
    [InlineData(3.5f, 0f, true)]
    [InlineData(0f, 3.5f, true)]
    [InlineData(3.51f, 0f, false)]
    [InlineData(4f, 0f, false)]
    [InlineData(20f, 0f, false)]
    public void ShouldSnapKillerToTarget_OnlyWithinMeleeDistance(float deltaX, float deltaY, bool expected)
    {
        SchrodingersCatAbility.ShouldSnapKillerToTarget(0f, 0f, deltaX, deltaY).Should().Be(expected);
    }

    [Fact]
    public void MaxMeleeKillSnapDistance_IsInclusiveBoundary()
    {
        SchrodingersCatAbility.MaxMeleeKillSnapDistance.Should().Be(3.5f);
    }
}
