using System;
using System.Collections.Generic;
using SuperNewRoles.Patch;
using UnityEngine;

namespace SuperNewRoles.EndGame
{
    class FinalStatusPatch
    {
        public static class FinalStatusData
        {
            public static List<Tuple<Vector3, bool>> localPlayerPositions = new();
            public static List<DeadPlayer> deadPlayers = new();
            public static Dictionary<int, FinalStatus> FinalStatuses = new();

            public static void ClearFinalStatusData()
            {
                localPlayerPositions = new List<Tuple<Vector3, bool>>();
                deadPlayers = new List<DeadPlayer>();
                FinalStatuses = new Dictionary<int, FinalStatus>();
            }
        }
        public static string GetStatusText(FinalStatus status) => ModTranslation.GetString("FinalStatus" + status.ToString()); //ローカル関数

    }
    public enum FinalStatus
    {
        Alive,
        Kill,
        Exiled,
        NekomataExiled,
        SheriffKill,
        SheriffMisFire,
        MeetingSheriffKill,
        MeetingSheriffMisFire,
        SelfBomb,
        BySelfBomb,
        Ignite,
        Disconnected,
        Dead,
        Sabotage
    }
}