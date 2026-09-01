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
    private static readonly Dictionary<string, PublicGameRow> RowsByRoom = new(StringComparer.OrdinalIgnoreCase);
    private static readonly HashSet<string> AmbiguousRooms = new(StringComparer.OrdinalIgnoreCase);
    private static bool _fetching;
    private static DateTime _fetchedAtUtc = DateTime.MinValue;

    public static string NormalizeCode(string code)
    {
        return string.IsNullOrWhiteSpace(code) ? string.Empty : code.Trim().ToUpperInvariant();
    }

    public static bool TryGet(string regionId, int gameId, string code, out PublicGameRow row)
    {
        row = null;
        string key = RoomKey(regionId, gameId, code);
        return !string.IsNullOrEmpty(key)
            && !AmbiguousRooms.Contains(key)
            && RowsByRoom.TryGetValue(key, out row)
            && row != null;
    }

    public static bool TryGetByListing(int gameId, string code, out PublicGameRow row)
    {
        row = null;
        string normalizedCode = NormalizeCode(code);
        PublicGameRow unique = null;
        foreach (PublicGameRow candidate in RowsByRoom.Values)
        {
            if (candidate == null || candidate.GameId != gameId)
                continue;
            if (!string.IsNullOrEmpty(normalizedCode)
                && !string.Equals(NormalizeCode(candidate.Code), normalizedCode, StringComparison.OrdinalIgnoreCase))
                continue;
            if (unique != null)
            {
                row = null;
                return false;
            }
            unique = candidate;
        }
        row = unique;
        return unique != null;
    }

    public static IEnumerator Refresh(bool force = false)
    {
        if (_fetching)
        {
            while (_fetching)
                yield return null;
            yield break;
        }

        if (!force && RowsByRoom.Count > 0 && DateTime.UtcNow - _fetchedAtUtc < TimeSpan.FromSeconds(2))
            yield break;

        _fetching = true;
        List<PublicGameRow> rows = null;
        try
        {
            yield return PlayerSafetyApiClient.ListGames(list => rows = list);
            if (rows != null)
            {
                RowsByRoom.Clear();
                AmbiguousRooms.Clear();
                foreach (PublicGameRow row in rows)
                {
                    if (row == null || row.GameId == int.MinValue || string.IsNullOrEmpty(row.RegionId))
                        continue;
                    string key = RoomKey(row.RegionId, row.GameId, row.Code);
                    if (string.IsNullOrEmpty(key) || AmbiguousRooms.Contains(key)) continue;
                    if (RowsByRoom.ContainsKey(key))
                    {
                        RowsByRoom.Remove(key);
                        AmbiguousRooms.Add(key);
                        continue;
                    }
                    RowsByRoom[key] = row;
                }
                _fetchedAtUtc = DateTime.UtcNow;
            }
        }
        finally
        {
            _fetching = false;
        }
    }

    private static string RoomKey(string regionId, int gameId, string code)
    {
        string region = string.IsNullOrWhiteSpace(regionId) ? string.Empty : regionId.Trim().ToLowerInvariant();
        string normalizedCode = NormalizeCode(code);
        return string.IsNullOrEmpty(region) || string.IsNullOrEmpty(normalizedCode)
            ? string.Empty
            : $"{region}\u0000{gameId}\u0000{normalizedCode}";
    }
}
