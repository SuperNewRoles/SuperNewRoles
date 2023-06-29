using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using TMPro;
using UnityEngine;

namespace SuperNewRoles.Replay
{
    public static class ReplayFileSelector
    {
        static int CompareLastWriteTime(FileInfo fileX, FileInfo fileY)
        {
            return DateTime.Compare(fileX.LastWriteTimeUtc, fileY.LastWriteTimeUtc);
        }
        [HarmonyPatch(typeof(MatchMaker), nameof(MatchMaker.Start))]
        public class MatchMakerStartPatch
        {
            public static List<JoinGameButton> Buttons;
            public static List<ReplayData> Replays;
            public static List<string> FileNames;
            public static void Postfix(MatchMaker __instance)
            {
                if (!ReplayManager.IsReplaySelector) return;
                ReplayManager.IsReplaySelector = false;
                Buttons = new();
                Replays = new();
                FileNames = new();
                HostLocalGameButton hostlocalgamebutton = GameObject.FindObjectOfType<HostLocalGameButton>();
                hostlocalgamebutton.GetComponent<SpriteRenderer>().enabled = false;
                hostlocalgamebutton.transform.FindChild("menuHostBanner").GetComponent<SpriteRenderer>().enabled = false;
                hostlocalgamebutton.transform.FindChild("CreateHnSGameButton").gameObject.SetActive(false);
                hostlocalgamebutton.transform.FindChild("CreateGameButton").gameObject.SetActive(false);
                hostlocalgamebutton.transform.FindChild("CreateText").gameObject.SetActive(false);
                TextMeshPro titletext = hostlocalgamebutton.transform.FindChild("Text_TMP").GetComponent<TextMeshPro>();
                GameObject.Destroy(titletext.GetComponent<TextTranslatorTMP>());
                titletext.text = ModTranslation.GetString("ReplayName");
                titletext.alignment = TextAlignmentOptions.Center;
                titletext.transform.localPosition = new(0, 0.7f, -1);
                titletext.transform.localScale = Vector3.one * 0.9f;
                GameDiscovery gamediscovery = GameObject.FindObjectOfType<GameDiscovery>();
                gamediscovery.transform.FindChild("Text_TMP").gameObject.SetActive(false);
                gamediscovery.transform.FindChild("Background").transform.localPosition = new(0, 0.38f, 0);
                gamediscovery.transform.FindChild("Background").transform.localScale = new(1.4f, 1.65f, 1.55f);
                gamediscovery.transform.FindChild("Mask").transform.localScale = Vector3.one * 100;
                prefab = gamediscovery.ButtonPrefab;
                prefabparent = gamediscovery.transform;
                string text = "";
                string filePath = Path.GetDirectoryName(Application.dataPath) + @"\SuperNewRoles\Replay\";
                DirectoryInfo d = new(filePath);
                FileInfo[] FileInfos = d.GetFiles();
                Array.Sort(FileInfos, CompareLastWriteTime);
                foreach (FileInfo info in FileInfos)
                {
                    FileNames.Add(info.Name);
                }
                Logger.Info(FileNames.Count.ToString(),"FILECOUNTEREEEEEE");
                UpdatePage(0);
            }
            public static JoinGameButton prefab;
            public static Transform prefabparent;
            public const int MaxPageCount = 4;
            public static void CreatePageButton(int page)
            {
                for (int i = page * MaxPageCount; (i < FileNames.Count && (i - (page * MaxPageCount)) < MaxPageCount); i++)
                {
                    Logger.Info((i - (page * 5)).ToString());
                    JoinGameButton button = GameObject.Instantiate(prefab, prefabparent);
                    Buttons.Add(button);
                    button.transform.localPosition = new(0, 2f - ((i - (page * MaxPageCount)) * 1.1f), -1);
                    button.transform.localScale = new(1.6f, 2, 1.8f);
                    Logger.Info(FileNames[i]);
                    (ReplayData replay, bool IsSuc) = ReplayReader.ReadReplayDataSelector(FileNames[i]);
                    string[] filenames = FileNames[i].Split("/");
                    string[] filesplited = filenames[filenames.Length - 1].Split(".");
                    string filename = string.Join('.', filesplited[..(filesplited.Length - 1)]);
                    button.gameNameText.text = filename;
                    button.gameNameText.transform.localScale = new(1, 0.8f, 1);
                    button.gameNameText.transform.localPosition = new(0, 0.05f, 0);
                    button.gameNameText.alignment = TextAlignmentOptions.Left;
                    button.gameObject.SetActive(false);
                    Replays.Add(replay);
                }
            }
            public static void UpdatePage(int page)
            {
                if (Buttons.Count < (page * MaxPageCount) + 1)
                {
                    CreatePageButton(page);
                }
                for (int i = page * MaxPageCount; (i < FileNames.Count && (i - (page * MaxPageCount)) < MaxPageCount); i++)
                {
                    Buttons[i].gameObject.SetActive(true);
                }
            }
        }
    }
}
