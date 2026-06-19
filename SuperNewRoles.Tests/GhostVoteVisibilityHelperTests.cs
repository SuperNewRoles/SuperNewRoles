using FluentAssertions;
using SuperNewRoles.Roles.Ability;
using Xunit;

namespace SuperNewRoles.Tests;

public class GhostVoteVisibilityHelperTests
{
    [Fact]
    public void ShouldOverrideAnonymousVotesForGhost_ReturnsTrue_ForNormalGhost()
    {
        GhostVoteVisibilityHelper.ShouldOverrideAnonymousVotesForGhost(
            isDead: true,
            shouldHideGhostRolesFor: false
        ).Should().BeTrue();
    }

    [Fact]
    public void ShouldOverrideAnonymousVotesForGhost_ReturnsFalse_ForOrpheusReversibleCorpseVictim()
    {
        GhostVoteVisibilityHelper.ShouldOverrideAnonymousVotesForGhost(
            isDead: true,
            shouldHideGhostRolesFor: true
        ).Should().BeFalse();
    }

    [Fact]
    public void ShouldOverrideAnonymousVotesForGhost_ReturnsFalse_ForAlivePlayer()
    {
        GhostVoteVisibilityHelper.ShouldOverrideAnonymousVotesForGhost(
            isDead: false,
            shouldHideGhostRolesFor: false
        ).Should().BeFalse();
    }
}
