using System;
using System.Security.Cryptography;
using System.Text;
using FluentAssertions;
using SuperNewRoles.Safety.Identity;
using Xunit;

namespace SuperNewRoles.Tests;

public class PlayerIdentityStoreTests
{
    [Fact]
    public void Unprotect_FailedSnrak1_DoesNotReturnMagicBlob()
    {
        byte[] leftover = Wrap("SNRAK1", new byte[16]);

        PlayerIdentityStore.Unprotect(leftover).Should().BeNull();
        leftover.Should().StartWith(Encoding.ASCII.GetBytes("SNRAK1"));
    }

    [Fact]
    public void SelectUnlockableStored_SkipsFailedSnrak1_AndUsesNextPlainKey()
    {
        using ECDsa key = PlayerIdentityCrypto.CreateKey();
        byte[] pkcs8 = PlayerIdentityCrypto.ExportPkcs8(key);
        byte[] leftoverPrefsOrExternal = Wrap("SNRAK1", new byte[24]);

        byte[] selected = PlayerIdentityStore.SelectUnlockableStored(leftoverPrefsOrExternal, pkcs8);

        selected.Should().Equal(pkcs8);
        using ECDsa restored = PlayerIdentityCrypto.ImportPkcs8(selected);
        PlayerIdentityCrypto.ExportUncompressedPublicKey(restored)
            .Should().Equal(PlayerIdentityCrypto.ExportUncompressedPublicKey(key));
    }

    [Fact]
    public void SelectUnlockableStored_OnlyFailedSnrak1_ReturnsNull()
    {
        byte[] leftover = Wrap("SNRAK1", new byte[24]);
        PlayerIdentityStore.SelectUnlockableStored(leftover).Should().BeNull();
        PlayerIdentityStore.IsUnlockable(leftover).Should().BeFalse();
    }

    [Fact]
    public void IsUnlockable_PlainPkcs8_IsTrue()
    {
        using ECDsa key = PlayerIdentityCrypto.CreateKey();
        byte[] pkcs8 = PlayerIdentityCrypto.ExportPkcs8(key);
        PlayerIdentityStore.IsUnlockable(pkcs8).Should().BeTrue();
        PlayerIdentityStore.Unprotect(pkcs8).Should().Equal(pkcs8);
    }

    private static byte[] Wrap(string magic, byte[] payload)
    {
        byte[] prefix = Encoding.ASCII.GetBytes(magic);
        byte[] wrapped = new byte[prefix.Length + payload.Length];
        Buffer.BlockCopy(prefix, 0, wrapped, 0, prefix.Length);
        Buffer.BlockCopy(payload, 0, wrapped, prefix.Length, payload.Length);
        return wrapped;
    }
}
