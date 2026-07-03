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
    private const float LoadingCompleteVisibleSeconds = 0.65f;
    private const float LoadingCompleteFadeSeconds = 0.75f;
    private const float CenteredLoadingTextBaseScale = 0.3f;

    public static volatile bool PleaseDoWillLoad = false;
    private static volatile bool IsLoading = true;
    private static bool Inited = false;
    private static volatile bool waitOneFrame = false;
    private static volatile bool StartupContinuedBeforeCosmeticsFinished = false;
    private static bool LoadingTextFadeStarted = false;
    private static bool UseCenteredLoadingTextScale = false;
    private static float CenteredLoadingTextBaseOrthographicSize = -1f;

    [HarmonyPatch(typeof(SplashManager), nameof(SplashManager.Update))]
    public static class SplashManagerUpdatePatch
    {
        public static bool Prefix(SplashManager __instance)
        {
            bool splashFinished = Time.time - __instance.startTime > __instance.minimumSecondsBeforeSceneChange;
            string translateText = ModTranslation.GetString("CustomCosmetics_Loading") + (int)((Time.time * 4) % 4) switch
            {
                0 => "",
                1 => ".",
                2 => "..",
                _ => "..."
            };

            // スプラッシュシーンアニメーションが終わった場合
            if (LoadingText != null && splashFinished && !LoadingTextFadeStarted)
            {
                ApplyCenteredLoadingTextLayout();
            }
            if (PleaseDoWillLoad)
            {
                // メインスレッドで読み込む必要あり
                IsLoading = false;
                PleaseDoWillLoad = false;
                if (LoadingText != null)
                    LoadingText.text = $"{translateText}\n Loading AssetBundles...";
                waitOneFrame = true;
                return false;
            }
            if (waitOneFrame)
            {
                waitOneFrame = false;
                CustomCosmeticsLoader.willLoad?.Invoke();
                CustomCosmeticsLoader.willLoad = null;
                RefreshStartupLoadAfterFallbackIfNeeded();
                ShowLoadedTextAndStartFadeOut(splashFinished);
                Logger.Info("Loading done");
                return false;
            }
            if (IsLoading)
            {
                if (LoadingText == null)
                    return false;

                string downloadSizeText = CustomCosmeticsLoader.GetDownloadSizeProgressText();
                string downloadSizeLine = string.IsNullOrEmpty(downloadSizeText) ? "" : $"\nDownloaded: {downloadSizeText}";
                if (CustomCosmeticsLoader.AssetBundlesDownloading)
                    LoadingText.text = $"{translateText}\n AssetBundles: {CustomCosmeticsLoader.AssetBundlesDownloadedCount}/{CustomCosmeticsLoader.AssetBundlesAllCount}\nSprites: {CustomCosmeticsLoader.SpritesAllCount - CustomCosmeticsLoader.SpritesDownloadingCount}/{CustomCosmeticsLoader.SpritesAllCount}{downloadSizeLine}";
                else
                    LoadingText.text = $"{translateText}\n Sprites: {CustomCosmeticsLoader.SpritesAllCount - CustomCosmeticsLoader.SpritesDownloadingCount}/{CustomCosmeticsLoader.SpritesAllCount}{downloadSizeLine}";
                return false;
            }
            return true;
        }
    }

    public static TextMeshPro LoadingText;

    private static void ShowLoadedTextAndStartFadeOut(bool useCenteredLayout)
    {
        if (LoadingText == null)
            return;

        UseCenteredLoadingTextScale = useCenteredLayout;
        if (useCenteredLayout)
            ApplyCenteredLoadingTextLayout();

        // そのままだと大きすぎるので、30%に縮小して表示する
        LoadingText.text = $"<size=20%>{ModTranslation.GetString("CustomCosmetics_Loaded")}</size>";
        SetLoadingTextAlpha(1f);

        StartLoadingTextFadeOut(useCenteredLayout ? LoadingCompleteVisibleSeconds : 0f);
    }

    private static void StartLoadingTextFadeOut(float visibleSeconds)
    {
        if (LoadingText == null || LoadingTextFadeStarted)
            return;

        LoadingTextFadeStarted = true;
        var runner = LoadingText.gameObject.GetComponent<LoadingTextFadeRunner>() ?? LoadingText.gameObject.AddComponent<LoadingTextFadeRunner>();
        runner.StartCoroutine(FadeOutLoadedText(visibleSeconds).WrapToIl2Cpp());
    }

    private static IEnumerator FadeOutLoadedText(float visibleSeconds)
    {
        if (visibleSeconds > 0f)
            yield return new WaitForSeconds(visibleSeconds);

        float elapsed = 0f;
        while (elapsed < LoadingCompleteFadeSeconds)
        {
            elapsed += Time.deltaTime;
            float alpha = 1f - Mathf.Clamp01(elapsed / LoadingCompleteFadeSeconds);
            SetLoadingTextAlpha(alpha);
            yield return null;
        }

        SetLoadingTextAlpha(0f);
        if (LoadingText != null && LoadingText.gameObject != null)
        {
            UnityEngine.Object.Destroy(LoadingText.gameObject);
            LoadingText = null;
        }
    }

    private static void ApplyCenteredLoadingTextLayout()
    {
        if (LoadingText == null)
            return;

        LoadingText.transform.localPosition = new(0, 0, -100);
        LoadingText.transform.localScale = Vector3.one * GetCenteredLoadingTextScale();
        LoadingText.alignment = TextAlignmentOptions.Center;
    }

    public static void RefreshLoadingTextScaleForCurrentCamera()
    {
        if (LoadingText == null || !UseCenteredLoadingTextScale)
            return;

        LoadingText.transform.localScale = Vector3.one * GetCenteredLoadingTextScale();
    }

    private static float GetCenteredLoadingTextScale()
    {
        Camera camera = Camera.main;
        if (camera == null || !camera.orthographic || camera.orthographicSize <= 0f)
            return CenteredLoadingTextBaseScale;

        if (CenteredLoadingTextBaseOrthographicSize <= 0f)
            CenteredLoadingTextBaseOrthographicSize = camera.orthographicSize;

        return CenteredLoadingTextBaseScale * camera.orthographicSize / CenteredLoadingTextBaseOrthographicSize;
    }

    private static void SetLoadingTextAlpha(float alpha)
    {
        if (LoadingText == null)
            return;

        Color color = LoadingText.color;
        color.a = Mathf.Clamp01(alpha);
        LoadingText.color = color;
    }

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
            LoadingText.alignment = TextAlignmentOptions.BottomRight;
            LoadingTextFadeStarted = false;
            UseCenteredLoadingTextScale = false;
            CenteredLoadingTextBaseOrthographicSize = -1f;
            text.SetParent(null, true);
            UnityEngine.Object.DontDestroyOnLoad(text.gameObject);
            text.gameObject.AddComponent<LoadingTextFadeRunner>();
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
                    WaitStartupTask(SuperNewRolesPlugin.CustomRPCManagerLoadTask, nameof(SuperNewRolesPlugin.CustomRPCManagerLoadTask));
                    Logger.Info("CustomRPCManagerLoadTask done");
                    WaitStartupTask(SuperNewRolesPlugin.HarmonyPatchAllTask, nameof(SuperNewRolesPlugin.HarmonyPatchAllTask));
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

        private static void WaitStartupTask(Task task, string taskName)
        {
            if (task == null)
                return;

            try
            {
                task.Wait();
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

public sealed class LoadingTextFadeRunner : MonoBehaviour
{
    public void Update()
    {
        CustomLoadingScreen.RefreshLoadingTextScaleForCurrentCamera();
    }
}
