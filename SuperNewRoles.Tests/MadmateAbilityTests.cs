using FluentAssertions;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability;
using Xunit;

namespace SuperNewRoles.Tests;

public class MadmateAbilityTests
{
    [Fact]
    public void CouldKnowImpostors_RecomputesAfterTaskCheckReset()
    {
        var data = new MadmateData(
            hasImpostorVision: false,
            couldUseVent: false,
            couldKnowImpostors: true,
            taskNeeded: 1,
            specialTasks: new TaskOptionData(0, 0, 0)
        );

        data.CouldKnowImpostors(complete: 1, all: 1).Should().BeTrue();
        data.CouldKnowImpostors(complete: 0, all: 1).Should().BeTrue();

        data.ResetTaskCheck();

        data.CouldKnowImpostors(complete: 0, all: 1).Should().BeFalse();
    }

    [Fact]
    public void CouldKnowImpostors_DoesNotCacheFailedOrDisabledChecks()
    {
        var disabled = new MadmateData(
            hasImpostorVision: false,
            couldUseVent: false,
            couldKnowImpostors: false,
            taskNeeded: 1,
            specialTasks: new TaskOptionData(0, 0, 0)
        );

        disabled.CouldKnowImpostors(complete: 1, all: 1).Should().BeFalse();

        var enabled = new MadmateData(
            hasImpostorVision: false,
            couldUseVent: false,
            couldKnowImpostors: true,
            taskNeeded: 2,
            specialTasks: new TaskOptionData(0, 0, 0)
        );

        enabled.CouldKnowImpostors(complete: -1, all: -1).Should().BeFalse();
        enabled.CouldKnowImpostors(complete: 0, all: 0).Should().BeFalse();
        enabled.CouldKnowImpostors(complete: 1, all: 3).Should().BeFalse();
        enabled.CouldKnowImpostors(complete: 2, all: 3).Should().BeTrue();
    }

    [Fact]
    public void CouldKnowImpostors_AllowsImmediateKnowledgeOnlyWhenRequiredTaskIsZero()
    {
        var data = new MadmateData(
            hasImpostorVision: false,
            couldUseVent: false,
            couldKnowImpostors: true,
            taskNeeded: 0,
            specialTasks: new TaskOptionData(0, 0, 0)
        );

        data.CouldKnowImpostors(complete: 0, all: 0).Should().BeTrue();
    }
}
