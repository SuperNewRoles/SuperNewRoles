using System;
using System.Security.Cryptography;
using System.Text;
using FluentAssertions;
using SuperNewRoles.Safety.Identity;
using SuperNewRoles.Safety.Listing;
using Xunit;

namespace SuperNewRoles.Tests;

public class PlayerIdentityCryptoTests
{
    [Fact]
    public void SignAndVerify_AcceptsOwnKey_AndRejectsOtherKey()
    {
        using ECDsa a = PlayerIdentityCrypto.CreateKey();
        using ECDsa b = PlayerIdentityCrypto.CreateKey();
        byte[] pubA = PlayerIdentityCrypto.ExportUncompressedPublicKey(a);
        byte[] pubB = PlayerIdentityCrypto.ExportUncompressedPublicKey(b);
        byte[] body = Encoding.UTF8.GetBytes("{\"note\":\"x\"}");
        long ts = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        string nonce = PlayerIdentityCrypto.NewNonce();

        byte[] signature = PlayerIdentityCrypto.Sign(a, "blocks.create", ts, nonce, body);

        PlayerIdentityCrypto.Verify(pubA, "blocks.create", ts, nonce, body, signature).Should().BeTrue();
        PlayerIdentityCrypto.Verify(pubB, "blocks.create", ts, nonce, body, signature).Should().BeFalse();
        PlayerIdentityCrypto.Verify(pubA, "blocks.list", ts, nonce, body, signature).Should().BeFalse();
    }

    [Fact]
    public void Verify_RejectsStaleTimestamp()
    {
        using ECDsa key = PlayerIdentityCrypto.CreateKey();
        byte[] pub = PlayerIdentityCrypto.ExportUncompressedPublicKey(key);
        long ts = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - 400;
        byte[] signature = PlayerIdentityCrypto.Sign(key, "conduct.get", ts, "abc", Array.Empty<byte>());
        PlayerIdentityCrypto.Verify(pub, "conduct.get", ts, "abc", Array.Empty<byte>(), signature).Should().BeFalse();
    }

    [Fact]
    public void Pkcs8RoundTrip_PreservesAbilityToSign()
    {
        using ECDsa original = PlayerIdentityCrypto.CreateKey();
        byte[] pkcs8 = PlayerIdentityCrypto.ExportPkcs8(original);
        using ECDsa restored = PlayerIdentityCrypto.ImportPkcs8(pkcs8);
        byte[] pub = PlayerIdentityCrypto.ExportUncompressedPublicKey(restored);
        long ts = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        byte[] signature = PlayerIdentityCrypto.Sign(restored, "games.list", ts, "n1", Array.Empty<byte>());
        PlayerIdentityCrypto.Verify(pub, "games.list", ts, "n1", Array.Empty<byte>(), signature).Should().BeTrue();
        PlayerIdentityCrypto.ComputeInternalIdHex(pub).Should().HaveLength(64);
    }
}

public class PublicGameListingFilterTests
{
    [Fact]
    public void HidesHostBlockedByViewer_AndHostWhoBlockedViewer()
    {
        var games = new[]
        {
            new OccupancyGame("AAAAAA", "host-a", new[] { "host-a", "p1" }),
            new OccupancyGame("BBBBBB", "host-b", new[] { "host-b", "blocked-player" }),
            new OccupancyGame("CCCCCC", "host-c", new[] { "host-c" }),
        };

        PublicGameListingFilter.Filter(
            games,
            viewerId: "viewer",
            blockedIds: new[] { "host-a", "blocked-player" },
            hostsWhoBlockedViewer: new[] { "host-c" })
            .Should().ContainSingle(g => g.Code == "BBBBBB")
            .Which.HasBlockedPlayer.Should().BeTrue();
    }

    [Fact]
    public void OccupantBlockDoesNotHideRoom()
    {
        var games = new[]
        {
            new OccupancyGame("ROOM01", "host", new[] { "host", "blocked-player" }),
        };

        var result = PublicGameListingFilter.Filter(
            games,
            viewerId: "viewer",
            blockedIds: new[] { "blocked-player" },
            hostsWhoBlockedViewer: Array.Empty<string>());

        result.Should().ContainSingle();
        result[0].HasBlockedPlayer.Should().BeTrue();
        result[0].Code.Should().Be("ROOM01");
    }

    [Fact]
    public void UnknownHostStaysVisible()
    {
        var games = new[]
        {
            new OccupancyGame("ROOM02", "", new[] { "blocked-player" }),
        };

        var result = PublicGameListingFilter.Filter(
            games,
            viewerId: "viewer",
            blockedIds: new[] { "blocked-player" },
            hostsWhoBlockedViewer: Array.Empty<string>());

        result.Should().ContainSingle();
        result[0].HasBlockedPlayer.Should().BeTrue();
    }
}
