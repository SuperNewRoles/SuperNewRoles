using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AmongUs.GameOptions;
using InnerNet;
using SuperNewRoles.Mode;
using UnityEngine;

namespace SuperNewRoles.Replay;
public static class ReplayFileReader
{
    public static (BinaryReader, string) CreateReader(string filename)
    {
        string filePath = Path.GetDirectoryName(Application.dataPath) + @"\SuperNewRoles\Replay\";
        DirectoryInfo d = new(filePath);
        if (!d.Exists) d.Create();
        filePath += filename;// + ".replay";
        BinaryReader reader = null;
        try
        {
            reader = new BinaryReader(new FileStream(filePath, FileMode.Open, FileAccess.Read));
        }
        catch
        {
            return (null, filePath);
        }
        return (reader, filePath);
    }
    public static ReplayData ReadDoorData(BinaryReader reader, ReplayData replay)
    {
        bool IsDoor = reader.ReadBoolean();
        replay.DoorTrues = new();
        if (IsDoor)
        {
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                replay.DoorTrues.Add(reader.ReadBoolean());
            }
        }
        return replay;
    }
    public static ReplayData ReadSNRData(BinaryReader reader, ReplayData replay)
    {
        replay.ReplayDataMod = reader.ReadString();
        replay.RecordVersion = new Version(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadByte());
        replay.RecordTime = DateTime.ParseExact(reader.ReadString(), "yyyy-MM-dd-HH-mm-ss", null);
        return replay;
    }
    public static ReplayData ReadGameData(BinaryReader reader, ReplayData replay)
    {
        replay.GameMode = (GameModes)reader.ReadByte();
        replay.CustomMode = (ModeId)reader.ReadByte();
        replay.AllPlayersCount = reader.ReadInt32();
        replay.AllBotsCount = reader.ReadInt32();
        return replay;
    }
    public static bool IsCheckSumSuc(BinaryReader reader, ReplayData replay)
    {
        int checksum = reader.ReadInt32();
        int randomnum = reader.ReadInt32();
        return checksum == (replay.AllPlayersCount - (int)replay.CustomMode + randomnum);
    }
    public static ReplayData ReadReplayData(BinaryReader reader, ReplayData replay)
    {
        replay.RecordRate = reader.ReadSingle();
        replay.IsPosFloat = reader.ReadBoolean();
        return replay;
    }
    public static GameOptionsFactory factory = new(new UnityLogger().TryCast<Hazel.ILogger>());
    public static ReplayData ReadGameOptionData(BinaryReader reader, ReplayData replay)
    {
        int length = reader.ReadInt32();
        byte[] options = reader.ReadBytes(length);
        replay.GameOptions = factory.FromBytes(options);
        return replay;
    }
    public static ReplayData ReadCustomOptionData(BinaryReader reader, ReplayData replay)
    {
        Dictionary<int, int> options = new();
        int optionnum = reader.ReadInt32();
        for (int i = 0; i < optionnum; i++)
        {
            int id = reader.ReadInt32();
            int selection = reader.ReadInt32();
            options.Add(id, selection);
        }
        replay.CustomOptionSelections = options;
        return replay;
    }
    public static ReplayData ReadPlayerData(BinaryReader reader, ReplayData replay)
    {
        replay.ReplayPlayers = new();
        for (int i = 0; i < replay.AllPlayersCount; i++)
        {
            ReplayPlayer player = new()
            {
                PlayerId = reader.ReadByte()
            };
            if (reader.ReadBoolean())
            {
                player.IsBot = reader.ReadBoolean();
                player.PlayerName = reader.ReadString();
                player.ColorId = reader.ReadInt32();
                player.HatId = reader.ReadString();
                player.PetId = reader.ReadString();
                player.VisorId = reader.ReadString();
                player.NamePlateId = reader.ReadString();
                player.SkinId = reader.ReadString();
                int taskcount = reader.ReadInt32();
                player.Tasks = new();
                for (int i2 = 0; i2 < taskcount; i2++)
                {
                    uint taskId = reader.ReadUInt32();
                    byte taskType = reader.ReadByte();
                    player.Tasks.Add((taskId, taskType));
                }
                player.RoleId = (RoleId)reader.ReadByte();
                player.RoleType = (RoleTypes)reader.ReadByte();
            }
            else
            {
                Logger.Info("プレイヤー情報が保存されていませんでした。:" + player.PlayerId);
            }
            replay.ReplayPlayers.Add(player);
        }
        return replay;
    }
}