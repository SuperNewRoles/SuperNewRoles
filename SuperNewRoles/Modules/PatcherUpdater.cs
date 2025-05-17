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
    private static List<string> patchers =
    [
        "SuperNewRolesUpdatePatcher.dll",
        "BepInEx.SplashScreen.Patcher.BepInEx6.dll",
        "BepInEx.SplashScreen.GUI.exe"
    ];
    public static void Initialize()
    {
        GameObject gameObject = new("PatcherUpdater");
        GameObject.DontDestroyOnLoad(gameObject);
        gameObject.AddComponent<PatcherUpdaterComponent>().StartCoroutine(CheckAndDownloadPatcher(gameObject).WrapToIl2Cpp());
    }
    private static IEnumerator CheckAndDownloadPatcher(GameObject gameObject)
    {
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
        foreach (string patcher in patchers)
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
        UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();
        if (request.result != UnityWebRequest.Result.Success)
        {
            Logger.Error("Failed to download patcher: " + request.error);
            yield break;
        }
        Logger.Info("Patcher downloaded");
        File.WriteAllBytes(BepInEx.Paths.PatcherPluginPath + "/" + fileName, request.downloadHandler.data);
    }
}

public class PatcherUpdaterComponent : MonoBehaviour
{
}