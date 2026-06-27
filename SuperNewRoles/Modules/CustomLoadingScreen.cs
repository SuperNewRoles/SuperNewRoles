using System;
using System.Collections;
using System.Threading.Tasks;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using HarmonyLib;
using SuperNewRoles.CustomCosmetics;
using TMPro;
using UnityEngine;

namespace SuperNewRoles.Modules;

public static class CustomLoadingScreen
{
    public static bool PleaseDoWillLoad = false;
    private static bool IsLoading = true;
    private static bool Inited = false;
    private static bool waitOneFrame = false;

    [HarmonyPatch(typeof(SplashManager), nameof(SplashManager.Update))]
    public static class SplashManagerUpdatePatch
    {
        public static bool Prefix()
        {
            if (PleaseDoWillLoad)
            {
                // メインスレッドで読み込む必要あり
                IsLoading = false;
                PleaseDoWillLoad = false;
                LoadingText.text = "Loading AssetBundles...";
                waitOneFrame = true;
                return false;
            }
            if (waitOneFrame)
            {
                waitOneFrame = false;
                CustomCosmeticsLoader.willLoad?.Invoke();
                CustomCosmeticsLoader.willLoad = null;
                LoadingText.text = "";
                Logger.Info("Loading done");
                return false;
            }
            if (IsLoading)
            {
                if (CustomCosmeticsLoader.AssetBundlesDownloading)
                    LoadingText.text = $"Loading... {CustomCosmeticsLoader.AssetBundlesDownloadedCount}/{CustomCosmeticsLoader.AssetBundlesAllCount}\nSprites: {CustomCosmeticsLoader.SpritesDownloadingCount}/{CustomCosmeticsLoader.SpritesAllCount}";
                else
                    LoadingText.text = $"Loading... {(CustomCosmeticsLoader.SpritesAllCount - CustomCosmeticsLoader.SpritesDownloadingCount)}/{CustomCosmeticsLoader.SpritesAllCount}";
                return false;
            }
            return true;
        }
    }

    public static TextMeshPro LoadingText;

    [HarmonyPatch(typeof(SplashManager), nameof(SplashManager.Start))]
    public static class SplashManagerStartPatch
    {

        public static void Postfix(SplashManager __instance)
        {
            if (Inited)
                return;
            Logger.Info("SplashManagerStartPostfix");
            Transform text = AssetManager.Instantiate("LoadingText", __instance.transform).transform;
            text.localPosition = new(3.82f, -4.63f, -1f);
            text.localScale = Vector3.one * 0.15f;
            LoadingText = text.GetComponent<TextMeshPro>();
            Inited = true;
            ModManager.Instance.StartCoroutine(CustomCosmeticsLoader.LoadCosmeticsTaskAsync((c) => ModManager.Instance.StartCoroutine(c.WrapToIl2Cpp())).WrapToIl2Cpp());
            PatcherUpdater.Initialize();
            CustomCosmeticsLoader.runned = false;

            Task.Run(() =>
            {
                Logger.Info("Started");
                IsLoading = true;
                Logger.Info("Waiting load");
                SuperNewRolesPlugin.CustomRPCManagerLoadTask?.Wait();
                Logger.Info("CustomRPCManagerLoadTask done");
                SuperNewRolesPlugin.HarmonyPatchAllTask?.Wait();
                Logger.Info("HarmonyPatchAllTask done");
                while (!CustomCosmeticsLoader.runned)
                    Task.Delay(100).Wait();
                Logger.Info("CustomCosmeticsLoaderSplashManagerStartPatch done");
                PleaseDoWillLoad = true;
            });
        }
    }
}