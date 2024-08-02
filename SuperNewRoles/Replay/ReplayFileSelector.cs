using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using InnerNet;
using SuperNewRoles.Roles;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace SuperNewRoles.Replay
{
    public static class ReplayFileSelector
    {
        [HarmonyPatch(typeof(NameTextBehaviour), nameof(NameTextBehaviour.Start))]
        public class NameTextBehaviourStartPatch
        {
            public static void Postfix(NameTextBehaviour __instance)
            {
                if (__instance.name == "ReplayFileSelectorChangeFileName")
                {
                    __instance.nameSource.SetText("");
                }
            }
        }
        [HarmonyPatch(typeof(EndGameNavigation), nameof(EndGameNavigation.ShowProgression))]
        public class ShowProgressionPatch
        {
            public static void Postfix()
            {
                // 未完成のため封印
                return;

                if (ReplayManager.IsReplayMode)
                    return;
                SignInScreen screen = FastDestroyableSingleton<AccountManager>.Instance.signInScreen;
                SignInScreen popup = GameObject.Instantiate(screen);
                SceneManager.MoveGameObjectToScene(popup.gameObject, SceneManager.GetActiveScene());
                popup.transform.FindChild("Fill").gameObject.SetActive(false);
                popup.GetComponent<TransitionOpen>().enabled = false;
                popup.transform.localScale = Vector3.one * 0.4f;
                popup.transform.localPosition = new(3.5f, 2, -5);
                TextMeshPro TitleTMP = popup.transform.FindChild("TitleText_TMP").GetComponent<TextMeshPro>();
                GameObject.Destroy(TitleTMP.GetComponent<TextTranslatorTMP>());
                TitleTMP.text = ModTranslation.GetString("ReplayReplaySaved");
                TitleTMP.transform.localScale = Vector3.one * 1.5f;
                TextMeshPro InfoTMP = popup.transform.FindChild("InfoText_TMP").GetComponent<TextMeshPro>();
                GameObject.Destroy(InfoTMP.GetComponent<PlatformTextTranslationTMP>());
                InfoTMP.text = Path.GetFileNameWithoutExtension(ReplayManager.LastSavedName);
                InfoTMP.transform.localScale = Vector3.one * 1.9f;
                InfoTMP.transform.localPosition = new(0, 0, -0.1f);
                InfoTMP.alignment = TextAlignmentOptions.Center;
                popup.signInButton.transform.localPosition = new(0, -1, -0.1f);
                popup.signInButton.transform.localScale = Vector3.one * 1.7f;
                popup.signInButton.OnClick = new();
                popup.signInButton.OnClick.AddListener((UnityAction)(() =>
                {
                    SignInScreen popup = GameObject.Instantiate(screen);
                    popup.gameObject.SetActive(false);
                    SceneManager.MoveGameObjectToScene(popup.gameObject, SceneManager.GetActiveScene());
                    TextMeshPro TitleTMP = popup.transform.FindChild("TitleText_TMP").GetComponent<TextMeshPro>();
                    GameObject.Destroy(TitleTMP.GetComponent<TextTranslatorTMP>());
                    TitleTMP.text = ModTranslation.GetString("ReplayChangeName");
                    TitleTMP.transform.localScale = Vector3.one * 1.5f;
                    TextMeshPro InfoTMP = popup.transform.FindChild("InfoText_TMP").GetComponent<TextMeshPro>();
                    GameObject.Destroy(InfoTMP.GetComponent<PlatformTextTranslationTMP>());
                    InfoTMP.text = ModTranslation.GetString("ReplayFileName");
                    InfoTMP.transform.localScale = Vector3.one * 1.3f;
                    InfoTMP.transform.localPosition = new(-2.3f, 0.5f, -0.1f);
                    InfoTMP.alignment = TextAlignmentOptions.Center;
                    popup.gameObject.SetActive(true);
                    popup.signInButton.transform.localPosition = new(1, -1.15f, -0.1f);
                    popup.signInButton.transform.localScale = Vector3.one * 1.2f;
                    popup.signInButton.OnClick = new();
                    popup.signInButton.OnClick.AddListener((UnityAction)(() =>
                    {
                    }));
                    NameTextBehaviour ntb = GameObject.Instantiate(FastDestroyableSingleton<AccountManager>.Instance.accountTab.editNameScreen.nameText, popup.transform);
                    ntb.name = "ReplayFileSelectorChangeFileName";
                    ntb.transform.FindChild("Text_TMP/Pipe").localScale = new(1, 1.3f, 1);
                    ntb.transform.localPosition = new(0, -0.1f, 0);
                    var textBox = ntb.GetComponent<TextBoxTMP>();
                    textBox.AllowPaste = true;
                    textBox.allowAllCharacters = true;
                    textBox.AllowEmail = true;
                    textBox.AllowSymbols = true;
                    textBox.outputText.fontSize = 4f;

                    TextMeshPro ButtonText = popup.signInButton.transform.FindChild("Text_TMP").GetComponent<TextMeshPro>();
                    GameObject.Destroy(ButtonText.GetComponent<TextTranslatorTMP>());
                    ButtonText.text = ModTranslation.GetString("ReplayChangeName");

                    PassiveButton cancelbtn = GameObject.Instantiate(popup.signInButton, popup.signInButton.transform);
                }));
                TextMeshPro ButtonText = popup.signInButton.transform.FindChild("Text_TMP").GetComponent<TextMeshPro>();
                GameObject.Destroy(ButtonText.GetComponent<TextTranslatorTMP>());
                ButtonText.text = ModTranslation.GetString("ReplayChangeName");
                popup.gameObject.SetActive(true);
                Logger.Info("AAAA");
            }
        }
        static int CompareLastWriteTime(FileInfo fileX, FileInfo fileY)
        {
            return DateTime.Compare(fileY.LastWriteTimeUtc, fileX.LastWriteTimeUtc);
        }
        [HarmonyPatch(typeof(GameDiscovery), nameof(GameDiscovery.Update))]
        public class GameDiscoveryUpdatePatch
        {
            public static void Postfix(GameDiscovery __instance)
            {
                if (Input.GetKeyDown(KeyCode.RightArrow))
                {
                    MatchMakerStartPatch.RightButton.OnClick.Invoke();
                }
                else if (Input.GetKeyDown(KeyCode.LeftArrow))
                {
                    MatchMakerStartPatch.LeftButton.OnClick.Invoke();
                }
            }
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
                VersionShower vs = GameObject.FindObjectOfType<VersionShower>();
                GameObject.Destroy(vs.GetComponent<AspectPosition>());
                vs.transform.localPosition -= new Vector3(0, 0.065f);
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
                PassiveButton Button = GameObject.Find("HelpButton").GetComponent<PassiveButton>();
                Button = GameObject.Instantiate(Button);
                GameObject.Destroy(Button.GetComponent<BoxCollider2D>());
                GameObject.Destroy(Button.GetComponent<AspectPosition>());
                Button.GetComponent<SpriteRenderer>().sprite = MeetingUpdatePatch.Meeting_AreaTabChange;
                PolygonCollider2D collider = Button.gameObject.AddComponent<PolygonCollider2D>();
                Button.Colliders = new[] { collider };
                Button.OnClick = new();
                Button.OnClick.AddListener((UnityAction)(() =>
                {
                    if (!((CurrentPage + 1) < 0 || Math.Ceiling(FileNames.Count * 1.0 / MaxPageCount) <= (CurrentPage + 1)))
                    {
                        CurrentPage++;
                        UpdatePage(CurrentPage);
                    }
                }));
                Button.transform.localPosition = new(4.6f, 0, 0);
                Button.transform.localScale = Vector3.one * 0.11f;
                RightButton = Button;
                Button = GameObject.Instantiate(Button);
                Button.GetComponent<SpriteRenderer>().sprite = MeetingUpdatePatch.Meeting_AreaTabChange;
                Button.OnClick = new();
                Button.OnClick.AddListener((UnityAction)(() =>
                {

                    if (!((CurrentPage - 1) < 0 || Math.Ceiling(FileNames.Count * 1.0 / MaxPageCount) <= (CurrentPage - 1)))
                    {
                        CurrentPage--;
                        UpdatePage(CurrentPage);
                    }
                }));
                Button.transform.localPosition = new(-4.6f, 0, 0);
                Button.transform.localScale = new(-0.11f, 0.11f, 0.11f);
                LeftButton = Button;
                CurrentPage = 0;
                UpdatePage(0);
            }
            public static int CurrentPage;
            public static JoinGameButton prefab;
            public static Transform prefabparent;
            public const int MaxPageCount = 4;
            [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.ExitGame))]
            class EndGameNavigationExitPatch
            {
                public static void Postfix(AmongUsClient __instance)
                {
                    if (ReplayManager.IsReplayMode)
                        ReplayManager.IsReplaySelector = true;
                    ReplayManager.IsReplayMode = false;
                }
            }
            [HarmonyPatch(typeof(EndGameNavigation), nameof(EndGameNavigation.NextGame))]
            class EndGameNavigationNextGamePatch
            {
                public static void Postfix(AmongUsClient __instance)
                {
                    if (ReplayManager.IsReplayMode)
                    {
                        (ReplayData replay, bool IsSuc) = ReplayReader.ReadReplayDataFirst(ReplayManager.CurrentReplay.FilePath);
                        ReplayManager.IsReplayMode = true;
                    }
                }
            }
            public static void CreatePageButton(int page)
            {
                for (int i = page * MaxPageCount; (i < FileNames.Count && (i - (page * MaxPageCount)) < MaxPageCount); i++)
                {
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
                    button.gameNameText.transform.localScale = new(1, 0.75f, 1);
                    button.gameNameText.transform.localPosition = new(0, 0.05f, 0);
                    button.gameNameText.alignment = TextAlignmentOptions.Left;
                    //日時
                    TextMeshPro tmp = GameObject.Instantiate(button.gameNameText, button.transform);
                    tmp.text = replay.RecordTime.ToString("yyyy/MM/dd/ hh-mm-ss");
                    tmp.transform.localScale *= 0.8f;
                    tmp.transform.localPosition = new(0.125f, -0.13f, 0);
                    //モード
                    tmp = GameObject.Instantiate(button.gameNameText, button.transform);
                    tmp.text = ModTranslation.GetString(replay.CustomMode.ToString() + "ModeName");
                    tmp.transform.localScale *= 0.5f;
                    tmp.transform.localPosition = new(2.5f, 0.1f, 0);
                    //人数
                    tmp = GameObject.Instantiate(button.gameNameText, button.transform);
                    tmp.text = (replay.AllPlayersCount - replay.AllBotsCount).ToString() + "人";
                    tmp.transform.localScale *= 0.5f;
                    tmp.transform.localPosition = new(2.5f, 0f, 0);
                    //バージョン
                    tmp = GameObject.Instantiate(button.gameNameText, button.transform);
                    if (replay.RecordVersion != null)
                        tmp.text = "v" + replay.RecordVersion.ToString();
                    else
                        tmp.text = "v0.0.0.0";
                    tmp.transform.localScale *= 0.5f;
                    tmp.transform.localPosition = new(2.5f, -0.1f, 0);
                    //
                    button.gameObject.SetActive(false);
                    button.GetComponent<BoxCollider2D>().size = new(5, 0.5f);
                    PassiveButton pbtn = button.GetComponent<PassiveButton>();
                    pbtn.OnClick = new();
                    string currentpath = FileNames[i];
                    pbtn.OnClick.AddListener((UnityAction)(() =>
                    {
                        OnClick(replay, currentpath);
                    }));
                    Replays.Add(replay);
                    replay.binaryReader.Close();
                }
            }
            public static void OnClick(ReplayData replay, string path)
            {
                if (!replay.CheckSum)
                {
                    OpenAlert(ModTranslation.GetString("ReplayErrorFileDataBroken"));
                }
                else if (!replay.ReplayDataMod.StartsWith(SuperNewRolesPlugin.ThisPluginModName + "-"))
                {
                    OpenCheckAlert(string.Format(ModTranslation.GetString("ReplayErrorDataModNoneThisMod"), replay.ReplayDataMod), replay.GameMode == AmongUs.GameOptions.GameModes.Normal, path);
                }
                else if (SuperNewRolesPlugin.ThisVersion.Major != replay.RecordVersion.Major ||
                         SuperNewRolesPlugin.ThisVersion.Minor != replay.RecordVersion.Minor ||
                         SuperNewRolesPlugin.ThisVersion.Build != replay.RecordVersion.Build)
                {
                    OpenCheckAlert(string.Format(ModTranslation.GetString("ReplayErrorDiffVersion"), replay.RecordVersion.ToString(), SuperNewRolesPlugin.ThisVersion.ToString()), replay.GameMode == AmongUs.GameOptions.GameModes.Normal, path);
                }
                else
                {
                    OpenCheckAlert("", replay.GameMode == AmongUs.GameOptions.GameModes.Normal, path);
                }
            }
            public static (GameObject, TextMeshPro) OpenAlert(string alerttext)
            {
                if (AlertTmp == null)
                {
                    GameObject HelpMenu = GameObject.Find("HelpMenu");
                    if (HelpMenu == null)
                    {
                        GameObject.Find("HelpButton").GetComponent<PassiveButton>().OnClick.Invoke();
                        HelpMenu = GameObject.Find("HelpMenu");
                    }
                    GameObject alert = GameObject.Instantiate(HelpMenu);
                    HelpMenu.gameObject.SetActive(false);
                    alert.transform.FindChild("Discord-Logo-Color").gameObject.SetActive(false);
                    AlertTmp = alert.GetComponentInChildren<TextMeshPro>();
                    AlertTmp.alignment = TextAlignmentOptions.Center;
                    GameObject.Destroy(AlertTmp.GetComponent<TextTranslatorTMP>());
                    alert.GetComponent<SpriteRenderer>().size = new(4.5f, 2);
                    alert.transform.FindChild("CloseButton").localPosition = new(-2.2f, 0.95f, -1);
                }
                AlertTmp.text = alerttext;
                AlertTmp.transform.localPosition = new(0.05f, 2.1f, -1);
                AlertTmp.transform.parent.gameObject.SetActive(false);
                AlertTmp.transform.parent.gameObject.SetActive(true);
                if (okbtn != null)
                    okbtn.gameObject.SetActive(false);
                return (AlertTmp.transform.parent.gameObject, AlertTmp);
            }
            public static TextMeshPro AlertTmp;
            public static void OpenCheckAlert(string alerttext, bool IsNormal, string FileName)
            {
                (GameObject alert, TextMeshPro tmp) = OpenAlert(alerttext);
                tmp.transform.localPosition = new(0.05f, 2.25f, -1);
                if (tmp.text != "")
                    tmp.text += "\n";
                tmp.text += ModTranslation.GetString("ReplayCheckStart");
                HostLocalGameButton game = GameObject.FindObjectOfType<HostLocalGameButton>();
                if (okbtn == null)
                {
                    okbtn = GameObject.Instantiate(game.transform.FindChild("CreateGameButton").GetComponent<PassiveButton>(), alert.transform);
                    okbtn.OnClick = new();
                    okbtn.OnClick.AddListener((UnityAction)(() =>
                    {
                        CreateRoom(FileName, IsNormal);
                    }));
                }
                okbtn.gameObject.SetActive(true);
                okbtn.transform.localPosition = new(0, -0.6f, -14);
                GameObject.Destroy(okbtn.GetComponentInChildren<TextTranslatorTMP>());
                okbtn.GetComponentInChildren<TextMeshPro>().text = ModTranslation.GetString("ReplayPlay");
            }
            public static PassiveButton okbtn;
            public static PassiveButton RightButton;
            public static PassiveButton LeftButton;
            public static void CreateRoom(string FileName, bool IsNormal)
            {
                (ReplayData replay, bool IsSuc) = ReplayReader.ReadReplayDataFirst(FileName);
                ReplayManager.IsReplayMode = true;
                Logger.Info((replay == null).ToString() + ":" + (ReplayManager.CurrentReplay == null).ToString() + ":" + (ReplayManager.CurrentReplay.binaryReader == null).ToString());
                HostLocalGameButton game = GameObject.FindObjectOfType<HostLocalGameButton>();
                if (IsNormal)
                    game.transform.FindChild("CreateGameButton").GetComponent<PassiveButton>().OnClick.Invoke();
                else
                    game.transform.FindChild("CreateHnSGameButton").GetComponent<PassiveButton>().OnClick.Invoke();
            }
            public static void UpdatePage(int page)
            {
                if (Buttons.Count < (page * MaxPageCount) + 1)
                {
                    CreatePageButton(page);
                }
                foreach (JoinGameButton jgb in Buttons)
                {
                    jgb.gameObject.SetActive(false);
                }
                for (int i = page * MaxPageCount; (i < Buttons.Count && (i - (page * MaxPageCount)) < MaxPageCount); i++)
                {
                    Buttons[i].gameObject.SetActive(true);
                }
                LeftButton.gameObject.SetActive(CurrentPage > 0);
                RightButton.gameObject.SetActive(Math.Ceiling(FileNames.Count * 1.0 / MaxPageCount) > (CurrentPage + 1));
            }
        }
    }
}