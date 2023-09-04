using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SuperNewRoles.Replay
{
    public static class ReplayReader
    {
        public static void ClearAndReloads()
        {
            return;
            if (reader != null)
                reader.Close();
            reader = null;
        }
        public static BinaryReader reader;
        static string filePath;
        //返り値はちゃんと読み込めたか(フアイルのデータがこわれていないか)
        public static (ReplayData, bool) ReadReplayDataSelector(string filename)
        {
            (reader, filePath) = ReplayFileReader.CreateReader(filename);
            var replay = new ReplayData();
            if (reader == null)
                return (replay, false);
            replay.binaryReader = reader;
            try
            {
                replay = ReplayFileReader.ReadSNRData(reader, replay);
                replay = ReplayFileReader.ReadGameData(reader, replay);

                if (!ReplayFileReader.IsCheckSumSuc(reader, replay))
                    return (replay, false);
            }
            catch (EndOfStreamException)
            {
                Logger.Info("ふぁいるのないようすくなすぎ！");
                return (replay, false);
            }
            replay.CheckSum = true;
            replay.FilePath = filename;
            return (replay, true);
        }
        //返り値はちゃんと読み込めたか(フアイルのデータがこわれていないか)
        public static (ReplayData, bool) ReadReplayDataFirst(string filename)
        {
            (reader, filePath) = ReplayFileReader.CreateReader(filename);
            var replay = new ReplayData();
            try
            {
                replay = ReplayFileReader.ReadSNRData(reader, replay);
                replay = ReplayFileReader.ReadGameData(reader, replay);

                if (!ReplayFileReader.IsCheckSumSuc(reader, replay))
                    return (replay, false);
            }
            catch (EndOfStreamException)
            {
                Logger.Info("ふぁいるのないようすくなすぎ！");
                return (replay, false);
            }
            replay = ReplayFileReader.ReadReplayData(reader, replay);
            replay = ReplayFileReader.ReadGameOptionData(reader, replay);
            replay = ReplayFileReader.ReadCustomOptionData(reader, replay);
            replay = ReplayFileReader.ReadPlayerData(reader, replay);
            replay = ReplayFileReader.ReadDoorData(reader, replay);
            ReplayManager.CurrentReplay = replay;
            replay.binaryReader = reader;
            replay.FilePath = filename;
            return (replay, true);
        }
    }
}