using System;
using System.Collections.Generic;
using System.IO;
using SuperNewRoles.Patches;
using SuperNewRoles.Replay.ReplayActions;
using UnityEngine;

namespace SuperNewRoles.Replay
{
    public class ReplayTurn
    {
        public Dictionary<byte, List<Vector2>> Positions;
        public List<ReplayAction> Actions;
        public bool IsGameEnd;
        public ReplayEndGameData CurrentEndGameData;
    }
    public class ReplayEndGameData
    {
        public List<byte> WinnerPlayers;
        public GameOverReason OverReason;
        public WinCondition WinCond;
        public ReplayEndGameData(BinaryReader reader)
        {
            WinnerPlayers = new();
            int winnercount = reader.ReadInt32();
            for (int i = 0; i < winnercount; i++)
            {
                WinnerPlayers.Add(reader.ReadByte());
            }
            OverReason = (GameOverReason)reader.ReadByte();
            WinCond = (WinCondition)reader.ReadByte();
        }
    }
}