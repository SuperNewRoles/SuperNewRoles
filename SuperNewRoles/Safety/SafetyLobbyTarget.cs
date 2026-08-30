using System.Collections.Generic;

namespace SuperNewRoles.Safety;

/// <summary>
/// フレンド一覧の clientId は試合をまたいで再利用される。
/// 通報もブロックも、今の GameId と記録時の部屋が一致し、今の部屋にその clientId がいるときだけ対象にする。
/// </summary>
internal static class SafetyLobbyTarget
{
    public static bool IsCurrentMatch(int currentGameId, int recordedGameId)
    {
        return currentGameId != 0 && recordedGameId == currentGameId;
    }

    public static int? ResolveClientId(
        int currentGameId,
        int recordedGameId,
        int recordedClientId,
        string recordedName,
        IReadOnlyList<(int Id, string Name)> liveClients)
    {
        _ = recordedName;
        if (!IsCurrentMatch(currentGameId, recordedGameId) || recordedClientId < 0 || liveClients == null)
            return null;

        for (int i = 0; i < liveClients.Count; i++)
        {
            if (liveClients[i].Id == recordedClientId)
                return recordedClientId;
        }

        return null;
    }
}
