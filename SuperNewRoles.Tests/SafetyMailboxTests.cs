using System;
using System.Globalization;
using FluentAssertions;
using SuperNewRoles.RequestInGame;
using SuperNewRoles.Safety;
using Xunit;

namespace SuperNewRoles.Tests;

public class SafetyMailboxTests
{
    [Fact]
    public void MixesSafetyThreadByUpdatedAt()
    {
        var older = new RequestInGameManager.Thread("r1", "Bug", "hi", "2026-08-01T10:00:00Z", false, "open", "#32CD32", "●", "2026-08-01T10:00:00Z");
        var newer = new RequestInGameManager.Thread("r2", "Thanks", "hi", "2026-08-01T08:00:00Z", true, "open", "#32CD32", "●", "2026-08-01T12:00:00Z");
        var inbox = new SafetyNoticeInbox
        {
            Unread = true,
            UpdatedAt = DateTimeOffset.Parse("2026-08-01T11:00:00Z", CultureInfo.InvariantCulture).UtcDateTime,
            Events =
            {
                DateTimeOffset.Parse("2026-08-01T09:00:00Z", CultureInfo.InvariantCulture).UtcDateTime,
                DateTimeOffset.Parse("2026-08-01T11:00:00Z", CultureInfo.InvariantCulture).UtcDateTime
            }
        };

        var merged = SafetyMailbox.Merge(new[] { older, newer }, inbox, "【報告されたプレイヤーの処置について】");

        merged.Should().HaveCount(3);
        merged[0].thread_id.Should().Be("r2");
        merged[1].isSafetyNotice.Should().BeTrue();
        merged[1].title.Should().Be("【報告されたプレイヤーの処置について】");
        merged[1].unread.Should().BeTrue();
        merged[1].safetyEvents.Should().HaveCount(2);
        merged[2].thread_id.Should().Be("r1");
    }

    [Fact]
    public void TreatsNaiveReportTimestampsAsUtcWhenMixingWithSafetyNotices()
    {
        var report = new RequestInGameManager.Thread(
            "r-new",
            "最新の報告",
            "hi",
            "2026-08-29T12:00:00",
            false,
            "open",
            "#32CD32",
            "●",
            "2026-08-29T12:00:00");
        var inbox = new SafetyNoticeInbox
        {
            Unread = true,
            UpdatedAt = DateTimeOffset.Parse("2026-08-29T05:00:00Z", CultureInfo.InvariantCulture).UtcDateTime,
            Events = { DateTimeOffset.Parse("2026-08-29T05:00:00Z", CultureInfo.InvariantCulture).UtcDateTime }
        };

        report.updated_at.Should().Be(new DateTime(2026, 8, 29, 12, 0, 0, DateTimeKind.Utc));
        var merged = SafetyMailbox.Merge(new[] { report }, inbox, "【報告されたプレイヤーの処置について】");
        merged[0].thread_id.Should().Be("r-new");
        merged[1].isSafetyNotice.Should().BeTrue();
    }

    [Fact]
    public void DoesNotAddThreadWhenInboxIsEmpty()
    {
        var report = new RequestInGameManager.Thread("r1", "Bug", "hi", "2026-08-01T10:00:00Z", false, "open", "#32CD32", "●");
        SafetyMailbox.Merge(new[] { report }, new SafetyNoticeInbox(), "title").Should().Equal(report);
        SafetyMailbox.Merge(new[] { report }, null, "title").Should().HaveCount(1);
    }

    [Fact]
    public void InboxFromOmitsIdentityFields()
    {
        var inbox = SafetyNoticeInbox.From("""
            {"unread":true,"updated_at":"2026-08-01T11:00:00.000Z","events":[{"at":"2026-08-01T11:00:00.000Z","unread":true}]}
            """);
        inbox.Unread.Should().BeTrue();
        inbox.Events.Should().HaveCount(1);
        inbox.HasEvents.Should().BeTrue();
    }
}
