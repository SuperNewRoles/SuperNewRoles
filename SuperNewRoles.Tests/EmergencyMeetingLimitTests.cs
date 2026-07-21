using System.Reflection;
using FluentAssertions;
using SuperNewRoles.CustomOptions.Categories;
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

}
