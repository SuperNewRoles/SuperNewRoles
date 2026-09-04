using System;
using System.Security.Cryptography;
using System.Text;
using FluentAssertions;
using SuperNewRoles.Safety.Api;
using SuperNewRoles.Safety.Identity;
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

public class PlayerSafetyUnsignedConductTests
{
    [Theory]
    [InlineData("GET", "/v1/conduct", true)]
    [InlineData("GET", "/v1/conduct?lang=ja", true)]
    [InlineData("POST", "/v1/conduct/consent", false)]
    [InlineData("GET", "/v1/notices", false)]
    [InlineData("GET", "/v1/conduct/consent", false)]
    public void OnlyUnsignedConductGetIsAllowedWithoutKey(string method, string path, bool allowed)
    {
        PlayerSafetyApiClient.IsUnsignedConductGet(method, path).Should().Be(allowed);
    }
}
