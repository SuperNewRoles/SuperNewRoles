using System;
using System.Collections;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using HarmonyLib;
using Newtonsoft.Json.Linq;
using SuperNewRoles.CustomCosmetics;
using SuperNewRoles.Mode;
using SuperNewRoles.Replay;
using TMPro;
using UnityEngine;

namespace SuperNewRoles.Patches;

[HarmonyPatch]
public static class CredentialsPatch
{
    public static string baseCredentials => $@"<size=130%>{SuperNewRolesPlugin.ColorModName}</size> v{SuperNewRolesPlugin.ThisVersion}";

    [HarmonyPatch(typeof(VersionShower), nameof(VersionShower.Start))]
    private static class VersionShowerPatch
    {
        public static string modColor = "#a6d289";
        static void Postfix(VersionShower __instance)
        {
            if (GameObject.FindObjectOfType<MainMenuManager>() == null)
                return;
            var credentials = UnityEngine.Object.Instantiate<TMPro.TextMeshPro>(__instance.text);
            credentials.transform.position = new Vector3(2, -0.15f, 0);
            credentials.transform.localScale = Vector3.one * 2;
            //ブランチ名表示
            string credentialsText = "";
            if (SuperNewRolesPlugin.IsBeta)//masterビルド以外の時
            {
                //色+ブランチ名+コミット番号
                credentialsText = $"\r\n<color={modColor}>{ThisAssembly.Git.Branch}({ThisAssembly.Git.Commit})</color>";
            }
            credentialsText += ModTranslation.GetString("creditsMain");
            credentials.SetText(credentialsText);

            credentials.alignment = TMPro.TextAlignmentOptions.Center;
            credentials.fontSize *= 0.9f;
            _ = AutoUpdate.checkForUpdate(credentials);

            var version = UnityEngine.Object.Instantiate(credentials);
            version.transform.position = new Vector3(2, -0.5f, 0);
            version.transform.localScale = Vector3.one * 1.5f;
            version.SetText($"{SuperNewRolesPlugin.ModName} v{SuperNewRolesPlugin.VersionString}");

            //            credentials.transform.SetParent(amongUsLogo.transform);
            //            version.transform.SetParent(amongUsLogo.transform);
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
                    if (ModHelpers.IsDebugMode())
                    {
                        __instance.text.text += "\n" + ModTranslation.GetString("DebugModeOn");
                    }
                    if (!ModeHandler.IsMode(ModeId.Default) || ModeHandler.IsMode(ModeId.HideAndSeek))
                    {
                        __instance.text.text += "\n" + ModTranslation.GetString("SettingMode") + ":" + ModeHandler.GetThisModeIntro();
                    }
                }
                catch { }
                //ブランチ名表示
                if (SuperNewRolesPlugin.IsBeta)//masterビルド以外の時
                {
                    //改行+Branch名+コミット番号
                    __instance.text.text += "\n" + $"{ThisAssembly.Git.Branch}({ThisAssembly.Git.Commit})";
                }
                __instance.transform.localPosition = CachedPlayer.LocalPlayer.Data.IsDead
                    ? new Vector3(3.45f, __instance.transform.localPosition.y, __instance.transform.localPosition.z)
                    : new Vector3(4.2f, __instance.transform.localPosition.y, __instance.transform.localPosition.z);

                float xAspectPosition = !DestroyableSingleton<ChatController>.Instance.chatButton.activeInHierarchy ? 1.2f : 1.79f; // チャットボタンが表示されているなら左にずらす

                __instance.gameObject.GetComponent<AspectPosition>().DistanceFromEdge = new Vector3(xAspectPosition, 0.1f, 0.5f);
            }
            else
            {
                __instance.text.text = $"{baseCredentials}\n{ModTranslation.GetString("creditsFull")}\n{__instance.text.text}";
                __instance.transform.localPosition = new Vector3(4f, __instance.transform.localPosition.y, __instance.transform.localPosition.z);
            }
            /*
            if (CustomHats.HatManagerPatch.IsLoadingnow)
            {
                __instance.text.text += $"\n{ModTranslation.GetString("LoadHat")}";
            }
            */
        }
    }
    public static GenericPopup popup;

    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
    public static class LogoPatch
    {
        public static SpriteRenderer renderer;
        public static Sprite bannerSprite;
        // ☆ス☆ー☆パ☆ー☆な☆感☆じ☆の
        // ☆バ☆ナ☆ー☆ス☆プ☆ラ☆イ☆ト
        public static Sprite SuperNakanzinoBannerSprite;
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
        public static string DevsData = "";
        public static string SupporterData = "";
        public static string TransData = "";

        public static async Task<HttpStatusCode> FetchBoosters()
        {
            if (!Downloaded)
            {
                Downloaded = true;
                HttpClient http = new();
                http.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue { NoCache = true, OnlyIfCached = false };
                var response = await http.GetAsync(new Uri($"https://raw.githubusercontent.com/{SuperNewRolesPlugin.ModUrl}/master/CreditsData.json"), HttpCompletionOption.ResponseContentRead);
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
                            DevsData += $"{current["number"]?.ToString()} : {current["name"]?.ToString()}\n";
                        }
                    }

                    var Sponsers = jobj["Supporter"];
                    for (JToken current = Sponsers.First; current != null; current = current.Next)
                    {
                        if (current.HasValues)
                        {
                            SupporterData += current["name"]?.ToString() + "\n";
                        }
                    }

                    var Translator = jobj["Translate"];
                    for (JToken current = Translator.First; current != null; current = current.Next)
                    {
                        if (current.HasValues)
                        {
                            TransData += $"{current["name"]?.ToString()}<size=40%>({current["language"]?.ToString()})</size>\n";
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

            var devTitletext = obj.transform.FindChild("StatNumsText_TMP");
            devTitletext.GetComponent<TextMeshPro>().text = ModTranslation.GetString("Developer");
            devTitletext.localPosition = new Vector3(-3.25f, -1.65f, -2f);
            devTitletext.localScale = new Vector3(1.5f, 1.5f, 1f);

            var devText = obj.transform.FindChild("StatsText_TMP");
            devText.localPosition = new Vector3(-1f, -1.65f, -2f);
            devText.localScale = new Vector3(1.25f, 1.25f, 1f);
            devText.GetComponent<TextMeshPro>().text = DevsData;

            var supporterTitletext = UnityEngine.Object.Instantiate(devTitletext, obj.transform);
            supporterTitletext.GetComponent<TextMeshPro>().text = $"<align={"left"}>{ModTranslation.GetString("Supporter")}</align>";
            supporterTitletext.localPosition = new Vector3(1.45f, -1.65f, -2f);
            supporterTitletext.localScale = new Vector3(1.5f, 1.5f, 1f);

            var supporterText = UnityEngine.Object.Instantiate(devText, obj.transform);
            supporterText.localPosition = new Vector3(3f, -1.65f, -2f);
            supporterText.localScale = new Vector3(1.25f, 1.25f, 1f);
            supporterText.GetComponent<TextMeshPro>().text = SupporterData;

            var transTitletext = UnityEngine.Object.Instantiate(devTitletext, obj.transform);
            transTitletext.GetComponent<TextMeshPro>().text = $"<align={"left"}>{ModTranslation.GetString("Translator")}</align>";
            transTitletext.localPosition = new Vector3(1.45f, -4.5f, -2f);
            transTitletext.localScale = new Vector3(1.5f, 1.5f, 1f);

            var transText = UnityEngine.Object.Instantiate(devText, obj.transform);
            transText.localPosition = new Vector3(3f, -4.5f, -2f);
            transText.localScale = new Vector3(1.25f, 1.25f, 1f);
            transText.GetComponent<TextMeshPro>().text = TransData;

            var textobj = obj.transform.FindChild("Title_TMP");
            UnityEngine.Object.Destroy(textobj.GetComponent<TextTranslatorTMP>());
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
                AnnouncementPopup.AnnouncementBodyText.text = AutoUpdate.announcement;
            }
            ConfigRoles.IsUpdated = false;
        }
        public static void Postfix(MainMenuManager __instance)
        {
            AprilFoolsManager.SetRandomModMode();

            __instance.gameModeButtons.GetComponent<AspectPosition>().DistanceFromEdge = new(0, 0, -5);
            if (AprilFoolsManager.IsApril(2024))
            {
                __instance.accountButtons.GetComponent<AspectPosition>().DistanceFromEdge = new(0, 0, -5);
            }

            __instance.StartCoroutine(Blacklist.FetchBlacklist().WrapToIl2Cpp());
            AmongUsClient.Instance.StartCoroutine(CustomRegulation.FetchRegulation().WrapToIl2Cpp());
            if (ConfigRoles.IsUpdated)
            {
                __instance.StartCoroutine(ShowAnnouncementPopUp(__instance).WrapToIl2Cpp());
            }

            DownLoadCustomCosmetics.CosmeticsLoad();

            instance = __instance;

            AmongUsClient.Instance.StartCoroutine(ModDownloader.DownloadModData(__instance).WrapToIl2Cpp());
            AmongUsClient.Instance.StartCoroutine(ViewBoosterCoro(__instance).WrapToIl2Cpp());

            //ViewBoosterPatch(__instance);

            FastDestroyableSingleton<ModManager>.Instance.ShowModStamp();

            var amongUsLogo = GameObject.Find("bannerLogo_AmongUs");
            if (amongUsLogo != null)
            {
                amongUsLogo.transform.localScale *= 0.6f;
                amongUsLogo.transform.position += Vector3.up * 0.25f;
            }

            var snrLogo = new GameObject("bannerLogo");
            snrLogo.transform.position = new(2, AprilFoolsManager.getCurrentBannerYPos(), AprilFoolsManager.IsApril(2024) ? -6 : 0);
            snrLogo.transform.localScale = Vector3.one * 0.95f;
            //snrLogo.transform.localScale = Vector3.one;
            renderer = snrLogo.AddComponent<SpriteRenderer>();

            LoadSprites();
            renderer.sprite = bannerRendSprite;
            __instance.howToPlayButton.transform.localPosition = new(-1.925f, -1.75f, 0);
            PassiveButton FreePlayButton = __instance.howToPlayButton.transform.parent.FindChild("FreePlayButton").GetComponent<PassiveButton>();
            FreePlayButton.transform.localPosition = new(-0.05f, -1.75f, 0);
            ReplayManager.CreateReplayButton(__instance, FreePlayButton);
        }

        public static void LoadSprites()
        {
            if (bannerSprite == null) bannerSprite = AssetManager.GetAsset<Sprite>("banner.png");
            if (SuperNakanzinoBannerSprite == null) SuperNakanzinoBannerSprite = AssetManager.GetAsset<Sprite>("banner_April.png");
            if (horseBannerSprite == null) horseBannerSprite = AssetManager.GetAsset<Sprite>("SuperHorseRoles.png");
        }

        public static Sprite bannerRendSprite
        {
            get
            {
                //if (HorseModeOption.enableHorseMode) return horseBannerSprite;
                Sprite aprilBannerSprite = AprilFoolsManager.getCurrentBanner();
                //if (AprilFoolsManager.IsApril(2023)
                //    return SuperNakanzinoBannerSprite;
                return aprilBannerSprite != null ? aprilBannerSprite : bannerSprite;
            }
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
                        renderer.sprite = bannerRendSprite;
                        AmongUsClient.Instance.StartCoroutine(Effects.Lerp(fadeDuration, new Action<float>((p) =>
                        {
                            renderer.color = new Color(1, 1, 1, p);
                        })));
                    }
                })));
            }
        }
    }
}