using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SuperNewRoles.Replay
{
    public static class ReplayReader
    {
        public static void ClearAndReloads() {
            if (reader != null)
                reader.Close();
            reader = null;
        }
        static BinaryReader reader;
        static string filePath;
        public static ReplayData replay;
        //返り値はちゃんと読み込めたか(フアイルのデータがこわれていないか)
        public static (ReplayData, bool) ReadReplayDataFirst(string filename)
        {
            (reader, filePath) = ReplayFileReader.CreateReader(filename);
            replay = new();
            replay = ReplayFileReader.ReadSNRData(reader, replay);
            replay = ReplayFileReader.ReadGameData(reader, replay);
            if (!ReplayFileReader.IsCheckSumSuc(reader, replay))
                return (replay, false);
            replay = ReplayFileReader.ReadReplayData(reader, replay);
            replay = ReplayFileReader.ReadGameOptionData(reader, replay);
            replay = ReplayFileReader.ReadCustomOptionData(reader, replay);
            replay = ReplayFileReader.ReadPlayerData(reader, replay);
            return (replay, true);
        }
    }
}
