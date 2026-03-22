using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using HarmonyLib;
using Il2CppSystem.Dynamic.Utils;
using UnityEngine;
using UnityEngine.Networking;

namespace SuperNewRoles.Modules;

public static class CustomServer
{
    private const int HttpsProbeSampleCount = 3;
    private const int HttpsProbeTimeoutSeconds = 3;
    private const string HttpHeadMethod = "HEAD";
    private const string HttpGetMethod = "GET";
    private static bool IsSelectingRegionByLatency = false;
    private static bool ShouldRunLatencySelectionOnMainMenu = false;
    private static bool HasRunLatencySelectionThisSession = false;

    public static IRegionInfo[] defaultRegions;
    private static readonly string OldSNRJP_ServerName = "<size=150%><color=#ffa500>Super</color><color=#ff0000>New</color><color=#00ff00>Roles</color></size>\n<align=\"center\">Tokyo</align>";
    private static readonly string OldSNRJP_ServerNameNormalized = NormalizeRegionName(OldSNRJP_ServerName);
    private const string LegacySNRHost = "cs.supernewroles.com";
    public static string SNRJP_ServerName => "<size=150%><color=#ffa500>Super</color><color=#ff0000>New</color><color=#00ff00>Roles</color></size>\n<align=\"center\">Tokyo (JP)</align>";
    public static string SNRUSEast_ServerName => "<size=150%><color=#ffa500>Super</color><color=#ff0000>New</color><color=#00ff00>Roles</color></size>\n<align=\"center\">USEast</align>";
    public static IRegionInfo SNRRegionJP { get; private set; }
    public static IRegionInfo SNRRegionUSEast { get; private set; }

    private sealed class RegionLatencyResult
    {
        public IRegionInfo Region { get; }
        public string ProbeUrl { get; }
        public int MedianLatencyMs { get; }
        public int SuccessfulSamples { get; }

        public RegionLatencyResult(IRegionInfo region, string probeUrl, int medianLatencyMs, int successfulSamples)
        {
            Region = region;
            ProbeUrl = probeUrl;
            MedianLatencyMs = medianLatencyMs;
            SuccessfulSamples = successfulSamples;
        }
    }

    private sealed class HttpsProbeResult
    {
        public string Method = string.Empty;
        public bool Success;
        public int RttMs;
        public long ResponseCode;
        public string Error = string.Empty;
        public string Note = string.Empty;
    }

    private static IRegionInfo[] GetSNRRegions()
    {
        return new IRegionInfo[] { SNRRegionJP, SNRRegionUSEast }
            .Where(x => x != null)
            .ToArray();
    }

    private static bool HasValidCurrentRegion(ServerManager serverManager)
    {
        return serverManager.CurrentRegion != null && serverManager.AvailableRegions.Contains(serverManager.CurrentRegion);
    }

    private static bool IsLegacyTokyoRegion(IRegionInfo region)
    {
        if (region == null)
            return false;

        if (string.Equals(NormalizeRegionName(region.Name), OldSNRJP_ServerNameNormalized, StringComparison.Ordinal))
            return true;

        if (string.IsNullOrWhiteSpace(region.Name)
            || region.Servers == null)
            return false;

        bool looksLegacyTokyoName = NormalizeRegionName(region.Name).IndexOf("Tokyo</align>", StringComparison.OrdinalIgnoreCase) >= 0;
        if (!looksLegacyTokyoName)
            return false;

        return region.Servers.Any(server =>
            server != null
            && string.Equals(ExtractHost(server.Ip), LegacySNRHost, StringComparison.OrdinalIgnoreCase));
    }

    private static string NormalizeRegionName(string name)
    {
        return (name ?? string.Empty).Replace("\r\n", "\n").Replace('\r', '\n').Trim();
    }

    private static string ExtractHost(string ipOrUrl)
    {
        if (string.IsNullOrWhiteSpace(ipOrUrl))
            return string.Empty;

        string value = ipOrUrl.Trim();
        if (Uri.TryCreate(value, UriKind.Absolute, out Uri absoluteUri))
            return absoluteUri.Host;

        if (Uri.TryCreate("https://" + value, UriKind.Absolute, out Uri withSchemeUri))
            return withSchemeUri.Host;

        int slashIndex = value.IndexOf('/');
        if (slashIndex >= 0)
            value = value.Substring(0, slashIndex);

        int colonIndex = value.IndexOf(':');
        if (colonIndex >= 0)
            value = value.Substring(0, colonIndex);

        return value;
    }

    private static string BuildProbeUrl(IRegionInfo region)
    {
        if (region?.Servers == null || region.Servers.Length == 0)
            return null;

        ServerInfo server = region.Servers.FirstOrDefault(s => s != null);
        if (server == null)
            return null;

        string host = ExtractHost(server.Ip);
        if (string.IsNullOrWhiteSpace(host))
            host = ExtractHost(region.PingServer);
        if (string.IsNullOrWhiteSpace(host))
            return null;

        if (server.Port == 0 || server.Port == 443)
            return $"https://{host}/";

        return $"https://{host}:{server.Port}/";
    }

    private static int GetMedianLatencyMs(List<int> samples)
    {
        if (samples == null || samples.Count == 0)
            return int.MaxValue;

        int[] ordered = samples.OrderBy(x => x).ToArray();
        int mid = ordered.Length / 2;
        if (ordered.Length % 2 == 1)
            return ordered[mid];

        return (ordered[mid - 1] + ordered[mid]) / 2;
    }

    private static void SelectBestSNRRegionByHttpsLatency(string reason)
    {
        ServerManager serverManager = FastDestroyableSingleton<ServerManager>.Instance;
        if (serverManager == null)
        {
            Logger.Error("ServerManager is null. Skip HTTPS latency-based region selection.", "CustomServer");
            return;
        }

        IRegionInfo[] regions = GetSNRRegions();
        if (regions.Length == 0)
        {
            Logger.Error("No SNR regions available for HTTPS latency-based selection.", "CustomServer");
            return;
        }

        if (IsSelectingRegionByLatency)
        {
            Logger.Info($"HTTPS latency selection already running. Skip ({reason}).", "CustomServer");
            return;
        }

        if (AmongUsClient.Instance == null)
        {
            Logger.Warning($"AmongUsClient is null. Cannot run HTTPS latency selection ({reason}).", "CustomServer");
            ApplyFailedLatencyFallback(serverManager, regions, reason);
            return;
        }

        IsSelectingRegionByLatency = true;
        Logger.Info($"Start HTTPS latency-based SNR region selection ({reason}).", "CustomServer");
        AmongUsClient.Instance.StartCoroutine(CoSelectBestSNRRegionByHttpsLatency(serverManager, regions, reason).WrapToIl2Cpp());
    }

    private static IEnumerator CoSelectBestSNRRegionByHttpsLatency(ServerManager serverManager, IRegionInfo[] regions, string reason)
    {
        List<RegionLatencyResult> latencyResults = new();

        try
        {
            foreach (IRegionInfo region in regions)
            {
                string probeUrl = BuildProbeUrl(region);
                if (string.IsNullOrWhiteSpace(probeUrl))
                {
                    Logger.Warning($"Skipping region latency probe because probe URL could not be built: {region?.Name}", "CustomServer");
                    continue;
                }

                Logger.Info($"HTTPS probe target: {region.Name} -> {probeUrl}", "CustomServer");
                List<int> successfulRtts = new();
                for (int sample = 1; sample <= HttpsProbeSampleCount; sample++)
                {
                    HttpsProbeResult probeResult = new();
                    yield return CoMeasureHttpsProbeWithFallback(probeUrl, probeResult);

                    if (probeResult.Success)
                        successfulRtts.Add(probeResult.RttMs);

                    string noteText = string.IsNullOrWhiteSpace(probeResult.Note) ? string.Empty : $" note={probeResult.Note}";
                    string errorText = string.IsNullOrWhiteSpace(probeResult.Error) ? "-" : probeResult.Error;
                    Logger.Info(
                        $"HTTPS sample {sample}/{HttpsProbeSampleCount}: region={region.Name} method={probeResult.Method} code={probeResult.ResponseCode} success={probeResult.Success} rtt={probeResult.RttMs}ms error={errorText}{noteText}",
                        "CustomServer");
                }

                if (successfulRtts.Count == 0)
                {
                    Logger.Warning($"HTTPS probe failed for all samples: {region.Name} ({probeUrl})", "CustomServer");
                    continue;
                }

                int medianLatency = GetMedianLatencyMs(successfulRtts);
                latencyResults.Add(new RegionLatencyResult(region, probeUrl, medianLatency, successfulRtts.Count));
                Logger.Info(
                    $"HTTPS median: region={region.Name} url={probeUrl} median={medianLatency}ms successSamples={successfulRtts.Count}/{HttpsProbeSampleCount}",
                    "CustomServer");
            }

            if (latencyResults.Count > 0)
            {
                int bestLatencyMs = latencyResults.Min(x => x.MedianLatencyMs);
                List<RegionLatencyResult> tiedResults = latencyResults.Where(x => x.MedianLatencyMs == bestLatencyMs).ToList();

                IRegionInfo selectedRegion = null;
                if (tiedResults.Count > 1 && serverManager.CurrentRegion != null)
                {
                    selectedRegion = tiedResults
                        .Select(x => x.Region)
                        .FirstOrDefault(x => x != null && x.Name.Equals(serverManager.CurrentRegion.Name, StringComparison.OrdinalIgnoreCase));
                }

                if (selectedRegion == null)
                {
                    foreach (IRegionInfo region in regions)
                    {
                        if (region != null && tiedResults.Any(x => x.Region.Name.Equals(region.Name, StringComparison.OrdinalIgnoreCase)))
                        {
                            selectedRegion = region;
                            break;
                        }
                    }
                }

                if (selectedRegion == null)
                    selectedRegion = tiedResults[0].Region;

                Logger.Info($"Selected SNR region by HTTPS latency ({reason}): {selectedRegion.Name} ({bestLatencyMs}ms)", "CustomServer");
                serverManager.SetRegion(selectedRegion);
                yield break;
            }

            ApplyFailedLatencyFallback(serverManager, regions, reason);
        }
        finally
        {
            IsSelectingRegionByLatency = false;
        }
    }

    private static IEnumerator CoMeasureHttpsProbeWithFallback(string url, HttpsProbeResult result)
    {
        HttpsProbeResult headResult = new();
        yield return CoSendHttpsProbe(url, HttpHeadMethod, headResult);
        bool headUsable = headResult.Success && headResult.ResponseCode != 405 && headResult.ResponseCode != 501;
        if (headUsable)
        {
            result.Method = headResult.Method;
            result.Success = headResult.Success;
            result.RttMs = headResult.RttMs;
            result.ResponseCode = headResult.ResponseCode;
            result.Error = headResult.Error;
            result.Note = headResult.Note;
            yield break;
        }

        HttpsProbeResult getResult = new();
        yield return CoSendHttpsProbe(url, HttpGetMethod, getResult);

        result.Method = getResult.Method;
        result.Success = getResult.Success;
        result.RttMs = getResult.RttMs;
        result.ResponseCode = getResult.ResponseCode;
        result.Error = getResult.Error;
        result.Note = $"fallback_from_head_code={headResult.ResponseCode}";
    }

    private static IEnumerator CoSendHttpsProbe(string url, string method, HttpsProbeResult result)
    {
        result.Method = method;
        result.Success = false;
        result.RttMs = 0;
        result.ResponseCode = 0;
        result.Error = string.Empty;
        result.Note = string.Empty;

        UnityWebRequest request = method == HttpGetMethod
            ? UnityWebRequest.Get(url)
            : new UnityWebRequest(url, method);

        request.downloadHandler = new DownloadHandlerBuffer();
        request.timeout = HttpsProbeTimeoutSeconds;
        float startedAt = Time.realtimeSinceStartup;

        yield return request.SendWebRequest();

        result.RttMs = Mathf.Max(0, Mathf.RoundToInt((Time.realtimeSinceStartup - startedAt) * 1000f));
        result.ResponseCode = request.responseCode;
        result.Error = request.error;
        result.Success = request.responseCode > 0;

        request.Dispose();
    }

    private static void ApplyFailedLatencyFallback(ServerManager serverManager, IRegionInfo[] regions, string reason)
    {
        if (HasValidCurrentRegion(serverManager))
        {
            Logger.Warning($"All HTTPS latency probes failed ({reason}). Keep current region: {serverManager.CurrentRegion.Name}", "CustomServer");
            return;
        }

        IRegionInfo fallbackRegion = regions.FirstOrDefault();
        if (fallbackRegion != null)
        {
            Logger.Warning($"All HTTPS latency probes failed ({reason}). Current region is invalid, fallback to first SNR region: {fallbackRegion.Name}", "CustomServer");
            serverManager.SetRegion(fallbackRegion);
            return;
        }

        Logger.Error($"All HTTPS latency probes failed ({reason}) and no fallback region is available.", "CustomServer");
    }

    public static void UpdateRegions()
    {
        ServerManager serverManager = FastDestroyableSingleton<ServerManager>.Instance;
        SNRRegionJP = new StaticHttpRegionInfo(SNRJP_ServerName, StringNames.NoTranslation,
                "cs.supernewroles.com", new([
                        new("http-1", SNRURLs.SNRCS_JP, 443, false)
                    ])).TryCast<IRegionInfo>();
        SNRRegionUSEast = new StaticHttpRegionInfo(SNRUSEast_ServerName, StringNames.NoTranslation,
                "cs-useast.supernewroles.com", new([
                        new("http-1", SNRURLs.SNRCS_USEast, 443, false)
                    ])).TryCast<IRegionInfo>();
        var regions = new IRegionInfo[2] {
                SNRRegionJP,
                SNRRegionUSEast
            };

        IRegionInfo currentRegion = serverManager.CurrentRegion;
        Logger.Info($"Adding {regions.Length} regions");
        foreach (IRegionInfo region in regions)
        {
            if (region == null)
                Logger.Error("Could not add region", "CustomServer");
            else
            {
                if (currentRegion != null && region.Name.Equals(currentRegion.Name, StringComparison.OrdinalIgnoreCase))
                    currentRegion = region;
                serverManager.AddOrUpdateRegion(region);
            }
        }

        // AU remembers the previous region that was set, so we need to restore it
        if (currentRegion != null)
        {
            Logger.Info("Resetting previous region");
            serverManager.SetRegion(currentRegion);
        }

        // remove legacy Tokyo region(s) and persist immediately
        var filteredRegions = serverManager.AvailableRegions.Where(x => !IsLegacyTokyoRegion(x)).ToArray();
        int removedLegacyTokyoCount = serverManager.AvailableRegions.Length - filteredRegions.Length;
        if (removedLegacyTokyoCount > 0)
        {
            serverManager.AvailableRegions = filteredRegions;
            bool currentRegionAvailable = HasValidCurrentRegion(serverManager);

            if (!currentRegionAvailable)
                serverManager.SetRegion(SNRRegionJP);
            else
                serverManager.SaveServers();

            Logger.Info($"Removed {removedLegacyTokyoCount} legacy SNR Tokyo region(s) and persisted server regions", "CustomServer");
        }

        ShouldRunLatencySelectionOnMainMenu = true;
        HasRunLatencySelectionThisSession = false;
        Logger.Info("Queued HTTPS latency-based SNR region selection for MainMenu post-login.", "CustomServer");
    }

    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.LateUpdate))]
    public static class MainMenuManager_LateUpdate
    {
        public static void Postfix(MainMenuManager __instance)
        {
            if (!ShouldRunLatencySelectionOnMainMenu || HasRunLatencySelectionThisSession)
                return;

            if (FastDestroyableSingleton<EOSManager>.Instance == null || !FastDestroyableSingleton<EOSManager>.Instance.HasFinishedLoginFlow())
                return;

            HasRunLatencySelectionThisSession = true;
            ShouldRunLatencySelectionOnMainMenu = false;
            SelectBestSNRRegionByHttpsLatency("MainMenu.LateUpdate: post-login startup selection");
        }
    }
}
