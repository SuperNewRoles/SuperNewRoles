using System;
using System.Reflection;
using System.Runtime.Serialization;
using FluentAssertions;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.CrewMate;
using Xunit;

namespace SuperNewRoles.Tests;

public class BaitAbilityTests
{
    [Fact]
    public void ShouldAutoReport_WhenSelfWasKilledByAnotherPlayer()
    {
        var bait = CreateEx(7);
        var finder = CreateEx(8);

        BaitAbility.ShouldAutoReport(bait, finder, MurderResultFlags.Succeeded, bait).Should().BeTrue();
    }

    [Fact]
    public void ShouldAutoReport_WhenTargetIsNotSelf()
    {
        var bait = CreateEx(7);
        var finder = CreateEx(8);
        var other = CreateEx(1);

        BaitAbility.ShouldAutoReport(bait, finder, MurderResultFlags.Succeeded, other).Should().BeFalse();
    }

    [Fact]
    public void ShouldAutoReport_WhenKillWasSuicide()
    {
        var bait = CreateEx(7);

        BaitAbility.ShouldAutoReport(bait, bait, MurderResultFlags.Succeeded, bait).Should().BeFalse();
    }

    [Fact]
    public void ShouldAutoReport_WhenMurderDidNotSucceed()
    {
        var bait = CreateEx(7);
        var finder = CreateEx(8);

        BaitAbility.ShouldAutoReport(bait, finder, MurderResultFlags.FailedError, bait).Should().BeFalse();
    }

    [Fact]
    public void ShouldAutoReport_WhenSamePlayerIdEvenIfDifferentInstances()
    {
        var bait = CreateEx(7);
        var baitAlias = CreateEx(7);
        var finder = CreateEx(8);

        BaitAbility.ShouldAutoReport(bait, finder, MurderResultFlags.Succeeded, baitAlias).Should().BeTrue();
    }

    [Theory]
    [InlineData(0.5f, false, 0, 1.0f)]
    [InlineData(0f, false, 0, 0.5f)]
    [InlineData(2f, false, 0, 2.5f)]
    public void CalculateReportDelay_AddsMinimumFlashDelay(
        float reportTime,
        bool randomDelay,
        int delayVariation,
        float expected)
    {
        BaitAbility.CalculateReportDelay(reportTime, randomDelay, delayVariation).Should().Be(expected);
    }

    [Fact]
    public void CalculateReportDelay_RandomDelayUsesInclusiveRange()
    {
        int capturedMin = -1;
        int capturedMaxExclusive = -1;

        float delay = BaitAbility.CalculateReportDelay(
            reportTime: 2f,
            randomDelay: true,
            delayVariation: 1,
            randomRange: (min, maxExclusive) =>
            {
                capturedMin = min;
                capturedMaxExclusive = maxExclusive;
                return 3;
            });

        capturedMin.Should().Be(1);
        capturedMaxExclusive.Should().Be(4);
        delay.Should().Be(3.5f);
    }

    private static ExPlayerControl CreateEx(byte playerId)
    {
        var ex = (ExPlayerControl)FormatterServices.GetUninitializedObject(typeof(ExPlayerControl));
        var backingField = typeof(ExPlayerControl).GetField("<PlayerId>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("PlayerId backing field was not found.");
        backingField.SetValue(ex, playerId);
        return ex;
    }
}
