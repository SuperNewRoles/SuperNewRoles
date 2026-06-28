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
    private const int StartupLoadFallbackTimeoutMilliseconds = 60000;

    public static volatile bool PleaseDoWillLoad = false;
    private static volatile bool IsLoading = true;
    private static bool Inited = false;
    private static volatile bool waitOneFrame = false;
    private static volatile bool StartupContinuedBeforeCosmeticsFinished = false;

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
                RefreshStartupLoadAfterFallbackIfNeeded();
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

    public static void RefreshStartupLoadAfterFallbackIfNeeded()
    {
        if (!StartupContinuedBeforeCosmeticsFinished || !CustomCosmeticsLoader.runned)
            return;

        StartupContinuedBeforeCosmeticsFinished = false;
        CustomCosmeticsLoader.RefreshAfterStartupLoadCompleted();
    }

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
                try
                {
                    Logger.Info("Started");
                    IsLoading = true;
                    long startTicks = Environment.TickCount64;
                    Logger.Info("Waiting load");
                    WaitStartupTask(SuperNewRolesPlugin.CustomRPCManagerLoadTask, nameof(SuperNewRolesPlugin.CustomRPCManagerLoadTask), ref startTicks);
                    Logger.Info("CustomRPCManagerLoadTask done");
                    WaitStartupTask(SuperNewRolesPlugin.HarmonyPatchAllTask, nameof(SuperNewRolesPlugin.HarmonyPatchAllTask), ref startTicks);
                    Logger.Info("HarmonyPatchAllTask done");
                    if (!WaitForCustomCosmeticsLoader(ref startTicks))
                    {
                        StartupContinuedBeforeCosmeticsFinished = true;
                        Logger.Warning($"Custom cosmetics loading did not finish within {StartupLoadFallbackTimeoutMilliseconds / 1000} seconds. Continue startup while loading continues in background.");
                    }
                    Logger.Info("CustomCosmeticsLoaderSplashManagerStartPatch done");
                }
                catch (Exception ex)
                {
                    Logger.Error($"CustomLoadingScreen startup wait failed: {ex}");
                }
                finally
                {
                    PleaseDoWillLoad = true;
                }
            });
        }

        private static int GetRemainingTimeout(ref long startTicks)
        {
            long elapsed = Environment.TickCount64 - startTicks;
            return Math.Max(0, StartupLoadFallbackTimeoutMilliseconds - (int)elapsed);
        }

        private static void WaitStartupTask(Task task, string taskName, ref long startTicks)
        {
            if (task == null)
                return;

            int remaining = GetRemainingTimeout(ref startTicks);
            if (remaining <= 0)
            {
                Logger.Warning($"{taskName} did not finish before startup fallback timeout. Continue startup while it remains in progress.");
                return;
            }

            try
            {
                if (!task.Wait(remaining))
                {
                    Logger.Warning($"{taskName} did not finish before startup fallback timeout. Continue startup while it remains in progress.");
                    return;
                }
            }
            catch (AggregateException ex)
            {
                Logger.Error($"{taskName} failed: {ex}");
                return;
            }

            if (task.IsFaulted)
                Logger.Error($"{taskName} failed: {task.Exception}");
        }

        private static bool WaitForCustomCosmeticsLoader(ref long startTicks)
        {
            while (!CustomCosmeticsLoader.runned)
            {
                if (GetRemainingTimeout(ref startTicks) <= 0)
                    return false;
                Task.Delay(100).Wait();
            }
            return true;
        }

    }
}
