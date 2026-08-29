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

    [Fact]
    public void LevelImposterCustomMapId_Is7()
    {
        LevelImposterSupport.CustomMapId.Should().Be((byte)7);
        LevelImposterSupport.PluginGuid.Should().Be("com.DigiWorm.LevelImposter");
        LevelImposterSupport.ReactorPluginGuid.Should().Be("gg.reactor.api");
    }
}
