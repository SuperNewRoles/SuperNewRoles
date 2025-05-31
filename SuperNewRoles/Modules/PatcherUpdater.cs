using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace SuperNewRoles.Modules;

public static class PatcherUpdater
{
    private static readonly List<string> patchers =
    [
        "SuperNewRolesUpdatePatcher.dll",
        "BepInEx.SplashScreen.Patcher.BepInEx6.dll",
        "BepInEx.SplashScreen.GUI.exe"
    ];
    private static readonly List<string> patchers_android =
    [
        "SuperNewRolesUpdatePatcher.dll",
        "BepInEx.SplashScreen.Patcher.BepInEx6.dll",
        "BepInEx.SplashScreen.GUI.exe"
    ];
    private static readonly List<string> currentPatchers = ModHelpers.IsAndroid() ? patchers_android : patchers;
    public static void Initialize()
    {
        GameObject gameObject = new("PatcherUpdater");
        GameObject.DontDestroyOnLoad(gameObject);
        gameObject.AddComponent<PatcherUpdaterComponent>().StartCoroutine(CheckAndDownloadPatcher(gameObject).WrapToIl2Cpp());
    }
    private static IEnumerator CheckAndDownloadPatcher(GameObject gameObject)
    {
        Logger.Info("Removing old patcher");
        foreach (string patcher in currentPatchers)
        {
            string nowFileName = BepInEx.Paths.PatcherPluginPath + "/" + patcher + ".old";
            try
            {
                if (File.Exists(nowFileName))
                    File.Delete(nowFileName);
            }
            catch (Exception e)
            {
                Logger.Error("Failed to delete old patcher: " + e.Message);
            }
        }

        Logger.Info("Old patcher removed");
        Logger.Info("Checking and downloading patcher");
        UnityWebRequest request = UnityWebRequest.Get($"{VersionUpdatesUI.ApiUrl}patchers/data.json");
        yield return request.SendWebRequest();
        if (request.result != UnityWebRequest.Result.Success)
        {
            Logger.Error("Failed to download patcher data: " + request.error);
            yield break;
        }
        Logger.Info("Patcher data downloaded");
        string data = request.downloadHandler.text;
        Dictionary<string, object> patchersData = JsonParser.Parse(data) as Dictionary<string, object>;
        MD5 md5 = MD5.Create();
        foreach (string patcher in currentPatchers)
        {
            if (File.Exists(BepInEx.Paths.PatcherPluginPath + "/" + patcher))
            {
                // MD5チェック
                byte[] fileMD5 = md5.ComputeHash(File.ReadAllBytes(BepInEx.Paths.PatcherPluginPath + "/" + patcher));
                string md5String = BitConverter.ToString(fileMD5).Replace("-", "").ToLower();
                Logger.Info(patcher + " md5: " + md5String + " patcherData: " + patchersData[patcher]);
                if (patchersData.TryGetValue(patcher, out object value) && md5String.Equals(value as string, StringComparison.OrdinalIgnoreCase))
                {
                    Logger.Info(patcher + " exists");
                    continue;
                }
                Logger.Info(patcher + " md5 mismatch, downloading");
            }
            else
                Logger.Info(patcher + " does not exist, downloading");

            yield return DownloadPatcher(patcher).WrapToIl2Cpp();
            Logger.Info(patcher + " downloaded");
            continue;
        }
        GameObject.Destroy(gameObject);
    }
    private static IEnumerator DownloadPatcher(string fileName)
    {
        string url = $"{VersionUpdatesUI.ApiUrl}patchers/{fileName}";
        SNRHttpClient request = SNRHttpClient.Get(url);
        request.ignoreSslErrors = true;
        yield return request.SendWebRequest();
        if (request.error != null)
        {
            Logger.Error("Failed to download patcher: " + request.error);
            yield break;
        }
        Logger.Info("Patcher downloaded");
        string nowFileName = BepInEx.Paths.PatcherPluginPath + "/" + fileName;
        // ファイルが存在する場合はリネームしてから保存(これで次回起動時に適用される)
        if (File.Exists(nowFileName))
            File.Move(nowFileName, nowFileName + ".old");
        File.WriteAllBytes(nowFileName, request.downloadHandler.data);
    }
}

public class PatcherUpdaterComponent : MonoBehaviour
{
}