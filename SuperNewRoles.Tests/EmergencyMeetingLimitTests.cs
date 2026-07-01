using System.Reflection;
using FluentAssertions;
using SuperNewRoles.CustomOptions.Categories;
using SuperNewRoles.GameSettings;
using SuperNewRoles.Modules;
using Xunit;

namespace SuperNewRoles.Tests;

public class EmergencyMeetingLimitTests
{
    [Fact]
    public void Options_DefaultToOffAndBelongToGameSettings()
    {
        var limitField = typeof(GameSettingOptions).GetField(nameof(GameSettingOptions.IsLimitEmergencyMeeting), BindingFlags.Public | BindingFlags.Static);
        var countField = typeof(GameSettingOptions).GetField(nameof(GameSettingOptions.EmergencyMeetingLimitCount), BindingFlags.Public | BindingFlags.Static);

        limitField.Should().NotBeNull();
        var limitAttribute = limitField!.GetCustomAttribute<CustomOptionBoolAttribute>();
        limitAttribute.Should().NotBeNull();
        limitAttribute!.DefaultValue.Should().BeFalse();
        limitAttribute.ParentFieldName.Should().Be(nameof(Categories.GameSettings));

        countField.Should().NotBeNull();
        var countAttribute = countField!.GetCustomAttribute<CustomOptionIntAttribute>();
        countAttribute.Should().NotBeNull();
        countAttribute!.Min.Should().Be(0);
        countAttribute.Max.Should().Be(20);
        countAttribute.Step.Should().Be(1);
        countAttribute.DefaultValue.Should().Be(10);
        countAttribute.ParentFieldName.Should().Be(nameof(GameSettingOptions.IsLimitEmergencyMeeting));
    }

    [Fact]
    public void RecordMeeting_CountsOnlyEmergencyMeetings()
    {
        bool originalLimited = GameSettingOptions.IsLimitEmergencyMeeting;
        int originalLimit = GameSettingOptions.EmergencyMeetingLimitCount;

        try
        {
            GameSettingOptions.IsLimitEmergencyMeeting = true;
            GameSettingOptions.EmergencyMeetingLimitCount = 2;
            EmergencyMeetingLimit.Reset();

            EmergencyMeetingLimit.RecordMeeting(target: null);

            EmergencyMeetingLimit.EmergencyCount.Should().Be(1);
            EmergencyMeetingLimit.RemainingCount.Should().Be(1);
            EmergencyMeetingLimit.IsLimitReached.Should().BeFalse();
        }
        finally
        {
            GameSettingOptions.IsLimitEmergencyMeeting = originalLimited;
            GameSettingOptions.EmergencyMeetingLimitCount = originalLimit;
            EmergencyMeetingLimit.Reset();
        }
    }

    [Fact]
    public void ZeroLimit_BlocksFirstEmergencyMeeting()
    {
        bool originalLimited = GameSettingOptions.IsLimitEmergencyMeeting;
        int originalLimit = GameSettingOptions.EmergencyMeetingLimitCount;

        try
        {
            GameSettingOptions.IsLimitEmergencyMeeting = true;
            GameSettingOptions.EmergencyMeetingLimitCount = 0;
            EmergencyMeetingLimit.Reset();

            EmergencyMeetingLimit.IsLimitReached.Should().BeTrue();
            EmergencyMeetingLimit.ShouldBlockMeeting(target: null).Should().BeTrue();
        }
        finally
        {
            GameSettingOptions.IsLimitEmergencyMeeting = originalLimited;
            GameSettingOptions.EmergencyMeetingLimitCount = originalLimit;
            EmergencyMeetingLimit.Reset();
        }
    }
}
