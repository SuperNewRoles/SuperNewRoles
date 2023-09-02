using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AmongUs.GameOptions;
using Il2CppSystem.IO;
using SuperNewRoles.Mode;
using UnityEngine;

namespace SuperNewRoles.Replay
{
    public enum ReplayState
    {
        Play,
        Pause,
        FastPlay,
        PlayRewind,
        None
    }
    public enum MovingPlatformState
    {
        None,
        Init,
        WalkTo1st,
        WalkTo2nd,
        WaitEffect1st,
        Slide,
        WalkTo3rd,
        WaitEffect2nd
    }
    public enum LadderState
    {
        None,
        Init,
        WalkTo1st,
        WaitEffect1st,
        WalkTo2nd,
        WaitEffect2nd
    }
    public enum UIState
    {
        ShowOperationUI,
        Default,
        HideAllUI
    }
    public class ReplayData
    {
        public bool IsFirstLoaded = false;
        public string FilePath;
        public ReplayState CurrentPlayState = ReplayState.Play;
        public UIState CurrentUIState = UIState.Default;

        public MovingPlatformState CurrentMovingPlatformState = MovingPlatformState.None;
        public int MovingPlatformFrameCount = 0;
        public Vector3 MovingPlatformPosition = new(-999, -999, -999);

        public Dictionary<byte, LadderState> CurrentLadderState = new();
        public Vector3 LadderPosition = new(-999, -999, -999);
        public Dictionary<byte, Ladder> CurrentLadder = new();

        public List<bool> DoorTrues;

        public string ReplayDataMod;
        public Version RecordVersion;
        public DateTime RecordTime;
        public bool CheckSum = false;

        public int AllPlayersCount;
        public int AllBotsCount;
        public GameModes GameMode;
        public ModeId CustomMode;

        public float RecordRate;
        public bool IsPosFloat;

        public IGameOptions GameOptions;

        public Dictionary<int, int> CustomOptionSelections;

        public List<ReplayPlayer> ReplayPlayers;

        public System.IO.BinaryReader binaryReader;

        public void UpdateCustomOptionByData()
        {
            foreach (CustomOption opt in CustomOption.options)
            {
                var selection = CustomOptionSelections.FirstOrDefault(x => x.Key == opt.id);
                //nullチェック
                if (selection.Equals(default(KeyValuePair<int, int>)))
                {
                    opt.UpdateSelection(0);
                }
                else
                {
                    opt.UpdateSelection(selection.Value);
                }
            }
        }
    }
}