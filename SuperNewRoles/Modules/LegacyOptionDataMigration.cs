using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        if (!file.Exists)
        {
            string configtext = File.ReadAllText(SuperNewRolesPlugin.Instance.Config.ConfigFilePath);
            for (int i = 0; i < CustomOptionHolder.presets.Length; i++)
            {
                //もうそのプリセットが存在していたらスキップ
                if (new FileInfo(OptionSaver.PresetFileNameBase+i.ToString()+"."+OptionSaver.Extension).Exists)
                    continue;
                //プリセットがなかったらスキップ
                if (!configtext.Contains($"[Preset{i}]"))
                    continue;
                Logger.Info("StartProcess:"+i.ToString());
                Dictionary<ushort, byte> SaveValues = new();
                foreach (CustomOption opt in CustomOption.options)
                {
                    int selection = SuperNewRolesPlugin.Instance.Config.Bind($"Preset{i}", opt.id.ToString(), opt.defaultSelection).Value;
                    if (selection != opt.defaultSelection)
                    {
                        SaveValues.Add((ushort)opt.id, (byte)selection);
                    }
                }
                if (SaveValues.Count > 0)
                {
                    BinaryWriter writer = new(new FileStream(OptionSaver.PresetFileNameBase + i + "." + OptionSaver.Extension, FileMode.OpenOrCreate, FileAccess.Write));
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
