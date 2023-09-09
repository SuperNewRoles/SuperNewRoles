using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx.Configuration;
using Il2CppSystem.Net;
using LibCpp2IL.Elf;
using UnityEngine;
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
            bool IsFirstPresetUpdated = false;
            for (int i = 0; i < CustomOptionHolder.presets.Length; i++)
            {
                //もうそのプリセットが存在していたらスキップ
                if (new FileInfo(OptionSaver.PresetFileNameBase+i.ToString()+"."+OptionSaver.Extension).Exists)
                    continue;
                //プリセットがなかったらスキップ
                if (!configtext.Contains($"[Preset{i}]"))
                    continue;
                Dictionary<ushort, byte> SaveValues = new();
                string presettext = $"Preset{i}";
                foreach (CustomOption opt in CustomOption.options)
                {
                    int selection = SuperNewRolesPlugin.Instance.Config.Bind(presettext, opt.id.ToString(), opt.defaultSelection).Value;
                    if (selection != opt.defaultSelection)
                    {
                        SaveValues.Add((ushort)opt.id, (byte)selection);
                    }
                }
                if (SaveValues.Count > 0)
                {
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
                    Logger.Info("Called:"+i.ToString());
                    if (i == 0)
                        IsFirstPresetUpdated = true;
                }
            }
            OptionSaver.WriteOptionData();
            OptionSaver.ReadAndSetOption();
            if (IsFirstPresetUpdated)
            {
                foreach (CustomOption opt in CustomOption.options)
                {
                    opt.selection = Mathf.Clamp(CustomOption.CurrentValues.TryGetValue((ushort)opt.id, out byte valueselection) ? valueselection : opt.defaultSelection, 0, opt.selections.Length - 1);
                }
                DeleteOptionConfig();
            }
        }
    }
    public static void DeleteOptionConfig()
    {
        Logger.Info("Called");
        StringBuilder deletedtext = new();
        StringBuilder deletetext = new();
        bool IsDelete = false;
        string section = string.Empty;
        string[] array = File.ReadAllLines(SuperNewRolesPlugin.Instance.Config.ConfigFilePath);
        for (int i = 0; i < array.Length; i++)
        {
            string text = array[i].Trim();
            if (text.StartsWith("#"))
            {
                if (IsDelete)
                    deletetext.AppendLine(array[i]);
                else
                    deletedtext.AppendLine(array[i]);
                continue;
            }

            if (text.StartsWith("[") && text.EndsWith("]"))
            {
                IsDelete = false;
                section = text.Substring(1, text.Length - 2);
                if (section.StartsWith("Preset") && section.Length is 7 or 8)
                {
                    IsDelete = true;
                    deletetext.AppendLine(array[i]);
                }
                else
                    deletedtext.AppendLine(array[i]);
                continue;
            }

            string[] array2 = text.Split(new char[1] { '=' }, 2);
            if (array2.Length == 2 && IsDelete)
            {
                deletetext.AppendLine(array[i]);
            }
            else
            {
                deletedtext.AppendLine(array[i]);
            }
        }
        FileInfo olddata = new(SuperNewRolesPlugin.Instance.Config.ConfigFilePath + ".oldoption");
        if (!olddata.Exists) olddata.Create().Dispose();
        File.WriteAllText(olddata.FullName, deletetext.ToString());
        File.WriteAllText(SuperNewRolesPlugin.Instance.Config.ConfigFilePath, deletedtext.ToString());
    }
}
