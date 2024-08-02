using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using HarmonyLib;
using Newtonsoft.Json.Linq;

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
    public static string announcement = "None";
    public static string announcementtitle = "None";
    public static string announcementtitlever = "None";
    public static GenericPopup InfoPopup;
    private static bool IsLoad = false;
    public static string updateURL = null;
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
            if (newver == Version)
            {
                if (ConfigRoles.DebugMode.Value)
                {
                    SuperNewRolesPlugin.Logger.LogInfo("最新バージョンです");
                }
            }
            else
            {
                if (ConfigRoles.DebugMode.Value)
                {
                    SuperNewRolesPlugin.Logger.LogInfo("古いバージョンです");
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
            if (ConfigRoles.DebugMode.Value)
            {
                SuperNewRolesPlugin.Logger.LogInfo("Error:" + e);
            }
            return false;
        }
    }
}