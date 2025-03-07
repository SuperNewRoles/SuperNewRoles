using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using SuperNewRoles.Modules;
using UnityEngine;

namespace SuperNewRoles.SuperTrophies;

public static class SuperTrophySaver
{
    private static readonly string SaveFilePath = Path.Combine(Path.GetDirectoryName(Application.dataPath), "SuperNewRoles", "SaveData", "SuperTrophyData.dat");
    private static readonly Dictionary<string, TrophySaveData> SavedTrophies = new();

    [Serializable]
    private class TrophySaveData
    {
        public bool Completed { get; set; }
        public long TrophyData { get; set; }
        public TrophyRank TrophyRank { get; set; }
    }

    public static void Initialize()
    {
        LoadData();
    }

    public static void SaveData()
    {
        try
        {
            Logger.Info("トロフィーデータを保存しています...");
            // 現在のトロフィーデータを保存用のディクショナリに更新
            UpdateTrophyDictionary();

            // バイナリシリアライズして保存
            using (FileStream fs = new(SaveFilePath, FileMode.Create))
            {
                BinaryFormatter formatter = new();
                formatter.Serialize(fs, SavedTrophies);
            }
            Logger.Info("トロフィーデータが正常に保存されました");
        }
        catch (Exception ex)
        {
            Logger.Error($"トロフィーデータの保存中にエラーが発生しました: {ex.Message}");
        }
    }

    public static void LoadData()
    {
        try
        {
            if (File.Exists(SaveFilePath))
            {
                Logger.Info("トロフィーデータを読み込んでいます...");
                using (FileStream fs = new FileStream(SaveFilePath, FileMode.Open))
                {
                    BinaryFormatter formatter = new();
                    SavedTrophies.Clear();
                    var loadedData = (Dictionary<string, TrophySaveData>)formatter.Deserialize(fs);

                    foreach (var pair in loadedData)
                    {
                        SavedTrophies[pair.Key] = pair.Value;
                    }
                }
                Logger.Info($"{SavedTrophies.Count}個のトロフィーデータが読み込まれました");

                // トロフィーインスタンスにデータを適用
                ApplyTrophyData();
            }
            else
            {
                Logger.Info("トロフィーデータファイルが見つかりませんでした。新規作成します。");
                SavedTrophies.Clear();
            }
        }
        catch (Exception ex)
        {
            Logger.Error($"トロフィーデータの読み込み中にエラーが発生しました: {ex.Message}");
            SavedTrophies.Clear();
        }
    }

    private static void UpdateTrophyDictionary()
    {
        // 全てのトロフィーインスタンスから保存用データを更新
        if (SuperTrophyManager.trophies != null)
        {
            foreach (var trophy in SuperTrophyManager.trophies)
            {
                var trophyIdString = trophy.TrophyId.ToString();

                if (!SavedTrophies.TryGetValue(trophyIdString, out var data))
                {
                    data = new TrophySaveData();
                    SavedTrophies[trophyIdString] = data;
                }

                data.Completed = trophy.Completed;
                data.TrophyData = trophy.TrophyData;
                data.TrophyRank = trophy.TrophyRank;
            }
        }
    }

    private static void ApplyTrophyData()
    {
        // 保存されたデータをトロフィーインスタンスに適用
        if (SuperTrophyManager.trophies != null)
        {
            foreach (var trophy in SuperTrophyManager.trophies)
            {
                var trophyIdString = trophy.TrophyId.ToString();

                if (SavedTrophies.TryGetValue(trophyIdString, out var data))
                {
                    trophy.Completed = data.Completed;
                    trophy.TrophyData = data.TrophyData;
                }
            }
        }
    }
}