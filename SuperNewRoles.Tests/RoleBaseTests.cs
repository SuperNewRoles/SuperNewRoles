using FluentAssertions;
using SuperNewRoles.Roles;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability;
using Xunit;

namespace SuperNewRoles.Tests;

// Role/Ability ID 合成ロジックの決定論性を検証するテスト。
public class RoleBaseTests
{
    // 目的: 入力 (playerId, role, index) に対して決まった 64bit ID が生成されることを確認
    [Fact]
    public void GenerateDeterministicAbilityId_Is_Deterministic_And_Encodes_PlayerId()
    {
        byte playerId = 1;
        // Using a player parent to ensure playerId is encoded in top 8 bits.
        var id = ExPlayerControlExtensions.GenerateDeterministicAbilityId(playerId, new AbilityParentPlayer(null), typeof(object));
        // top 8 bits should equal the playerId
        ((id >> 56) & 0xFF).Should().Be(playerId);
        // Deterministic across multiple calls
        var id2 = ExPlayerControlExtensions.GenerateDeterministicAbilityId(playerId, new AbilityParentPlayer(null), typeof(object));
        id2.Should().Be(id);
    }
}
