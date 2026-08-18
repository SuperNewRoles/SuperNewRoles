using FluentAssertions;
using SuperNewRoles.Roles.Neutral;
using Xunit;

namespace SuperNewRoles.Tests;

public class FrankensteinAbilityTests
{
    [Theory]
    [InlineData(false, false, false)]
    [InlineData(false, true, false)]
    [InlineData(true, false, true)]
    [InlineData(true, true, false)]
    public void ShouldBlockIncomingKill_OnlyBlocksWhileMonsterIsNotResolvingItsOwnKill(
        bool isMonster,
        bool isMonsterKillInProgress,
        bool expected)
    {
        FrankensteinAbility.ShouldBlockIncomingKill(isMonster, isMonsterKillInProgress)
            .Should().Be(expected);
    }
}
