using System;
using System.Collections.Generic;

namespace SuperNewRoles.Safety.Listing;

public sealed class OccupancyGame
{
    public OccupancyGame(string code, string hostIdentityId, IReadOnlyList<string> occupantIds)
    {
        Code = code;
        HostIdentityId = hostIdentityId;
        OccupantIds = occupantIds ?? Array.Empty<string>();
    }

    public string Code { get; }
    public string HostIdentityId { get; }
    public IReadOnlyList<string> OccupantIds { get; }
}

public sealed class FilteredPublicGame
{
    public FilteredPublicGame(string code, bool hasBlockedPlayer)
    {
        Code = code;
        HasBlockedPlayer = hasBlockedPlayer;
    }

    public string Code { get; }
    public bool HasBlockedPlayer { get; }
}

/// <summary>
/// 公開一覧のホスト基準フィルタ。クライアントはIDを持たないため、本番の除外はサーバーが行う。
/// </summary>
public static class PublicGameListingFilter
{
    public static IReadOnlyList<FilteredPublicGame> Filter(
        IReadOnlyList<OccupancyGame> games,
        string viewerId,
        IReadOnlyCollection<string> blockedIds,
        IReadOnlyCollection<string> hostsWhoBlockedViewer)
    {
        var blocked = new HashSet<string>(blockedIds ?? Array.Empty<string>(), StringComparer.Ordinal);
        var blockedByHost = new HashSet<string>(hostsWhoBlockedViewer ?? Array.Empty<string>(), StringComparer.Ordinal);
        var result = new List<FilteredPublicGame>();

        foreach (OccupancyGame game in games ?? Array.Empty<OccupancyGame>())
        {
            if (game == null || string.IsNullOrEmpty(game.HostIdentityId)) continue;
            if (blocked.Contains(game.HostIdentityId)) continue;
            if (blockedByHost.Contains(game.HostIdentityId)) continue;

            bool hasBlockedOccupant = false;
            foreach (string occupant in game.OccupantIds)
            {
                if (occupant == game.HostIdentityId) continue;
                if (blocked.Contains(occupant))
                {
                    hasBlockedOccupant = true;
                    break;
                }
            }

            result.Add(new FilteredPublicGame(game.Code, hasBlockedOccupant));
        }

        return result;
    }
}
