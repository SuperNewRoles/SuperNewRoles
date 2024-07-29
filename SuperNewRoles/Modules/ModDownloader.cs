using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using BepInEx;
using BepInEx.IL2CPP;
using BepInEx.Unity.IL2CPP;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using Il2CppSystem.Data;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using static SuperNewRoles.Modules.CustomRegulation;

namespace SuperNewRoles.Modules;
public static class ModDownloader
{
    public class ModObject
    {
        public bool DataGetted = false;
        public bool Installed
        {
            get
            {
                if (!_installed.HasValue)
                    _installed = IL2CPPChainloader.Instance.Plugins.TryGetValue(ModGUId, out PluginInfo pinfo);
                return _installed.Value;
            }
        }
        public bool? _installed;
        public PassiveButton DescButton;
        public bool ButtonInited => DescButton != null;
        public string RepoURL = "NoneURL";
        public string ModId = "NoneId";
        public string ModGUId = "NoneId";
        public string VersionTag = "バージョン...どこｯ...!?";
        public List<string> DependencyMod = new();
        public List<string> AddDownloadURLs = new();
        public List<string> DownloadAssetsURL = new();
        public List<string> AssetsName = new();
        public string DescriptionShort;
        public string DescriptionLong;
        public string ModName;
        public List<TextMeshPro> InstallText = new();
        public GameObject DescriptionPopup;
        public ModObject(string ModId, string ModGUId, string RepoURL, string DescriptionShort, string DescriptionLong, List<string> DependencyMod = null, List<string> AddDownloadURLs = null)
        {
            this.ModId = ModId;
            this.RepoURL = RepoURL;
            this.DependencyMod = DependencyMod == null ? new() : DependencyMod;
            this.AddDownloadURLs = AddDownloadURLs == null ? new() : AddDownloadURLs;
            this.DescriptionShort = DescriptionShort;
            this.DescriptionLong = DescriptionLong;
            this.ModGUId = ModGUId;
            this.ModName = RepoURL.Split("/")[1];
        }
    }
    public static List<ModObject> ModObjects;
    public static GameObject Popup;
    public static TransitionOpen DownloadingPopup;
    public static TextMeshPro DownloadingPopupStatusText;
    public static PassiveButton DownloadingPopupCloseButton;
    public static void Load()
    {
    }
    public static ModObject GetModByGUId(string guid)
    {
        return ModObjects.FirstOrDefault(x => x.ModGUId == guid);
    }
    public static void InstallByGuid(string guid)
    {
        ModObject obj = GetModByGUId(guid);
        if (obj == null)
        {
            Logger.Info("MODがnullでした:" + guid);
            return;
        }
        if (obj.Installed)
        {
            Logger.Info("MODがインストール済みでした:" + guid);
            return;
        }
        if (DownloadingPopup == null)
        {
            var template = GameObject.FindObjectOfType<MainMenuManager>().transform.FindChild("StatsPopup").GetComponent<TransitionOpen>(); ;
            DownloadingPopup = GameObject.Instantiate(template, template.transform.parent);
            DownloadingPopup.OnClose = new();
            DownloadingPopup.transform.localPosition = new(0, 0, -30);
            GameObject.Destroy(DownloadingPopup.GetComponent<StatsPopup>());
            DownloadingPopup.transform.FindChild("Background").localScale = new Vector3(0.75f, 0.5f, 0.5f);
            //DownloadingPopup.transform.FindChild("Background").localPosition = new Vector3(1.5f, 1f, 1f);
            DownloadingPopup.transform.FindChild("Background/IgnoreClicks").GetComponent<PassiveButton>().OnClick = new();
            DownloadingPopup.transform.FindChild("CloseButton").gameObject.SetActive(false);
            DownloadingPopupStatusText = DownloadingPopup.transform.FindChild("StatsText_TMP").GetComponent<TextMeshPro>();
            DownloadingPopupStatusText.alignment = TextAlignmentOptions.Center;
            DownloadingPopupStatusText.transform.localPosition = new(0, 0, -2);
            DownloadingPopupStatusText.transform.localScale = Vector3.one * 0.8f;
            DownloadingPopup.transform.FindChild("StatNumsText_TMP").gameObject.SetActive(false);
            DownloadingPopup.transform.FindChild("Title_TMP").GetComponent<TextMeshPro>().text = "導入中...";
            DownloadingPopup.transform.FindChild("Title_TMP").transform.localPosition = new(0, 0.65f, -2);
            DownloadingPopup.transform.FindChild("Title_TMP").transform.localScale = Vector3.one * 1.7f;
            GameObject.Destroy(DownloadingPopup.transform.FindChild("Title_TMP").GetComponent<TextTranslatorTMP>());
            PassiveButton ButtonTemplate = AccountManager.Instance.transform.FindChild("InfoTextBox/Button1").GetComponent<PassiveButton>();

            var descbtn = GameObject.Instantiate(ButtonTemplate, DownloadingPopup.transform);
            descbtn.transform.localPosition = new(0, -0.75f, 0);
            descbtn.transform.localScale = Vector3.one * 0.8f;
            descbtn.OnClick = new();
            descbtn.OnClick.AddListener((UnityAction)(() =>
            {
                DownloadingPopup.gameObject.SetActive(false);
            }));
            GameObject.Destroy(descbtn.GetComponentInChildren<TextTranslatorTMP>());
            descbtn.GetComponentInChildren<TextMeshPro>().text = "閉じる";
            descbtn.gameObject.SetActive(false);
            DownloadingPopupCloseButton = descbtn;
        }
        DownloadingPopupStatusText.text = "ファイルをダウンロード中...";
        DownloadingPopup.gameObject.SetActive(true);
        AmongUsClient.Instance.StartCoroutine(InstallMod(obj).WrapToIl2Cpp());
    }
    public static IEnumerator InstallMod(ModObject obj)
    {
        Logger.Info("インストール開始");
        foreach (string downloadurl in obj.DownloadAssetsURL)
        {
            string[] splited = downloadurl.Split("/");
            DownloadingPopupStatusText.text = "ダウンロード中：" + splited[splited.Length - 1];
            yield return null;
            UnityWebRequest request = UnityWebRequest.Get(downloadurl);
            yield return request.SendWebRequest();
            if (request.responseCode != (long)HttpStatusCode.OK || request.downloadHandler == null)
            {
                Logger.Info("reponseがおかしい:" + request.responseCode.ToString());
                DownloadingPopupStatusText.text = splited[splited.Length - 1] + "のダウンロードが失敗しました。";
                yield return null;
                continue;
            }
            string pluginfolder = Path.GetDirectoryName(Application.dataPath) + @"\BepInEx\plugins\";
            string ziptempfolder = Path.GetDirectoryName(Application.dataPath) + @"\SuperNewRoles\ModDownloader\";
            if (File.Exists(pluginfolder + splited[splited.Length - 1])) // Clear old file in case it wasnt;
                continue;

            //File.Move(fullname, fullname + ".old"); // rename current executable to old

            if (downloadurl.EndsWith(".zip"))
            {
                DownloadingPopupStatusText.text = splited[splited.Length - 1] + "を保存中";
                yield return null;
                Directory.CreateDirectory(ziptempfolder + @"\SuperNewRoles\CustomHatsChache\");
                var fileStream = File.Create(ziptempfolder + splited[splited.Length - 1]);
                foreach (byte result in request.downloadHandler.GetData())
                {
                    // probably want to have proper name here
                    fileStream.WriteByte(result);
                }
                fileStream.Close();

                DownloadingPopupStatusText.text = splited[splited.Length - 1] + "を解析中";
                yield return null;
                List<string> plugins = new();
                ZipArchive zip = ZipFile.OpenRead(ziptempfolder + splited[splited.Length - 1]);
                foreach (ZipArchiveEntry entry in zip.Entries)
                {
                    if (entry.FullName.Contains("plugins/") && entry.FullName.EndsWith(".dll"))
                        plugins.Add(entry.FullName);
                }
                foreach (string pluginname in plugins)
                {
                    string[] splitedplname = pluginname.Split("/");
                    DownloadingPopupStatusText.text = splitedplname[splitedplname.Length - 1] + "をインストール中";
                    yield return null;
                    ZipArchiveEntry entry = zip.GetEntry(pluginname);
                    entry.ExtractToFile(pluginfolder + splitedplname[splitedplname.Length - 1], true);
                }
                zip.Dispose();
                DownloadingPopupStatusText.text = splited[splited.Length - 1] + "を保存中";
                yield return null;
                File.Delete(ziptempfolder + splited[splited.Length - 1]);
            }
            else
            {
                var fileStream = File.Create(pluginfolder + splited[splited.Length - 1]);
                DownloadingPopupStatusText.text = splited[splited.Length - 1] + "をインストール中";
                yield return null;
                foreach (byte result in request.downloadHandler.GetData())
                {
                    // probably want to have proper name here
                    fileStream.WriteByte(result);
                }
                fileStream.Close();
            }
            obj._installed = true;
        }
        DownloadingPopupStatusText.text = "完了！\n再起動すると適用されます。";
        DownloadingPopupCloseButton.gameObject.SetActive(true);
        foreach (TextMeshPro tmp in obj.InstallText)
        {
            if (tmp != null)
            {
                tmp.text = "インストール済み";
            }
        }
    }
    public static void OnPopupOpen(MainMenuManager __instance)
    {
        if (Popup != null)
        {
            static void pasonclick(MainMenuManager __instance)
            {
                Transform TextTemplate = Popup.transform.FindChild("StatsText_TMP(Clone)");
                PassiveButton ButtonTemplate = AccountManager.Instance.transform.FindChild("InfoTextBox/Button1").GetComponent<PassiveButton>();
                int index = -1;
                foreach (ModObject modobj in ModObjects)
                {
                    index++;
                    if (modobj.ButtonInited) continue;
                    //DescriptionButton
                    var descbtn = GameObject.Instantiate(ButtonTemplate, TextTemplate.parent);
                    descbtn.transform.localPosition = new(1.77f, 1.265f - (index * 0.835f), -2);
                    descbtn.transform.localScale = Vector3.one * 0.6f;
                    descbtn.OnClick = new();
                    ModObject mobj = modobj;
                    descbtn.OnClick.AddListener((UnityAction)(() =>
                    {
                        if (mobj.DescriptionPopup == null)
                        {
                            var template = __instance.transform.FindChild("StatsPopup");
                            var obj = GameObject.Instantiate(template, Popup.transform.parent).gameObject;
                            mobj.DescriptionPopup = obj;
                            obj.transform.localPosition = new(0, 0, -20);
                            obj.transform.FindChild("Background").localScale = new(1, 0.85f, 1);
                            GameObject.Destroy(obj.GetComponent<StatsPopup>());
                            obj.transform.localScale = Vector3.one * 0.5f;
                            var devtitletext = obj.transform.FindChild("Title_TMP");
                            GameObject.Destroy(devtitletext.GetComponent<TextTranslatorTMP>());
                            devtitletext.GetComponent<TextMeshPro>().text = mobj.ModName;
                            devtitletext.GetComponent<TextMeshPro>().alignment = TextAlignmentOptions.Center;
                            devtitletext.localPosition = new Vector3(0, 1.9f, -2);
                            devtitletext.localScale = new Vector3(1.5f, 1.5f, 1f);

                            var depetitletext = obj.transform.FindChild("StatNumsText_TMP");
                            GameObject.Destroy(depetitletext.GetComponent<TextTranslatorTMP>());
                            depetitletext.GetComponent<TextMeshPro>().text = "前提MOD(自動でインストールされます)";
                            depetitletext.GetComponent<TextMeshPro>().alignment = TextAlignmentOptions.Center;
                            depetitletext.localPosition = new Vector3(0, -0.75f, -2);
                            depetitletext.localScale = Vector3.one * 2.5f;

                            var desctext = GameObject.Instantiate(depetitletext, depetitletext.parent);
                            desctext.GetComponent<TextMeshPro>().text = mobj.DescriptionLong;
                            desctext.GetComponent<TextMeshPro>().alignment = TextAlignmentOptions.Center;
                            desctext.localPosition = new Vector3(0, 0.5f, -2);
                            desctext.localScale = Vector3.one * 2f;

                            var depetext = GameObject.Instantiate(depetitletext, depetitletext.parent);
                            depetext.GetComponent<TextMeshPro>().text = mobj.DependencyMod.Count <= 0 ? "なし" : string.Join('、', mobj.DependencyMod);
                            depetext.GetComponent<TextMeshPro>().alignment = TextAlignmentOptions.Center;
                            depetext.localPosition = new Vector3(0, -1.1f, -2);
                            depetext.localScale = Vector3.one * 1.4f;

                            var guidtext = obj.transform.FindChild("StatsText_TMP");
                            guidtext.GetComponent<TextMeshPro>().text = mobj.ModGUId;
                            guidtext.GetComponent<TextMeshPro>().alignment = TextAlignmentOptions.Center;
                            guidtext.localPosition = new Vector3(0, 1.6f, -2);
                            guidtext.localScale = Vector3.one * 0.9f;

                            var installButton = GameObject.Instantiate(ButtonTemplate, obj.transform);
                            GameObject.Destroy(installButton.GetComponentInChildren<TextTranslatorTMP>());
                            installButton.GetComponentInChildren<TextMeshPro>().text = mobj.Installed ? "インストール済み" : "ダウンロード";
                            mobj.InstallText.Add(installButton.GetComponentInChildren<TextMeshPro>());
                            installButton.transform.localPosition = new(0, -1.7f, 0);
                            installButton.transform.localScale = Vector3.one * 0.85f;
                            installButton.OnClick = new();
                            installButton.OnClick.AddListener((UnityAction)(() =>
                            {
                                InstallByGuid(mobj.ModGUId);
                            }));
                        }
                        mobj.DescriptionPopup.gameObject.SetActive(true);
                    }));
                    modobj.DescButton = descbtn;
                    GameObject.Destroy(descbtn.GetComponentInChildren<TextTranslatorTMP>());
                    descbtn.GetComponentInChildren<TextMeshPro>().text = "説明";
                    //DownloadButton
                    var dlbtn = GameObject.Instantiate(ButtonTemplate, TextTemplate.parent);
                    dlbtn.transform.localPosition = new(3f, 1.265f - (index * 0.835f), -2);
                    dlbtn.transform.localScale = Vector3.one * 0.6f;
                    dlbtn.OnClick = new();
                    dlbtn.OnClick.AddListener((UnityAction)(() =>
                    {
                        InstallByGuid(mobj.ModGUId);
                    }));
                    GameObject.Destroy(dlbtn.GetComponentInChildren<TextTranslatorTMP>());
                    dlbtn.GetComponentInChildren<TextMeshPro>().text = mobj.Installed ? "インストール済み" : "ダウンロード";
                    mobj.InstallText.Add(dlbtn.GetComponentInChildren<TextMeshPro>());
                }
            }
            pasonclick(__instance);
            Popup.SetActive(true);
        }
    }
    public static IEnumerator DownloadModData(MainMenuManager __instance)
    {
        var datarequest = UnityWebRequest.Get("https://raw.githubusercontent.com/SuperNewRoles/SuperNewRolesData/main/ModDownloadData.json");
        yield return datarequest.SendWebRequest();
        if (datarequest.isNetworkError || datarequest.isHttpError)
        {
            Logger.Info("CANT!!!");
            yield break;
        }
        var datajson = JObject.Parse(datarequest.downloadHandler.text);
        ModObjects = new();

        for (var regulation = datajson["Mods"].First; regulation != null; regulation = regulation.Next)
        {
            List<string> DependencyMod = new();
            for (var regulation1 = regulation["DependencyMod"].First; regulation1 != null; regulation1 = regulation1.Next)
            {
                DependencyMod.Add(regulation1.ToString());
            }
            List<string> AddDownloadURLs = new();
            for (var regulation1 = regulation["AddDownloadURLs"].First; regulation1 != null; regulation1 = regulation1.Next)
            {
                AddDownloadURLs.Add(regulation1.ToString());
            }
            ModObject dataobj = new(regulation["ModId"].ToString(),
                                    regulation["ModGUId"].ToString(),
                                    regulation["RepoURL"].ToString(),
                                    regulation["DescriptionShort"].ToString(),
                                    regulation["DescriptionLong"].ToString(),
                                    DependencyMod,
                                    AddDownloadURLs);
            ModObjects.Add(dataobj);
        }
        foreach (ModObject obj in ModObjects)
        {
            if (obj.DataGetted) continue;
            var request = UnityWebRequest.Get("https://api.github.com/repos/" + obj.RepoURL + "/releases");
            yield return request.SendWebRequest();
            if (request.isNetworkError || request.isHttpError)
            {
                Logger.Info("むりやった:" + obj.RepoURL);
                continue;
            }
            string downloadtext = request.downloadHandler.text[1..];
            StringBuilder downloadtextnew = new();
            int NONECOUNT = 0;
            foreach (char txt in downloadtext)
            {
                downloadtextnew.Append(txt);
                if (txt == '{')
                {
                    NONECOUNT++;
                }
                else if (txt == '}')
                {
                    NONECOUNT--;
                    if (NONECOUNT <= 0)
                    {
                        break;
                    }
                }
            }
            var json = JObject.Parse(downloadtextnew.ToString());
            obj.VersionTag = json["tag_name"]?.ToString();
            List<string> Dlls = new();
            List<string> DllNames = new();
            List<string> Zips = new();
            List<string> ZipNames = new();
            for (var regulation = json["assets"].First; regulation != null; regulation = regulation.Next)
            {
                if (regulation["name"].ToString().EndsWith(".zip"))
                {
                    Zips.Add(regulation["browser_download_url"].ToString());
                    ZipNames.Add(regulation["name"].ToString());
                }
                else if (regulation["name"].ToString().EndsWith(".dll"))
                {
                    Dlls.Add(regulation["browser_download_url"].ToString());
                    DllNames.Add(regulation["name"].ToString());
                }
                else
                {
                    Logger.Info("ナニコレ:" + regulation["name"].ToString());
                }
            }
            if (Zips.Count <= 0)
            {
                obj.DownloadAssetsURL = Dlls;
            }
            else
            {
                obj.DownloadAssetsURL = Zips;
            }
            obj.DataGetted = true;
        }
        if (__instance != null)
        {
            var template = __instance.transform.FindChild("StatsPopup");
            var obj = GameObject.Instantiate(template, template.transform.parent).gameObject;
            Popup = obj;
            GameObject.Destroy(obj.GetComponent<StatsPopup>());
            var devtitletext = obj.transform.FindChild("Title_TMP");
            GameObject.Destroy(devtitletext.GetComponent<TextTranslatorTMP>());
            devtitletext.GetComponent<TextMeshPro>().text = "ModDownloader";
            devtitletext.localPosition = new Vector3(0, 2.351f, -2);
            devtitletext.localScale = new Vector3(1.5f, 1.5f, 1f);

            Transform TextTemplate = obj.transform.FindChild("StatsText_TMP");
            PassiveButton ButtonTemplate = AccountManager.Instance.transform.FindChild("InfoTextBox/Button1").GetComponent<PassiveButton>();
            int index = 0;
            foreach (ModObject modobj in ModObjects)
            {
                if (!modobj.DataGetted) continue;
                var text = GameObject.Instantiate(TextTemplate, TextTemplate.parent);
                text.localPosition = new Vector3(0, -2.8f - (index * 0.8f), -2f);
                text.localScale = new Vector3(1.75f, 1.75f, 1f);
                text.GetComponent<TextMeshPro>().text = modobj.ModName;
                //Description

                text = GameObject.Instantiate(TextTemplate, TextTemplate.parent);
                text.localPosition = new Vector3(-1, -1.95f - (index * 0.775f), -2f);
                text.localScale = new Vector3(1.25f, 1.25f, 1.25f);
                text.GetComponent<TextMeshPro>().text = modobj.DescriptionShort;
                index++;
            }
            GameObject.Destroy(TextTemplate.gameObject);
            GameObject.Destroy(obj.transform.FindChild("StatNumsText_TMP").gameObject);

            obj.transform.FindChild("Background").localScale = new Vector3(1.5f, 1f, 1f);
            obj.transform.FindChild("CloseButton").localPosition = new Vector3(-3.75f, 2.65f, 0);
        }
    }
}