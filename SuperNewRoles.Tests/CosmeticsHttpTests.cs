using System.Collections;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using SuperNewRoles.CustomCosmetics;
using Xunit;

namespace SuperNewRoles.Tests;

public class CosmeticsHttpTests
{
    [Fact]
    public void GetMaxConcurrentDownloads_UsesOneHundredStreams_WhenSocketsHttp2IsAvailable()
    {
        CosmeticsHttp.GetMaxConcurrentDownloads(isAndroid: true, socketsHttpHandlerAvailable: true)
            .Should().Be(100);
        CosmeticsHttp.GetMaxConcurrentDownloads(isAndroid: false, socketsHttpHandlerAvailable: true)
            .Should().Be(100);
    }

    [Fact]
    public void GetMaxConcurrentDownloads_KeepsAndroidTcpCap_WhenMultiplexUnavailable()
    {
        CosmeticsHttp.GetMaxConcurrentDownloads(isAndroid: true, socketsHttpHandlerAvailable: false)
            .Should().Be(15);
        CosmeticsHttp.GetMaxConcurrentDownloads(isAndroid: false, socketsHttpHandlerAvailable: false)
            .Should().Be(30);
    }

    [Fact]
    public void CustomCosmeticsLoaderCapsBufferedResponseConcurrency()
    {
        CustomCosmeticsLoader.MAX_CONCURRENT_DOWNLOADS
            .Should().BeLessOrEqualTo(CosmeticsHttp.BufferedResponseMaxConcurrentDownloads);
    }

    [Fact]
    public void TryCreateSocketsHandler_PrefersHttp2WithoutHttp3()
    {
        bool created = CosmeticsHttp.TryCreateSocketsHandler(
            isAndroid: true,
            ignoreSslErrors: false,
            out SocketsHttpHandler handler);

        using (handler)
        {
            created.Should().BeTrue();
            handler.Should().NotBeNull();
            handler.EnableMultipleHttp2Connections.Should().BeTrue();
            handler.MaxConnectionsPerServer.Should().Be(CosmeticsHttp.AndroidHttp11MaxConnectionsPerServer);
            handler.SslOptions.RemoteCertificateValidationCallback.Should().BeNull();
        }

        CosmeticsHttp.ConfiguredRequestVersion.Should().Be(HttpVersion.Version20);
        CosmeticsHttp.ConfiguredVersionPolicy.Should().Be(HttpVersionPolicy.RequestVersionOrLower);
        CosmeticsHttp.ConfiguredUseHttp3.Should().BeFalse();
        CosmeticsHttp.ConfiguredEnableMultipleHttp2Connections.Should().BeTrue();
    }

    [Fact]
    public void TryCreateSocketsHandler_UsesDesktopConnectionCap()
    {
        bool created = CosmeticsHttp.TryCreateSocketsHandler(
            isAndroid: false,
            ignoreSslErrors: false,
            out SocketsHttpHandler handler);

        using (handler)
        {
            created.Should().BeTrue();
            handler.MaxConnectionsPerServer.Should().Be(CosmeticsHttp.DesktopHttp11MaxConnectionsPerServer);
            handler.SslOptions.RemoteCertificateValidationCallback.Should().BeNull();
        }
    }

    [Fact]
    public async Task PumpEnumeratorAsync_RunsNestedEnumeratorCurrent()
    {
        int innerMoves = 0;
        bool copiedAfterInner = false;

        IEnumerator Inner()
        {
            innerMoves++;
            yield return null;
            innerMoves++;
        }

        IEnumerator Outer()
        {
            yield return Inner();
            copiedAfterInner = true;
        }

        await CosmeticsHttpRequest.PumpEnumeratorAsync(Outer());

        innerMoves.Should().Be(2);
        copiedAfterInner.Should().BeTrue();
    }

    [Fact]
    public void IsHostProbeReachable_False_WhenEmptySnrFallback()
    {
        CosmeticsHttpRequest.IsHostProbeReachable(0, null).Should().BeFalse();
        CosmeticsHttpRequest.IsHostProbeReachable(0, "").Should().BeFalse();
        CosmeticsHttpRequest.IsHostProbeReachable(0, "The request was canceled or timed out.").Should().BeFalse();
    }

    [Fact]
    public void IsHostProbeReachable_True_WhenHttpStatusReturned()
    {
        CosmeticsHttpRequest.IsHostProbeReachable(200, null).Should().BeTrue();
        CosmeticsHttpRequest.IsHostProbeReachable(404, "HTTP Error 404").Should().BeTrue();
        CosmeticsHttpRequest.IsHostProbeReachable(500, "HTTP Error 500").Should().BeTrue();
    }

    [Fact]
    public async Task SendAsync_SnrFallback_PumpsSnrClient_WhenHttpClientFailsWithoutTimeout()
    {
        CosmeticsHttpRequest request = CosmeticsHttpRequest.Get("http://[invalid");
        request.timeout = 2f;
        request.ignoreSslErrors = false;

        await request.SendAsync();

        request.error.Should().NotBeNullOrEmpty();
        request.error.Should().Contain("Connection Error");
        request.responseCode.Should().Be(0);
        CosmeticsHttpRequest.IsHostProbeReachable(request.responseCode, request.error).Should().BeFalse();
    }
}
