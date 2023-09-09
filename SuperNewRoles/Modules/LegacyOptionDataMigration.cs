using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibCpp2IL.Elf;
using static Rewired.Controller;

namespace SuperNewRoles.Modules;
public static class LegacyOptionDataMigration
{
    /// <summary>
    /// 設定の移行処理
    /// </summary>
    public static void Load()
    {
        FileInfo file = new(OptionSaver.OptionSaverFileName);
        Logger.Info("--------------------StartMigration---------------------");
        Logger.Info("Exists:"+file.Exists);
        if (!file.Exists)
        {
            string configtext = File.ReadAllText(SuperNewRolesPlugin.Instance.Config.ConfigFilePath);
            Logger.Info("-------ConfigText-----");
            //Logger.Info(configtext);
            Logger.Info("-------ConfigEnd!-----");
            for (int i = 0; i < CustomOptionHolder.presets.Length; i++)
            {
                //もうそのプリセットが存在していたらスキップ
                if (new FileInfo(OptionSaver.PresetFileNameBase+i.ToString()+"."+OptionSaver.Extension).Exists)
                    continue;
                Logger.Info(i.ToString());
                Logger.Info($"[Preset{i}]");
                Logger.Info(configtext.Contains($"[Preset{i}]").ToString());
                Logger.Info("--------");
                //プリセットがなかったらスキップ
                if (!configtext.Contains($"[Preset{i}]"))
                    continue;
                Logger.Info("StartProcess:"+i.ToString());
                Dictionary<ushort, byte> SaveValues = new();
                try
                {
                    foreach (var option in CustomOption.options)
                    {
                        if (SuperNewRolesPlugin.Instance.Config.TryGetEntry<int>(new("Preset"+i.ToString(),option.id.ToString()), out var entry))
                        {
                            if (entry.Value != option.defaultSelection)
                            {
                                Logger.Info("NODEFAULT!!!!");
                                SaveValues.Add((ushort)option.id, (byte)entry.Value);
                            }
                        }
                    }
                }
                catch (FormatException except)
                {
                    Logger.Info("Parseで多分エラーでたから、コレまでのやつを使うで！");
                    Logger.Info("FormatException:"+except.ToString());
                }
                if (SaveValues.Count > 0)
                {
                    Logger.Info("WRITE!!!");
                    BinaryWriter writer = new(new FileStream(OptionSaver.PresetFileNameBase + i.ToString() + "." + OptionSaver.Extension, FileMode.OpenOrCreate, FileAccess.Write));
                    writer.Write(OptionSaver.Version);
                    OptionSaver.WriteCheckSum(writer);
                    writer.Write(SaveValues.Count);
                    foreach (var data in SaveValues)
                    {
                        writer.Write(data.Key);
                        writer.Write(data.Value);
                    }
                    writer.Close();
                }
            }
            OptionSaver.WriteOptionData();
            OptionSaver.ReadAndSetOption();
        }
    }
}
