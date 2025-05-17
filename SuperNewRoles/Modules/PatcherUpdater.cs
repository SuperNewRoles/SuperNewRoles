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
    public static void Initialize(SplashManager __instance)
    {
        __instance.StartCoroutine(CheckAndDownloadPatcher().WrapToIl2Cpp());
    }
    private static IEnumerator CheckAndDownloadPatcher()
    {
        GameObject gameObject = new("PatcherUpdater");
        GameObject.DontDestroyOnLoad(gameObject);

        UnityWebRequest request = UnityWebRequest.Get($"{VersionUpdatesUI.ApiUrl}patchers/data.json");
        yield return request.SendWebRequest();
        if (request.result != UnityWebRequest.Result.Success)
        {
            Logger.Error("Failed to download patcher data: " + request.error);
            yield break;
        }
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
                if (md5String.Equals(patchersData[patcher] as string, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                yield return DownloadPatcher(patcher).WrapToIl2Cpp();
                Logger.Info(patcher + " exists");
                continue;
            }
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
