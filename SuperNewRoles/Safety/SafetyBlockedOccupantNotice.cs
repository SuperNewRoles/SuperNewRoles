using System;
using System.Collections;
using System.Collections.Generic;
using SuperNewRoles.Safety.Api;

namespace SuperNewRoles.Safety;

internal static class SafetyBlockedOccupantNotice
{
    public const string WarningPrefix = "⚠ ";

    public static string JoinNames(IEnumerable<string> names)
    {
        var joined = new List<string>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (string name in names ?? Array.Empty<string>())
        {
            if (string.IsNullOrWhiteSpace(name)) continue;
            string trimmed = name.Trim();
            if (!seen.Add(trimmed)) continue;
            joined.Add(trimmed);
        }
        return string.Join("、", joined);
    }

    public static string Format(IEnumerable<string> names)
    {
        string joined = JoinNames(names);
        if (string.IsNullOrEmpty(joined))
            return ModTranslation.GetString("SafetyBlockedPlayerInRoomUnknown");
        return ModTranslation.GetString("SafetyBlockedPlayerInRoom", joined);
    }

    public static bool HostNameLooksWarned(string hostName)
    {
        return !string.IsNullOrEmpty(hostName) && hostName.StartsWith(WarningPrefix, StringComparison.Ordinal);
    }
}

internal static class SafetyBlockedOccupantCache
{
    private static readonly Dictionary<string, PublicGameRow> RowsByCode = new(StringComparer.OrdinalIgnoreCase);
    private static bool _fetching;
    private static DateTime _fetchedAtUtc = DateTime.MinValue;

    public static string NormalizeCode(string code)
    {
        return string.IsNullOrWhiteSpace(code) ? string.Empty : code.Trim().ToUpperInvariant();
    }

    public static bool TryGet(string code, out PublicGameRow row)
    {
        row = null;
        string key = NormalizeCode(code);
        return !string.IsNullOrEmpty(key) && RowsByCode.TryGetValue(key, out row) && row != null;
    }

    public static IEnumerator Refresh(bool force = false)
    {
        if (_fetching)
        {
            while (_fetching)
                yield return null;
            yield break;
        }

        if (!force && RowsByCode.Count > 0 && DateTime.UtcNow - _fetchedAtUtc < TimeSpan.FromSeconds(2))
            yield break;

        _fetching = true;
        List<PublicGameRow> rows = null;
        try
        {
            yield return PlayerSafetyApiClient.ListGames(list => rows = list);
            if (rows != null)
            {
                RowsByCode.Clear();
                foreach (PublicGameRow row in rows)
                {
                    string key = NormalizeCode(row?.Code);
                    if (string.IsNullOrEmpty(key)) continue;
                    RowsByCode[key] = row;
                }
                _fetchedAtUtc = DateTime.UtcNow;
            }
        }
        finally
        {
            _fetching = false;
        }
    }
}
