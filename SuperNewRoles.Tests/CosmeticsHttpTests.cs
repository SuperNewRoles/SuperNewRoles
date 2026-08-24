using System.Net;
using System.Net.Http;
using FluentAssertions;
using SuperNewRoles.CustomCosmetics;
using Xunit;

namespace SuperNewRoles.Tests;

public class CosmeticsHttpTests
{
    [Fact]
    public void GetMaxConcurrentDownloads_UsesThirtyStreams_WhenSocketsHttp2IsAvailable()
    {
        CosmeticsHttp.GetMaxConcurrentDownloads(isAndroid: true, socketsHttpHandlerAvailable: true)
            .Should().Be(30);
        CosmeticsHttp.GetMaxConcurrentDownloads(isAndroid: false, socketsHttpHandlerAvailable: true)
            .Should().Be(30);
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
    public void TryCreateSocketsHandler_PrefersHttp2WithoutHttp3()
    {
        bool created = CosmeticsHttp.TryCreateSocketsHandler(
            isAndroid: true,
            ignoreSslErrors: true,
            out SocketsHttpHandler handler);

        using (handler)
        {
            created.Should().BeTrue();
            handler.Should().NotBeNull();
            handler.EnableMultipleHttp2Connections.Should().BeTrue();
            handler.MaxConnectionsPerServer.Should().Be(CosmeticsHttp.AndroidHttp11MaxConnectionsPerServer);
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
        }
    }
}
