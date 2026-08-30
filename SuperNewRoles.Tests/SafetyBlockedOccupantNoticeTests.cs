using System.Collections.Generic;
using FluentAssertions;
using SuperNewRoles.Modules;
using SuperNewRoles.Safety;
using SuperNewRoles.Safety.Api;
using Xunit;

namespace SuperNewRoles.Tests;

public class SafetyBlockedOccupantNoticeTests
{
    [Fact]
    public void RecentPlayerRow_ReadsNegativeGameIdAndParticipationAlias()
    {
        var row = RecentPlayerRow.From(new Dictionary<string, object>
        {
            ["name"] = "Target",
            ["public_id"] = "participation-id",
            ["client_id"] = 7L,
            ["game_code"] = "ROOM20",
            ["game_id"] = -321L,
            ["last_seen_at"] = "2026-08-30T00:00:00.000Z",
        });

        row.Name.Should().Be("Target");
        row.PublicId.Should().Be("participation-id");
        row.ClientId.Should().Be(7);
        row.GameCode.Should().Be("ROOM20");
        row.GameId.Should().Be(-321);
    }

    [Fact]
    public void JoinNames_DedupesAndSkipsEmpty()
    {
        SafetyBlockedOccupantNotice.JoinNames(new[] { " Alice ", "", "Bob", "Alice", null })
            .Should().Be("Alice、Bob");
    }

    [Fact]
    public void PublicGameRow_ReadsBlockedNames()
    {
        var row = PublicGameRow.From(new Dictionary<string, object>
        {
            ["code"] = "ROOM01",
            ["host_name"] = "Host",
            ["player_count"] = 3L,
            ["max_players"] = 15L,
            ["has_blocked_player"] = true,
            ["blocked_player_names"] = new List<object> { "BlockedName", "  " },
        });

        row.Code.Should().Be("ROOM01");
        row.HasBlockedPlayer.Should().BeTrue();
        row.BlockedPlayerNames.Should().Equal("BlockedName");
        SafetyBlockedOccupantNotice.JoinNames(row.BlockedPlayerNames).Should().Be("BlockedName");
    }

    [Fact]
    public void PublicGameRow_TreatsNamesAsBlockedEvenWithoutFlag()
    {
        var row = PublicGameRow.From(new Dictionary<string, object>
        {
            ["code"] = "ROOM02",
            ["blocked_player_names"] = new List<object> { "X" },
        });
        row.HasBlockedPlayer.Should().BeTrue();
        row.BlockedPlayerNames.Should().Equal("X");
    }

    [Fact]
    public void HostNameLooksWarned_DetectsListingPrefix()
    {
        SafetyBlockedOccupantNotice.HostNameLooksWarned("⚠ Alice").Should().BeTrue();
        SafetyBlockedOccupantNotice.HostNameLooksWarned("Alice").Should().BeFalse();
    }
}
