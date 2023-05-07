using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AmongUs.GameOptions;
using SuperNewRoles.Mode;

namespace SuperNewRoles.Replay
{
    public class ReplayData
    {
        public static ReplayData currentReplayData;

        public string ReplayDataMod;
        public Version RecordVersion;
        public DateTime RecordTime;

        public int AllPlayersCount;
        public int AllBotsCount;
        public GameModes GameMode;
        public ModeId CustomMode;

        public float RecordRate;
        public bool IsPosFloat;

        public IGameOptions GameOptions;

        public Dictionary<int, int> CustomOptionSelections;

        public List<ReplayPlayer> ReplayPlayers;

        public void UpdateCustomOptionByData() {
            foreach (CustomOption opt in CustomOption.options) {
                var selection = CustomOptionSelections.FirstOrDefault(x => x.Key == opt.id);
                //nullチェック
                if (selection.Equals(default(KeyValuePair<int, int>)))
                {
                    opt.UpdateSelection(0);
                }
                else {
                    opt.UpdateSelection(selection.Value);
                }
            }
        }
    }
}
