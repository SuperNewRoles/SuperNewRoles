using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SuperNewRoles.Modules;
using SuperNewRoles.RequestInGame;
using SuperNewRoles.Safety.Api;

namespace SuperNewRoles.Safety;

public sealed class SafetyNoticeInbox
{
    public bool Unread;
    public DateTime UpdatedAt;
    public List<DateTime> Events = new();

    public bool HasEvents => Events != null && Events.Count > 0;

    public static SafetyNoticeInbox From(string text)
    {
        var inbox = new SafetyNoticeInbox();
        if (string.IsNullOrEmpty(text) || JsonParser.Parse(text) is not Dictionary<string, object> root)
            return inbox;

        inbox.Unread = root.TryGetValue("unread", out var unread) && unread is bool unreadFlag && unreadFlag;
        if (root.TryGetValue("updated_at", out var updated) && updated is string updatedText && DateTimeOffset.TryParse(updatedText, out DateTimeOffset updatedAt))
            inbox.UpdatedAt = updatedAt.UtcDateTime;

        if (root.TryGetValue("events", out var list) && list is List<object> events)
        {
            foreach (var item in events)
            {
                if (item is not Dictionary<string, object> row)
                    continue;
                if (row.TryGetValue("at", out var at) && at is string atText && DateTimeOffset.TryParse(atText, out DateTimeOffset parsed))
                    inbox.Events.Add(parsed.UtcDateTime);
            }
        }

        if (inbox.UpdatedAt == default && inbox.Events.Count > 0)
            inbox.UpdatedAt = inbox.Events[inbox.Events.Count - 1];
        return inbox;
    }
}

internal static class SafetyMailbox
{
    public const string ThreadId = "snr-safety-notices";
    private const string StatusColor = "#5C9EAD";

    public static IEnumerator LoadThreads(Action<List<RequestInGameManager.Thread>> callback)
    {
        List<RequestInGameManager.Thread> reportThreads = null;
        yield return RequestInGameManager.GetThreads(threads => reportThreads = threads);
        SafetyNoticeInbox inbox = null;
        yield return PlayerSafetyApiClient.GetNotices(value => inbox = value);
        if (reportThreads == null && (inbox == null || !inbox.HasEvents))
        {
            callback(null);
            yield break;
        }
        callback(Merge(reportThreads ?? new List<RequestInGameManager.Thread>(), inbox, ModTranslation.GetString("SafetyReportActionedTitle")));
    }

    public static List<RequestInGameManager.Thread> Merge(
        IEnumerable<RequestInGameManager.Thread> reportThreads,
        SafetyNoticeInbox inbox,
        string title)
    {
        var threads = reportThreads?.ToList() ?? new List<RequestInGameManager.Thread>();
        RequestInGameManager.Thread safety = ToThread(inbox, title);
        if (safety != null)
            threads.Add(safety);
        return threads
            .OrderByDescending(thread => thread.updated_at)
            .ToList();
    }

    public static RequestInGameManager.Thread ToThread(SafetyNoticeInbox inbox, string title)
    {
        if (inbox == null || !inbox.HasEvents)
            return null;

        DateTime updatedAt = inbox.UpdatedAt != default ? inbox.UpdatedAt : inbox.Events[inbox.Events.Count - 1];
        DateTime createdAt = inbox.Events[0];
        return new RequestInGameManager.Thread(
            ThreadId,
            title ?? string.Empty,
            string.Empty,
            createdAt.ToUniversalTime().ToString("o"),
            inbox.Unread,
            string.Empty,
            StatusColor,
            "●",
            updatedAt.ToUniversalTime().ToString("o"),
            true,
            inbox.Events);
    }
}
