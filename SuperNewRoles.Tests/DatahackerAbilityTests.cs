using FluentAssertions;
using SuperNewRoles.Roles.CrewMate;
using Xunit;

namespace SuperNewRoles.Tests;

public class DatahackerAbilityTests
{
    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 0)]
    public void EvaluateTaskProgress_DoesNotActivate_WhenPlayerHasNoTasks(int completedTasks, int totalTasks)
    {
        var result = DatahackerAbility.EvaluateTaskProgress(
            completedTasks,
            totalTasks,
            taskRequirePercent: 100f,
            exposeTasksLeft: 2
        );

        result.exposedToImpostors.Should().BeFalse();
        result.hackingCompleted.Should().BeFalse();
    }

    [Theory]
    [InlineData(0, 1, 100f, 2, true, false)]
    [InlineData(1, 1, 100f, 2, true, true)]
    [InlineData(2, 4, 50f, 0, true, true)]
    [InlineData(1, 4, 50f, 0, false, false)]
    public void EvaluateTaskProgress_UsesTaskProgressAndExposeThreshold(
        int completedTasks,
        int totalTasks,
        float taskRequirePercent,
        int exposeTasksLeft,
        bool expectedExposed,
        bool expectedCompleted)
    {
        var result = DatahackerAbility.EvaluateTaskProgress(
            completedTasks,
            totalTasks,
            taskRequirePercent,
            exposeTasksLeft
        );

        result.exposedToImpostors.Should().Be(expectedExposed);
        result.hackingCompleted.Should().Be(expectedCompleted);
    }
}
