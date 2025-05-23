using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using HarmonyLib;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace SuperNewRoles;

public class AutoUpdate
{
    static AnnouncementPanel firstpanel;
    [HarmonyPatch(typeof(AnnouncementPopUp), nameof(AnnouncementPopUp.Update))]
    public static class AnnouncementUpdatePatch
    {
        public static void Postfix(AnnouncementPopUp __instance)
        {
            if (firstpanel is null) firstpanel = __instance.ListScroller.Inner.transform.GetComponentInChildren<AnnouncementPanel>();
            if (firstpanel is null) return;
            firstpanel.DateText.text = announcementtitle;
            firstpanel.TitleText.text = announcementtitlever;
            firstpanel.announcement.Text = announcement;
            firstpanel.announcement.Title = announcementtitle;
            firstpanel.announcement.ShortTitle = announcementtitle;
            firstpanel.announcement.SubTitle = announcementtitlever;
            if (__instance.selectedPanel == firstpanel)
            {
                __instance.SubTitle.text = announcementtitlever;
                __instance.Title.text = string.Format(ModTranslation.GetString("announcementUpdateTitle"), announcementtitle);
                __instance.AnnouncementBodyText.text = announcement;
            }
        }
    }
    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.LateUpdate))]
    public static class MainMenuManagerLateUpdatePatch
    {
        public static void Postfix(MainMenuManager __instance)
        {
            if (IsSNR2 && !showedSNR2Popup)
            {
                showedSNR2Popup = true;
                GameObject popup = new GameObject("SNR2Popup");
                popup.transform.localPosition = new(0, 0, -100f);

                GameObject background = new GameObject("SNR2PopupBackground");
                background.transform.SetParent(popup.transform);
                background.transform.localPosition = new(0, 0, 1f);
                background.transform.localScale = Vector3.one * 100;

                var render = background.AddComponent<SpriteRenderer>();
                render.sprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.Black.png", 200);
                PassiveButton passiveButton2 = background.AddComponent<PassiveButton>();
                passiveButton2.Colliders = new Collider2D[] { background.AddComponent<PolygonCollider2D>() };
                passiveButton2.OnClick = new();
                passiveButton2.OnMouseOut = new();
                passiveButton2.OnMouseOver = new();
                render.color = new Color(0, 0, 0, 0.5f);

                GameObject bg = new GameObject("SNR2PopupBG");
                bg.transform.SetParent(popup.transform);
                bg.AddComponent<SpriteRenderer>().sprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.BG.png", 200);
                bg.transform.localPosition = Vector3.zero;
                bg.transform.localScale = new(0.9f, 1.1f, 1f);
                bg.transform.localRotation = Quaternion.identity;
                TextMeshPro baseTMP = GameObject.FindObjectOfType<VersionShower>().text;
                TextMeshPro text = GameObject.Instantiate(baseTMP, popup.transform);
                text.transform.localPosition = Vector3.zero;
                text.transform.localScale = Vector3.one;
                text.transform.localRotation = Quaternion.identity;
                GameObject.Destroy(text.gameObject.GetComponent<TextTranslatorTMP>());
                GameObject.Destroy(text.gameObject.GetComponent<AspectPosition>());
                text.text = "<b>" + ModTranslation.GetString("SNR2PopupTitle") + "</b>";//新しいSuperNewRolesはすぐそこに";
                text.transform.localPosition = new(0, 1, 0);
                text.transform.localScale = Vector3.one * 2.5f;
                text.color = Color.white;
                text.alignment = TextAlignmentOptions.Center;
                TextMeshPro text2 = GameObject.Instantiate(baseTMP, popup.transform);
                text2.transform.localRotation = Quaternion.identity;
                GameObject.Destroy(text2.gameObject.GetComponent<TextTranslatorTMP>());
                GameObject.Destroy(text2.gameObject.GetComponent<AspectPosition>());
                text2.color = Color.white;
                text2.alignment = TextAlignmentOptions.Center;
                OpenHyperlinks openHyperlinks = text2.gameObject.AddComponent<OpenHyperlinks>();
                openHyperlinks.pTextMeshPro = text2;
                openHyperlinks.Text = ModTranslation.GetString("SNR2PopupText");
                openHyperlinks.LinkColor = Color.green;
                openHyperlinks.UpdateTMPMesh();
                text2.transform.localScale = Vector3.one * 2;

                GameObject closeButton = new GameObject("CloseButton");
                closeButton.transform.SetParent(popup.transform);
                closeButton.transform.localPosition = new(-1.5f, -1, -5);
                closeButton.transform.localScale = Vector3.one * 0.86f;
                var closeRender = closeButton.AddComponent<SpriteRenderer>();
                closeRender.sprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.Button.png", 200);
                closeRender.color = new Color(1, 1, 1, 0.5f);
                PassiveButton passiveButton = closeButton.AddComponent<PassiveButton>();
                passiveButton.Colliders = new Collider2D[] { closeButton.AddComponent<PolygonCollider2D>() };
                passiveButton.OnClick = new();
                passiveButton.OnClick.AddListener((UnityAction)(() => { GameObject.Destroy(popup); }));
                passiveButton.OnMouseOut = new();
                passiveButton.OnMouseOut.AddListener((UnityAction)(() => { closeRender.color = new Color(1, 1, 1, 0.5f); }));
                passiveButton.OnMouseOver = new();
                passiveButton.OnMouseOver.AddListener((UnityAction)(() => { closeRender.color = new Color(1, 1, 1, 1); }));

                TextMeshPro closeText = GameObject.Instantiate(text, closeButton.transform);
                closeText.transform.localPosition = new(0, 0, 0);
                closeText.transform.localScale = Vector3.one * 1.5f;
                closeText.text = "閉じる";
                closeText.color = Color.white;
                closeText.alignment = TextAlignmentOptions.Center;

                GameObject updateButton = new GameObject("UpdateButton");
                updateButton.transform.SetParent(popup.transform);
                updateButton.transform.localPosition = new(1.5f, -1, -5);
                updateButton.transform.localScale = Vector3.one * 0.86f;
                var updateRender = updateButton.AddComponent<SpriteRenderer>();
                updateRender.sprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.Button.png", 200);
                updateRender.color = new Color(1, 1, 1, 0.5f);
                PassiveButton updatePassiveButton = updateButton.AddComponent<PassiveButton>();
                updatePassiveButton.Colliders = new Collider2D[] { updateButton.AddComponent<PolygonCollider2D>() };
                updatePassiveButton.OnClick = new();
                updatePassiveButton.OnClick.AddListener((UnityAction)(() => { SNR2Update(); }));
                updatePassiveButton.OnMouseOut = new();
                updatePassiveButton.OnMouseOut.AddListener((UnityAction)(() => { updateRender.color = new Color(1, 1, 1, 0.5f); }));
                updatePassiveButton.OnMouseOver = new();
                updatePassiveButton.OnMouseOver.AddListener((UnityAction)(() => { updateRender.color = new Color(1, 1, 1, 1); }));


                TextMeshPro updateText = GameObject.Instantiate(text, updateButton.transform);
                updateText.transform.localPosition = new(0, 0, 0);
                updateText.transform.localScale = Vector3.one * 1.5f;
                updateText.text = "アップデート";
                updateText.color = Color.white;
                updateText.alignment = TextAlignmentOptions.Center;
            }
        }
    }
    public static async Task SNR2Update()
    {
        // アップデート中ポップアップを生成
        GameObject updatePopup = CreateUpdatePopup();
        TextMeshPro statusText = updatePopup.transform.Find("StatusText").GetComponent<TextMeshPro>();

        // アップデート中表示
        statusText.text = ModTranslation.GetString("UpdatingInProgress");

        // アップデート実行（進捗表示付き）
        bool updateSuccess = await UpdateWithProgress(statusText);

        if (updateSuccess)
        {
            // 完了メッセージ表示
            statusText.text = ModTranslation.GetString("UpdateCompleteRestartRequired");
        }
        else
        {
            // エラーメッセージ表示
            statusText.text = ModTranslation.GetString("UpdateFailed");
        }
    }

    private static GameObject CreateUpdatePopup()
    {
        GameObject popup = new GameObject("UpdateProgressPopup");
        popup.transform.localPosition = new(0, 0, -120f);

        // 背景
        GameObject background = new GameObject("UpdatePopupBackground");
        background.transform.SetParent(popup.transform);
        background.transform.localPosition = new(0, 0, 1f);
        background.transform.localScale = Vector3.one * 100;

        var render = background.AddComponent<SpriteRenderer>();
        render.sprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.Black.png", 200);
        render.color = new Color(0, 0, 0, 0.7f);

        // ポップアップ本体
        GameObject bg = new GameObject("UpdatePopupBG");
        bg.transform.SetParent(popup.transform);
        bg.AddComponent<SpriteRenderer>().sprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.BG.png", 200);
        bg.transform.localPosition = Vector3.zero;
        bg.transform.localScale = new(0.7f, 0.8f, 1f);
        bg.transform.localRotation = Quaternion.identity;

        // タイトルテキスト
        TextMeshPro baseTMP = GameObject.FindObjectOfType<VersionShower>().text;
        TextMeshPro titleText = GameObject.Instantiate(baseTMP, popup.transform);
        titleText.transform.localPosition = new(0, 1, 0);
        titleText.transform.localScale = Vector3.one * 2.5f;
        titleText.transform.localRotation = Quaternion.identity;
        GameObject.Destroy(titleText.gameObject.GetComponent<TextTranslatorTMP>());
        GameObject.Destroy(titleText.gameObject.GetComponent<AspectPosition>());
        titleText.text = "<b>" + ModTranslation.GetString("UpdateProgressTitle") + "</b>";
        titleText.color = Color.white;
        titleText.alignment = TextAlignmentOptions.Center;

        // ステータステキスト
        TextMeshPro statusText = GameObject.Instantiate(baseTMP, popup.transform);
        statusText.name = "StatusText";
        statusText.transform.localPosition = new(0, 0, 0);
        statusText.transform.localScale = Vector3.one * 2f;
        statusText.transform.localRotation = Quaternion.identity;
        GameObject.Destroy(statusText.gameObject.GetComponent<TextTranslatorTMP>());
        GameObject.Destroy(statusText.gameObject.GetComponent<AspectPosition>());
        statusText.color = Color.white;
        statusText.alignment = TextAlignmentOptions.Center;

        return popup;
    }

    public static async Task<bool> UpdateWithProgress(TextMeshPro statusText)
    {
        try
        {
            HttpClient http = new();
            http.DefaultRequestHeaders.Add("User-Agent", "SuperNewRoles Updater");

            // SuperNewRoles.dll のダウンロード
            statusText.text = ModTranslation.GetString("UpdatingInProgress") + " (0%)";
            var response = await http.GetAsync(new System.Uri(updateURL), HttpCompletionOption.ResponseHeadersRead);
            if (response.StatusCode != HttpStatusCode.OK || response.Content == null)
            {
                System.Console.WriteLine("Server returned no data: " + response.StatusCode.ToString());
                return false;
            }

            var totalBytes = response.Content.Headers.ContentLength ?? 0;
            var downloadedBytes = 0L;

            string codeBase = Assembly.GetExecutingAssembly().CodeBase;
            System.UriBuilder uri = new(codeBase);
            string fullname = System.Uri.UnescapeDataString(uri.Path);
            if (File.Exists(fullname + ".old"))
                File.Delete(fullname + ".old");

            File.Move(fullname, fullname + ".old");

            using (var responseStream = await response.Content.ReadAsStreamAsync())
            using (var fileStream = File.Create(fullname))
            {
                var buffer = new byte[8192];
                int bytesRead;
                while ((bytesRead = await responseStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    await fileStream.WriteAsync(buffer, 0, bytesRead);
                    downloadedBytes += bytesRead;

                    var progress = totalBytes > 0 ? (int)(downloadedBytes * 50 / totalBytes) : 25;
                    statusText.text = ModTranslation.GetString("UpdatingInProgress") + $" ({progress}%)";
                }
            }

            // Agartha.dll のダウンロード
            statusText.text = ModTranslation.GetString("UpdatingInProgress") + " (50%)";
            string agarthaURL = updateURL.Replace("SuperNewRoles.dll", "Agartha.dll");
            response = await http.GetAsync(new System.Uri(agarthaURL), HttpCompletionOption.ResponseHeadersRead);
            if (response.StatusCode != HttpStatusCode.OK || response.Content == null)
            {
                System.Console.WriteLine("Server returned no data: " + response.StatusCode.ToString());
                return false;
            }

            totalBytes = response.Content.Headers.ContentLength ?? 0;
            downloadedBytes = 0L;

            codeBase = Assembly.GetExecutingAssembly().CodeBase.Replace("SuperNewRoles.dll", "Agartha.dll");
            uri = new(codeBase);
            fullname = System.Uri.UnescapeDataString(uri.Path);
            if (File.Exists(fullname + ".old"))
                File.Delete(fullname + ".old");

            File.Move(fullname, fullname + ".old");

            using (var responseStream = await response.Content.ReadAsStreamAsync())
            using (var fileStream = File.Create(fullname))
            {
                var buffer = new byte[8192];
                int bytesRead;
                while ((bytesRead = await responseStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    await fileStream.WriteAsync(buffer, 0, bytesRead);
                    downloadedBytes += bytesRead;

                    var progress = totalBytes > 0 ? 50 + (int)(downloadedBytes * 50 / totalBytes) : 75;
                    statusText.text = ModTranslation.GetString("UpdatingInProgress") + $" ({progress}%)";
                }
            }

            statusText.text = ModTranslation.GetString("UpdatingInProgress") + " (100%)";
            SuperNewRolesPlugin.IsUpdate = true;
            return true;
        }
        catch (System.Exception ex)
        {
            SuperNewRolesPlugin.Instance.Log.LogError(ex.ToString());
            System.Console.WriteLine(ex);
        }
        return false;
    }

    private static bool showedSNR2Popup = false;
    public static string announcement = "None";
    public static string announcementtitle = "None";
    public static string announcementtitlever = "None";
    public static GenericPopup InfoPopup;
    private static bool IsLoad = false;
    public static string updateURL = null;
    public static bool IsSNR2;
    public static async Task<bool> Update()
    {
        try
        {
            HttpClient http = new();
            http.DefaultRequestHeaders.Add("User-Agent", "SuperNewRoles Updater");
            var response = await http.GetAsync(new System.Uri(updateURL), HttpCompletionOption.ResponseContentRead);
            if (response.StatusCode != HttpStatusCode.OK || response.Content == null)
            {
                System.Console.WriteLine("Server returned no data: " + response.StatusCode.ToString());
                return false;
            }
            string codeBase = Assembly.GetExecutingAssembly().CodeBase;
            System.UriBuilder uri = new(codeBase);
            string fullname = System.Uri.UnescapeDataString(uri.Path);
            if (File.Exists(fullname + ".old")) // Clear old file in case it wasnt;
                File.Delete(fullname + ".old");

            File.Move(fullname, fullname + ".old"); // rename current executable to old

            using (var responseStream = await response.Content.ReadAsStreamAsync())
            {
                using var fileStream = File.Create(fullname);
                // probably want to have proper name here
                responseStream.CopyTo(fileStream);
            }

            //アガルタ
            updateURL = updateURL.Replace("SuperNewRoles.dll", "Agartha.dll");
            response = await http.GetAsync(new System.Uri(updateURL), HttpCompletionOption.ResponseContentRead);
            if (response.StatusCode != HttpStatusCode.OK || response.Content == null)
            {
                System.Console.WriteLine("Server returned no data: " + response.StatusCode.ToString());
                return false;
            }
            codeBase = Assembly.GetExecutingAssembly().CodeBase.Replace("SuperNewRoles.dll", "Agartha.dll");
            uri = new(codeBase);
            fullname = System.Uri.UnescapeDataString(uri.Path);
            if (File.Exists(fullname + ".old")) // Clear old file in case it wasnt;
                File.Delete(fullname + ".old");

            File.Move(fullname, fullname + ".old"); // rename current executable to old

            using (var responseStream = await response.Content.ReadAsStreamAsync())
            {
                using var fileStream = File.Create(fullname);
                // probably want to have proper name here
                responseStream.CopyTo(fileStream);
            }
            SuperNewRolesPlugin.IsUpdate = true;
            return true;
        }
        catch (System.Exception ex)
        {
            SuperNewRolesPlugin.Instance.Log.LogError(ex.ToString());
            System.Console.WriteLine(ex);
        }
        return false;
    }
    public static async Task<bool> checkForUpdate(TMPro.TextMeshPro setData)
    {
        Logger.Info("checkForUpdateが来ました");
        try
        {
            HttpClient http = new();
            http.DefaultRequestHeaders.Add("User-Agent", "SuperNewRoles Updater");
            var response = await http.GetAsync(new System.Uri($"https://api.github.com/repos/{SuperNewRolesPlugin.ModUrl}/releases/latest"), HttpCompletionOption.ResponseContentRead);
            Logger.Info($"https://api.github.com/repos/{SuperNewRolesPlugin.ModUrl}/releases/latest", "リリース情報のURL");
            if (response.StatusCode != HttpStatusCode.OK || response.Content == null)
            {
                System.Console.WriteLine("Server returned no data: " + response.StatusCode.ToString());
                return false;
            }
            string json = await response.Content.ReadAsStringAsync();
            JObject data = JObject.Parse(json);

            string tagname = data["tag_name"]?.ToString();
            if (tagname == null)
            {
                Logger.Info("自動アップデートなのにタグね～じゃん！フィクションはバグだけにしとけよな！");
                return false; // Something went wrong
            }
            string changeLog = data["body"]?.ToString();
            string title = data["name"]?.ToString();
            if (changeLog != null) announcement = changeLog;
            // check version
            SuperNewRolesPlugin.NewVersion = tagname.Replace("v", "");
            System.Version newver = System.Version.Parse(SuperNewRolesPlugin.NewVersion);
            System.Version Version = SuperNewRolesPlugin.ThisVersion;
            //announcement = string.Format(ModTranslation.GetString("announcementUpdate"), newver, announcement);
            announcementtitle = newver.ToString();
            announcementtitlever = title;
            if (!ConfigRoles.AutoUpdate.Value)
            {
                Logger.Info("AutoUpdateRETURN", "AutoUpdate");
                return false;
            }
            if (!IsLoad)
            {
                IsLoad = true;
            }
            // v3.0.0.0のテストここ
            // newver = System.Version.Parse("3.0.0.0");
            if (newver == Version)
            {
                if (DebugModeManager.IsDebugMode)
                {
                    SuperNewRolesPlugin.Logger.LogInfo("最新バージョンです");
                }
            }
            else
            {
                if (DebugModeManager.IsDebugMode)
                {
                    SuperNewRolesPlugin.Logger.LogInfo("古いバージョンです");
                }
                // 新しいバージョンのメジャーバージョンが3以上かチェック
                if (newver.Major >= 3)
                {
                    IsSNR2 = true;
                    JToken assets2 = data["assets"];
                    if (!assets2.HasValues)
                    {
                        Logger.Info("AssetsのValueがありませんでした。");
                        return false;
                    }
                    for (JToken current = assets2.First; current != null; current = current.Next)
                    {
                        string browser_download_url = current["browser_download_url"]?.ToString();
                        if (browser_download_url.EndsWith("SuperNewRoles.dll"))
                        {
                            updateURL = browser_download_url;
                        }
                    }
                    return false;
                }
                JToken assets = data["assets"];
                if (!assets.HasValues)
                {
                    Logger.Info("AssetsのValueがありませんでした。");
                    return false;
                }
                for (JToken current = assets.First; current != null; current = current.Next)
                {
                    string browser_download_url = current["browser_download_url"]?.ToString();
                    if (browser_download_url.EndsWith("SuperNewRoles.dll"))
                    {
                        updateURL = browser_download_url;
                        await Update();
                        setData.SetText(ModTranslation.GetString("creditsMain") + "\n" + string.Format(ModTranslation.GetString("creditsUpdateOk"), SuperNewRolesPlugin.NewVersion));
                        ConfigRoles.IsUpdate.Value = true;
                    }
                }
            }
            return false;
        }
        catch (System.Exception e)
        {
            if (DebugModeManager.IsDebugMode)
            {
                SuperNewRolesPlugin.Logger.LogInfo("Error:" + e);
            }
            return false;
        }
    }
}