using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperNewRoles.Modules;
public static class OptionSaver
{
    static readonly DirectoryInfo directory = new("./SuperNewRoles/SaveData/");
    public static readonly string OptionSaverFileName = $"{directory.FullName}/Options.{Extension}";
    public const string Extension = "data";
    public static readonly string PresetFileNameBase = $"{directory.FullName}/PresetOptions_";
    public const byte Version = 0;
    public static object FileLocker = new();
    public static void Load()
    {
        if (!directory.Exists)
        {
            directory.Create();
            directory.Attributes |= FileAttributes.Hidden;
        }
        ReadAndSetOption();
    }
    public static void WriteCheckSum(BinaryWriter writer)
    {
        int random = ModHelpers.GetRandomInt(15);
        writer.Write((byte)random);
        writer.Write((byte)(random * random));
    }
    public static (int, bool) ReadLeadAndChecksum(BinaryReader reader)
    {
        try
        {
            byte version = reader.ReadByte();
            int randomint = reader.ReadByte();
            return (version, reader.ReadByte() == (randomint * randomint));
        }
        catch (EndOfStreamException)
        {
            return (-1, false);
        }
    }
    public static void WriteNowOptions()
    {
        if (!AmongUsClient.Instance.AmHost)
            return;
        WriteOptionData();
        WriteNowPreset();
    }
    public static void WriteOptionData()
    {
        lock (FileLocker)
        {
            BinaryWriter writer = new(new FileStream(OptionSaverFileName, FileMode.OpenOrCreate, FileAccess.Write));
            writer.Write(Version);
            WriteCheckSum(writer);
            writer.Write((byte)CustomOption.preset);
            writer.Close();
        }
    }
    public static void WriteNowPreset()
    {
        lock (FileLocker)
        {
            BinaryWriter writer = new(new FileStream(PresetFileNameBase + CustomOption.preset + "." + Extension, FileMode.OpenOrCreate, FileAccess.Write));
            writer.Write(Version);
            WriteCheckSum(writer);
            List<CustomOption> options = CustomOption.options.FindAll(x => x.defaultSelection != x.selection);
            writer.Write(options.Count);
            foreach (CustomOption option in options)
            {
                writer.Write((ushort)option.id);
                writer.Write((byte)option.selection);
            }
            writer.Close();
        }
    }
    public static void ReadAndSetOption()
    {
        (bool Suc,int preset) = LoadSavedOption();
        if (!Suc)
        {
            Logger.Info("プリセットナンバー読み込みでエラーが発生しました。:"+preset.ToString());
            CustomOption.CurrentValues = new();
            return;
        }
        (Suc, int code, Dictionary<ushort, byte> data) = LoadPreset(preset);
        if (!Suc)
        {
            Logger.Info("プリセット読み込みでエラーが発生しました。:" + code.ToString());
            CustomOption.CurrentValues = new();
            return;
        }
        CustomOption.CurrentValues = data;
    }
    public static (bool, int, Dictionary<ushort, byte>) LoadPreset(int num)
    {
        lock (FileLocker)
        {
            BinaryReader reader;
            try
            {
                reader = new(new FileStream(PresetFileNameBase + num.ToString() + "." + Extension, FileMode.Open, FileAccess.Read));
            }
            catch (FileNotFoundException ex)
            {
                //後で処理書く
                return (false, -1, null);
            }
            (int version, bool Checksum) = ReadLeadAndChecksum(reader);
            if (!Checksum)
            {
                Logger.Info("フアイルの データが こわれています！");
                reader.Close();
                return (false, -2, null);
            }
            Dictionary<ushort, byte> Options = new();
            if (Version != version)
            {
                Logger.Info("Optionのバージョンが違います。なう:" + Version.ToString() + "、おるど:" + version.ToString());
                //ここに移行処理
                switch (version)
                {
                    default:
                        Logger.Info("不正なバージョンが入力されました：" + version.ToString());
                        break;
                }
            }
            try
            {
                int optioncount = reader.ReadInt32();
                ushort id;
                byte selection;
                for (int i = 0; i < optioncount; i++)
                {
                    id = reader.ReadUInt16();
                    selection = reader.ReadByte();
                    if (!Options.TryAdd(id, selection))
                    {
                        Logger.Info("追加に失敗：" + id.ToString() + ":" + selection.ToString());
                    }
                }
            }
            catch (EndOfStreamException)
            {
                reader.Close();
                return (false, -3, null);
            }
            reader.Close();
            return (true, 0, Options);
        }
    }
    public static (bool,int) LoadSavedOption()
    {
        lock (FileLocker)
        {
            BinaryReader reader;
            try
            {
                reader = new(new FileStream(OptionSaverFileName, FileMode.Open, FileAccess.Read));
            }
            catch (FileNotFoundException ex)
            {
                //後で処理書く
                return (false, -1);
            }
            (int version, bool Checksum) = ReadLeadAndChecksum(reader);
            if (!Checksum)
            {
                reader.Close();
                Logger.Info("フアイルの データが こわれています！");
                return (false, -2);
            }
            int preset = -4;
            if (Version != version)
            {
                Logger.Info("Optionのバージョンが違います。なう:" + Version.ToString() + "、おるど:" + version.ToString());
                //ここに移行処理
                switch (version)
                {
                    default:
                        Logger.Info("不正なバージョンが入力されました：" + version.ToString());
                        break;
                }
            }
            try
            {
                preset = reader.ReadByte();
            }
            catch (EndOfStreamException)
            {
                reader.Close();
                return (false, -3);
            }
            reader.Close();
            return (true, preset);
        }
    }
}