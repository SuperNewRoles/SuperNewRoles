using System;
using System.Collections;
using System.Linq;
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

    [HarmonyPatch(typeof(HudManager))]
    public static class HudManagerPatch
    {
        public static GameObject TextObject;
        public static AspectPosition TextAspectPositionObject;
        public static TextMeshPro TextTMPObject;

        [HarmonyPatch(nameof(HudManager.Start)), HarmonyPostfix]
        public static void StartPostfix(HudManager __instance)
        {
            TextObject = GameObject.Instantiate(__instance.roomTracker.gameObject, __instance.transform);
            TextObject.name = "Version Text";
            TextObject.layer = 5;
            GameObject.Destroy(TextObject.GetComponent<RoomTracker>());
            TextAspectPositionObject = TextObject.AddComponent<AspectPosition>();
            TextAspectPositionObject.Alignment = AspectPosition.EdgeAlignments.LeftTop;
            TextAspectPositionObject.DistanceFromEdge = new(1.85f, 0.35f);
            TextAspectPositionObject.parentCam = Camera.main;
            TextAspectPositionObject.updateAlways = true;
            TextAspectPositionObject.useGUILayout = true;
            TextTMPObject = TextObject.GetComponent<TextMeshPro>();
            TextTMPObject.fontSizeMax = 2;
            TextTMPObject.fontSizeMin = 2;
            TextTMPObject.alignment = TextAlignmentOptions.TopLeft;
            TextTMPObject.autoSizeTextContainer = true;
            TextTMPObject.enableWordWrapping = false;
        }

        [HarmonyPatch(nameof(HudManager.Update)), HarmonyPostfix]
        public static void UpdatePostfix()
        {
            AspectPosition position = TextAspectPositionObject;
            TextMeshPro text = TextTMPObject;
            if (AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started)
            {
                text.text = $"{baseCredentials}";
                try
                {
                    if (ModHelpers.IsDebugMode()) text.text += $"\n{ModTranslation.GetString("DebugModeOn")}";
                    if (!ModeHandler.IsMode(ModeId.Default, ModeId.HideAndSeek))
                        text.text += $"\n{ModTranslation.GetString("SettingMode")}:{ModeHandler.GetThisModeIntro()}";
                }
                catch { }
                //ブランチ名表示
                if (SuperNewRolesPlugin.IsBeta)//masterビルド以外の時
                {
                    //改行+Branch名+コミット番号
                    text.text += $"\n{ThisAssembly.Git.Branch}({ThisAssembly.Git.Commit})";
                }

                position.Alignment = AspectPosition.EdgeAlignments.RightTop;
                position.DistanceFromEdge = new(4.2f, 0.35f);
                text.alignment = TextAlignmentOptions.TopRight;
            }
            else
            {
                text.text = $"{baseCredentials}\n{ModTranslation.GetString("creditsFull")}";

                position.Alignment = AspectPosition.EdgeAlignments.LeftTop;
                position.DistanceFromEdge = new(1.85f, 0.35f);
                text.alignment = TextAlignmentOptions.TopLeft;
            }
        }
    }

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
                            TransData += $"{current["name"]?.ToString()} <size=100%>({current["language"]?.ToString()})</size>\n";
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
            obj.name = "CreditsPopup";
            obj.GetComponent<StatsPopup>().SelectableButtons.ToList().ForEach(button => GameObject.Destroy(button.gameObject));
            CreditsPopup = obj;
            GameObject.Destroy(obj.GetComponent<StatsPopup>());

            CreditsPopup.transform.FindChild("Background").localScale = new Vector3(1.5f, 1f, 1f);
            CreditsPopup.transform.FindChild("CloseButton").localPosition = new Vector3(-3.75f, 2.65f, 0);

            var textobj = CreditsPopup.transform.FindChild("Title_TMP");
            UnityEngine.Object.Destroy(textobj.GetComponent<TextTranslatorTMP>());
            textobj.GetComponent<TextMeshPro>().text = "<size=200%>Credit for SNR</size>";
            textobj.localScale = new Vector3(1.5f, 1.5f, 1f);

            var statsTextTransform = CreditsPopup.transform.FindChild("StatsText_TMP"); // Findの使用回数を減らす為に中身のないStatsTextを複製
            statsTextTransform.gameObject.name = "CreditText_TMP";
            const string titleFormat = $"<size=200%><align={"left"}>{{0}}</align></size>";
            const string textFormat = $"<size=150%><align={"left"}>{{0}}</align></size>";

            var developerTitleText = UnityEngine.Object.Instantiate(statsTextTransform, CreditsPopup.transform);
            developerTitleText.gameObject.name = "DeveloperText";
            developerTitleText.GetComponent<TextMeshPro>().text = string.Format(titleFormat, ModTranslation.GetString("Developer"));
            developerTitleText.position = new Vector3(0.1f, -1.15f, -12f);
            developerTitleText.localPosition = new Vector3(0.1f, -1.15f, -2f);
            developerTitleText.localScale = new Vector3(1.5f, 1.5f, 1f);

            var devText = UnityEngine.Object.Instantiate(developerTitleText, CreditsPopup.transform);
            devText.position = new Vector3(-0.2f, -1.1f, -12f);
            devText.localPosition = new Vector3(-0.2f, -1.1f, -2f);
            devText.localScale = new Vector3(1.25f, 1.25f, 1f);
            devText.GetComponent<TextMeshPro>().text = string.Format(textFormat, DevsData);

            var transTitleText = UnityEngine.Object.Instantiate(statsTextTransform, CreditsPopup.transform);
            transTitleText.gameObject.name = "TranslatorText";
            transTitleText.GetComponent<TextMeshPro>().text = string.Format(titleFormat, ModTranslation.GetString("Translator"));
            transTitleText.position = new Vector3(0.1f, -4.15f, -12f);
            transTitleText.localPosition = new Vector3(0.1f, -4.15f, -2f);
            transTitleText.localScale = new Vector3(1.5f, 1.5f, 1f);

            var transText = UnityEngine.Object.Instantiate(transTitleText, CreditsPopup.transform);
            transText.position = new Vector3(-0.2f, -4.1f, -12f);
            transText.localPosition = new Vector3(-0.2f, -4.1f, -2f);
            transText.localScale = new Vector3(1.25f, 1.25f, 1f);
            transText.GetComponent<TextMeshPro>().text = string.Format(textFormat, TransData);

            // サポーターは現在不在

            UnityEngine.Object.Destroy(statsTextTransform.gameObject); // 用済みなオブジェクトを削除
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
            DownLoadCustomCosmetics.CosmeticsLoad();
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