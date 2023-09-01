using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SuperNewRoles.Mode;
using UnityEngine;

namespace SuperNewRoles.Replay;
public static class ReplayFileWriter
{
    public static (BinaryWriter, string) CreateWriter()
    {
        string filePath = Path.GetDirectoryName(Application.dataPath) + @"\SuperNewRoles\Replay\";
        DirectoryInfo d = new(filePath);
        if (!d.Exists) d.Create();
        filePath += "Replay_" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".replay";
        var writer = new BinaryWriter(new FileStream(filePath, FileMode.CreateNew, FileAccess.Write));
        return (writer, filePath);
    }
    public static void WriteDoorData(BinaryWriter writer)
    {
        ElectricalDoors electrical = ShipStatus.Instance?.transform?.Find("Electrical")?.GetComponent<ElectricalDoors>();
        if (electrical == null)
        {
            Logger.Info("エレキドアがありませんでした。");
            writer.Write(false);
        }
        else
        {
            writer.Write(true);
            writer.Write(electrical.Doors.Count);
            foreach (StaticDoor door in electrical.Doors)
            {
                writer.Write(door.IsOpen);
            }
        }
    }
    public static void WriteSNRData(BinaryWriter writer)
    {
        Version version = SuperNewRolesPlugin.ThisVersion;
        //バージョン
        writer.Write(SuperNewRolesPlugin.ThisPluginModName + "-" + version.Revision.ToString());
        writer.Write(version.Major);
        writer.Write(version.Minor);
        writer.Write(version.Build);
        writer.Write((byte)version.Revision);
        writer.Write(DateTime.UtcNow.ToString("yyyy-MM-dd-HH-mm-ss"));
    }
    public static void WriteGameData(BinaryWriter writer)
    {
        writer.Write((byte)GameOptionsManager.Instance.CurrentGameOptions.GameMode);
        writer.Write((byte)ModeHandler.GetMode());
        writer.Write(GameData.Instance.AllPlayers.Count);
        writer.Write(BotManager.AllBots.Count);
    }
    public static void WriteCheckSam(BinaryWriter writer)
    {
        int randomnum = ModHelpers.GetRandomInt(9);
        writer.Write(GameData.Instance.AllPlayers.Count - (byte)ModeHandler.GetMode() + randomnum);
        writer.Write(randomnum);
    }
    public static void WriteReplayData(BinaryWriter writer, float RecordRate, bool IsPosFloat)
    {
        writer.Write(RecordRate);
        writer.Write(IsPosFloat);
    }
    public static void WriteGameOptionData(BinaryWriter writer)
    {
        byte[] option = GameOptionsManager.Instance.gameOptionsFactory.ToBytes(GameOptionsManager.Instance.CurrentGameOptions);
        writer.Write(option.Length);
        writer.Write(option);
    }
    public static void WriteCustomOptionData(BinaryWriter writer)
    {
        List<CustomOption> Options = CustomOption.options.FindAll(x => x.GetSelection() != 0);
        writer.Write(Options.Count);
        foreach (CustomOption option in Options)
        {
            writer.Write(option.id);
            writer.Write(option.GetSelection());
        }
    }
    public static void WritePlayerData(BinaryWriter writer, Dictionary<byte, GameData.PlayerOutfit> FirstOutfits, Dictionary<byte, RoleId> FirstRoleIds)
    {
        foreach (GameData.PlayerInfo player in GameData.Instance.AllPlayers)
        {
            writer.Write(player.PlayerId);
            var outfitdata = FirstOutfits.FirstOrDefault(x => x.Key == player.PlayerId);
            var roledata = FirstRoleIds.FirstOrDefault(x => x.Key == player.PlayerId);
            //nullチェック
            if (outfitdata.Equals(default(Dictionary<byte, GameData.PlayerOutfit>)) ||
                roledata.Equals(default(Dictionary<byte, RoleId>)))
            {
                writer.Write(false);
                continue;
            }
            writer.Write(true);
            if (player.Object != null && player.Object.IsBot())
                writer.Write(true);
            else
                writer.Write(false);
            GameData.PlayerOutfit outfit = outfitdata.Value;
            writer.Write(outfit.PlayerName);
            writer.Write(outfit.ColorId);
            writer.Write(outfit.HatId);
            writer.Write(outfit.PetId);
            writer.Write(outfit.VisorId);
            writer.Write(outfit.NamePlateId);
            writer.Write(outfit.SkinId);
            writer.Write(player.Tasks.Count);
            foreach (GameData.TaskInfo task in player.Tasks)
            {
                writer.Write(task.Id);
                writer.Write(task.TypeId);
            }
            writer.Write((byte)roledata.Value);
            writer.Write((byte)player.Role.Role);
        }
    }
}