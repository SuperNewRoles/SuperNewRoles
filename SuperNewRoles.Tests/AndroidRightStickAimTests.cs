using FluentAssertions;
using SuperNewRoles.Modules;
using Xunit;

namespace SuperNewRoles.Tests;

public class AndroidRightStickAimTests
{
    [Fact]
    public void Requesters_Remain_Active_Until_Last_Request_Is_Removed()
    {
        var state = new AndroidRightStickAimCoreState();

        state.SetRequesterVisible(1, true);
        state.SetRequesterVisible(2, true);
        state.HasActiveRequesters.Should().BeTrue();

        state.SetRequesterVisible(1, false);
        state.HasActiveRequesters.Should().BeTrue();

        state.SetRequesterVisible(2, false);
        state.HasActiveRequesters.Should().BeFalse();
    }

    [Fact]
    public void ResolveDirection_Keeps_Last_NonZero_Input_When_Released()
    {
        var state = new AndroidRightStickAimCoreState();
        state.SetRequesterVisible(1, true);

        state.ResolveDirection(0f, 1f, -1f, 0f).Should().Be((0f, 1f));
        state.ResolveDirection(0f, 0f, -1f, 0f).Should().Be((0f, 1f));

        state.SetRequesterVisible(1, false);
        state.ResolveDirection(0f, 0f, -1f, 0f).Should().Be((-1f, 0f));
    }

    [Fact]
    public void TriggerHappy_Visibility_Predicate_Hides_During_Blocked_States()
    {
        AndroidAimVisibilityPolicy.ShouldShowForTriggerHappy(true, true, true, true, false, false, false, true).Should().BeTrue();
        AndroidAimVisibilityPolicy.ShouldShowForTriggerHappy(true, true, true, true, false, true, false, true).Should().BeFalse();
        AndroidAimVisibilityPolicy.ShouldShowForTriggerHappy(true, true, false, true, false, false, false, true).Should().BeFalse();
        AndroidAimVisibilityPolicy.ShouldShowForTriggerHappy(true, true, true, true, true, false, false, true).Should().BeFalse();
    }

    [Fact]
    public void Kunoichi_Visibility_Predicate_Hides_During_Blocked_States()
    {
        AndroidAimVisibilityPolicy.ShouldShowForKunoichi(true, true, true, true, false, false, false, false).Should().BeTrue();
        AndroidAimVisibilityPolicy.ShouldShowForKunoichi(true, true, true, true, false, true, false, false).Should().BeFalse();
        AndroidAimVisibilityPolicy.ShouldShowForKunoichi(true, true, true, true, true, false, false, false).Should().BeFalse();
        AndroidAimVisibilityPolicy.ShouldShowForKunoichi(true, true, true, true, false, false, false, true).Should().BeFalse();
    }
}
