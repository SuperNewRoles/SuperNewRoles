using FluentAssertions;
using SuperNewRoles;
using SuperNewRoles.Modules.Compatibility;
using Xunit;

namespace SuperNewRoles.Tests;

public class StaticsBroadcastVersionTests
{
    [Theory]
    [InlineData(0, 25)]
    [InlineData(24, 49)]
    [InlineData(25, 25)]
    [InlineData(40, 40)]
    [InlineData(49, 49)]
    [InlineData(50, 75)]
    public void ApplyDisableServerAuthorityFlag_Adds25OnlyWhenRevisionBelow25(int input, int expected)
    {
        Statics.ApplyDisableServerAuthorityFlag(input).Should().Be(expected);
        Statics.ApplyDisableServerAuthorityFlag(expected).Should().Be(expected);
    }

    [Fact]
    public void ApplyDisableServerAuthorityFlag_DoesNotStackTo50()
    {
        int vanilla = Statics.ComputeAmongUsBroadcastVersion(2024, 8, 10, 0);
        int once = Statics.ApplyDisableServerAuthorityFlag(vanilla);
        int twice = Statics.ApplyDisableServerAuthorityFlag(once);

        (once - vanilla).Should().Be(25);
        twice.Should().Be(once);
        (twice % 50).Should().Be(25);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(0, 25)]
    [InlineData(25, 0)]
    [InlineData(25, 25)]
    public void V19SteamAndAndroid_AreCompatible(int pcFlag, int androidFlag)
    {
        int pc = 50663600 + pcFlag; // Constants: 2026.7.20.0
        int android = 50663650 + androidFlag; // Constants: 2026.7.21.0
        Statics.AreAmongUsBroadcastVersionsCompatible(pc, android).Should().BeTrue();
        Statics.AreAmongUsBroadcastVersionsCompatible(android, pc).Should().BeTrue();
    }

    [Theory]
    [InlineData(15)]
    [InlineData(16)]
    public void V18_IsNotCompatibleWithV19(int oldDay)
    {
        foreach (int oldFlag in new[] { 0, 25 })
        foreach (int currentFlag in new[] { 0, 25 })
        foreach (int currentDay in new[] { 20, 21 })
        {
            int oldVersion = Statics.ComputeAmongUsBroadcastVersion(2026, 7, oldDay) + oldFlag;
            int currentVersion = Statics.ComputeAmongUsBroadcastVersion(2026, 7, currentDay) + currentFlag;
            Statics.AreAmongUsBroadcastVersionsCompatible(oldVersion, currentVersion).Should().BeFalse();
            Statics.AreAmongUsBroadcastVersionsCompatible(currentVersion, oldVersion).Should().BeFalse();
        }
    }
    [Fact]
    public void LevelImposterCustomMapId_Is7()
    {
        LevelImposterSupport.CustomMapId.Should().Be((byte)7);
        LevelImposterSupport.PluginGuid.Should().Be("com.DigiWorm.LevelImposter");
        LevelImposterSupport.ReactorPluginGuid.Should().Be("gg.reactor.api");
    }
}
