using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using SuperNewRoles.Patches;
using TMPro;
using UnityEngine;

namespace SuperNewRoles.Achievement
{
    class AchievementManagerSNR
    {
        public static List<AchievementData> AllAchievementData = new();
        public static List<AchievementData> CompletedAchievement = new();
        public static List<AchievementType> WaitCompleteData = new();
        public static Dictionary<byte, int> PlayerData = new();
        public static string currentData;
        public static AchievementData SelectedData {
            get
            {
                return _selectedData;
            }
            set
            {
                _selectedData = value;
                ConfigRoles.AchievementSelectedId.Value = _selectedData.Id;
            }
        }
        public static GameObject AchievementButtonAsset {
            get {
                if (_achievementButtonAsset == null)
                {
                    var resourceAudioAssetBundleStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("SuperNewRoles.Resources.AchievementButton");
                    var assetBundleBundle = AssetBundle.LoadFromMemory(resourceAudioAssetBundleStream.ReadFully());
                    _achievementButtonAsset = assetBundleBundle.LoadAsset<GameObject>("AchievementButton.prefab").DontUnload();
                }
                return _achievementButtonAsset;
            }
        }
        private static GameObject _achievementButtonAsset;
        public static AchievementData _selectedData;
        public static Transform EndGamePopup;
        public static DateTime GameStartTime;
        public static void OnEndGame(EndGameManager __instance, WinCondition condition) {
            if (WaitCompleteData.Count <= 0) return;
            __instance.transform.FindChild("BackgroundLayer").localScale = new(10.6667f, 10, 1);
            EndGamePopup = GameObject.Instantiate(AchievementButtonAsset).transform;
            EndGamePopup.FindChild("AchievementDescription").gameObject.SetActive(true);
            EndGamePopup.transform.localPosition = new(2.6f, 2.3f, -13.5f);
            if (condition == WinCondition.HAISON)
            {
                EndGamePopup.FindChild("AchievementDescription").GetComponent<TextMeshPro>().text = $"廃村だったため、\n進捗を獲得できませんでした。";
                WaitCompleteData = new();
                return;
            }
            else if ((DateTime.UtcNow - GameStartTime).Seconds < 90f)
            {
                Logger.Info((DateTime.UtcNow - GameStartTime).Seconds.ToString(),"Seconds");
                EndGamePopup.FindChild("AchievementDescription").GetComponent<TextMeshPro>().text = $"ゲーム時間が短かったため、\n進捗を獲得できませんでした。";
                WaitCompleteData = new();
                return;
            }
            if (WaitCompleteData.Count == 1)
                EndGamePopup.FindChild("AchievementDescription").GetComponent<TextMeshPro>().text = $"進捗「{GetAchievementData(WaitCompleteData[0]).Name}」\nを達成しました";
            else
                EndGamePopup.FindChild("AchievementDescription").GetComponent<TextMeshPro>().text = $"進捗「{GetAchievementData(WaitCompleteData[0]).Name}」\nとその他{WaitCompleteData.Count - 1}の進捗を達成しました。";
            EndGamePopup.FindChild("CompleteMark").gameObject.SetActive(true);
            CompleteAchievement(WaitCompleteData.ToArray());
            WaitCompleteData = new();
        }
        public static AchievementData GetAchievementData(AchievementType type)
        {
            return AllAchievementData.FirstOrDefault(x => x.TypeData == type);
        }
        public static void CompleteAchievement(AchievementType type, bool EndGameComplete = true)
        {
            AchievementData data = GetAchievementData(type);
            if (data is null) return;
            if (data.Complete) return;
            if (EndGameComplete)
            {
                WaitCompleteData.Add(type);
                return;
            }
            currentData += $"{data.Id}\n";

            string AppDataLocalPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            using (StreamWriter sw = new($"{AppDataLocalPath}/SuperNewRoles/AchievementData.txt", false))
            {
                sw.Write(currentData);
            }
            CompletedAchievement.Add(data);
            data.Complete = true;
        }
        public static void CompleteAchievement(params AchievementType[] types)
        {
            string AppDataLocalPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            foreach (AchievementType type in types)
            {
                AchievementData data = GetAchievementData(type);
                if (data is null) return;
                if (data.Complete) return;
                currentData += $"{data.Id}\n";
                CompletedAchievement.Add(data);
                data.Complete = true;
            }
            using (StreamWriter sw = new($"{AppDataLocalPath}/SuperNewRoles/AchievementData.txt", false))
            {
                sw.Write(currentData);
            }
        }
        public static List<string> GetCompletedTitle()
        {
            List<string> data = new();
            foreach (AchievementData adata in CompletedAchievement)
            {
                data.Add(adata.Title);
            }
            return data;
        }
        public static AchievementData GetPlayerData(PlayerControl player)
        {
            if (player == null) return null;
            return PlayerData.ContainsKey(player.PlayerId) ? GetAchievementData((AchievementType)PlayerData[player.PlayerId]) : null;
        }
    }
}
