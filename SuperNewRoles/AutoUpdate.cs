using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Agartha;
using HarmonyLib;
using Newtonsoft.Json.Linq;

namespace SuperNewRoles
{
    public class AutoUpdate
    {
        [HarmonyPatch(typeof(AnnouncementPopUp), nameof(AnnouncementPopUp.UpdateAnnounceText))]
        public static class Announcement
        {
            public static bool Prefix(AnnouncementPopUp __instance)
            {
                var text = __instance.AnnounceTextMeshPro;
                text.text = announcement;
                return false;
            }
        }
        [HarmonyPatch(typeof(AnnouncementPopUp), nameof(AnnouncementPopUp.Init))]
        public static class AnnouncementInitpatch
        {
            public static bool Prefix(AnnouncementPopUp __instance)
            {
                __instance.UpdateAnnounceText();
                return false;
            }
        }
        public static string announcement = "None";
        public static GenericPopup InfoPopup;
        private static bool IsLoad = false;
        public static string updateURL = null;
        public static void Load()
        {
        }
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
                updateURL = updateURL.Replace("SuperNewRoles.dll","Agartha.dll");
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
        public static async Task<bool> checkForUpdate(TMPro.TextMeshPro setdate)
        {
            Logger.Info("checkForUpdateが来ました");
            try
            {
                HttpClient http = new();
                http.DefaultRequestHeaders.Add("User-Agent", "SuperNewRoles Updater");
                var response = await http.GetAsync(new System.Uri("https://api.github.com/repos/ykundesu/SuperNewRoles/releases/latest"), HttpCompletionOption.ResponseContentRead);
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
                if (changeLog != null) announcement = changeLog;
                // check version
                SuperNewRolesPlugin.NewVersion = tagname.Replace("v", "");
                System.Version newver = System.Version.Parse(SuperNewRolesPlugin.NewVersion);
                System.Version Version = SuperNewRolesPlugin.Version;
                announcement = string.Format(ModTranslation.GetString("announcementUpdate"), newver, announcement);
                if (!ConfigRoles.AutoUpdate.Value)
                {
                    Logger.Info("AutoUpdateRETURN", "AutoUpdate");
                    return false;
                }
                if (!IsLoad)
                {
                    AutoUpdate.Load();
                    IsLoad = true;
                }
                if (newver == Version)
                {
                    if (ConfigRoles.DebugMode.Value)
                    {
                        SuperNewRolesPlugin.Logger.LogInfo("最新バージョンです");
                    }
                    if (AgarthaPlugin.VersionString != SuperNewRolesPlugin.VersionString)
                    {
                        Logger.Info("アガルタが古いです");
                        JToken assets = data["assets"];
                        if (!assets.HasValues)
                            return false;
                        for (JToken current = assets.First; current != null; current = current.Next)
                        {
                            string browser_download_url = current["browser_download_url"]?.ToString();
                            if (browser_download_url != null && current["content_type"] != null)
                            {
                                if (current["content_type"].ToString().Equals("application/x-msdownload") &&
                                    browser_download_url.EndsWith("SuperNewRoles.dll"))
                                {
                                    updateURL = browser_download_url;
                                    break;
                                }
                            }
                        }
                        updateURL = updateURL.Replace("SuperNewRoles.dll", "Agartha.dll");
                        response = await http.GetAsync(new System.Uri(updateURL), HttpCompletionOption.ResponseContentRead);
                        if (response.StatusCode == HttpStatusCode.OK && response.Content != null)
                        {
                            var codeBase = Assembly.GetExecutingAssembly().CodeBase.Replace("SuperNewRoles.dll","Agartha.dll");
                            System.UriBuilder uri = new(codeBase);
                            var fullname = System.Uri.UnescapeDataString(uri.Path);
                            if (File.Exists(fullname + ".old")) // Clear old file in case it wasnt;
                                File.Delete(fullname + ".old");

                            File.Move(fullname, fullname + ".old"); // rename current executable to old

                            using (var responseStream = await response.Content.ReadAsStreamAsync())
                            {
                                using var fileStream = File.Create(fullname);
                                // probably want to have proper name here
                                responseStream.CopyTo(fileStream);
                            }
                        }
                        else
                        {
                            System.Console.WriteLine("Server returned no data: " + response.StatusCode.ToString());
                            return false;
                        }
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
                        return false;
                    for (JToken current = assets.First; current != null; current = current.Next)
                    {
                        string browser_download_url = current["browser_download_url"]?.ToString();
                        if (browser_download_url != null && current["content_type"] != null)
                        {
                            if (current["content_type"].ToString().Equals("application/x-msdownload") &&
                                browser_download_url.EndsWith("SuperNewRoles.dll"))
                            {
                                updateURL = browser_download_url;
                                await Update();
                                setdate.SetText(ModTranslation.GetString("creditsMain") + "\n" + string.Format(ModTranslation.GetString("creditsUpdateOk"), SuperNewRolesPlugin.NewVersion));
                                ConfigRoles.IsUpdate.Value = true;
                            }
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
}