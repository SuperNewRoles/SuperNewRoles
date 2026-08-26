using FluentAssertions;
using SuperNewRoles.Patches;
using Xunit;

namespace SuperNewRoles.Tests;

public class PhantomAbilityHostFixTests
{
    [Theory]
    [InlineData(true, true)]
    [InlineData(false, false)]
    public void ShouldProcessCheckLocally_IsTrue_OnlyForHost(bool amHost, bool expected)
    {
        PhantomAbilityHostFix.ShouldProcessCheckLocally(amHost).Should().Be(expected);
    }
}
