using System;
using System.Collections.Generic;

namespace SuperNewRoles.Modules;

/// <summary>
/// Impostor の公開ルーム一覧から、参加可否の集計を作る。
/// </summary>
public readonly struct PublicRoomSummary
{
    public int Total { get; }
    public int Busy { get; }
    public int Joinable { get; }

    private PublicRoomSummary(int total, int busy, int joinable)
    {
        Total = total;
        Busy = busy;
        Joinable = joinable;
    }

    /// <summary>
    /// /api/games/all_for_web の JSON を集計する。
    /// games は公開ルームのみ。metadata.allGamesCount は非公開も含むので使わない。
    /// </summary>
    public static bool TryParse(string json, out PublicRoomSummary summary)
    {
        summary = default;
        if (string.IsNullOrWhiteSpace(json))
            return false;

        try
        {
            if (JsonParser.Parse(json) is not Dictionary<string, object> root ||
                !root.TryGetValue("games", out var value) ||
                value is not List<object> games)
                return false;

            int busy = 0;
            int joinable = 0;
            foreach (var item in games)
            {
                if (!TryClassify(item, out bool isJoinable, out bool isBusy))
                    return false;
                if (isJoinable) joinable++;
                if (isBusy) busy++;
            }

            summary = new PublicRoomSummary(games.Count, busy, joinable);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    // Impostor の GameState。API は数値でも名前でも返す。
    // NotStarted=0, Starting=1, Started=2, Ended=3, Destroyed=4
    private enum RoomState
    {
        Unknown = -1,
        NotStarted = 0,
        Starting = 1,
        Started = 2,
        Ended = 3,
        Destroyed = 4,
    }

    private static bool TryClassify(object item, out bool isJoinable, out bool isBusy)
    {
        isJoinable = false;
        isBusy = false;
        if (item is not Dictionary<string, object> game ||
            !game.TryGetValue("GameState", out var rawState))
            return false;

        RoomState state = ParseState(rawState);
        if (state == RoomState.Unknown)
            return false;

        bool hasOccupancy = TryGetOccupancy(game, out bool isFull);
        // 待機中かつ空きがある部屋だけ参加可能。終了・破棄は「部屋作成」を抑止しない。
        isJoinable = state == RoomState.NotStarted && hasOccupancy && !isFull;
        // 開始中・開始済み・終了、または満員の待機部屋を1部屋につき1回だけ数える。破棄済みは除外する。
        isBusy = state is RoomState.Starting or RoomState.Started or RoomState.Ended ||
                 (isFull && state == RoomState.NotStarted);
        return true;
    }

    private static RoomState ParseState(object raw)
    {
        return raw?.ToString() switch
        {
            "0" or "NotStarted" => RoomState.NotStarted,
            "1" or "Starting" => RoomState.Starting,
            "2" or "Started" => RoomState.Started,
            "3" or "Ended" => RoomState.Ended,
            "4" or "Destroyed" => RoomState.Destroyed,
            _ => RoomState.Unknown,
        };
    }

    private static bool TryGetOccupancy(Dictionary<string, object> game, out bool isFull)
    {
        isFull = false;
        game.TryGetValue("PlayerCount", out var players);
        game.TryGetValue("MaxPlayers", out var capacity);
        if (!int.TryParse(players?.ToString(), out int playerCount) ||
            !int.TryParse(capacity?.ToString(), out int maxPlayers) ||
            playerCount < 0 ||
            maxPlayers <= 0)
            return false;

        isFull = playerCount >= maxPlayers;
        return true;
    }
}
