using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;
using BepInEx.IL2CPP.Utils;
using HarmonyLib;
using Newtonsoft.Json.Linq;
using SuperNewRoles.CustomCosmetics;
using SuperNewRoles.CustomOption;
using SuperNewRoles.Patch;
using TMPro;
using Twitch;
using UnityEngine;
using UnityEngine.UI;

namespace SuperNewRoles.Patches
{
    [HarmonyPatch]
    public static class CredentialsPatch
    {
        public static string baseCredentials = $@"<size=130%><color=#ffa500>Super</color><color=#ff0000>New</color><color=#00ff00>Roles</color></size> v{SuperNewRolesPlugin.Version}";

        [HarmonyPatch(typeof(VersionShower), nameof(VersionShower.Start))]
        private static class VersionShowerPatch
        {
            static void Prefix()
            {
                //CustomPlate.UnlockedNamePlatesPatch.Postfix(HatManager.Instance);
            }
            public static string modColor = "#a6d289";
            static void Postfix(VersionShower __instance)
            {

                var amongUsLogo = GameObject.Find("bannerLogo_AmongUs");
                if (amongUsLogo == null) return;
                var credentials = UnityEngine.Object.Instantiate<TMPro.TextMeshPro>(__instance.text);
                credentials.transform.position = new Vector3(0, 0f, 0);
                //ブランチ名表示
                string credentialsText = "";
                if (ThisAssembly.Git.Branch != "master")//masterビルド以外の時
                {
                    //色+ブランチ名+コミット番号
                    credentialsText = $"\r\n<color={modColor}>{ThisAssembly.Git.Branch}({ThisAssembly.Git.Commit})</color>";
                }
                credentialsText += ModTranslation.GetString("creditsMain");
                credentials.SetText(credentialsText);

                credentials.alignment = TMPro.TextAlignmentOptions.Center;
                credentials.fontSize *= 0.9f;
                AutoUpdate.checkForUpdate(credentials);

                var version = UnityEngine.Object.Instantiate(credentials);
                version.transform.position = new Vector3(0, -0.35f, 0);
                version.SetText(string.Format(ModTranslation.GetString("creditsVersion"), SuperNewRolesPlugin.Version.ToString()));

                credentials.transform.SetParent(amongUsLogo.transform);
                version.transform.SetParent(amongUsLogo.transform);
            }
        }

        [HarmonyPatch(typeof(PingTracker), nameof(PingTracker.Update))]
        private static class PingTrackerPatch
        {
            static void Postfix(PingTracker __instance)
            {
                __instance.text.alignment = TextAlignmentOptions.TopRight;
                if (AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started)
                {
                    __instance.text.text = $"{baseCredentials}\n{__instance.text.text}";
                    try
                    {
                        if (DebugMode.IsDebugMode())
                        {
                            __instance.text.text += "\n" + ModTranslation.GetString("DebugModeOn");
                        }
                        if (!Mode.ModeHandler.IsMode(Mode.ModeId.Default))
                        {
                            __instance.text.text += "\n" + ModTranslation.GetString("SettingMode") + ":" + Mode.ModeHandler.ThisModeSetting.GetString();
                        }
                    }
                    catch { }
                    //ブランチ名表示
                    if (ThisAssembly.Git.Branch != "master")//masterビルド以外の時
                    {
                        //改行+Branch名+コミット番号
                        __instance.text.text += "\n" + $"{ThisAssembly.Git.Branch}({ThisAssembly.Git.Commit})";
                    }
                    __instance.transform.localPosition = CachedPlayer.LocalPlayer.Data.IsDead
                        ? new Vector3(3.45f, __instance.transform.localPosition.y, __instance.transform.localPosition.z)
                        : new Vector3(4.2f, __instance.transform.localPosition.y, __instance.transform.localPosition.z);
                    __instance.gameObject.GetComponent<AspectPosition>().DistanceFromEdge = new Vector3(1.2f, 0.1f, 0.5f);
                }
                else
                {
                    __instance.text.text = $"{baseCredentials}\n{ModTranslation.GetString("creditsFull")}\n{__instance.text.text}";
                    __instance.transform.localPosition = new Vector3(4f, __instance.transform.localPosition.y, __instance.transform.localPosition.z);
                }
                if (CustomHats.HatManagerPatch.IsLoadingnow)
                {
                    __instance.text.text += $"\n{ModTranslation.GetString("LoadHat")}";
                }
            }
        }
        public static GenericPopup popup;

        [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
        public static class LogoPatch
        {
            public static SpriteRenderer renderer;
            public static Sprite bannerSprite;
            public static Sprite horseBannerSprite;
            static IEnumerator ViewBoosterCoro(MainMenuManager __instance)
            {
                while (true)
                {
                    yield return new WaitForSeconds(1f);
                    if (Downloaded)
                    {
                        if (__instance != null)
                        {
                            ViewBoosterPatch(__instance);
                        }
                        break;
                    }
                }
            }
            public static string SponsersData = "";
            public static string DevsData = "";
            public static string TransData = "";

            public static async Task<HttpStatusCode> FetchBoosters()
            {
                if (!Downloaded)
                {
                    Downloaded = true;
                    HttpClient http = new();
                    http.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue { NoCache = true, OnlyIfCached = false };
                    var response = await http.GetAsync(new Uri("https://raw.githubusercontent.com/ykundesu/SuperNewRoles/master/CreditsData.json"), HttpCompletionOption.ResponseContentRead);
                    try
                    {
                        if (response.StatusCode != HttpStatusCode.OK)
                        {
                            SuperNewRolesPlugin.Logger.LogInfo("NOTOK!!!");
                            return response.StatusCode;
                        };
                        if (response.Content == null)
                        {
                            System.Console.WriteLine("Server returned no data: " + response.StatusCode.ToString());
                            return HttpStatusCode.ExpectationFailed;
                        }
                        string json = await response.Content.ReadAsStringAsync();
                        JToken jobj = JObject.Parse(json);

                        var devs = jobj["Devs"];
                        for (JToken current = devs.First; current != null; current = current.Next)
                        {
                            if (current.HasValues)
                            {
                                DevsData += current["name"]?.ToString() + "\n";
                            }
                        }

                        var Sponsers = jobj["Sponsers"];
                        for (JToken current = Sponsers.First; current != null; current = current.Next)
                        {
                            if (current.HasValues)
                            {
                                SponsersData += current["name"]?.ToString() + "\n";
                            }
                        }

                        var Translator = jobj["Translate"];
                        for (JToken current = Translator.First; current != null; current = current.Next)
                        {
                            if (current.HasValues)
                            {
                                TransData += current["name"]?.ToString() + "\n";
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        SuperNewRolesPlugin.Logger.LogError(e);
                    }
                }
                return HttpStatusCode.OK;
            }
            public static GameObject CreditsPopup;
            static void ViewBoosterPatch(MainMenuManager __instance)
            {
                var template = __instance.transform.FindChild("StatsPopup");
                var obj = GameObject.Instantiate(template, template.transform.parent).gameObject;
                CreditsPopup = obj;
                GameObject.Destroy(obj.GetComponent<StatsPopup>());
                var devtitletext = obj.transform.FindChild("StatNumsText_TMP");
                devtitletext.GetComponent<TextMeshPro>().text = ModTranslation.GetString("Developer");
                devtitletext.localPosition = new Vector3(-3.25f, -1.65f, -2f);
                devtitletext.localScale = new Vector3(1.5f, 1.5f, 1f);
                var devtext = obj.transform.FindChild("StatsText_TMP");
                devtext.localPosition = new Vector3(-1f, -1.65f, -2f);
                devtext.localScale = new Vector3(1.25f, 1.25f, 1f);
                devtext.GetComponent<TextMeshPro>().text = DevsData;

                var boostertitletext = GameObject.Instantiate(devtitletext, obj.transform);
                boostertitletext.GetComponent<TextMeshPro>().text = ModTranslation.GetString("Sponsor");
                boostertitletext.localPosition = new Vector3(1.45f, -1.65f, -2f);
                boostertitletext.localScale = new Vector3(1.5f, 1.5f, 1f);

                var boostertext = GameObject.Instantiate(devtext, obj.transform);
                boostertext.localPosition = new Vector3(3f, -1.65f, -2f);
                boostertext.localScale = new Vector3(1.25f, 1.25f, 1f);
                boostertext.GetComponent<TextMeshPro>().text = SponsersData;

                var transtitletext = GameObject.Instantiate(devtitletext, obj.transform);
                transtitletext.GetComponent<TextMeshPro>().text = ModTranslation.GetString("Translator");
                transtitletext.localPosition = new Vector3(0.5f, -4.5f, -2f);
                transtitletext.localScale = new Vector3(1.5f, 1.5f, 1f);

                var transtext = GameObject.Instantiate(devtext, obj.transform);
                transtext.localPosition = new Vector3(3f, -5f, -2f);
                transtext.localScale = new Vector3(1.5f, 1.5f, 1f);
                transtext.GetComponent<TextMeshPro>().text = TransData;

                var textobj = obj.transform.FindChild("Title_TMP");
                GameObject.Destroy(textobj.GetComponent<TextTranslatorTMP>());
                textobj.GetComponent<TextMeshPro>().text = ModTranslation.GetString("DevAndSpnTitle");
                textobj.localScale = new Vector3(1.5f, 1.5f, 1f);
                obj.transform.FindChild("Background").localScale = new Vector3(1.5f, 1f, 1f);
                obj.transform.FindChild("CloseButton").localPosition = new Vector3(-3.75f, 2.65f, 0);
            }
            static bool Downloaded = false;
            public static MainMenuManager instance;
            static IEnumerator ShowAnnouncementPopUp(MainMenuManager __instance)
            {
                while (true)
                {
                    SuperNewRolesPlugin.Logger.LogInfo(AutoUpdate.announcement);
                    if (AutoUpdate.announcement == "None")
                        yield return null;
                    else
                        break;
                }
                var AnnouncementPopup = __instance.transform.FindChild("Announcement").GetComponent<AnnouncementPopUp>();
                if (AnnouncementPopup != null)
                {
                    AnnouncementPopup.Show();
                    AnnouncementPopup.AnnounceTextMeshPro.text = AutoUpdate.announcement;
                }
                ConfigRoles.IsUpdated = false;
            }
            public static void Postfix(MainMenuManager __instance)
            {
                AmongUsClient.Instance.StartCoroutine(CustomRegulation.FetchRegulation());
                if (ConfigRoles.IsUpdated)
                {
                    __instance.StartCoroutine(ShowAnnouncementPopUp(__instance));
                }
                DownLoadCustomhat.Load();
                DownLoadClass.Load();
                DownLoadClassVisor.Load();

                instance = __instance;

                AmongUsClient.Instance.StartCoroutine(ViewBoosterCoro(__instance));

                //ViewBoosterPatch(__instance);

                DestroyableSingleton<ModManager>.Instance.ShowModStamp();

                var amongUsLogo = GameObject.Find("bannerLogo_AmongUs");
                if (amongUsLogo != null)
                {
                    amongUsLogo.transform.localScale *= 0.6f;
                    amongUsLogo.transform.position += Vector3.up * 0.25f;
                }

                var snrLogo = new GameObject("bannerLogo");
                snrLogo.transform.position = Vector3.up;
                renderer = snrLogo.AddComponent<SpriteRenderer>();
                LoadSprites();
                renderer.sprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.banner.png", 150f);

                LoadSprites();
                renderer.sprite = HorseModeOption.enableHorseMode ? horseBannerSprite : bannerSprite;

                if (File.Exists(Assembly.GetExecutingAssembly().Location.Replace("SuperNewRoles.dll", "Submerged.dll"))) return;
                SuperNewRolesPlugin.Logger.LogInfo("[Submerged]Passage ahhhhhh!:" + Assembly.GetExecutingAssembly().Location.Replace("SuperNewRoles.dll", "Submerged.dll"));
                //サブマージド追加ボタン

                var template = GameObject.Find("ExitGameButton");
                if (template == null) return;

                var button = UnityEngine.Object.Instantiate(template, null);
                button.transform.localPosition = new Vector3(button.transform.localPosition.x, button.transform.localPosition.y + 0.6f, button.transform.localPosition.z);

                PassiveButton passiveButton = button.GetComponent<PassiveButton>();
                passiveButton.OnClick = new Button.ButtonClickedEvent();
                passiveButton.OnClick.AddListener((UnityEngine.Events.UnityAction)onClick);

                var text = button.transform.GetChild(0).GetComponent<TMPro.TMP_Text>();
                __instance.StartCoroutine(Effects.Lerp(0.1f, new System.Action<float>((p) =>
                {
                    text.SetText(ModTranslation.GetString("サブマージドを適用する"));
                })));

                TwitchManager man = DestroyableSingleton<TwitchManager>.Instance;
                popup = UnityEngine.Object.Instantiate<GenericPopup>(man.TwitchPopup);
                popup.TextAreaTMP.fontSize *= 0.7f;
                popup.TextAreaTMP.enableAutoSizing = false;

                async void onClick()
                {
                    SuperNewRolesPlugin.Logger.LogInfo("[Submerged]Downloading Submerged!");
                    ShowPopup(ModTranslation.GetString("ダウンロード中です。\nサブマージドのファイルは大きいため、時間がかかります。"));
                    await DownloadSubmarged();
                    button.SetActive(false);
                }
            }

            private static IEnumerator Download()
            {
                throw new NotImplementedException();
            }

            public static void LoadSprites()
            {
                if (bannerSprite == null) bannerSprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.banner.png", 150f);
                if (horseBannerSprite == null) horseBannerSprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.SuperHorseRoles.png", 150f);
            }

            public static void UpdateSprite()
            {
                LoadSprites();
                if (renderer != null)
                {
                    float fadeDuration = 1f;
                    AmongUsClient.Instance.StartCoroutine(Effects.Lerp(fadeDuration, new Action<float>((p) =>
                    {
                        renderer.color = new Color(1, 1, 1, 1 - p);
                        if (p == 1)
                        {
                            renderer.sprite = HorseModeOption.enableHorseMode ? horseBannerSprite : bannerSprite;
                            AmongUsClient.Instance.StartCoroutine(Effects.Lerp(fadeDuration, new Action<float>((p) =>
                            {
                                renderer.color = new Color(1, 1, 1, p);
                            })));
                        }
                    })));
                }
            }

            public static async Task<bool> DownloadSubmarged()
            {
                try
                {
                    HttpClient httpa = new();
                    httpa.DefaultRequestHeaders.Add("User-Agent", "SuperNewRoles Downloader");
                    var responsea = await httpa.GetAsync(new Uri("https://api.github.com/repos/submergedAmongUs/submerged/releases/latest"), HttpCompletionOption.ResponseContentRead);
                    if (responsea.StatusCode != HttpStatusCode.OK || responsea.Content == null)
                    {
                        System.Console.WriteLine("Server returned no data: " + responsea.StatusCode.ToString());
                        return false;
                    }
                    string json = await responsea.Content.ReadAsStringAsync();
                    JObject data = JObject.Parse(json);
                    JToken assets = data["assets"];
                    if (!assets.HasValues)
                        return false;
                    string url = "";
                    for (JToken current = assets.First; current != null; current = current.Next)
                    {
                        string browser_download_url = current["browser_download_url"]?.ToString();
                        if (browser_download_url != null && current["content_type"] != null)
                        {
                            if (current["content_type"].ToString().Equals("application/x-msdownload") &&
                                browser_download_url.EndsWith(".dll"))
                            {
                                url = browser_download_url;
                            }
                        }
                    }
                    HttpClient http = new();
                    http.DefaultRequestHeaders.Add("User-Agent", "SuperNewRoles Downloader");
                    var response = await http.GetAsync(new Uri(url), HttpCompletionOption.ResponseContentRead);
                    if (response.StatusCode != HttpStatusCode.OK || response.Content == null)
                    {
                        System.Console.WriteLine("Server returned no data: " + response.StatusCode.ToString());
                        return false;
                    }
                    string code = Assembly.GetExecutingAssembly().Location.Replace("SuperNewRoles.dll", "Submerged.dll");

                    using (var responseStream = await response.Content.ReadAsStreamAsync())
                    {
                        using var fileStream = File.Create(code);
                        // probably want to have proper name here
                        responseStream.CopyTo(fileStream);
                    }
                    ShowPopup(ModTranslation.GetString("ダウンロード完了！\n再起動してください！"));
                    return true;
                }
                catch (System.Exception ex)
                {
                    SuperNewRolesPlugin.Instance.Log.LogError(ex.ToString());
                    System.Console.WriteLine(ex);
                }
                ShowPopup(ModTranslation.GetString("ダウンロード失敗！"));
                return false;
            }
            private static void ShowPopup(string message)
            {
                SetPopupText(message);
                popup.gameObject.SetActive(true);
            }

            public static void SetPopupText(string message)
            {
                if (popup == null)
                    return;
                if (popup.TextAreaTMP != null)
                {
                    popup.TextAreaTMP.text = message;
                }
            }
        }
    }
}