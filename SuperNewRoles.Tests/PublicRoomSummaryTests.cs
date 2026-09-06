using SuperNewRoles.Modules;
using Xunit;

namespace SuperNewRoles.Tests;

public class PublicRoomSummaryTests
{
    // 目的: games だけを数え、非公開を含む metadata.allGamesCount は無視すること
    [Fact]
    public void CountsStartedPublicRoomsWithoutCountingPrivateMetadata()
    {
        string json = Payload(
            string.Join(",", Game(0), Game(1), Game(2), Game(3), Game(4), Game("Started")),
            extra: ",\"metadata\":{\"allGamesCount\":99}");

        Assert.True(PublicRoomSummary.TryParse(json, out var summary));
        Assert.Equal(6, summary.Total);
        Assert.Equal(4, summary.Busy);
    }

    // 目的: 満員・開始中・終了は1部屋1回だけ数え、空き部屋と破棄済みは busy に入れないこと
    [Fact]
    public void CountsFullAndStartingRoomsOnceAndExcludesAvailableOrDestroyedRooms()
    {
        string json = Payload(string.Join(",",
            Game(0, 15, 15),          // 満員の待機部屋 → busy
            Game(0, 14, 15),          // 空きあり待機 → joinable
            Game(1, 15, 15),          // 開始中の満員 → busy（二重計上しない）
            Game("Starting", 3, 15),  // 開始中 → busy
            Game(2, 15, 15),          // 開始済み → busy
            Game(3, 3, 15),           // 終了済み（空きあり） → busy
            Game("Ended", 15, 15),    // 終了済み満員 → busy（二重計上しない）
            Game(4, 15, 15),          // 破棄済み満員 → 除外
            Game(0, 0, 0)));          // 不正な定員 → どちらにも含めない

        Assert.True(PublicRoomSummary.TryParse(json, out var summary));
        Assert.Equal(9, summary.Total);
        Assert.Equal(6, summary.Busy);
        Assert.Equal(1, summary.Joinable);
    }

    // 目的: 公開ルーム0件の成功応答は「取得失敗」ではなく 0 件として扱うこと
    [Fact]
    public void EmptySuccessfulListIsZero()
    {
        Assert.True(PublicRoomSummary.TryParse("{\"games\":[]}", out var summary));
        Assert.Equal(0, summary.Total);
        Assert.Equal(0, summary.Busy);
        Assert.Equal(0, summary.Joinable);
    }

    // 目的: 参加可能は「待機中かつ定員に空きがある」場合だけであること
    [Theory]
    [InlineData("0", 15, 15, 0)]
    [InlineData("0", 16, 15, 0)]
    [InlineData("0", 14, 15, 1)]
    [InlineData("\"NotStarted\"", 3, 15, 1)]
    [InlineData("1", 3, 15, 0)]
    [InlineData("2", 3, 15, 0)]
    [InlineData("3", 3, 15, 0)]
    [InlineData("4", 3, 15, 0)]
    public void OnlyWaitingRoomsWithSpaceAreJoinable(string state, int players, int capacity, int expected)
    {
        string json = $"{{\"games\":[{{\"GameState\":{state},\"PlayerCount\":{players},\"MaxPlayers\":{capacity}}}]}}";
        Assert.True(PublicRoomSummary.TryParse(json, out var summary));
        Assert.Equal(1, summary.Total);
        Assert.Equal(expected, summary.Joinable);
    }

    // 目的: 壊れた応答や未知の GameState を空サーバーと誤認しないこと
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("{}")]
    [InlineData("{\"games\":null}")]
    [InlineData("{\"games\":[{}]}")]
    [InlineData("{\"games\":[{\"GameState\":99}]}")]
    [InlineData("<html>Bad Gateway</html>")]
    public void UnavailableOrUnknownResponseIsNotAnEmptyServer(string? json)
    {
        Assert.False(PublicRoomSummary.TryParse(json, out _));
    }

    private static string Game(object state, int? players = null, int? maxPlayers = null)
    {
        string stateJson = state is string name && name is not ("0" or "1" or "2" or "3" or "4")
            ? $"\"{name}\""
            : state.ToString() ?? "null";
        string extra = players is null ? "" : $",\"PlayerCount\":{players},\"MaxPlayers\":{maxPlayers}";
        return $"{{\"GameState\":{stateJson}{extra}}}";
    }

    private static string Payload(string games, string extra = "") =>
        $"{{\"games\":[{games}]{extra}}}";
}
