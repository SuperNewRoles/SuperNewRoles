using FluentAssertions;
using SuperNewRoles.Safety;
using Xunit;

namespace SuperNewRoles.Tests;

public class IdentityRootTests
{
    [Fact]
    public void SubtypeWireValuesStayStable()
    {
        IdentityRoot.Flag.Should().Be(0xD1);
        ((byte)IdentityRootSubtype.Prove).Should().Be(1);
        ((byte)IdentityRootSubtype.HostKick).Should().Be(2);
        ((byte)IdentityRootSubtype.Notify).Should().Be(3);
        ((byte)IdentityRootSubtype.Hello).Should().Be(4);
        ((byte)IdentityRootSubtype.Challenge).Should().Be(5);
        ((byte)IdentityRootSubtype.ParticipantIds).Should().Be(6);
        ((byte)IdentityRootSubtype.IdentityAccepted).Should().Be(7);
        ((byte)IdentityRootSubtype.Warn).Should().Be(8);
        ((byte)IdentityRootSubtype.ConductBanLeave).Should().Be(9);
        ((byte)IdentityRootSubtype.HostBlockLeave).Should().Be(10);
    }
}
