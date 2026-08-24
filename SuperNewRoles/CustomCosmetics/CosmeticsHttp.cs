using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using SuperNewRoles;
using SuperNewRoles.Modules;

namespace SuperNewRoles.CustomCosmetics;

/// <summary>
/// カスタムコスメティック用の共有 HttpClient。
/// SocketsHttpHandler で HTTP/2 を優先し、非対応なら HTTP/1.1 keep-alive に落とす。
/// HTTP/3 は Unity/BepInEx/Android で QUIC が保証できないため使わない。
/// </summary>
public static class CosmeticsHttp
{
    public const int DefaultMaxConcurrentDownloads = 30;
    public const int AndroidTcpFallbackMaxConcurrentDownloads = 15;
    public const int AndroidHttp11MaxConnectionsPerServer = 8;
    public const int DesktopHttp11MaxConnectionsPerServer = 16;

    private static readonly HttpClient PooledClient;

    public static bool IsSocketsHttpHandlerAvailable { get; }
    public static bool IsPooledClientAvailable => PooledClient != null;
    public static Version ConfiguredRequestVersion { get; } = HttpVersion.Version20;
    public static HttpVersionPolicy ConfiguredVersionPolicy { get; } = HttpVersionPolicy.RequestVersionOrLower;
    public static bool ConfiguredEnableMultipleHttp2Connections { get; }
    public static bool ConfiguredUseHttp3 => false;

    static CosmeticsHttp()
    {
        try
        {
            PooledClient = CreatePooledClient(
                ModHelpers.IsAndroid(),
                ignoreSslErrors: true,
                out bool usedSocketsHandler,
                out bool enableMultipleHttp2Connections);
            IsSocketsHttpHandlerAvailable = usedSocketsHandler;
            ConfiguredEnableMultipleHttp2Connections = enableMultipleHttp2Connections;
            Logger.Info(usedSocketsHandler
                ? "Cosmetics HTTP: SocketsHttpHandler HTTP/2 preferred (RequestVersionOrLower, no HTTP/3)"
                : "Cosmetics HTTP: pooled HttpClientHandler (HTTP/1.1 keep-alive fallback)");
        }
        catch (Exception ex)
        {
            PooledClient = null;
            IsSocketsHttpHandlerAvailable = false;
            ConfiguredEnableMultipleHttp2Connections = false;
            Logger.Warning($"Cosmetics HTTP: pooled HttpClient unavailable, SNRHttpClient fallback. {ex.Message}");
        }
    }

    public static int GetMaxConcurrentDownloads(bool isAndroid, bool socketsHttpHandlerAvailable)
    {
        // HTTP/2 多重化時はストリーム数の上限。TCP を 30 本増やすわけではない。
        // 未対応時の Android は従来どおり 15 本の TCP に抑える。
        if (socketsHttpHandlerAvailable)
            return DefaultMaxConcurrentDownloads;
        return isAndroid ? AndroidTcpFallbackMaxConcurrentDownloads : DefaultMaxConcurrentDownloads;
    }

    public static bool TryCreateSocketsHandler(
        bool isAndroid,
        bool ignoreSslErrors,
        out SocketsHttpHandler handler)
    {
        handler = null;
        try
        {
            handler = new SocketsHttpHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                EnableMultipleHttp2Connections = true,
                MaxConnectionsPerServer = isAndroid
                    ? AndroidHttp11MaxConnectionsPerServer
                    : DesktopHttp11MaxConnectionsPerServer,
                PooledConnectionIdleTimeout = TimeSpan.FromMinutes(2),
                PooledConnectionLifetime = TimeSpan.FromMinutes(10),
                ConnectTimeout = TimeSpan.FromSeconds(10)
            };
            if (ignoreSslErrors)
                handler.SslOptions.RemoteCertificateValidationCallback = static (_, _, _, _) => true;
            return true;
        }
        catch
        {
            handler?.Dispose();
            handler = null;
            return false;
        }
    }

    internal static HttpClient GetPooledClient() => PooledClient;

    private static HttpClient CreatePooledClient(
        bool isAndroid,
        bool ignoreSslErrors,
        out bool usedSocketsHandler,
        out bool enableMultipleHttp2Connections)
    {
        usedSocketsHandler = false;
        enableMultipleHttp2Connections = false;

        HttpMessageHandler handler;
        if (TryCreateSocketsHandler(isAndroid, ignoreSslErrors, out SocketsHttpHandler socketsHandler))
        {
            handler = socketsHandler;
            usedSocketsHandler = true;
            enableMultipleHttp2Connections = socketsHandler.EnableMultipleHttp2Connections;
        }
        else
        {
            var fallback = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                MaxConnectionsPerServer = isAndroid
                    ? AndroidHttp11MaxConnectionsPerServer
                    : DesktopHttp11MaxConnectionsPerServer
            };
            if (ignoreSslErrors)
                fallback.ServerCertificateCustomValidationCallback = static (_, _, _, _) => true;
            handler = fallback;
        }

        return new HttpClient(handler, disposeHandler: true)
        {
            DefaultRequestVersion = HttpVersion.Version20,
            DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrLower,
            Timeout = Timeout.InfiniteTimeSpan
        };
    }
}

/// <summary>
/// Unity コルーチン向けの GET。可能なら共有 HttpClient、ダメなら SNRHttpClient。
/// </summary>
public sealed class CosmeticsHttpRequest
{
    public string url { get; }
    public SNRDownloadHandler downloadHandler { get; } = new();
    public string error { get; private set; }
    public long responseCode { get; private set; }
    public float timeout { get; set; } = 5f;
    public bool ignoreSslErrors { get; set; }
    public long maxResponseBytes { get; set; } = 128L * 1024 * 1024;
    public Action<long> downloadProgressChanged { get; set; }

    private CosmeticsHttpRequest(string url)
    {
        this.url = url;
    }

    public static CosmeticsHttpRequest Get(string url) => new(url);

    public IEnumerator SendWebRequest()
    {
        HttpClient client = CosmeticsHttp.GetPooledClient();
        if (client != null)
        {
            Task task = SendWithHttpClientAsync(client);
            while (!task.IsCompleted)
                yield return null;

            if (task.Status == TaskStatus.RanToCompletion)
                yield break;

            Exception inner = task.Exception?.InnerException ?? task.Exception;
            string reason = task.IsCanceled
                ? "The request was canceled or timed out."
                : inner?.Message ?? "The request failed without an exception.";
            Logger.Warning($"Cosmetics HTTP: HttpClient failed, SNRHttpClient fallback. {reason}");
        }

        yield return SendWithSnrHttpClient();
    }

    private async Task SendWithHttpClientAsync(HttpClient client)
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(Math.Max(0.1f, timeout)));
        using var request = new HttpRequestMessage(HttpMethod.Get, url)
        {
            Version = HttpVersion.Version20,
            VersionPolicy = HttpVersionPolicy.RequestVersionOrLower
        };
        request.Headers.TryAddWithoutValidation("User-Agent", "SNRHttpClient/1.0 (Unity)");
        request.Headers.TryAddWithoutValidation("Accept", "*/*");

        using HttpResponseMessage response = await client.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cts.Token).ConfigureAwait(false);

        responseCode = (long)response.StatusCode;
        using Stream responseStream = await response.Content.ReadAsStreamAsync(cts.Token).ConfigureAwait(false);
        using var memoryStream = new MemoryStream();
        byte[] buffer = new byte[8192];
        long total = 0;
        int read;
        while ((read = await responseStream.ReadAsync(buffer.AsMemory(0, buffer.Length), cts.Token).ConfigureAwait(false)) > 0)
        {
            total += read;
            if (total > maxResponseBytes)
                throw new InvalidOperationException($"HTTP response from {url} exceeded {maxResponseBytes} bytes.");
            memoryStream.Write(buffer, 0, read);
            downloadProgressChanged?.Invoke(total);
        }

        downloadHandler.data = memoryStream.ToArray();

        if ((int)response.StatusCode >= 400)
        {
            error = $"HTTP Error {response.StatusCode} ({response.ReasonPhrase ?? "Status Unknown"})";
            if (downloadHandler.text != null && downloadHandler.text.Length > 0 && downloadHandler.text.Length < 512)
                error += $": {downloadHandler.text}";
        }
    }

    private IEnumerator SendWithSnrHttpClient()
    {
        SNRHttpClient fallback = SNRHttpClient.Get(url);
        fallback.timeout = timeout;
        fallback.ignoreSslErrors = ignoreSslErrors;
        fallback.maxResponseBytes = maxResponseBytes;
        fallback.downloadProgressChanged = downloadProgressChanged;
        yield return fallback.SendWebRequest();
        error = fallback.error;
        responseCode = fallback.responseCode;
        downloadHandler.data = fallback.downloadHandler?.data;
    }
}
