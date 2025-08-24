using FluentAssertions;
using SuperNewRoles.Roles;
using Xunit;

namespace SuperNewRoles.Tests;

// Role/Ability ID 合成ロジックの決定論性を検証するテスト。
public class RoleBaseTests
{
    // 目的: 入力 (playerId, role, index) に対して決まった 64bit ID が生成されることを確認
    [Theory]
    [InlineData((byte)1, RoleId.Impostor, 0, 1_000_000 + ((int)RoleId.Impostor * 1000) + 0)]
    [InlineData((byte)15, RoleId.Crewmate, 42, 15_000_000 + ((int)RoleId.Crewmate * 1000) + 42)]
    [InlineData((byte)0, RoleId.SilverBullet, 999, 0_000_000 + ((int)RoleId.SilverBullet * 1000) + 999)]
    public void GenerateAbilityId_ComposesDeterministically(byte playerId, RoleId role, int index, long expected)
    {
        // 検証: GenerateAbilityId が (playerId * 1_000_000) + (role * 1000) + index の決定論的合成になること
        var id = (long)IRoleBase.GenerateAbilityId(playerId, role, index);
        // 目的: 期待される決定論的合成値と一致すること
        id.Should().Be(expected);
    }
}
