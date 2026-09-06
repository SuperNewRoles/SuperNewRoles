using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using FluentAssertions;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles;
using Xunit;

namespace SuperNewRoles.Tests;

// Issue #2140: 排他の最大数を0に設定しても、そのグループの役職が選出されていた。
// 原因: ApplyExclusivitySettings が assignedRoleIds.Count == 0 の場合に早期リターンし、
// 最初の抽選前に除外が適用されていなかった（0 >= 0 であるべき除外が動かない）。
public class ExclusivitySettingsTests
{
    private static RoleOptionManager.RoleOption CreateRoleOptionStub(RoleId roleId)
    {
        var option = (RoleOptionManager.RoleOption)RuntimeHelpers.GetUninitializedObject(typeof(RoleOptionManager.RoleOption));
        typeof(RoleOptionManager.RoleOption)
            .GetField("<RoleId>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(option, roleId);
        return option;
    }

    private static AssignTickets CreateTicketStub(RoleId roleId)
    {
        var ticket = (AssignTickets)RuntimeHelpers.GetUninitializedObject(typeof(AssignTickets));
        typeof(AssignTickets)
            .GetField("<RoleOption>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(ticket, CreateRoleOptionStub(roleId));
        return ticket;
    }

    [Fact]
    public void MaxAssignZero_ExcludesGroupedRoles_BeforeAnyAssignment()
    {
        RoleOptionManager.ClearExclusivitySettings();
        try
        {
            RoleOptionManager.AddExclusivitySetting(0, new[] { nameof(RoleId.WaveCannon), nameof(RoleId.Rocket), nameof(RoleId.SelfBomber) });

            var notHundred = new List<AssignTickets>
            {
                CreateTicketStub(RoleId.WaveCannon),
                CreateTicketStub(RoleId.Sheriff),
            };
            var hundred = new List<AssignTickets>
            {
                CreateTicketStub(RoleId.Rocket),
                CreateTicketStub(RoleId.Jackal),
            };

            // まだ何もアサインされていない状態でも、最大数0のグループは除外されること
            RoleOptionManager.ApplyExclusivitySettings(
                new List<RoleId>(),
                new[] { notHundred },
                new[] { hundred });

            notHundred.Should().HaveCount(1);
            notHundred[0].RoleOption.RoleId.Should().Be(RoleId.Sheriff);
            hundred.Should().HaveCount(1);
            hundred[0].RoleOption.RoleId.Should().Be(RoleId.Jackal);
        }
        finally
        {
            RoleOptionManager.ClearExclusivitySettings();
        }
    }

    [Fact]
    public void MaxAssignOne_KeepsTickets_BeforeAnyAssignment()
    {
        RoleOptionManager.ClearExclusivitySettings();
        try
        {
            RoleOptionManager.AddExclusivitySetting(1, new[] { nameof(RoleId.WaveCannon), nameof(RoleId.Rocket) });

            var notHundred = new List<AssignTickets>
            {
                CreateTicketStub(RoleId.WaveCannon),
                CreateTicketStub(RoleId.Rocket),
            };

            // 最大数1の場合、何もアサインされていなければ除外されないこと（0 >= 1 は偽）
            RoleOptionManager.ApplyExclusivitySettings(
                new List<RoleId>(),
                new[] { notHundred },
                new[] { new List<AssignTickets>() });

            notHundred.Should().HaveCount(2);
        }
        finally
        {
            RoleOptionManager.ClearExclusivitySettings();
        }
    }

    [Fact]
    public void MaxAssignOne_ExcludesGroup_AfterReachingLimit()
    {
        RoleOptionManager.ClearExclusivitySettings();
        try
        {
            RoleOptionManager.AddExclusivitySetting(1, new[] { nameof(RoleId.WaveCannon), nameof(RoleId.Rocket) });

            var notHundred = new List<AssignTickets>
            {
                CreateTicketStub(RoleId.WaveCannon),
                CreateTicketStub(RoleId.Rocket),
                CreateTicketStub(RoleId.Sheriff),
            };

            // グループ内の1役職がアサインされたら、グループ全体が除外されること（従来の挙動）
            RoleOptionManager.ApplyExclusivitySettings(
                new List<RoleId> { RoleId.WaveCannon },
                new[] { notHundred },
                new[] { new List<AssignTickets>() });

            notHundred.Should().HaveCount(1);
            notHundred[0].RoleOption.RoleId.Should().Be(RoleId.Sheriff);
        }
        finally
        {
            RoleOptionManager.ClearExclusivitySettings();
        }
    }
}
