using HarmonyLib;
using Newtonsoft.Json.Linq;
using SuperNewRoles.CustomCosmetics;
using SuperNewRoles.Patch;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Twitch;
using UnityEngine;
using UnityEngine.UI;
namespace SuperNewRoles.Patches
{
    [HarmonyPatch]
    public static class CredentialsPatch
    {

        public static string baseCredentials = $@"<size=130%><color=#ffa500>Super</color><color=#ff0000>New</color><color=#00ff00>Roles</color></size> v{SuperNewRolesPlugin.Version.ToString()}";

        private static Task<bool> kari;
        public static string contributorsCredentials = "<size=80%>GitHub Contributors: Alex2911, amsyarasyiq, gendelo3</size>";

        [HarmonyPatch(typeof(VersionShower), nameof(VersionShower.Start))]
        private static class VersionShowerPatch
        {
            static void Prefix(VersionShower __instance)
            {
                //CustomPlate.UnlockedNamePlatesPatch.Postfix(HatManager.Instance);
            }
            static void Postfix(VersionShower __instance)
            {

                var amongUsLogo = GameObject.Find("bannerLogo_AmongUs");
                if (amongUsLogo == null) return;
                var credentials = UnityEngine.Object.Instantiate<TMPro.TextMeshPro>(__instance.text);
                credentials.transform.position = new Vector3(0, 0f, 0);
                credentials.SetText(ModTranslation.getString("creditsMain"));
                credentials.alignment = TMPro.TextAlignmentOptions.Center;
                credentials.fontSize *= 0.9f;
                AutoUpdate.checkForUpdate(credentials);

                var version = UnityEngine.Object.Instantiate<TMPro.TextMeshPro>(credentials);
                version.transform.position = new Vector3(0, -0.35f, 0);
                version.SetText(string.Format(ModTranslation.getString("creditsVersion"), SuperNewRolesPlugin.Version.ToString()));

                credentials.transform.SetParent(amongUsLogo.transform);
                version.transform.SetParent(amongUsLogo.transform);



            }
        }

        [HarmonyPatch(typeof(PingTracker), nameof(PingTracker.Update))]
        private static class PingTrackerPatch
        {
            static void Postfix(PingTracker __instance)
            {
                __instance.text.alignment = TMPro.TextAlignmentOptions.TopRight;
                if (AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started)
                {

                    __instance.text.text = $"{baseCredentials}\n{__instance.text.text}";
                    try
                    {
                        if (DebugMode.IsDebugMode())
                        {
                            __instance.text.text += "\nデバッグモードが有効です";
                        }
                        if (!Mode.ModeHandler.isMode(Mode.ModeId.Default))
                        {
                            __instance.text.text += "\n" + ModTranslation.getString("SettingMode") + ":" + Mode.ModeHandler.ThisModeSetting.getString();
                        }
                    }
                    catch { }
                    if (CachedPlayer.LocalPlayer.Data.IsDead)
                    {
                        __instance.transform.localPosition = new Vector3(3.45f, __instance.transform.localPosition.y, __instance.transform.localPosition.z);
                    }
                    else
                    {
                        __instance.transform.localPosition = new Vector3(4.2f, __instance.transform.localPosition.y, __instance.transform.localPosition.z);
                    }
                }
                else
                {
                    __instance.text.text = $"{baseCredentials}\n{ModTranslation.getString("creditsFull")}\n{__instance.text.text}";
                    __instance.transform.localPosition = new Vector3(3.5f, __instance.transform.localPosition.y, __instance.transform.localPosition.z);

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
            private static PingTracker instance;
            static void Postfix(PingTracker __instance)
            {
                DownLoadCustomhat.Load();
                CustomCosmetics.DownLoadClass.Load();
                CustomCosmetics.DownLoadClassVisor.Load();
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
                loadSprites();
                renderer.sprite = ModHelpers.loadSpriteFromResources("SuperNewRoles.Resources.banner.png", 150f);

                instance = __instance;
                loadSprites();
                renderer.sprite = HorseModeOption.enableHorseMode ? horseBannerSprite : bannerSprite;

                if (File.Exists(Assembly.GetExecutingAssembly().Location.Replace("SuperNewRoles.dll", "Submerged.dll"))) return;
                SuperNewRolesPlugin.Logger.LogInfo("通過ぁぁぁ！:"+ Assembly.GetExecutingAssembly().Location.Replace("SuperNewRoles.dll", "Submerged.dll"));
                //サブマージド追加ボタン

                var template = GameObject.Find("ExitGameButton");
                if (template == null) return;

                var button = UnityEngine.Object.Instantiate(template, null);
                button.transform.localPosition = new Vector3(button.transform.localPosition.x, button.transform.localPosition.y + 0.6f, button.transform.localPosition.z);

                PassiveButton passiveButton = button.GetComponent<PassiveButton>();
                passiveButton.OnClick = new Button.ButtonClickedEvent();
                passiveButton.OnClick.AddListener((UnityEngine.Events.UnityAction)onClick);

                var text = button.transform.GetChild(0).GetComponent<TMPro.TMP_Text>();
                __instance.StartCoroutine(Effects.Lerp(0.1f, new System.Action<float>((p) => {
                    text.SetText(ModTranslation.getString("サブマージドを適用する"));
                })));

                TwitchManager man = DestroyableSingleton<TwitchManager>.Instance;
                popup = UnityEngine.Object.Instantiate<GenericPopup>(man.TwitchPopup);
                popup.TextAreaTMP.fontSize *= 0.7f;
                popup.TextAreaTMP.enableAutoSizing = false;

                void onClick()
                {
                    SuperNewRolesPlugin.Logger.LogInfo("ダウンロード！");
                    showPopup(ModTranslation.getString("ダウンロード中です。\nサブマージドのファイルは大きいため、時間がかかります。"));
                    DownloadSubmarged();
                    button.SetActive(false);
                }
            }

            public static void loadSprites()
            {
                if (bannerSprite == null) bannerSprite = ModHelpers.loadSpriteFromResources("SuperNewRoles.Resources.banner.png", 150f);
                if (horseBannerSprite == null) horseBannerSprite = ModHelpers.loadSpriteFromResources("SuperNewRoles.Resources.SuperHorseRoles.png", 150f);
            }

            public static void updateSprite()
            {
                loadSprites();
                if (renderer != null)
                {
                    float fadeDuration = 1f;
                    instance.StartCoroutine(Effects.Lerp(fadeDuration, new Action<float>((p) =>
                    {
                        renderer.color = new Color(1, 1, 1, 1 - p);
                        if (p == 1)
                        {
                            renderer.sprite = HorseModeOption.enableHorseMode ? horseBannerSprite : bannerSprite;
                            instance.StartCoroutine(Effects.Lerp(fadeDuration, new Action<float>((p) =>
                            {
                                renderer.color = new Color(1, 1, 1, p);
                            })));
                        }
                    })));
                }
            }

            private static Task DownloadTask = null;
            public static async Task<bool> DownloadSubmarged()
            {
                try
                {
                    HttpClient httpa = new HttpClient();
                    httpa.DefaultRequestHeaders.Add("User-Agent", "SuperNewRoles Downloader");
                    var responsea = await httpa.GetAsync(new System.Uri("https://api.github.com/repos/submergedAmongUs/submerged/releases/latest"), HttpCompletionOption.ResponseContentRead);
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
                    HttpClient http = new HttpClient();
                    http.DefaultRequestHeaders.Add("User-Agent", "SuperNewRoles Downloader");
                    var response = await http.GetAsync(new System.Uri(url), HttpCompletionOption.ResponseContentRead);
                    if (response.StatusCode != HttpStatusCode.OK || response.Content == null)
                    {
                        System.Console.WriteLine("Server returned no data: " + response.StatusCode.ToString());
                        return false;
                    }
                    string code = Assembly.GetExecutingAssembly().Location.Replace("SuperNewRoles.dll", "Submerged.dll");

                    using (var responseStream = await response.Content.ReadAsStreamAsync())
                    {
                        using (var fileStream = File.Create(code))
                        { // probably want to have proper name here
                            responseStream.CopyTo(fileStream);
                        }
                    }
                    showPopup(ModTranslation.getString("ダウンロード完了！\n再起動してください！"));
                    return true;
                }
                catch (System.Exception ex)
                {
                    SuperNewRolesPlugin.Instance.Log.LogError(ex.ToString());
                    System.Console.WriteLine(ex);
                }
                showPopup(ModTranslation.getString("ダウンロード失敗！"));
                return false;
            }
            private static void showPopup(string message)
            {
                setPopupText(message);
                popup.gameObject.SetActive(true);
            }

            public static void setPopupText(string message)
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
