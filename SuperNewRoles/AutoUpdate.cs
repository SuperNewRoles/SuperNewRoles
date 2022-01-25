using System;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.IL2CPP;
using Il2CppSystem;
using Hazel;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnhollowerBaseLib;
using System.IO;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Security.Cryptography;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Twitch;

namespace SuperNewRoles
{
    public class AutoUpdate
    {
        public static GenericPopup InfoPopup;
        private static bool IsLoad = false;
        public static string updateURL = null;
        public static void Load()
        {
            TwitchManager man = DestroyableSingleton<TwitchManager>.Instance;
            InfoPopup = UnityEngine.Object.Instantiate<GenericPopup>(man.TwitchPopup);
            InfoPopup.TextAreaTMP.fontSize *= 0.7f;
            InfoPopup.TextAreaTMP.enableAutoSizing = false;
        }
        public static async Task<bool> Update()
        {
            try
            {
                HttpClient http = new HttpClient();
                http.DefaultRequestHeaders.Add("User-Agent", "SuperNewRoles Updater");
                var response = await http.GetAsync(new System.Uri(updateURL), HttpCompletionOption.ResponseContentRead);
                if (response.StatusCode != HttpStatusCode.OK || response.Content == null)
                {
                    System.Console.WriteLine("Server returned no data: " + response.StatusCode.ToString());
                    return false;
                }
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                System.UriBuilder uri = new System.UriBuilder(codeBase);
                string fullname = System.Uri.UnescapeDataString(uri.Path);
                if (File.Exists(fullname + ".old")) // Clear old file in case it wasnt;
                    File.Delete(fullname + ".old");

                File.Move(fullname, fullname + ".old"); // rename current executable to old

                using (var responseStream = await response.Content.ReadAsStreamAsync())
                {
                    using (var fileStream = File.Create(fullname))
                    { // probably want to have proper name here
                        responseStream.CopyTo(fileStream);
                    }
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
            if (!ConfigRoles.AutoUpdate.Value) {
                return false;
            }
            if (!IsLoad)
            {
                AutoUpdate.Load();
                IsLoad = true;
            }
            try
            {
                HttpClient http = new HttpClient();
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
                    return false; // Something went wrong
                }
                // check version
                SuperNewRolesPlugin.NewVersion = tagname.Replace("v", "");
                System.Version newver = System.Version.Parse(SuperNewRolesPlugin.NewVersion);
                System.Version Version = SuperNewRolesPlugin.Version;
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
                        return false;
                    for (JToken current = assets.First; current != null; current = current.Next)
                    {
                        string browser_download_url = current["browser_download_url"]?.ToString();
                        if (browser_download_url != null && current["content_type"] != null)
                        {
                            if (current["content_type"].ToString().Equals("application/x-msdownload") &&
                                browser_download_url.EndsWith(".dll"))
                            {
                                updateURL = browser_download_url;
                                AutoUpdate.Update();
                                setdate.SetText(ModTranslation.getString("creditsMain") + "\n" + string.Format(ModTranslation.getString("creditsUpdateOk"), SuperNewRolesPlugin.NewVersion));
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
