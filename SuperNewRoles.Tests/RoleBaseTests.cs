using FluentAssertions;
using SuperNewRoles.Roles;
using Xunit;

namespace SuperNewRoles.Tests;

public class RoleBaseTests
{
    [Theory]
    [InlineData((byte)1, RoleId.Impostor, 0, 1_000_000 + ((int)RoleId.Impostor * 1000) + 0)]
    [InlineData((byte)15, RoleId.Crewmate, 42, 15_000_000 + ((int)RoleId.Crewmate * 1000) + 42)]
    [InlineData((byte)0, RoleId.Engineer, 999, 0_000_000 + ((int)RoleId.Engineer * 1000) + 999)]
    public void GenerateAbilityId_ComposesDeterministically(byte playerId, RoleId role, int index, long expected)
    {
        var id = (long)IRoleBase.GenerateAbilityId(playerId, role, index);
        id.Should().Be(expected);
    }
}
